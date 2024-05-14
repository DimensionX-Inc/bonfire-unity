using System;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    public class GenerateGuidWindow : EditorWindow
    {
        private string _guid;

        public static void ShowWindow()
        {
            GenerateGuidWindow window = CreateInstance<GenerateGuidWindow>();
            window.titleContent = new GUIContent("Generate Guid");
            window._guid = Guid.NewGuid().ToString();
            window.minSize = new Vector2(500, 50);
            window.maxSize = new Vector2(500, 50);
            window.ShowModal();
        }

        private void OnGUI()
        {
            GUILayout.Space(15);
            EditorGUIUtility.labelWidth = 50;
            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                {
                    _guid = EditorGUILayout.TextField("Guid", _guid);
                }
                GUI.enabled = true;

                if (GUILayout.Button("Generate", GUILayout.Width(80)))
                {
                    _guid = Guid.NewGuid().ToString();
                }

                if (GUILayout.Button("Copy", GUILayout.Width(80)))
                {
                    GUIUtility.systemCopyBuffer = _guid;
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}