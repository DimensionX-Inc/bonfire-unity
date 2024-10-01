using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
using DimX.SparkUtils.SO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DimX.SparkUtils
{
    public class BuildWindowMap : EditorWindow
    {
        private Editor _editor;
        private GameObject _go;
        private MapConfigData _mapConfig;
        private GameObject t;
        
        private Action _callback;
        private Metadata _metadata = new();

        private bool _doDeploy;
        private string _outputPath;
        
        private static readonly Dictionary<Guid, MapConfigData> _configs = new();

        /// <summary>
        /// Initialize and show the export window.
        /// </summary>
        public static void Show(GameObject go, Action callback)
        {
            var w = CreateInstance<BuildWindowMap>();
            w.titleContent = new GUIContent("Configure Map");

            w._editor = Editor.CreateEditor(go);
            w._go = go;
            w._callback = callback;
            w._metadata.Guid = GameObjectToGuid(go);
            w._metadata.Name = go.name;
            w._metadata.Type = "Maps.Map";

            w._doDeploy = false;
            w._outputPath = Path.Combine(Constants.AssetRoot, "Maps");
            
            _configs.Clear();
            BuildUtilities.FindConfigs(_configs);
            if (BuildUtilities.TryGetAssetGuid(go, out Guid prefabGuid))
            {
                if (!_configs.TryGetValue(prefabGuid, out w._mapConfig))
                {
                    w._mapConfig = MapConfigData.CreateConfig(go);
                    BuildUtilities.SaveConfigToDisk(w._mapConfig);
                }
            }

            w.Show();
        }
        
        #region Unity

        /// <summary>
        /// Draw the export window.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUILayout.LabelField("Source");
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.LabelField("Prefab", _go.name);
            }
            EditorGUI.indentLevel--;

            //////////////////////////////////////////////////

            GUILayout.Space(15);
            EditorGUILayout.LabelField("Metadata");
            EditorGUI.indentLevel++;
            {
                _metadata.Guid = new Guid(EditorGUILayout.TextField("Guid", _metadata.Guid.ToString()));
                _metadata.Name = EditorGUILayout.TextField("Name", _metadata.Name);
                _mapConfig.author = EditorGUILayout.TextField("Author", _mapConfig.author);
            }
            EditorGUI.indentLevel--;
            
            //////////////////////////////////////////////////
            
            TeleportableSurfaceGUI();
            
            //////////////////////////////////////////////////

            try
            {
                GUILayout.Space(15);
                EditorGUILayout.LabelField("Preview");
                DrawLine(new Color(0.3f, 0.3f, 0.3f), 1, -2);
                _editor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(256, 256), GUIStyle.none);
                DrawLine(new Color(0.3f, 0.3f, 0.3f), 1, -2);
            }
            catch
            {
                // NO-OP
            }
            
            //////////////////////////////////////////////////

            GUILayout.Space(15);
            _doDeploy = EditorGUILayout.Toggle("Auto Deploy", _doDeploy);
            _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);
            
            //////////////////////////////////////////////////

            GUILayout.Space(15);
            if (GUILayout.Button(_doDeploy ? "Build and Deploy" : "Build"))
            {
                LogUtility.Log("launching build task");
                Build();
                
            }
        }

        private void TeleportableSurfaceGUI()
        {
            GUILayout.Space(15);
            Undo.RecordObject(_mapConfig, "Map Config Edits");
            EditorGUILayout.LabelField("VR Teleport Surfaces");
            EditorGUILayout.LabelField(" - Leave this list empty to make all surfaces in the map teleportable.");
            for (int i = 0; i < _mapConfig.teleportSurfaces.Count; i++)
            {
                string childPath = _mapConfig.teleportSurfaces[i];
                Transform item = string.IsNullOrEmpty(childPath) ? null : _go.transform.Find(childPath);
                item = EditorGUILayout.ObjectField(
                    new GUIContent($"{i}"),
                    item,
                    typeof(Transform),
                    allowSceneObjects: true) as Transform;
                _mapConfig.teleportSurfaces[i] = BuildUtilities.GetChildPath(item);
            }
            Transform newItem = EditorGUILayout.ObjectField(
                new GUIContent("Add"),
                null,
                typeof(Transform),
                allowSceneObjects: true) as Transform;
            if (newItem)
            {
                _mapConfig.teleportSurfaces.Add(BuildUtilities.GetChildPath(newItem));
            }
            _mapConfig.teleportSurfaces.RemoveAll(string.IsNullOrEmpty);
        }

        private void Build()
        {
            // Generate Output Path
            var path = GetPath(_metadata);
            Directory.CreateDirectory(path);
            
            // Save Teleportable surfaces
            _metadata.KeyVals.Add(nameof(MapConfigData.teleportSurfaces), JsonConvert.SerializeObject(_mapConfig.teleportSurfaces));
            
            // Generate Metadata
            BuildUtilities.BuildMetadata(_metadata, path);
            _metadata.KeyVals[nameof(MapConfigData.author)] = _mapConfig.author;
            
            // Generate Preview
            BuildUtilities.BuildPreview(_editor, path);

            // Generate Geometry
            BuildUtilities.BuildGeometry(_go, path);

            // Generate Map File
            var file = $"{path}.dimxm";
            BuildUtilities.Compress(path, file);
            
            // Exit
            Close();
            if (_doDeploy)
            {
                Deploy.DeploySingleMap(file);
            }
            else
            {
                EditorUtility.RevealInFinder(file);
            }
            _callback.Invoke();
        }

        /// <summary>
        /// Destroy the export window.
        /// </summary>
        private void OnDestroy()
        {
            DestroyImmediate(_editor, true);
        }
        
        #endregion

        #region Utilities
        
        /// <summary>
        /// 
        /// </summary>
        private string GetPath(Metadata metadata)
        {
            return Path.Combine(_outputPath, metadata.Guid.ToString());
        }
        
        /// <summary>
        /// 
        /// </summary>
        private static void DrawLine(Color color, int thickness, int padding)
        {
            var r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        
        /// <summary>
        /// 
        /// </summary>
        private static Guid GameObjectToGuid(Object obj)
        {
            // Convert to GUID (from string) to validate and ensure proper format
            return new Guid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(obj)));
        }
        
        #endregion
    }
}