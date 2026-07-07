using UnityEngine;

namespace SHDH2367.Toolkit.Runtime
{
    public class RuntimeInstaller : MonoBehaviour
    {
        public ToolkitConfig config;
        public GameplayModeType installedMode = GameplayModeType.FirstPerson;
        public ModuleFlags installedModules = new ModuleFlags();

        [Header("Installed References")]
        public Behaviour gameManager;
        public Behaviour hudManager;
        public Camera playerCamera;

        public void ApplyFromConfig(ToolkitConfig source)
        {
            if (source == null)
                return;

            config = source;
            installedMode = source.gameplayMode;
            installedModules = source.modules ?? new ModuleFlags();

            if (gameManager != null)
                gameManager.enabled = installedModules.gameLoop;

            if (hudManager != null)
                hudManager.enabled = installedModules.hud;
        }
    }
}
