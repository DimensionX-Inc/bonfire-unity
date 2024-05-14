using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
using DimX.Common.Utilities.UI.Data;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityGLTF;
using DimX.SparkUtils.SO;
using Assembly = System.Reflection.Assembly;

namespace DimX.SparkUtils
{
    public static class BuildUtilities
    {
        public const string UnityBuiltInAssetsDirectoryPath = "Resources/unity_builtin_extra";
        public const string UnityBuiltInAssetsDirectoryPathAlt = "Library/unity default resources";
        public const string UnityBuiltInPlaceholderDirectoryName = "Unity Built In Assets";

        public static string[] IgnoredAssemblyRegex = new string[]
        {
            "netstandard",
            "DimX.Common.*",
            "UnityEngine.*",
            "UnityEditor.*",
            "Unity.TextMeshPro.*"
        };

        public enum PreviewType
        {
            Symbol,
            Config,
            Entity
        }
        
        private static readonly int h = 512;
        private static readonly int w = 512;

        const string SymbolPreviewAssetName = "Symbol";
        const string ConfigPreviewAssetName = "Config";
        const string EntityPreviewAssetName = "Entity";
        const string SparkAssetsDirectoryName = "Assets";

        public static string GetOrCreateSparkAssetsDirectory(string sparkPath)
        {
            string sparkAssetsDirectoryPath = Path.Combine(sparkPath, SparkAssetsDirectoryName);
            Directory.CreateDirectory(sparkAssetsDirectoryPath);
            return sparkAssetsDirectoryPath;
        }

        public static void CopyFiles(string destinationDirectory, List<string> sourceFiles)
        {
            foreach (var source in sourceFiles)
            {
                if (File.Exists(source))
                {
                    var name = new FileInfo(source).Name;
                    string destination = Path.Combine(destinationDirectory, name);
                    File.Copy(source, destination, overwrite: true);
                }
            }
        }

        public static void BuildDLL(string path, Metadata metadata, bool doDebug)
        {
            BuildDLL(path, metadata.Type, doDebug);
        }

        public static void BuildDLL(string path, string sparkType, bool doDebug)
        {
            // Locate assemblies that define the Spark's type
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dllLocations = GatherDLLLocations(sparkType, assemblies);

            // Copy necessary DLLs into Spark
            foreach (var dll in dllLocations.Distinct())
            {
                var name = new FileInfo(dll).Name;
                File.Copy(dll, Path.Combine(path, name), true);

                if (!doDebug) continue;
                name = name.Replace(".dll", ".pdb");
                File.Copy(dll, Path.Combine(path, name), true);
            }
        }

