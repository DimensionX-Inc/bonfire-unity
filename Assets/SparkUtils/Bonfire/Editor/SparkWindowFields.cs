using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DimX.Common.Assets.Types.Sparks;
using DimX.Common.Assets.Types.Sparks.Configs;
using DimX.Common.Utilities;
using DimX.Common.Utilities.Files;
using DimX.SparkUtils.SO;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    public static class SparkWindowFields
    {
        public static void CreateSource(SparkConfigData configData)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Source");
            EditorGUI.indentLevel++;
            {
                if(configData.prefab == null)
                {
                    // Config Sparks
                    EditorGUILayout.LabelField("Filename", Path.GetFileName(configData.previewPath));
                }
                else
                {
                    // All other Sparks
                    EditorGUILayout.LabelField("Prefab", configData.prefab.name);
                }
            }
            EditorGUI.indentLevel--;
        }

        private static string[] typesFetched;
        private static string _lastPath;

        public static void CreateMetadata(SparkConfigData configData)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Metadata");
            EditorGUI.indentLevel++;
            {
                configData.metadata.Guid = new Guid(EditorGUILayout.TextField("Guid", configData.metadata.Guid.ToString())?.ToLower());
                configData.metadata.Name = EditorGUILayout.TextField("Name", configData.metadata.Name);
                configData.author = EditorGUILayout.TextField("Author", configData.author);
                
                if (typesFetched == null)
                {
                    typesFetched = BuildUtilities.GetTypes(configData.prefab != null ? typeof(Spark) : typeof(Config));
                }

                var id = Math.Max(0, Array.IndexOf(typesFetched, configData.metadata.Type));
                EditorGUILayout.BeginHorizontal();
                configData.metadata.Type = typesFetched[EditorGUILayout.Popup("Type", id, typesFetched)];
                if (GUILayout.Button("Refresh", GUILayout.Width(60)))
                {
                    typesFetched = BuildUtilities.GetTypes(configData.prefab != null ? typeof(Spark) : typeof(Config));
                }
                EditorGUILayout.EndHorizontal();

                CreateMetadataKeyValues(configData);
            }
            EditorGUI.indentLevel--;
        }

        private static void CreateMetadataKeyValues(SparkConfigData data)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Key Values");
            EditorGUI.indentLevel++;
            {
                for (var index = 0; index < data.keyValuePairs.Count; index++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        var tuple = new StringTuple(
                            EditorGUILayout.TextField(data.keyValuePairs[index].Key),
                            EditorGUILayout.TextField(data.keyValuePairs[index].Value)
                        );
                        data.keyValuePairs[index] = tuple;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        data.keyValuePairs.Add(new StringTuple());
                    }
                    
                    GUI.enabled = data.keyValuePairs.Count > 0;
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        data.keyValuePairs.RemoveAt(data.keyValuePairs.Count - 1);
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        public static void CreateSupplemental(SparkConfigData configData)
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Supplemental");
            EditorGUI.indentLevel++;
            {
                for (var i = 0; i < configData.supplementalFiles.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.enabled = false;
                        configData.supplementalFiles[i] = EditorGUILayout.TextField(configData.supplementalFiles[i]);
                        GUI.enabled = true;
                        
                        if (GUILayout.Button("Browse", GUILayout.Width(75)))
                        {
                            string[] paths = FileBrowser.OpenFiles(title: "Select Supplemental Files",
                                directory: string.IsNullOrEmpty(_lastPath)
                                    ? Path.Combine(Constants.AssetRoot, "Sparks")
                                    : _lastPath,
                                multiSelect: true);
                            if (paths.Length > 0)
                            {
                                configData.supplementalFiles[i] = paths[0];
                                _lastPath = paths[0];
                                if (paths.Length > 1)
                                {
                                    configData.supplementalFiles.AddRange(paths.Skip(1));
                                }
                                configData.supplementalFiles = configData.supplementalFiles.Distinct().ToList();
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }   
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        configData.supplementalFiles.Add(string.Empty);
                    }
                    
                    GUI.enabled = configData.supplementalFiles.Count > 0;
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        configData.supplementalFiles.RemoveAt(configData.supplementalFiles.Count - 1);
                    }
                    GUI.enabled = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }

        public static void CreateAssetBundle(SerializedObject sparkConfigSerializedObject)
        {
            var buildAssetBundlesProp = sparkConfigSerializedObject.FindProperty(nameof(SparkConfigData.buildAssetBundles));
            EditorGUILayout.PropertyField(buildAssetBundlesProp);
            if (buildAssetBundlesProp.boolValue)
            {
                var buildTargetsProp = sparkConfigSerializedObject.FindProperty(nameof(SparkConfigData.buildTargets)); 
                EditorGUILayout.PropertyField(buildTargetsProp, includeChildren: true);
            }
        }

        public static void CreateGrabPoints(SparkConfigData config)
        {
            if (config.prefab == null)
            {
                return;
            }
            
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Miscellaneous");
            EditorGUI.indentLevel++;
            {
                BuildWindowSparkMisc.AddMiscGrabPoints(config.prefab, config);
            }
            EditorGUI.indentLevel--;
        }

        public static void CreatePreview(SparkConfigData configData, Editor editor)
        {
            try
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("Preview");

                if (!string.IsNullOrEmpty(configData.previewPath))
                {
                    var rect = EditorGUILayout.GetControlRect();
                    GUI.DrawTexture(new Rect(rect.x, rect.y, 256, 256), configData.Preview);
                    GUILayout.Space(256);
                }
                else
                {
                    if (configData.prefab == null)
                    {
                        if (configData.Preview == null)
                        {
                            return;
                        }

                        // Config Sparks - Show Texture2D
                        var rect = EditorGUILayout.GetControlRect();
                        GUI.DrawTexture(new Rect(rect.x, rect.y, 256, 256), configData.Preview);
                        GUILayout.Space(256);
                    }
                    else
                    {
                        // All other Sparks - Show 3D object preview
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        editor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), GUIStyle.none);
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10.0f);
                    }
                }

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Choose Preview", GUILayout.Width(125)))
                {
                    string[] paths = FileBrowser.OpenFiles(title: "Select Preview",
                        directory: string.IsNullOrEmpty(_lastPath)
                            ? Path.Combine(Constants.AssetRoot, "Sparks")
                            : _lastPath,
                        multiSelect: false,
                        filters: new string[] { "png", "jpg", "jpeg"});
                    if (paths.Length > 0 && paths[0] != string.Empty)
                    {
                        _lastPath = paths[0];

                        if (cancellationTokenSource == null)
                        {
                            cancellationTokenSource = new CancellationTokenSource();
                        }
                        else
                        {
                            cancellationTokenSource.Cancel();
                            cancellationTokenSource = new CancellationTokenSource();
                        }
                        LoadImageFromFile(configData, paths[0], cancellationTokenSource.Token)
                            .ContinueWith(x =>
                            {
                                if (x.IsFaulted)
                                {
                                    Debug.LogException(x.Exception);
                                }
                            });
                    }
                }

                if (!string.IsNullOrEmpty(configData.previewPath) && GUILayout.Button("Clear Preview", GUILayout.Width(125)))
                {
                    configData.previewPath = string.Empty;
                    configData.Preview = null;
                }
                EditorGUILayout.EndHorizontal();
            }
            catch
            {
                // NO-OP
            }
        }

        private static CancellationTokenSource cancellationTokenSource;

        private static async Task<Texture2D> LoadImageFromFile(SparkConfigData configData, string assetPath, CancellationToken token)
        {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBA32, true);
            texture2D.wrapMode = TextureWrapMode.Clamp;
            Texture2D image = texture2D;
            byte[] bytes = await File.ReadAllBytesAsync(assetPath);
            if (image.LoadImage(bytes))
            {
                if (token.IsCancellationRequested)
                {
                    return null;
                }
                image.name = Path.GetFileName(assetPath);
                configData.previewPath = assetPath;
                configData.Preview = image;
                return image;
            }
            UnityEngine.Object.Destroy((UnityEngine.Object) image);
            return null;
        }
    }
}