using SHDH2367.Toolkit.Runtime;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SHDH2367.Toolkit.Editor
{
    public class ToolkitDashboardWindow : EditorWindow
    {
        const string ConfigAssetPath = "Assets/SHDH2367Toolkit/ToolkitConfig.asset";
        ToolkitConfig config;
        Vector2 scroll;

        [MenuItem("Tools/SHDH2367 Toolkit/Dashboard")]
        static void Open()
        {
            var window = GetWindow<ToolkitDashboardWindow>("SHDH2367 Toolkit");
            window.minSize = new Vector2(520f, 780f);
            window.Show();
        }

        [MenuItem("Tools/SHDH2367 Toolkit/Setup Wizard")]
        static void OpenWizard()
        {
            ToolkitSetupWizard.Open();
        }

        void OnEnable()
        {
            config = LoadOrCreateConfig();
        }

        void OnGUI()
        {
            if (config == null)
                config = LoadOrCreateConfig();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.Space(6);

            DrawHeader();
            EditorGUILayout.Space(8);

            DrawGameplayConfig();
            EditorGUILayout.Space(10);

            DrawAddItemButtons();
            EditorGUILayout.Space(10);

            DrawTemplateConfig();
            EditorGUILayout.Space(10);

            DrawSceneActions();
            EditorGUILayout.Space(10);

            DrawValidation();
            EditorGUILayout.EndScrollView();
        }

        // ─── Sections ────────────────────────────────────────────────────────

        void DrawHeader()
        {
            EditorGUILayout.LabelField("SHDH2367 Interactivity Toolkit", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Select a gameplay mode, then use the buttons below to add elements to your scene one by one — or click \"Setup Full Scene\" to generate everything at once.",
                MessageType.Info);
        }

        void DrawGameplayConfig()
        {
            EditorGUILayout.LabelField("Gameplay Setup", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            config.gameplayMode = (GameplayModeType)EditorGUILayout.EnumPopup("Gameplay Mode", config.gameplayMode);
            config.gameDuration  = EditorGUILayout.FloatField("Game Duration (s)", config.gameDuration);
            config.collectibleCount = EditorGUILayout.IntSlider("Collectible Count (batch)", config.collectibleCount, 1, 50);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(config);
        }

        void DrawAddItemButtons()
        {
            EditorGUILayout.LabelField("Add to Scene", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Each button adds one element to the open scene. Level buttons use Arena Size & Obstacle Count from the template section below.",
                MessageType.None);

            EditorGUILayout.Space(4);

            // Row 1 — core rig & view
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Player", "Add Player\n(+ Camera)", () => SceneBootstrapBuilder.AddPlayerToScene(config),
                new Color(0.35f, 0.65f, 1f));
            DrawAddButton("Camera", "Add Camera\n(standalone)", () => SceneBootstrapBuilder.AddCameraToScene(),
                new Color(0.55f, 0.85f, 1f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Row 2 — lighting & UI
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Light", "Add Light\n(Directional)", () => SceneBootstrapBuilder.AddLightToScene(),
                new Color(1f, 0.9f, 0.4f));
            DrawAddButton("HUD", "Add HUD\n(Canvas + UI)", () => SceneBootstrapBuilder.AddHudToScene(config),
                new Color(0.5f, 1f, 0.65f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Row 3 — pickups & base post volume
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Collectable", "Add Collectable\n(one orb)", () => SceneBootstrapBuilder.AddCollectableToScene(),
                new Color(1f, 0.75f, 0.3f));
            DrawAddButton("Post Processing", "Add Post FX\n(Global Volume)", () => SceneBootstrapBuilder.AddPostProcessingToScene(),
                new Color(0.8f, 0.5f, 1f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Row 4 — level geometry (independent of gameplay mode dropdown)
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Arena", "Add Level\n(Arena + obstacles)", () => SceneBootstrapBuilder.AddArenaLevelToScene(config),
                new Color(0.72f, 0.55f, 0.38f));
            DrawAddButton("Side", "Add Level\n(Side-scroller strip)", () => SceneBootstrapBuilder.AddSideScrollerLevelToScene(config),
                new Color(0.35f, 0.72f, 0.65f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Row 5 — gameplay juice & grading
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Juice", "Add Effects\n(Screen juice → cam)", () => SceneBootstrapBuilder.AddJuiceFeedbackToMainCamera(),
                new Color(1f, 0.5f, 0.72f));
            DrawAddButton("Bloom", "Add Effects\n(Bloom + Vignette)", () => SceneBootstrapBuilder.AddExtraPostEffectsToScene(),
                new Color(0.45f, 0.32f, 0.62f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Row 6 — hazards & props
            EditorGUILayout.BeginHorizontal();
            DrawAddButton("Enemy", "Add Enemy\n(red hazard)", () => SceneBootstrapBuilder.AddEnemyToScene(),
                new Color(0.78f, 0.28f, 0.28f));
            DrawAddButton("Obstacle", "Add Obstacle\n(random cube)", () => SceneBootstrapBuilder.AddObstacleToScene(config),
                new Color(0.55f, 0.48f, 0.4f));
            EditorGUILayout.EndHorizontal();
        }

        void DrawTemplateConfig()
        {
            EditorGUILayout.LabelField("Level Template (Full Setup)", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            config.generateSampleLevel = EditorGUILayout.Toggle("Generate Sample Level", config.generateSampleLevel);
            config.arenaSize = EditorGUILayout.Vector2Field("Arena Size", config.arenaSize);
            config.obstacleCount = EditorGUILayout.IntSlider("Obstacle Count", config.obstacleCount, 0, 20);

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(config);
        }

        void DrawSceneActions()
        {
            EditorGUILayout.LabelField("Scene Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Setup Full Scene  (builds everything at once)", GUILayout.Height(36)))
                SceneBootstrapBuilder.BuildCurrentScene(config);

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Auto-Fix", GUILayout.Height(26)))
                ToolkitValidator.AutoFixBasics();
            if (GUILayout.Button("Open Setup Wizard", GUILayout.Height(26)))
                ToolkitSetupWizard.Open();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Preset (.json)", GUILayout.Height(24)))
                ExportPreset();
            if (GUILayout.Button("Import Preset (.json)", GUILayout.Height(24)))
                ImportPreset();
            EditorGUILayout.EndHorizontal();
        }

        void DrawValidation()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            var items = ToolkitValidator.Validate();
            foreach (var item in items)
            {
                var type = item.ok ? MessageType.None : MessageType.Warning;
                EditorGUILayout.HelpBox($"{item.name}: {(item.ok ? "OK" : "Needs attention")} — {item.details}", type);
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        static void DrawAddButton(string label, string displayText, System.Action onClick, Color tint)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = tint;
            if (GUILayout.Button(displayText, GUILayout.Height(52), GUILayout.ExpandWidth(true)))
                onClick?.Invoke();
            GUI.backgroundColor = prev;
        }

        static ToolkitConfig LoadOrCreateConfig()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<ToolkitConfig>(ConfigAssetPath);
            if (cfg != null)
                return cfg;

            string folder = "Assets/SHDH2367Toolkit";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "SHDH2367Toolkit");

            cfg = ScriptableObject.CreateInstance<ToolkitConfig>();
            AssetDatabase.CreateAsset(cfg, ConfigAssetPath);
            AssetDatabase.SaveAssets();
            return cfg;
        }

        void ExportPreset()
        {
            string path = EditorUtility.SaveFilePanel("Export Toolkit Preset", Application.dataPath, "ToolkitPreset", "json");
            if (string.IsNullOrEmpty(path))
                return;

            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();
        }

        void ImportPreset()
        {
            string path = EditorUtility.OpenFilePanel("Import Toolkit Preset", Application.dataPath, "json");
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, config);
            EditorUtility.SetDirty(config);
            Repaint();
        }
    }
}
