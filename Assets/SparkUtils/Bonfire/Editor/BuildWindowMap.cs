using System;
using System.IO;
using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace DimX.SparkUtils
{
    public class BuildWindowMap : EditorWindow
    {
        private Editor _editor;
        private GameObject _go;
        
        private Action _callback;
        private Metadata _metadata = new();

        private bool _doDeploy;
        private string _outputPath;

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

            w.ShowModal();
        }
        
        #region Unity

        /// <summary>
        /// Draw the export window.
        /// </summary>
        private void OnGUI()
        {
            LogUtility.Log();
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
            }
            EditorGUI.indentLevel--;
            
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

        private void Build()
        {
            // Generate Output Path
            var path = GetPath(_metadata);
            Directory.CreateDirectory(path);
            
            // Generate Metadata
            BuildUtilities.BuildMetadata(_metadata, path);
            
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