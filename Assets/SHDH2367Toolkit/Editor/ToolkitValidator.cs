using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace SHDH2367.Toolkit.Editor
{
    public static class ToolkitValidator
    {
        public struct ValidationItem
        {
            public string name;
            public bool ok;
            public string details;
        }

        const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";

        public static List<ValidationItem> Validate()
        {
            var result = new List<ValidationItem>();

            bool hasUrp = GraphicsSettings.defaultRenderPipeline != null;
            result.Add(new ValidationItem
            {
                name = "URP Render Pipeline",
                ok = hasUrp,
                details = hasUrp ? "Configured" : "No Scriptable Render Pipeline asset assigned"
            });

            bool hasInputActions = AssetDatabase.LoadMainAssetAtPath(InputActionsPath) != null;
            result.Add(new ValidationItem
            {
                name = "Input Action Asset",
                ok = hasInputActions,
                details = hasInputActions ? InputActionsPath : "Missing InputSystem_Actions.inputactions"
            });

            bool hasPlayerTag = InternalEditorUtility.tags.Contains("Player");
            result.Add(new ValidationItem
            {
                name = "Player Tag",
                ok = hasPlayerTag,
                details = hasPlayerTag ? "Exists" : "Tag 'Player' is missing"
            });

            var activeScene = SceneManager.GetActiveScene().path;
            bool inBuild = EditorBuildSettings.scenes.Any(s => s.path == activeScene);
            result.Add(new ValidationItem
            {
                name = "Active Scene In Build Settings",
                ok = inBuild,
                details = inBuild ? "Included" : "Not included"
            });

            return result;
        }

        public static void AutoFixBasics()
        {
            EnsurePlayerTag();
            EnsureActiveSceneInBuildSettings();
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        static void EnsurePlayerTag()
        {
            if (InternalEditorUtility.tags.Contains("Player"))
                return;

            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tagsProp = tagManager.FindProperty("tags");
            int index = tagsProp.arraySize;
            tagsProp.InsertArrayElementAtIndex(index);
            tagsProp.GetArrayElementAtIndex(index).stringValue = "Player";
            tagManager.ApplyModifiedProperties();
        }

        static void EnsureActiveSceneInBuildSettings()
        {
            var scenePath = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(scenePath))
                return;

            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == scenePath))
                return;

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
