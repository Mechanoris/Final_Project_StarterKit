using SHDH2367.Toolkit.Runtime;
using UnityEngine;

namespace SHDH2367.Toolkit.Editor
{
    public static class LevelTemplateGenerator
    {
        public static void Generate(GameplayModeType mode, ToolkitConfig config)
        {
            if (config == null)
                return;

            switch (mode)
            {
                case GameplayModeType.SideView:
                    GenerateSideView(config);
                    break;
                default:
                    GenerateArena(config);
                    break;
            }
        }

        /// <summary>3D arena floor, walls, and obstacles (uses arena size & obstacle count from config).</summary>
        public static void GenerateArena(ToolkitConfig config)
        {
            float width = Mathf.Max(8f, config.arenaSize.x);
            float depth = Mathf.Max(8f, config.arenaSize.y);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Toolkit_Floor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);

            CreateWall("Toolkit_Wall_North", new Vector3(0f, 1.5f, depth * 0.5f), new Vector3(width, 3f, 1f));
            CreateWall("Toolkit_Wall_South", new Vector3(0f, 1.5f, -depth * 0.5f), new Vector3(width, 3f, 1f));
            CreateWall("Toolkit_Wall_East", new Vector3(width * 0.5f, 1.5f, 0f), new Vector3(1f, 3f, depth));
            CreateWall("Toolkit_Wall_West", new Vector3(-width * 0.5f, 1.5f, 0f), new Vector3(1f, 3f, depth));

            int obstacleCount = Mathf.Max(0, config.obstacleCount);
            for (int i = 0; i < obstacleCount; i++)
            {
                GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obstacle.name = $"Toolkit_Obstacle_{i + 1}";
                obstacle.transform.position = new Vector3(
                    Random.Range(-width * 0.35f, width * 0.35f),
                    0.6f,
                    Random.Range(-depth * 0.35f, depth * 0.35f));
                obstacle.transform.localScale = new Vector3(Random.Range(1f, 2.4f), 1.2f, Random.Range(1f, 2.4f));
            }
        }

        /// <summary>Side-scroller strip: long floor, platforms, end marker.</summary>
        public static void GenerateSideView(ToolkitConfig config)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Toolkit_Side_Floor";
            floor.transform.position = new Vector3(10f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(24f, 1f, 6f);

            for (int i = 0; i < 6; i++)
            {
                GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
                platform.name = $"Toolkit_Platform_{i + 1}";
                platform.transform.position = new Vector3(3f + i * 3f, 0.8f + (i % 2) * 1.2f, 0f);
                platform.transform.localScale = new Vector3(2.2f, 0.4f, 4f);
            }

            GameObject endMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            endMarker.name = "Toolkit_Level_EndMarker";
            endMarker.transform.position = new Vector3(22f, 0.75f, 0f);
            endMarker.transform.localScale = new Vector3(0.6f, 1.5f, 0.6f);
        }

        static void CreateWall(string name, Vector3 pos, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
        }
    }
}
