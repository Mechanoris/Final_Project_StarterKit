using UnityEngine;

namespace SHDH2367.Toolkit.Runtime
{
    [CreateAssetMenu(
        fileName = "ToolkitConfig",
        menuName = "SHDH2367 Toolkit/Toolkit Config",
        order = 10)]
    public class ToolkitConfig : ScriptableObject
    {
        [Header("Core")]
        public GameplayModeType gameplayMode = GameplayModeType.FirstPerson;
        public ModuleFlags modules = new ModuleFlags();

        [Header("Game Loop")]
        public float gameDuration = 60f;
        public int collectibleCount = 8;

        [Header("Environment")]
        public bool generateSampleLevel = true;
        public Vector2 arenaSize = new Vector2(20f, 20f);
        public int obstacleCount = 6;
    }
}
