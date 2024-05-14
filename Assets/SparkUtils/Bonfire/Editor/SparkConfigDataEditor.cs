using UnityEditor;
using DimX.SparkUtils.SO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DimX.SparkUtils
{
    [CustomEditor(typeof(SparkConfigData))]
    public class SparkConfigDataEditor : Editor
    {
        private Editor _editor;
        private SparkConfigData _configData;
        private Object _source;
        private bool _showMessage;
        private Object _temp;

        private SerializedObject _sparkConfigSerializedObject;

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            _configData = (SparkConfigData)target;
            _sparkConfigSerializedObject ??= new SerializedObject(_configData);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Prefab:");
            _configData.prefab = (GameObject)EditorGUILayout.ObjectField(
                _configData.prefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Preview:");
            _configData.Preview = (Texture2D)EditorGUILayout.ObjectField(
                _configData.Preview, typeof(Texture2D), false);
            EditorGUILayout.EndHorizontal();
            
            SparkWindowFields.CreateMetadata(_configData);
            SparkWindowFields.CreateSupplemental(_configData);
            SparkWindowFields.CreateAssetBundle(_sparkConfigSerializedObject);
            SparkWindowFields.CreateGrabPoints(_configData);
            SparkWindowFields.CreatePreview(_configData, _editor);
        }
        
        private void OnDestroy()
        {
            if (_configData == null)
            {
                return;
            }

            // Add preview
            if (_configData.prefab != null)
            {
                BuildUtilities.GeneratePreview(_editor, _configData);
            }

            // Mark scriptable object as dirty
            EditorUtility.SetDirty(_configData);

            // Save changes to asset database and refresh
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}