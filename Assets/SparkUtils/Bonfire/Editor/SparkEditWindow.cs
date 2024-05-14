using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DimX.Common.Assets.Types.Sparks;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    public class SparkEditWindow : EditorWindow
    {
        private SparkTreeViewItem _item;
        private string _name;
        private Guid _guid;
        private string _type;
        private string[] _types;
        private Action<string, string, string, Guid, byte[]> _callback;
        private bool _hasErros;
        private string _previewPath;
        private Texture2D _previewTexture;
        
        private const int DisplayWidth = 300;
        private const int DisplayHeight = 300;

        public static void ShowWindow(SparkTreeViewItem item, Action<string, string, string, Guid, byte[]> callback)
        {
            if (item == null)
            {
                Debug.LogError($"SparkEditorWindow.ShowWindow: Missing SparkTreeViewItem parameter");
                return;
            }
            
            SparkEditWindow window = CreateInstance<SparkEditWindow>();
            window.titleContent = new GUIContent("Edit Spark");
            window.minSize = new Vector2(450, 500);
            window.maxSize = new Vector2(450, 500);
            
            window._callback = callback;
            window._item = item;
            window._guid = item.Guid;
            window._name = item.Name;
            window._type = item.Type;
            
            window._previewTexture = BuildUtilities.CreateDefaultTexture2D();
            window._previewTexture.SetPixels(item.Preview.GetPixels());
            window._previewTexture.Apply();
            
            window.ShowModal();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUIUtility.labelWidth = 100;
            EditorGUILayout.LabelField("Metadata");

            EditorGUI.indentLevel++;
            {
                _hasErros = false;
                
                // Guid
                GUILayout.BeginHorizontal();
                {
                    GUI.enabled = false;
                    {
                        _guid = new Guid(EditorGUILayout.TextField("Guid", _guid.ToString()));
                    }
                    GUI.enabled = true;
                    
                    if (GUILayout.Button("Generate", GUILayout.Width(80)))
                    {
                        _guid = Guid.NewGuid();
                    }
                }
                GUILayout.EndHorizontal();
                ValidateStringField(_guid.ToString(), "Guid is required");
                
                // Name
                _name = EditorGUILayout.TextField("Name", _name);
                ValidateStringField(_name, "Name is required");
                
                // Type
                string[] types = GetTypes();
                int id = Math.Max(0, Array.IndexOf(types, _type));
                _type = types[EditorGUILayout.Popup("Type", id, types)];
            }
            EditorGUI.indentLevel--;
            
            GUILayout.Space(15);
            
            // Preview
            EditorGUILayout.LabelField("Preview", GUILayout.Width(100));

            if (!string.IsNullOrWhiteSpace(_previewPath))
            {
                byte[] tmpBytes = File.ReadAllBytes(_previewPath);
                _previewTexture.LoadImage(tmpBytes);
            }

            GUILayout.BeginHorizontal();
            {
                Rect rect2 = EditorGUILayout.BeginVertical();
                {
                    if (GUILayout.Button("Browse", GUILayout.Width(75)))
                    {
                        string selected = EditorUtility.OpenFilePanel("Select Preview", _previewPath, "png");

                        if (!string.IsNullOrWhiteSpace(selected))
                        {
                            _previewPath = selected;
                        }
                    }
                    
                    var rect = EditorGUILayout.GetControlRect();
                    GUI.DrawTexture(new Rect(rect.x, rect.y, DisplayWidth, DisplayHeight), _previewTexture);
                    GUILayout.Space(DisplayHeight);
                }
                EditorGUILayout.EndVertical();
                GUI.Box(rect2, GUIContent.none);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Cancel"))
                {
                    Close();
                }
                
                if (!_hasErros && (!string.IsNullOrWhiteSpace(_previewPath) || _item.Guid != _guid || _item.Name != _name || _item.Type != _type))
                {
                    if (GUILayout.Button("Save"))
                    {
                        bool canSave = EditorUtility.DisplayDialog("Confirm Save",
                            "Are you sure you want to save the selected spark?",
                            "Ok",
                            "Cancel");

                        if (canSave)
                        {
                            Save();
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void Save()
        { 
            byte[] bytes = ScaleTexture(_previewTexture).EncodeToPNG();
            _callback?.Invoke(_item.Path, _name, _type, _guid, bytes);
            
            Close();
        }

        private Texture2D ScaleTexture(Texture2D source)
        {
            Texture2D result = BuildUtilities.CreateDefaultTexture2D();
            Color[] rpixels = result.GetPixels();
            float incX = (1.0f / result.width);
            float incY = (1.0f / result.height);
            
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % result.width), 
                                                      incY * Mathf.Floor(px / result.height));
            }

            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        private void ValidateStringField(string value, string errorMessage)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            GUILayout.Space(15);
            
            _hasErros = true;
        }

        #region Utilities
        private string[] GetTypes()
        {
            if (_types != null)
            {
                return _types;
            }
            
            Type type = typeof(Spark);
            IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            IEnumerable<string> strings = types.Where(y => type.IsAssignableFrom(y)).Select(x => x.ToString());
            _types = strings.Select(x => x.Replace("DimX.Common.Assets.Types.", "")).ToArray();
            
            return _types;
        }
        #endregion
    }
}