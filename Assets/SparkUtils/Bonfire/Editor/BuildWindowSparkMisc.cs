using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
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
                AddGPField(ref _grabPointL, ref configData._grabPointPrimary, "Grab Point Left", go, configData.metadata);
                AddGPField(ref _grabPointR, ref configData._grabPointSecondary, "Grab Point Right", go, configData.metadata);
            }
            else
            {
                _grabPointL = default;
                _grabPointR = default;
                AddGPField(ref _grabPoint, ref configData._grabPointPrimary, "Grab Point", go, configData.metadata);
            }
        }

        /// <param name="data">Used to support backwards compatibility where grab points were in metadata's key values</param>
        private static void AddGPField(
            ref Transform grabPointTransform, 
            ref StringTuple grabPointPath, 
            string label, 
            GameObject go, 
            Metadata data = null)
        {
            string key = label.Replace(" ", "");
            
            //Maintain backwards compatibility
            if((grabPointPath == default || string.IsNullOrEmpty(grabPointPath.Key))
               &&
               data != null && data.KeyVals.TryGetValue(key, out string metaPath))
            {
                grabPointPath = new StringTuple(key, metaPath);
            }
            
            //check if value is already set
            if (grabPointPath != default)
            {
                Transform temp = go.transform.Find(grabPointPath.Value.Replace($"{go.name}/", string.Empty));
                grabPointTransform = temp == null ? grabPointTransform : temp;
            }

            //create object field tied to that value
            grabPointTransform =
                EditorGUILayout.ObjectField(label, grabPointTransform, typeof(Transform),
                    true) as Transform;
            
            //set value in configData
            if (grabPointTransform != default)
            {
                if (!EnsureObjectIsChild(grabPointTransform, go.transform, out var pathToChild))
                {
                    Debug.LogError("Grab point must be a child transform within the prefab");
                    grabPointTransform = null;
                    grabPointPath = default;
                }
                else
                {
                    grabPointPath = new StringTuple(key, pathToChild);
                }
            }
            else
            {
                grabPointPath = default;
            }

            //set label if valid
            if (grabPointPath != default)
            {
                EditorGUILayout.LabelField(grabPointPath.Value);
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
