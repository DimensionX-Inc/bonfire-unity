using System;
using System.Collections.Generic;
using System.IO;
using DimX.SparkUtils.SO;
using UnityEngine;
using UnityEditor;

namespace DimX.SparkUtils
{
    public class BuildWindowSpark : EditorWindow
    {
        private Editor _editor;
        private Action _callback;

        private bool _doDebug;
        private bool _doDeploy;
        private string _outputPath;
        private string _assetPath;

        private SerializedObject _sparkConfigSerializedObject;

        private static Dictionary<Guid, SparkConfigData> _configs;
        private static SparkConfigData _configData;

        /// <summary>
        /// Initialize and show the export window.
        /// </summary>
        public static void Show(GameObject go, Action callback)
        {
            // Instantiate and Configure
            var w = CreateWindow(go);

            w._editor = Editor.CreateEditor(go);
            w._callback = callback;
            
            var guid = new Guid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go)));
            
            if (_configs.TryGetValue(guid, out var sparkConfigData))
            {
                _configData = sparkConfigData;
            }
            else
            {
                // Check if we have a SparkConfigData ScriptableObject
                _configData = CreateInstance<SparkConfigData>();
                _configData.name = go.name;
                _configData.prefab = go;
                _configData.metadata.Guid = guid;
                _configData.metadata.Name = go.name;
                _configData.metadata.Type = "Sparks.Entities.Entity";
                _configData.Preview = BuildUtilities.LoadPreview(_configData, w._editor);

                _configs.Add(guid, _configData);
            }
            
            w.ShowUtility();
        }

        /// <summary>
        /// Initialize and show the export window.
        /// </summary>
        public static void Show(Texture2D texture2D)
        {
            // Instantiate and Configure
            var w = CreateWindow(texture2D);

            var guid = new Guid(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(texture2D)));

            if(_configs.TryGetValue(guid, out var sparkConfigData))
            {
                _configData = sparkConfigData;
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(w._assetPath);

                _configData = CreateInstance<SparkConfigData>();
                _configData.name = name;
                _configData.metadata.Name = name;
                _configData.metadata.Guid = guid;
                _configData.metadata.Type = "Sparks.Configs.Config";
                _configData.previewPath = w._assetPath;
                _configData.Preview = BuildUtilities.LoadPreview(_configData, w._editor);
                _configs.Add(guid, _configData);
            }

            w.ShowUtility();
        }

        #region Unity

        /// <summary>
        /// Draw the export window.
        /// </summary>
        private void OnGUI()
        {
            _sparkConfigSerializedObject ??= new SerializedObject(_configData);

            SparkWindowFields.CreateSource(_configData);
            SparkWindowFields.CreateMetadata(_configData);
            SparkWindowFields.CreateSupplemental(_configData);
            SparkWindowFields.CreateAssetBundle(_sparkConfigSerializedObject);
            SparkWindowFields.CreateGrabPoints(_configData);
            SparkWindowFields.CreatePreview(_configData, _editor);

            //////////////////////////////////////////////////

            GUILayout.Space(15);
            _doDebug = EditorGUILayout.Toggle("Do Debug?", _doDebug);
            _doDeploy = EditorGUILayout.Toggle("Auto Deploy?", _doDeploy);
            _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);

            //////////////////////////////////////////////////

            GUILayout.Space(15);
            if (GUILayout.Button(_doDeploy ? "Build and Deploy" : "Build"))
            {
                BuildUtilities.GeneratePreview(_editor, _configData);
                BuildUtilities.BuildSpark(_doDebug, _doDeploy, _outputPath, _configData, _editor);
                
                _callback?.Invoke();
                Close();
            }

            _sparkConfigSerializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Destroy the export window.
        /// </summary>
        private void OnDestroy()
        {
            // Determine the scriptable object path (should be in same location as the spark)
            var asset = $"{Path.GetDirectoryName(_assetPath)}/{_configData.name}.asset";

            if (File.Exists(asset))
            {
                // If scriptable object exists, mark it as dirty
                EditorUtility.SetDirty(_configData);
            }
            else
            {
                // If scriptable object does not exist, create it
                AssetDatabase.CreateAsset(_configData, asset);
            }

            //  Save changes to the asset database and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Destroy the Editor window
            DestroyImmediate(_editor, true);
        }

        #endregion

        #region Utilities

        private static BuildWindowSpark CreateWindow(UnityEngine.Object asset)
        {
            var w = CreateInstance<BuildWindowSpark>();
            w.titleContent = new GUIContent("Configure Spark");
            w._outputPath = Path.Combine(Constants.AssetRoot, "Sparks");
            w._assetPath = AssetDatabase.GetAssetPath(asset);

            LoadConfigs();

            return w;
        }

        private static void LoadConfigs()
        {
            _configs = new Dictionary<Guid, SparkConfigData>();

            // Find all SparkConfigData Scriptable Objects (GUIDs are returned)
            var assets = AssetDatabase.FindAssets("t:SparkConfigData");

            foreach (var asset in assets)
            {
                // Get Scriptable Object Asset Path
                var assetPath = AssetDatabase.GUIDToAssetPath(asset);

                // Load the Scriptable Object
                var configData = AssetDatabase.LoadAssetAtPath<SparkConfigData>(assetPath);

                // Determine what kind of spark the config is associated with
                UnityEngine.Object source = configData.prefab == null ? configData.Preview : configData.prefab;

                // Get the asset path of the spark
                var sourcePath = AssetDatabase.GetAssetPath(source);

                if (string.IsNullOrEmpty(sourcePath))
                {
                    continue;
                }

                // Get the guid of the spark
                var guid = new Guid(AssetDatabase.AssetPathToGUID(sourcePath));

                // Add to config list
                _configs.Add(guid, configData);
            }
        }

        #endregion
    }
}
