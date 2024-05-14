using DimX.Common.Assets.Types.Common;
using DimX.SparkUtils.SO;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils
{
    public static class BuildWindowSparkMisc
    {
        private static Transform _grabPoint;
        private static Transform _grabPointL;
        private static Transform _grabPointR;

        #region Grab Points

        /// <summary>
        /// 
        /// </summary>
        public static void AddMiscGrabPoints(GameObject go, SparkConfigData configData)
        {
            configData._useGrabPoint = EditorGUILayout.Toggle("Use Grab Point(s)?", configData._useGrabPoint);
            
            if (!configData._useGrabPoint)
            {
                return;
            }
            
            configData._useDifferentHands = EditorGUILayout.Toggle("Use Different Hands?", configData._useDifferentHands);
            
            if (configData._useDifferentHands)
            {
                _grabPoint = default;
                AddGrabPointField(ref _grabPointL, "Grab Point Left", go, configData.metadata);
                AddGrabPointField(ref _grabPointR, "Grab Point Right", go, configData.metadata);
            }
            else
            {
                _grabPointL = default;
                _grabPointR = default;
                AddGrabPointField(ref _grabPoint, "Grab Point", go, configData.metadata);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void AddGrabPointField(ref Transform grabPoint, string label, GameObject go, Metadata metadata)
        {
            var keyVal = label.Replace(" ", "");
            
            if (metadata.KeyVals.TryGetValue(keyVal, out string value))
            {
                Transform temp = go.transform.Find(value.Replace($"{go.name}/", string.Empty));
                grabPoint = temp == null ? grabPoint : temp;
            }
            
            grabPoint = EditorGUILayout.ObjectField(new GUIContent(label), grabPoint, typeof(Transform), true) as Transform;
            
            CheckAndAddGrabPointToMetadata(ref grabPoint, keyVal, go, metadata);
            if (metadata.KeyVals.TryGetValue(keyVal, out var path)) EditorGUILayout.LabelField(path);
        }

        /// <summary>
        /// 
        /// </summary>
        private static void CheckAndAddGrabPointToMetadata(ref Transform grabPoint, string keyVal, GameObject go, Metadata metadata)
        {
            if (grabPoint != default)
            {
                if (!EnsureObjectIsChild(grabPoint, go.transform, out var pathToChild))
                {
                    Debug.LogError("Grab point must be a child transform within the prefab");
                    grabPoint = null;
                }
                else
                {
                    metadata.KeyVals[keyVal] = pathToChild;
                }
            }
            else
            {
                metadata.KeyVals.Remove(keyVal);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static bool EnsureObjectIsChild(Transform child, Transform expectedParent, out string pathFromChildToParent)
        {
            int i = 0;
            pathFromChildToParent = child.name;
            if (child.name == expectedParent.name)
            {
                return true;
            }

            while (i < 50 && child.parent != null && child.parent.name != expectedParent.name)
            {
                child = child.parent;
                //Unity paths are always '/'
                pathFromChildToParent = child.name + '/' + pathFromChildToParent;
                i++;
            }

            if (child.parent == null || i > 50)
            {
                pathFromChildToParent = null;
                return false;
            }

            pathFromChildToParent = child.parent.name + '/' + pathFromChildToParent;

            return true;
        }

        #endregion
    }
}
