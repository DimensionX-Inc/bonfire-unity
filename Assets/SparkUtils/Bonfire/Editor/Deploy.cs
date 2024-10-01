using System.IO;
using DimX.Common.Assets.Loaders;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    /// <summary>
    /// 
    /// </summary>
    public class Deploy
    {
        [MenuItem("DimX/Deploy Asset(s)/Deploy All Maps")]
        public static void DeployAllMaps()
        {
            DeployAssets("Maps");
            
        }
        
        [MenuItem("DimX/Deploy Asset(s)/Deploy All Sparks")]
        public static void DeployAllSparks()
        {
            DeployAssets("Sparks");
        }
        
        public static void DeploySingleMap(string sourceFilePath)
        {
            DeployAsset(sourceFilePath, "Maps");
        }

        public static void DeploySingleSpark(string sourceFilePath)
        {
            DeployAsset(sourceFilePath, "Sparks");
        }
        
        private static void DeployAsset(string sourceFilePath, string type)
        {
            AssetLoader.LoadPreview(sourceFilePath, preview =>
            {
                GetMatchingDirectories(type, out _, out string destinationDirectory);

                var sourceFile = new FileInfo(sourceFilePath);
                string destinationFilePath =
                    Path.Combine(destinationDirectory, $"{preview.Metadata.Guid}.{sourceFile.Extension}");
                
                Directory.CreateDirectory(destinationDirectory);
                File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
            
                DisplaySuccessMessage(type);
            });
        }
        
        private static void DeployAssets(string type)
        {
            GetMatchingDirectories(type, out string sourceDirectory, out string destinationDirectory);

            Directory.CreateDirectory(destinationDirectory);
            foreach (var dirPath in Directory.GetDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceDirectory, destinationDirectory));
            }
            foreach (var newPath in Directory.GetFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourceDirectory, destinationDirectory), overwrite: true);
            }

            DisplaySuccessMessage(type);
        }

        private static void GetMatchingDirectories(string type, out string sourceDirectory, out string destinationDirectory)
        {
            sourceDirectory = Path.Combine(Constants.AssetRoot, type);
            destinationDirectory = Path.Combine(Constants.BonfireRoot, type, $"My {type}");
        }

        private static void DisplaySuccessMessage(string type)
        {
            var msg = $"Successfully Deployed Asset(s) to Bonfire Builder: 'My {type}'";
            EditorUtility.DisplayDialog("SparkSDK", msg, "Okay");
            Debug.Log(msg);
        }
    }
}