using System.IO;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkSDK
{
    /// <summary>
    /// 
    /// </summary>
    public static class Generate
    {
        [MenuItem("Assets/Dimension X/Generate Template/Assembly")]
        [MenuItem("DimX/Generate Template/Assembly")]
        public static void CreateAsmDefFunc()
        {
            Write("Sparks.TODO.asmdef", GenerateAsmDef.Content);
        }
        
        [MenuItem("Assets/Dimension X/Generate Template/Class: Cause")]
        [MenuItem("DimX/Generate Template/Class: Cause")]
        public static void CreateCauseFunc()
        {
            Write("CauseTODO.cs", GenerateCause.Content);
        }
        
        [MenuItem("Assets/Dimension X/Generate Template/Class: Effect")]
        [MenuItem("DimX/Generate Template/Class: Effect")]
        public static void CreateEffectFunc()
        {
            Write("EffectTODO.cs", GenerateEffect.Content);
        }
        
        [MenuItem("Assets/Dimension X/Generate Template/Class: Trait")]
        [MenuItem("DimX/Generate Template/Class: Trait")]
        public static void CreateTraitFunc()
        {
            Write("TraitTODO.cs", GenerateTrait.Content);
        }
        
        [MenuItem("Assets/Dimension X/Generate Template/Class: Spark")]
        [MenuItem("DimX/Generate Template/Class: Spark")]
        public static void CreateSparkFunc()
        {
            Write("SparkTODO.cs", GenerateSpark.Content);
        }

        #region Private
        
        private static string Root
        {
            get
            {
                var obj = Selection.activeObject;
                var file = (obj == null) ? "Assets" : AssetDatabase.GetAssetPath(obj.GetInstanceID());
                var path = (File.Exists(file)) ? Path.GetDirectoryName(file) : file;
                return path;
            }
        }

        private static void Write(string name, string content)
        {
            var file = Path.Combine(Root, name);
            if (File.Exists(file))
            {
                Debug.LogError($"Template Already Exists: {file}");
                return;
            }
            File.WriteAllText(file, content);
            AssetDatabase.Refresh();
            Debug.LogError($"Wrote Template to: {file}");
        }
        
        #endregion
    }
}