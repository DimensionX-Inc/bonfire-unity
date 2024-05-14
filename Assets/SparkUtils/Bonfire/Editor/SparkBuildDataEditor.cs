using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DimX.SparkUtils.SO;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

namespace DimX.SparkUtils
{
    [CustomEditor(typeof(SparkBuildData))]
    public class SparkBuildDataEditor : Editor
    {
        private Texture2D _previewTex;
        private Vector2 _scrollPosition;
        private int _previewIndex = 0;
        private Editor _gameObjectEditor = null;

        public override async void OnInspectorGUI()
        {
            var buildData = (SparkBuildData)target;

            base.OnInspectorGUI();
            EditorGUILayout.Space(10.0f);
            
            if (GUILayout.Button("Generate All Configs (WARNING: Destructive)"))
            {
                EditorCoroutineUtility.StartCoroutine(GenerateConfigs(buildData), buildData);
            }
            
            if (GUILayout.Button("Generate Missing Configs"))
            {
                EditorCoroutineUtility.StartCoroutine(
                    GenerateConfigs(buildData, false), buildData);
            }
            if (GUILayout.Button("Fetch Existing Configs")) { FetchConfigs(buildData); }
            
            if (GUILayout.Button("Build Sparks"))
            {
                // Define the progress reporter
                var progressReporter = new Progress<float>(progress =>
                {
                    EditorUtility.DisplayProgressBar("Processing", "Please wait...", progress);
                });

                try
                {
                    // Run the task with the progress reporter
                    await BuildSparks(buildData, progressReporter);
                }
                finally
                {
                    // Ensure the progress bar is cleared after completion
                    EditorUtility.ClearProgressBar();
                }
            }
            
            EditorGUILayout.Space(10.0f);
            if (GUILayout.Button("Assign Textures"))
            {
                AssignPreviewImages();
            }

            if (GUILayout.Button("Generate Textures"))
            {
                EditorCoroutineUtility.StartCoroutine(GenerateTextures(buildData), buildData);
            }

            if (buildData.configs.Count <= 0) return;
            EditorGUILayout.Space(10.0f);
            var config = buildData.configs[_previewIndex];
            
            GUILayout.Label($"Preview Index: {_previewIndex + 1} of {buildData.configs.Count}");
            
            DisplayConfigDetails(config);
            
            DisplayPreviewImage(config);
            
            DisplayNavigationButtons(buildData.configs);
            
            if (GUILayout.Button("Save Preview", GUILayout.Height(40)))
            {
                SavePreview(config);
            }

            if (config.Preview != null)
            {
                GUILayout.Label(config.Preview, GUI.skin.box);
            }

            //EditorUtility.SetDirty(buildData); // Save changes made to the config
            AssetDatabase.SaveAssetIfDirty(buildData);
        }

        private IEnumerator GenerateTextures(SparkBuildData buildData)
        {
            if (buildData.configs.Count <= 0) yield break;
            for(var i = 0; i < buildData.configs.Count; i++)
            {
                _previewIndex = i;
                yield return new WaitForSeconds(0.25f);
                SavePreview(buildData.configs[i]);
            }
        }

        void DisplayConfigDetails(SparkConfigData config) // Replace ConfigType with your actual type
        {
            GUILayout.BeginVertical(GUI.skin.box);
            DisplayTextField("Name:", ref config.metadata.Name);
            DisplayTextField("GUID:", ref config.metadata.Guid);
            DisplayTextField("Type:", ref config.metadata.Type);
            GUILayout.EndVertical();
        }

        private static void DisplayTextField(string label, ref string value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(50));
            value = GUILayout.TextField(value);
            GUILayout.EndHorizontal();
        }

