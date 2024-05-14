using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    /// <summary>
    /// 
    /// </summary>
    public static class Build
    {
        [MenuItem("Assets/Dimension X/Build Map(s)")]
        [MenuItem("DimX/Build Asset(s)/Build Map(s)")]
        public static void BuildMapsMenu()
        {
            void Process(int id)
            {
                if (id < Selection.gameObjects.Length)
                {
                    GameObject go = Selection.gameObjects[id];
                    BuildWindowMap.Show(go, () => Process(++id));
                }
            }
            Process(0);
        }

        [MenuItem("Assets/Dimension X/Build Map(s)", true)]
        [MenuItem("DimX/Build Asset(s)/Build Map(s)", true)]
        private static bool BuildMapsMenuValidation()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem("Assets/Dimension X/Build Spark(s)")]
        [MenuItem("DimX/Build Asset(s)/Build Spark(s)")]
        public static void BuildSparksMenu()
        {
            void Process(int id)
            {
                if (id < Selection.gameObjects.Length)
                {
                    GameObject go = Selection.gameObjects[id];
                    BuildWindowSpark.Show(go, () => Process(++id));
                }
                else if(Selection.activeObject is Texture2D)
                {
                    BuildWindowSpark.Show(Selection.activeObject as Texture2D);
                }
            }
            Process(0);
        }
        
        [MenuItem("Assets/Dimension X/Build Spark(s)", true)]
        [MenuItem("DimX/Build Asset(s)/Build Spark(s)", true)]
        private static bool BuildSparksMenuValidation()
        {
            return Selection.activeObject is GameObject || Selection.activeObject is Texture2D;
        }

        [MenuItem("DimX/Utilities/Spark Browser")]
        public static void BrowseSparks()
        {
            SparkBrowserWindow.ShowWindow();
        }
        
        [MenuItem("DimX/Utilities/Generate Guid")]
        public static void GenerateGuid()
        {
            GenerateGuidWindow.ShowWindow();
        }
    }
}