        private static List<string> GatherDLLLocations(string sparkType, Assembly[] assemblies)
        {
            List<string> dllLocations = new();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    // Since 'DimX.Common.Assets.Types' gets truncated when creating Metadata,
                    // this (correctly) will skip core Bonfire DLLs (e.g. DimX.Common.Assets.dll)
                    if (type.ToString() == sparkType)
                    {
                        dllLocations.Add(assembly.Location);
                        dllLocations.AddRange(GetReferencedAssemblies(assembly, assemblies).Select(x => x.Location));
                        return dllLocations;
                    }
                }
            }
            return dllLocations;
        }

        private static IEnumerable<Assembly> GetReferencedAssemblies(Assembly assembly, Assembly[] allAssemblies, List<Assembly> assembliesProcessedAlready = null)
        {
            var dlls = new List<Assembly>();
            var referenceNames = assembly.GetReferencedAssemblies();
            referenceNames = referenceNames.Where(a => 
                (assembliesProcessedAlready == null || assembliesProcessedAlready.All(x => x.FullName != a.FullName)) &&
                !IgnoredAssemblyRegex.Any(reg => Regex.IsMatch(a.FullName, reg))).ToArray();

            var references = allAssemblies.Where(x => referenceNames.Any(y => y.FullName == x.FullName)).Distinct();
            foreach (Assembly reference in references)
            {
                dlls.Add(reference);
                dlls.AddRange(GetReferencedAssemblies(reference, allAssemblies, dlls));
            }
        
            return dlls.Distinct();
        }

        /// <summary>
        /// Write metadata to file system with Key Value Pairs intact
        /// </summary>
        public static void BuildMetadata(Metadata metadata, SparkConfigData configData, string path)
        {
            metadata.KeyVals = configData.keyValuePairs.ToDictionary(x => x.Key, x => x.Value);
            if (configData._useGrabPoint)
            {
                if (configData._useDifferentHands)
                {
                    StringTuple primary = configData._grabPointPrimary;
                    metadata.KeyVals[primary.Key] = primary.Value;
                    StringTuple secondary = configData._grabPointSecondary;
                    metadata.KeyVals[secondary.Key] = secondary.Value;
                }
                else
                {
                    StringTuple primary = configData._grabPointPrimary;
                    metadata.KeyVals[primary.Key] = primary.Value;
                }
            }
            var text = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(Path.Combine(path, "Metadata.txt"), text);
        }
        
        /// <summary>
        /// Write metadata to file system.
        /// </summary>
        public static void BuildMetadata(Metadata metadata, string path)
        {
            var text = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(Path.Combine(path, "Metadata.txt"), text);
        }

        private static Texture2D BuildPreview(Editor editor)
        {
            if (editor == null)
            {
                return new Texture2D(1, 1);
            }
            
            var go = editor.target;
            var rgb = editor.RenderStaticPreview(AssetDatabase.GetAssetPath(go), null, w, h);

            var rgba = new Texture2D(w, h, TextureFormat.RGBA32, false);
            if (rgb != null)
            {
                rgba.SetPixels(rgb.GetPixels());
                rgba.Apply();
                var pxIn = rgba.GetPixel(0, 0);
                var pxOut = rgba.GetPixel(0, 0);
                pxOut.a = 0;
                for (var x = 0; x < w; ++x)
                {
                    for (var y = 0; y < h; ++y)
                    {
                        if (rgba.GetPixel(x, y) == pxIn)
                        {
                            rgba.SetPixel(x, y, pxOut);
                        }
                    }
                }
                rgba.Apply();
            }

            return rgba;
        }
        
        public static void GeneratePreview(Editor editor, SparkConfigData configData)
        {
             configData.Preview = BuildPreview(editor);
        }
        /// <summary>
        /// Write preview image to file system.
        /// </summary>
        public static void BuildPreview(Editor editor, string path)
        {
            File.WriteAllBytes(Path.Combine(path, "Preview.png"), BuildPreview(editor).EncodeToPNG());
        }
        
        public static void BuildPreview(string fileIn, string pathOut)
        {
            Texture2D source =  new Texture2D(w, h, TextureFormat.RGBA32, false);
            byte[] tmpBytes = File.ReadAllBytes(fileIn);
            source.LoadImage(tmpBytes);
            
            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] rpixels = result.GetPixels();
            float incX = (1.0f / w);
            float incY = (1.0f / h);
            
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % w), incY * Mathf.Floor(px / w));
            }

            result.SetPixels(rpixels, 0);
            result.Apply();
            
            File.WriteAllBytes(Path.Combine(pathOut, "Preview.png"), result.EncodeToPNG());
        }
        
        public static void BuildPreview(PreviewType previewType, string path)
        {
            Texture2D preview = previewType switch
            {
                PreviewType.Symbol => Resources.Load<Texture2D>(SymbolPreviewAssetName),
                PreviewType.Config => Resources.Load<Texture2D>(ConfigPreviewAssetName),
                PreviewType.Entity => Resources.Load<Texture2D>(EntityPreviewAssetName),
                _ => throw new ArgumentOutOfRangeException(nameof(previewType), previewType, null)
            };
            if (preview)
            {
                File.WriteAllBytes(Path.Combine(path, "Preview.png"), preview.EncodeToPNG());
            }
            else
            {
                LogUtility.LogError($"Could not load preview image for type {previewType}");
            }
        }

        public static Texture2D LoadPreview(SparkConfigData configData, Editor editor)
        {
            if (!string.IsNullOrWhiteSpace(configData.previewPath))
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>(configData.previewPath);
            }
            
            if (configData.prefab.GetComponentInChildren<RectTransform>())
            {
                return Resources.Load<Texture2D>(SymbolPreviewAssetName);
            }
            
            if (configData.prefab.GetComponentInChildren<Renderer>())
            {
                return BuildPreview(editor);
            }
            
            return Resources.Load<Texture2D>(ConfigPreviewAssetName);
        }
        
        /// <summary>
        /// Write geometry to file system.
        /// </summary>
        public static void BuildGeometry(GameObject go, string directory)
        {
            var settings = ScriptableObject.CreateInstance<GLTFSettings>();
            settings.ExportAnimations = true;
            settings.ExportDisabledGameObjects = true;
            var exporter = new GLTFSceneExporter(new[] { go.transform }, new ExportContext(settings));
            exporter.SaveGLB(directory, go.name);
        }

        public static void BuildKindling(GameObject go, string sparkDirectory)
        {
            Kindling kindling = new(go);
            string assetsDirectory = GetOrCreateSparkAssetsDirectory(sparkDirectory);
            List<Texture> textures = kindling.GetTextures().ToList();
            LogUtility.LogElements(textures.Select(t => t.name), $"Kindling Textures {textures.Count}");
            foreach (var texture in textures)
            {
                if (texture is not Texture2D tex) continue;
                WriteTextureBytesToFile(assetsDirectory, tex);
            }
            string kindlingPath = Path.Combine(sparkDirectory, go.name + ".kindling");
            using StreamWriter sw = File.CreateText(kindlingPath);
            string kindlingJson = kindling.ToJson();
            sw.Write(kindlingJson);
        }

        /// <summary>
        /// Compress directory to zip file.
        /// </summary>
        public static void Compress(string pathIn, string fileOut, bool doDebug = false)
        {
            if (Directory.Exists(pathIn))
            {
                // Delete Existing Zip
                if (File.Exists(fileOut))
                {
                    File.Delete(fileOut);
                }
                
                // Create Containing Directory (if Necessary)
                Directory.CreateDirectory(Path.GetDirectoryName(fileOut));

                // Compress Source Directory to Zip
                ZipFile.CreateFromDirectory(pathIn, fileOut);

                // Delete Source Directory
                if (!doDebug && File.Exists(fileOut))
                {
                    Directory.Delete(pathIn, true);
                }
            }
            else
            {
                Debug.LogError($"Failed to locate source directory: {pathIn}");
            }
        }
        
        public static Texture2D CreateDefaultTexture2D()
        {
            return new Texture2D(w, h, TextureFormat.RGBA32, false);
        }
        
        public static string GetAbsoluteAssetPath(UnityEngine.Object asset)
        {
            string relativePath = AssetDatabase.GetAssetPath(asset);
            relativePath = relativePath.Replace("Assets" + Path.DirectorySeparatorChar, "");
            string path = Path.Combine(Application.dataPath, relativePath);
            if (IsBuiltInAssetPath(path))
            {
                path = Path.Combine(UnityBuiltInPlaceholderDirectoryName, asset.name);
            }
            return path;
        }
        
        public static bool IsBuiltInAssetPath(string assetPath)
        {
            return assetPath.Contains(UnityBuiltInAssetsDirectoryPath) || assetPath.Contains(UnityBuiltInAssetsDirectoryPathAlt);
        }

        public static void WriteTextureBytesToFile(string directoryPath, Texture2D tex)
        {
            Texture2D readableTex = tex.isReadable ? tex : DuplicateTexture(tex);
            byte[] bytes = readableTex.EncodeToPNG();
            if (bytes is not null && bytes.Length > 0)
            {
                string path = Path.Combine(directoryPath, tex.name + ".png");
                File.WriteAllBytes(path, bytes);
            }
            else
            {
                LogUtility.LogError($"Texture {tex.name} failed to encode to png.");
            }
            if (!tex.isReadable) UnityEngine.Object.DestroyImmediate(readableTex);
        }

        static Texture2D DuplicateTexture(Texture source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
 
            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }
        
        public static string[] GetTypes(Type type)
        {
            string[] _types = null;
            
            if (_types == null)
            {
                var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
                var strings = types.Where(y => type.IsAssignableFrom(y)).Select(x => x.ToString());
                _types = strings.Select(x => x.Replace("DimX.Common.Assets.Types.", "")).ToArray();
            }
            
            return _types;
        }
        
        public static void BuildSpark(bool doDebug, bool doDeploy, string outputPath, SparkConfigData configData, Editor editor)
        {
            // Generate Output Path
            var path = Path.Combine(outputPath, configData.metadata.Guid.ToString());
                
            // Delete Directory with supplemental assets
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
                
            Directory.CreateDirectory(path);
                
            // Copy Files
            string sparkAssetsDirectory = GetOrCreateSparkAssetsDirectory(path);
            CopyFiles(sparkAssetsDirectory, configData.supplementalFiles);

            // Generate DLL(s)
            BuildDLL(path, configData.metadata, doDebug);

            // Generate Metadata
            BuildMetadata(configData.metadata, configData, path);

            // Generate Preview
            GeneratePreview(configData, path, editor);
                
            // Sparks other than Config Sparks
            if(configData.prefab != null)
            {
                // Generate Asset Bundle
                if (configData.buildAssetBundles)
                {
                    foreach (var buildTarget in configData.buildTargets)
                    {
                        AssetBundleUtility.BuildAssetBundle(path, buildTarget, configData.prefab);
                    }
                }
                else
                {
                    // Generate Kindling
                    if (configData.prefab.GetComponentInChildren<RectTransform>() ||
                        configData.prefab.GetComponentInChildren<ParticleSystem>())
                    {
                        BuildKindling(configData.prefab, path);
                    }

                    // Generate Geometry
                    if (configData.prefab.GetComponentInChildren<Renderer>())
                    {
                        BuildGeometry(configData.prefab, path);
                    }
                }
            }

            // Generate Spark File
            var file = $"{path}.dimxs";
            Compress(path, file);
            
            if (doDeploy)
            {
                Deploy.DeploySingleSpark(file);
            }
            else
            {
                EditorUtility.RevealInFinder(file);
            }
        }
        
        private static void GeneratePreview(SparkConfigData configData, string path, Editor editor)
        {
            if (!string.IsNullOrWhiteSpace(configData.previewPath))
            {
                BuildPreview(configData.previewPath, path);
            }
            else if (configData.prefab.GetComponentInChildren<RectTransform>())
            {
                BuildPreview(PreviewType.Symbol, path);
            }
            else if (configData.prefab.GetComponentInChildren<Renderer>() && editor != null)
            {
                BuildPreview(editor, path);
            }
            else
            {
                BuildPreview(PreviewType.Config, path);
            }

            byte[] rawData = File.ReadAllBytes(Path.Combine(path, "Preview.png"));
            configData.Preview.LoadImage(rawData);
        }
    }
}