        private static void DisplayTextField(string label, ref Guid value)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(50));
            var cVal = value.ToString();
            var nVal = GUILayout.TextField(cVal);
            if (!string.Equals(cVal, nVal))
            {
                value = Guid.Parse(nVal);
            }
            GUILayout.EndHorizontal();
        }

        private void DisplayPreviewImage(SparkConfigData config)
        {
            if (config.prefab == null) return;
            if (_gameObjectEditor == null || _gameObjectEditor.target != config.prefab)
            {
                if(_gameObjectEditor != null) DestroyImmediate(_gameObjectEditor);
                _gameObjectEditor = CreateEditor(config.prefab);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            _gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), GUIStyle.none);
            EditorGUILayout.EndVertical();
        }
        
        void DisplayNavigationButtons(IReadOnlyCollection<SparkConfigData> configs)
        {
            if (configs.Count <= 0) return;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Previous")) { if(_previewIndex > 0) _previewIndex--; }
            if (GUILayout.Button("Next")) { if(_previewIndex < configs.Count - 1) _previewIndex++; }
            GUILayout.EndHorizontal();
        }

        private void SavePreview(SparkConfigData config)
        {
            var go = _gameObjectEditor.target;
            var rgb = _gameObjectEditor.RenderStaticPreview(AssetDatabase.GetAssetPath(go), null, 512, 512);

            var rgba = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            if (rgb == null) return;
            rgba.SetPixels(rgb.GetPixels());
            rgba.Apply();
            var pxIn = rgba.GetPixel(0, 0);
            var pxOut = rgba.GetPixel(0, 0);
            pxOut.a = 0;
            for (var x = 0; x < 512; ++x)
            {
                for (var y = 0; y < 512; ++y)
                {
                    if (rgba.GetPixel(x, y) == pxIn)
                    {
                        rgba.SetPixel(x, y, pxOut);
                    }
                }
            }

            rgba.Apply();

            // Save the texture as PNG
            var bytes = ImageConversion.EncodeToPNG(rgba);
            var assetPath = AssetDatabase.GetAssetPath(go);
            var directory = System.IO.Path.GetDirectoryName(assetPath);
            var filename = System.IO.Path.GetFileNameWithoutExtension(assetPath) + "_Preview.png";
            var fullPath = System.IO.Path.Combine(directory, filename);
            System.IO.File.WriteAllBytes(fullPath, bytes);

            // Refresh the AssetDatabase
            AssetDatabase.Refresh();
                
            // Set the texture as readable
            var textureImporter = AssetImporter.GetAtPath(fullPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.isReadable = true;
                textureImporter.maxTextureSize = 512;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
            }

            // Load the texture as an asset
            var loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            config.Preview = loadedTexture;
            EditorUtility.SetDirty(config); // Save changes made to the config
            AssetDatabase.SaveAssetIfDirty(config);
        }

        private static Task BuildSparks(SparkBuildData buildData, IProgress<float> progress = null)
        {
            for (var index = 0; index < buildData.configs.Count; index++)
            {
                progress?.Report((float)index/(float)buildData.configs.Count);
                var config = buildData.configs[index];
                if (config.prefab == null)
                {
                    Debug.LogError($"Missing valid prefab reference for: {config.name}");
                    continue;
                }

                if (config.Preview == null)
                {
                    Debug.LogError($"Missing valid preview reference for: {config.name}");
                    continue;
                }

                // todo The following code duplicates BuildUtilities.BuildSpark

                // Generate Output Path
                var path = Path.Combine(buildData.outputPath, config.metadata.Guid.ToString());

                // Delete Directory with supplemental assets
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                Directory.CreateDirectory(path);

                // Generate Asset(s)
                string assetsPath = BuildUtilities.GetOrCreateSparkAssetsDirectory(path);
                BuildUtilities.CopyFiles(assetsPath, config.supplementalFiles);

                // Generate DLL(s)
                BuildUtilities.BuildDLL(path, config.metadata.Type, buildData.doDebug);

                // Generate Metadata
                BuildUtilities.BuildMetadata(config.metadata, path);

                // Generate Preview
                if (config.Preview == null) throw new Exception("Need a valid preview");
                File.WriteAllBytes(Path.Combine(path, "Preview.png"), config.Preview.EncodeToPNG());

                // Generate Kindling
                if (config.prefab.GetComponentInChildren<RectTransform>() ||
                    config.prefab.GetComponentInChildren<ParticleSystem>())
                {
                    BuildUtilities.BuildKindling(config.prefab, path);
                }
                // Generate Geometry
                else if (config.prefab.GetComponentInChildren<Renderer>())
                {
                    BuildUtilities.BuildGeometry(config.prefab, path);
                }

                // Generate Spark File
                var file = $"{path}.dimxs";
                BuildUtilities.Compress(path, file);
            }

            // Exit
            if (buildData.autoDeploy)
            {
                Deploy.DeployAllSparks();
            }

            return Task.CompletedTask;
        }
        
        public static void AssignPreviewImages()
        {
            var configs = Resources.FindObjectsOfTypeAll<SparkConfigData>(); // Replace ConfigType with your actual config class

            foreach (var config in configs)
            {
                var configPath = AssetDatabase.GetAssetPath(config);
                var configDirectory = Path.GetDirectoryName(configPath);
                var previewImagePath = FindPreviewImagePath(config.prefab.name, configDirectory);

                if (!string.IsNullOrEmpty(previewImagePath))
                {
                    var previewImage = AssetDatabase.LoadAssetAtPath<Texture2D>(previewImagePath);
                    config.Preview = previewImage; // Assuming 'preview' is the field to hold the Texture2D
                    EditorUtility.SetDirty(config); // Save changes made to the config
                }
                else
                {
                    Debug.LogWarning($"Preview image not found for prefab {config.prefab.name}");
                }
            }
        }

        private static string FindPreviewImagePath(string prefabName, string directoryPath)
        {
            var searchPattern = $"{prefabName}_preview.png";
            var files = Directory.GetFiles(directoryPath, searchPattern);

            return files.Length > 0 ? files[0] : // Returns the path of the first found image
                null;
        }

        private IEnumerator GenerateConfigs(SparkBuildData buildData, bool overrideExisting = true)
        {
            buildData.configs.Clear();
            var path = AssetDatabase.GetAssetPath(buildData);
            var directoryPath = Path.GetDirectoryName(path);
            var prefabPaths = Directory.GetFiles(directoryPath, 
                "*.*", SearchOption.AllDirectories)
                .Where(file => 
                    file.ToLower().EndsWith("prefab") || 
                    file.ToLower().EndsWith("fbx"));
            var generatedConfigs = new List<SparkConfigData>();
            AssetDatabase.DisallowAutoRefresh();
            foreach (var prefabPath in prefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                {
                    Debug.LogError("Failed to load prefab at path: " + prefabPath);
                    yield break;
                }

                var directory = Path.GetDirectoryName(prefabPath);
                var configPath = Path.Combine(directory, prefab.name + "_Config.asset");

                var existingConfig = AssetDatabase.LoadAssetAtPath<SparkConfigData>(configPath);
                if (existingConfig != null && overrideExisting)
                    AssetDatabase.DeleteAsset(configPath);
                else { if (existingConfig) continue; }
                
                var config = ScriptableObject.CreateInstance<SparkConfigData>();
                config.metadata.Guid = Guid.Parse(
                    AssetDatabase.AssetPathToGUID(
                        AssetDatabase.GetAssetPath(prefab))); 
                config.metadata.Name = prefab.name;
                config.prefab = prefab;

                generatedConfigs.Add(config);

                configPath = AssetDatabase.GenerateUniqueAssetPath(configPath);

                AssetDatabase.CreateAsset(config, configPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.AllowAutoRefresh();
            AssetDatabase.Refresh();

            buildData.configs = generatedConfigs;
            EditorUtility.SetDirty(buildData); // Save changes made to the config
            AssetDatabase.SaveAssetIfDirty(buildData);

            EditorCoroutineUtility.StartCoroutine(GeneratePreviews(buildData.configs), buildData);
        }
        
        private IEnumerator GeneratePreviews(List<SparkConfigData> configs)
        {
            foreach(var config in configs)
            {
                var prefab = config.prefab;
                _previewTex = AssetPreview.GetAssetPreview(prefab);
                
                yield return new WaitUntil(() =>
                    AssetPreview.IsLoadingAssetPreview(prefab.GetInstanceID()) == false);

                if (_previewTex == null)
                {
                    Debug.LogWarning($"could not find preview texture for: {prefab.name}");
                    continue;
                }

                var bytes = _previewTex.EncodeToPNG();
                var prefabPath = AssetDatabase.GetAssetPath(prefab);
                prefabPath = Path.ChangeExtension(prefabPath, null);
                var previewPath = $"{prefabPath}_Preview.png";
                if (File.Exists(previewPath))
                    AssetDatabase.DeleteAsset(previewPath);
                File.WriteAllBytes(previewPath, bytes);
                AssetDatabase.Refresh();

                var textureImporter = AssetImporter.GetAtPath(previewPath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.isReadable = true;
                    textureImporter.maxTextureSize = 512;
                    textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                    textureImporter.SaveAndReimport();
                }

                // Load the texture as an asset
                var loadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);
                config.Preview = loadedTexture;
            }
        }
        
        private void FetchConfigs(SparkBuildData buildData)
        {
            buildData.configs.Clear();
            var path = AssetDatabase.GetAssetPath(buildData);
            var directoryPath = Path.GetDirectoryName(path);
            var prefabPaths = Directory.GetFiles(directoryPath, "*.prefab", SearchOption.AllDirectories);
            var sb = new StringBuilder("No Config Files found for:\n");
            foreach (var prefabPath in prefabPaths)
            {
                var directory = Path.GetDirectoryName(prefabPath);
                var fileName = Path.GetFileNameWithoutExtension(prefabPath);
                var configPath = Path.Combine(directory, fileName + "_Config.asset");
                
                var existingConfig = AssetDatabase.LoadAssetAtPath<SparkConfigData>(configPath);

                if(existingConfig)
                    buildData.configs.Add(existingConfig);
                else
                    sb.Append($"{configPath}\n");
            }
            Debug.LogWarning(sb.ToString());
            EditorUtility.SetDirty(buildData); // Save changes made to the config
            AssetDatabase.SaveAssetIfDirty(buildData);
        }
    }
}