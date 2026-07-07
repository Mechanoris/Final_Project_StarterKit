using SHDH2367.Toolkit.Runtime;
using UnityEditor;
using UnityEngine;

namespace SHDH2367.Toolkit.Editor
{
    public class ToolkitSetupWizard : EditorWindow
    {
        const string ConfigAssetPath = "Assets/SHDH2367Toolkit/ToolkitConfig.asset";

        ToolkitConfig config;
        int step;

        public static void Open()
        {
            var window = GetWindow<ToolkitSetupWizard>("Toolkit Setup Wizard");
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }

        void OnEnable()
        {
            config = AssetDatabase.LoadAssetAtPath<ToolkitConfig>(ConfigAssetPath);
            if (config == null)
                config = ScriptableObject.CreateInstance<ToolkitConfig>();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("SHDH2367 Toolkit Wizard", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            switch (step)
            {
                case 0:
                    DrawModeStep();
                    break;
                case 1:
                    DrawModuleStep();
                    break;
                case 2:
                    DrawTemplateStep();
                    break;
                default:
                    DrawConfirmStep();
                    break;
            }

            DrawNavigation();
        }

        void DrawModeStep()
        {
            EditorGUILayout.HelpBox("Step 1: Choose gameplay mode.", MessageType.Info);
            config.gameplayMode = (GameplayModeType)EditorGUILayout.EnumPopup("Gameplay Mode", config.gameplayMode);
            config.gameDuration = EditorGUILayout.FloatField("Game Duration (seconds)", config.gameDuration);
            config.collectibleCount = EditorGUILayout.IntSlider("Collectibles", config.collectibleCount, 1, 50);
        }

        void DrawModuleStep()
        {
            EditorGUILayout.HelpBox("Step 2: Enable modules for this scene.", MessageType.Info);
            config.modules.player = EditorGUILayout.Toggle("Player", config.modules.player);
            config.modules.gameLoop = EditorGUILayout.Toggle("Game Loop", config.modules.gameLoop);
            config.modules.hud = EditorGUILayout.Toggle("HUD", config.modules.hud);
            config.modules.collectibles = EditorGUILayout.Toggle("Collectibles", config.modules.collectibles);
            config.modules.feedback = EditorGUILayout.Toggle("Feedback", config.modules.feedback);
        }

        void DrawTemplateStep()
        {
            EditorGUILayout.HelpBox("Step 3: Optional sample level generation.", MessageType.Info);
            config.generateSampleLevel = EditorGUILayout.Toggle("Generate Sample Level", config.generateSampleLevel);
            config.arenaSize = EditorGUILayout.Vector2Field("Arena Size", config.arenaSize);
            config.obstacleCount = EditorGUILayout.IntSlider("Obstacle Count", config.obstacleCount, 0, 20);
        }

        void DrawConfirmStep()
        {
            EditorGUILayout.HelpBox("Step 4: Apply setup into current scene.", MessageType.Warning);
            EditorGUILayout.LabelField("Mode", config.gameplayMode.ToString());
            EditorGUILayout.LabelField("Modules",
                $"Player:{config.modules.player}, Loop:{config.modules.gameLoop}, HUD:{config.modules.hud}, Collectibles:{config.modules.collectibles}, Feedback:{config.modules.feedback}");

            if (GUILayout.Button("Apply Setup Now", GUILayout.Height(34)))
            {
                EnsureConfigAsset();
                SceneBootstrapBuilder.BuildCurrentScene(config);
                Close();
            }
        }

        void DrawNavigation()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = step > 0;
            if (GUILayout.Button("Back", GUILayout.Height(26)))
                step--;
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            if (step < 3)
            {
                if (GUILayout.Button("Next", GUILayout.Height(26), GUILayout.Width(100)))
                    step++;
            }
            else
            {
                if (GUILayout.Button("Finish", GUILayout.Height(26), GUILayout.Width(100)))
                {
                    EnsureConfigAsset();
                    SceneBootstrapBuilder.BuildCurrentScene(config);
                    Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        void EnsureConfigAsset()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ToolkitConfig>(ConfigAssetPath);
            if (existing == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/SHDH2367Toolkit"))
                    AssetDatabase.CreateFolder("Assets", "SHDH2367Toolkit");
                AssetDatabase.CreateAsset(config, ConfigAssetPath);
            }
            else
            {
                EditorUtility.CopySerialized(config, existing);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
