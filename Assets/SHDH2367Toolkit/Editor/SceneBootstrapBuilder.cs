using SHDH2367.Toolkit.Runtime;
using SHDH2367.Toolkit.Runtime.Controllers;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace SHDH2367.Toolkit.Editor
{
    public static class SceneBootstrapBuilder
    {
        const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        const string FirstPersonPrefabPath = "Assets/Scenes/FirstPersonPlayer.prefab";
        const string ThirdPersonPrefabPath = "Assets/Scenes/ThirdPersonPlayer.prefab";
        const string SideViewPrefabPath = "Assets/Scenes/SideViewPlayer.prefab";

        // ─── Full scene setup ────────────────────────────────────────────────

        public static void BuildCurrentScene(ToolkitConfig config)
        {
            if (config == null)
            {
                Debug.LogError("ToolkitConfig is null. Cannot build scene.");
                return;
            }

            EnsureEventSystem();
            CreateGlobalVolume();

            var hud = config.modules.hud ? CreateHud() : null;
            var gm = config.modules.gameLoop ? CreateGameManager(config, hud) : null;
            var playerRoot = config.modules.player ? CreatePlayerRig(config) : null;

            if (playerRoot != null)
            {
                var attackType = FindMonoBehaviourType("PlayerAttack");
                if (attackType != null && playerRoot.GetComponent(attackType) == null)
                    playerRoot.AddComponent(attackType);
            }

            if (config.modules.feedback && playerRoot != null)
            {
                var feedbackCam = playerRoot.GetComponentInChildren<Camera>();
                TryAddJuiceFeedback(feedbackCam != null ? feedbackCam.gameObject : playerRoot);
            }

            if (config.modules.collectibles)
                CreateCollectibles(config);

            if (config.generateSampleLevel)
                LevelTemplateGenerator.Generate(config.gameplayMode, config);

            CreateInstaller(config, gm, hud, playerRoot);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        // ─── Individual "Add to scene" actions ──────────────────────────────

        public static void AddPlayerToScene(ToolkitConfig config)
        {
            EnsureEventSystem();
            var playerRoot = CreatePlayerRig(config);
            if (playerRoot != null)
            {
                var attackType = FindMonoBehaviourType("PlayerAttack");
                if (attackType != null && playerRoot.GetComponent(attackType) == null)
                    playerRoot.AddComponent(attackType);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[Toolkit] Player rig added ({config.gameplayMode}).");
        }

        public static void AddCameraToScene()
        {
            var camGO = new GameObject("Toolkit_Camera");
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 8f, -12f);
            camGO.transform.rotation = Quaternion.Euler(25f, 0f, 0f);
            var cam = camGO.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            camGO.AddComponent<AudioListener>();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Camera added to scene.");
        }

        public static void AddLightToScene()
        {
            var lightGO = new GameObject("Toolkit_DirectionalLight");
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            light.color = Color.white;
            light.shadows = LightShadows.Soft;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Directional light added to scene.");
        }

        public static void AddHudToScene(ToolkitConfig config)
        {
            EnsureEventSystem();
            CreateHud();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] HUD canvas added to scene.");
        }

        public static void AddCollectableToScene()
        {
            var collectibleType = FindMonoBehaviourType("Collectible");
            var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "Toolkit_Orb";
            orb.transform.localScale = Vector3.one * 0.6f;
            orb.transform.position = new Vector3(
                UnityEngine.Random.Range(-7f, 7f), 1f, UnityEngine.Random.Range(-7f, 7f));
            UnityEngine.Object.DestroyImmediate(orb.GetComponent<SphereCollider>());
            var trigger = orb.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.55f;
            if (collectibleType != null)
                orb.AddComponent(collectibleType);
            else
                Debug.LogWarning("[Toolkit] Collectible script not found. Orb created without behaviour.");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Collectable orb added to scene.");
        }

        public static void AddPostProcessingToScene()
        {
            CreateGlobalVolume();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Post processing volume added to scene.");
        }

        public static void AddArenaLevelToScene(ToolkitConfig config)
        {
            if (config == null) return;
            LevelTemplateGenerator.GenerateArena(config);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Arena level (floor, walls, obstacles) added.");
        }

        public static void AddSideScrollerLevelToScene(ToolkitConfig config)
        {
            if (config == null) return;
            LevelTemplateGenerator.GenerateSideView(config);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Side-scroller level strip added.");
        }

        public static void AddJuiceFeedbackToMainCamera()
        {
            var cam = Camera.main;
            if (cam == null)
                cam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (cam == null)
            {
                Debug.LogWarning("[Toolkit] No Camera found — add a camera first.");
                return;
            }
            TryAddJuiceFeedback(cam.gameObject);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[Toolkit] JuiceFeedback added to '{cam.gameObject.name}'.");
        }

        public static void AddExtraPostEffectsToScene()
        {
            CreateGlobalVolume();
            var profile = TryGetToolkitGlobalVolumeProfile();
            if (profile != null)
            {
                TryAddUrpBloom(profile);
                TryAddUrpVignette(profile);
            }
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Extra post FX (Bloom, Vignette) added to global volume.");
        }

        public static void AddObstacleToScene(ToolkitConfig config)
        {
            float w = config != null ? Mathf.Max(8f, config.arenaSize.x) : 12f;
            float d = config != null ? Mathf.Max(8f, config.arenaSize.y) : 12f;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Toolkit_Obstacle";
            cube.transform.position = new Vector3(
                UnityEngine.Random.Range(-w * 0.35f, w * 0.35f),
                0.6f,
                UnityEngine.Random.Range(-d * 0.35f, d * 0.35f));
            cube.transform.localScale = new Vector3(
                UnityEngine.Random.Range(1f, 2.4f), 1.2f, UnityEngine.Random.Range(1f, 2.4f));
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Obstacle cube added.");
        }

        public static void AddEnemyToScene()
        {
            var enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = "Toolkit_Enemy";
            enemy.transform.position = new Vector3(4f, 1f, 2f);
            UnityEngine.Object.DestroyImmediate(enemy.GetComponent<CapsuleCollider>());
            var col = enemy.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height = 1.8f;
            col.radius = 0.45f;

            var renderer = enemy.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                var mat = new Material(renderer.sharedMaterial);
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", new Color(0.85f, 0.18f, 0.18f));
                else
                    mat.color = new Color(0.85f, 0.18f, 0.18f);
                renderer.sharedMaterial = mat;
            }

            var enemyType = FindMonoBehaviourType("SimpleEnemy");
            if (enemyType != null)
                enemy.AddComponent(enemyType);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Toolkit] Enemy hazard added (trigger contact → game over).");
        }

        // ─── Player rig builders ─────────────────────────────────────────────

        static GameObject CreatePlayerRig(ToolkitConfig config)
        {
            switch (config.gameplayMode)
            {
                case GameplayModeType.ThirdPerson:
                    return CreateThirdPersonRig();
                case GameplayModeType.SideView:
                    return CreateSideViewRig();
                default:
                    return CreateFirstPersonRig();
            }
        }

        static void ConfigureToolkitPlayerInput(PlayerInput pi)
        {
            if (pi == null)
                return;
            if (pi.actions == null)
                pi.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            pi.defaultActionMap = "Player";
            pi.defaultControlScheme = "Keyboard&Mouse";
            // Controllers poll with ReadValue<>() directly — Invoke C# Events is required;
            // Send Messages (default) does not enable action maps for polling-based scripts.
            pi.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
        }

        static GameObject InstantiatePlayerPrefab(string prefabPath)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[Toolkit] Player prefab not found at '{prefabPath}'.");
                return null;
            }

            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (instance == null)
                return null;

            ConfigureToolkitPlayerInput(instance.GetComponent<PlayerInput>());
            return instance;
        }

        static GameObject CreateFirstPersonRig()
        {
            return InstantiatePlayerPrefab(FirstPersonPrefabPath);
        }

        static GameObject CreateThirdPersonRig()
        {
            return InstantiatePlayerPrefab(ThirdPersonPrefabPath);
        }

        static GameObject CreateSideViewRig()
        {
            return InstantiatePlayerPrefab(SideViewPrefabPath);
        }

        // ─── Scene element builders ──────────────────────────────────────────

        static Behaviour CreateGameManager(ToolkitConfig config, Behaviour hud)
        {
            var gameManagerType = FindMonoBehaviourType("GameManager");
            if (gameManagerType == null)
            {
                Debug.LogWarning("GameManager script was not found. Skipping Toolkit_GameManager creation.");
                return null;
            }

            var go = new GameObject("Toolkit_GameManager");
            var gm = (Behaviour)go.AddComponent(gameManagerType);
            SetMemberValue(gm, "gameDuration", config.gameDuration);
            SetMemberValue(gm, "hud", hud);
            SetMemberValue(gm, "restartAction", ResolveRestartActionReference());
            return gm;
        }

        static Behaviour CreateHud()
        {
            var hudType = FindMonoBehaviourType("HUDManager");
            if (hudType == null)
            {
                Debug.LogWarning("HUDManager script was not found. Skipping Toolkit_HUDCanvas creation.");
                return null;
            }

            var canvasGO = new GameObject("Toolkit_HUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var hud = (Behaviour)canvasGO.AddComponent(hudType);

            var timer = CreateText("TimerText", canvasGO.transform, new Vector2(110f, -30f), 24, TextAnchor.UpperLeft);
            var orb = CreateText("OrbCountText", canvasGO.transform, new Vector2(110f, -70f), 24, TextAnchor.UpperLeft);
            var crosshair = CreateImage("Crosshair", canvasGO.transform, new Vector2(0f, 0f), new Vector2(8f, 8f), Color.white);
            crosshair.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            crosshair.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

            var winPanel = CreatePanel("WinPanel", canvasGO.transform);
            var winText = CreateText("WinText", winPanel.transform, Vector2.zero, 36, TextAnchor.MiddleCenter);
            var winButton = CreateButton("WinRestartBtn", winPanel.transform, new Vector2(0f, -80f), "Restart");
            winPanel.SetActive(false);

            var losePanel = CreatePanel("LosePanel", canvasGO.transform);
            var loseText = CreateText("LoseText", losePanel.transform, Vector2.zero, 36, TextAnchor.MiddleCenter);
            var loseButton = CreateButton("LoseRestartBtn", losePanel.transform, new Vector2(0f, -80f), "Restart");
            losePanel.SetActive(false);

            var hpBarBg = CreateImage("HPBarBackground", canvasGO.transform, Vector2.zero, new Vector2(200f, 20f), new Color(0.15f, 0.15f, 0.15f, 0.8f));
            var hpBarBgRt = hpBarBg.rectTransform;
            hpBarBgRt.anchorMin = new Vector2(0f, 1f);
            hpBarBgRt.anchorMax = new Vector2(0f, 1f);
            hpBarBgRt.pivot = new Vector2(0f, 1f);
            hpBarBgRt.anchoredPosition = new Vector2(110f, -100f);

            var hpBarFill = CreateImage("HPBarFill", hpBarBg.transform, Vector2.zero, Vector2.zero, new Color(0.15f, 0.85f, 0.25f));
            hpBarFill.type = Image.Type.Filled;
            hpBarFill.fillMethod = Image.FillMethod.Horizontal;
            hpBarFill.fillAmount = 1f;
            var hpFillRt = hpBarFill.rectTransform;
            hpFillRt.anchorMin = Vector2.zero;
            hpFillRt.anchorMax = Vector2.one;
            hpFillRt.offsetMin = Vector2.zero;
            hpFillRt.offsetMax = Vector2.zero;

            var hpTextElem = CreateText("HPText", canvasGO.transform, new Vector2(320f, -97f), 18, TextAnchor.UpperLeft);
            hpTextElem.text = "5 / 5";

            SetMemberValue(hud, "timerText", timer);
            SetMemberValue(hud, "orbCountText", orb);
            SetMemberValue(hud, "crosshair", crosshair);
            SetMemberValue(hud, "hpBarBackground", hpBarBg);
            SetMemberValue(hud, "hpBarFill", hpBarFill);
            SetMemberValue(hud, "hpText", hpTextElem);
            SetMemberValue(hud, "winPanel", winPanel);
            SetMemberValue(hud, "winMessageText", winText);
            SetMemberValue(hud, "winRestartButton", winButton);
            SetMemberValue(hud, "losePanel", losePanel);
            SetMemberValue(hud, "loseMessageText", loseText);
            SetMemberValue(hud, "loseRestartButton", loseButton);

            return hud;
        }

        static void CreateCollectibles(ToolkitConfig config)
        {
            int count = Mathf.Max(1, config.collectibleCount);
            var collectibleType = FindMonoBehaviourType("Collectible");
            if (collectibleType == null)
                Debug.LogWarning("Collectible script was not found. Orbs will be created without collectible behavior.");

            for (int i = 0; i < count; i++)
            {
                var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                orb.name = $"Toolkit_Orb_{i + 1}";
                orb.transform.localScale = Vector3.one * 0.6f;
                orb.transform.position = new Vector3(UnityEngine.Random.Range(-7f, 7f), 1f, UnityEngine.Random.Range(-7f, 7f));
                UnityEngine.Object.DestroyImmediate(orb.GetComponent<SphereCollider>());
                var trigger = orb.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
                trigger.radius = 0.55f;
                if (collectibleType != null)
                    orb.AddComponent(collectibleType);
            }
        }

        static void CreateGlobalVolume()
        {
            if (GameObject.Find("Toolkit_GlobalVolume") != null)
                return;

            Type volumeType = Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            Type volumeProfileType = Type.GetType("UnityEngine.Rendering.VolumeProfile, Unity.RenderPipelines.Core.Runtime");
            if (volumeType == null || volumeProfileType == null)
                return;

            var volumeGO = new GameObject("Toolkit_GlobalVolume");
            var volume = volumeGO.AddComponent(volumeType);
            SetMemberValue(volume, "isGlobal", true);

            var profile = ScriptableObject.CreateInstance(volumeProfileType);
            TryAddUrpChromaticAberration(profile);
            SetMemberValue(volume, "profile", profile);
        }

        static ScriptableObject TryGetToolkitGlobalVolumeProfile()
        {
            var go = GameObject.Find("Toolkit_GlobalVolume");
            if (go == null)
                return null;
            var volumeType = Type.GetType("UnityEngine.Rendering.Volume, Unity.RenderPipelines.Core.Runtime");
            if (volumeType == null)
                return null;
            var volume = go.GetComponent(volumeType);
            if (volume == null)
                return null;
            return GetMemberValue(volume, "profile") as ScriptableObject;
        }

        static void CreateInstaller(ToolkitConfig config, Behaviour gm, Behaviour hud, GameObject player)
        {
            var root = new GameObject("Toolkit_RuntimeRoot");
            var installer = root.AddComponent<RuntimeInstaller>();
            installer.gameManager = gm;
            installer.hudManager = hud;
            installer.playerCamera = player != null ? player.GetComponentInChildren<Camera>() : null;
            installer.ApplyFromConfig(config);
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            go.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }

        static InputActionReference ResolveRestartActionReference()
        {
            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (asset == null)
                return null;

            var submit = asset.FindAction("Submit", throwIfNotFound: false);
            if (submit != null)
                return InputActionReference.Create(submit);

            var attack = asset.FindAction("Attack", throwIfNotFound: false);
            return attack != null ? InputActionReference.Create(attack) : null;
        }

        static void TryAddUrpChromaticAberration(ScriptableObject profile)
        {
            Type chromaticType = Type.GetType("UnityEngine.Rendering.Universal.ChromaticAberration, Unity.RenderPipelines.Universal.Runtime");
            if (chromaticType == null || profile == null)
                return;

            MethodInfo addMethod = profile.GetType().GetMethod("Add", new[] { typeof(Type), typeof(bool) });
            if (addMethod == null)
                return;

            var volumeComponent = addMethod.Invoke(profile, new object[] { chromaticType, true });
            var intensityParam = GetMemberValue(volumeComponent, "intensity");
            if (intensityParam == null)
                return;

            SetMemberValue(intensityParam, "overrideState", true);
            SetMemberValue(intensityParam, "value", 0f);
        }

        static void TryAddUrpBloom(ScriptableObject profile)
        {
            Type bloomType = Type.GetType("UnityEngine.Rendering.Universal.Bloom, Unity.RenderPipelines.Universal.Runtime");
            if (bloomType == null || profile == null || VolumeProfileHasComponent(profile, bloomType))
                return;

            MethodInfo addMethod = profile.GetType().GetMethod("Add", new[] { typeof(Type), typeof(bool) });
            if (addMethod == null)
                return;

            var volumeComponent = addMethod.Invoke(profile, new object[] { bloomType, true });
            var intensity = GetMemberValue(volumeComponent, "intensity");
            if (intensity != null)
            {
                SetMemberValue(intensity, "overrideState", true);
                SetMemberValue(intensity, "value", 0.22f);
            }

            var threshold = GetMemberValue(volumeComponent, "threshold");
            if (threshold != null)
            {
                SetMemberValue(threshold, "overrideState", true);
                SetMemberValue(threshold, "value", 1f);
            }
        }

        static void TryAddUrpVignette(ScriptableObject profile)
        {
            Type vigType = Type.GetType("UnityEngine.Rendering.Universal.Vignette, Unity.RenderPipelines.Universal.Runtime");
            if (vigType == null || profile == null || VolumeProfileHasComponent(profile, vigType))
                return;

            MethodInfo addMethod = profile.GetType().GetMethod("Add", new[] { typeof(Type), typeof(bool) });
            if (addMethod == null)
                return;

            var volumeComponent = addMethod.Invoke(profile, new object[] { vigType, true });
            var intensity = GetMemberValue(volumeComponent, "intensity");
            if (intensity != null)
            {
                SetMemberValue(intensity, "overrideState", true);
                SetMemberValue(intensity, "value", 0.28f);
            }
        }

        static bool VolumeProfileHasComponent(ScriptableObject profile, Type componentType)
        {
            if (profile == null || componentType == null)
                return false;

            var prop = profile.GetType().GetProperty("components", BindingFlags.Public | BindingFlags.Instance);
            if (prop == null)
                return false;

            if (prop.GetValue(profile) is not System.Collections.IEnumerable list)
                return false;

            foreach (object entry in list)
            {
                if (entry != null && componentType.IsInstanceOfType(entry))
                    return true;
            }
            return false;
        }

        static void TryAddJuiceFeedback(GameObject target)
        {
            if (target == null)
                return;
            var juiceType = FindMonoBehaviourType("JuiceFeedback");
            if (juiceType == null)
                return;
            if (target.GetComponent(juiceType) == null)
                target.AddComponent(juiceType);
        }

        // ─── Reflection helpers ──────────────────────────────────────────────

        static Type FindMonoBehaviourType(string typeName)
        {
            var allMonoBehaviours = TypeCache.GetTypesDerivedFrom<MonoBehaviour>();
            for (int i = 0; i < allMonoBehaviours.Count; i++)
            {
                var type = allMonoBehaviours[i];
                if (type.Name == typeName)
                    return type;
            }
            return null;
        }

        static object GetMemberValue(object target, string memberName)
        {
            if (target == null)
                return null;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();
            var field = type.GetField(memberName, flags);
            if (field != null)
                return field.GetValue(target);

            var prop = type.GetProperty(memberName, flags);
            if (prop != null && prop.CanRead)
                return prop.GetValue(target, null);

            return null;
        }

        static bool SetMemberValue(object target, string memberName, object value)
        {
            if (target == null)
                return false;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type type = target.GetType();
            var field = type.GetField(memberName, flags);
            if (field != null)
            {
                field.SetValue(target, value);
                return true;
            }

            var prop = type.GetProperty(memberName, flags);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value, null);
                return true;
            }

            return false;
        }

        // ─── uGUI helpers ────────────────────────────────────────────────────

        static Text CreateText(string name, Transform parent, Vector2 anchoredPos, int fontSize, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(400f, 60f);

            var text = go.GetComponent<Text>();
            text.text = name.Contains("Timer") ? "1:00" : "0 / 0";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.color = Color.white;
            return text;
        }

        static Image CreateImage(string name, Transform parent, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = color;
            return img;
        }

        static GameObject CreatePanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);
            return go;
        }

        static Button CreateButton(string name, Transform parent, Vector2 anchoredPos, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = new Vector2(180f, 48f);

            var txt = CreateText($"{name}_Label", go.transform, Vector2.zero, 24, TextAnchor.MiddleCenter);
            var txtRt = txt.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.text = label;
            return go.GetComponent<Button>();
        }
    }
}
