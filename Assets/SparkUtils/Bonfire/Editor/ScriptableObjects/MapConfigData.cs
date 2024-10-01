using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace DimX.SparkUtils.SO
{
    [CreateAssetMenu(fileName = "New Map Config", menuName = "DimX/Map/ConfigData")]
    public class MapConfigData : ScriptableObject, IConfig
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private string guid;
        public List<string> teleportSurfaces = new();
        public string author;
        
        public Guid Guid
        {
            get => Guid.Parse(guid);
            set => guid = value.ToString();
        }
        public GameObject Prefab
        {
            get => prefab;
            set => prefab = value;
        }

        public static MapConfigData CreateConfig(GameObject prefab)
        {
            MapConfigData config = ScriptableObject.CreateInstance<MapConfigData>();
            config.Prefab = prefab;
#if UNITY_EDITOR
            string path = AssetDatabase.GetAssetPath(prefab);
            config.guid = AssetDatabase.AssetPathToGUID(path);
#endif
            config.name = prefab.name;
            return config;
        }

        public void OnValidate()
        {
            for (int i = teleportSurfaces.Count - 1; i >= 0; i--)
            {
                if (!prefab.transform.Find(teleportSurfaces[i]))
                {
                    teleportSurfaces.RemoveAt(i);
                }
            }
        }
    }
}