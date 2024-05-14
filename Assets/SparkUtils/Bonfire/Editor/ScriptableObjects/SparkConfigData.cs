using System;
using System.Collections.Generic;
using DimX.Common.Assets.Types.Common;
using DimX.Common.Utilities;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace DimX.SparkUtils.SO
{
    [Serializable, CreateAssetMenu(fileName = "NEWSparkConfig", menuName = "DimX/Sparks/ConfigData")]
    public class SparkConfigData : ScriptableObject, ISerializationCallbackReceiver
    {
        public Metadata metadata;
        public List<StringTuple> keyValuePairs = new();
        public GameObject prefab;
        public List<string> supplementalFiles;
        public bool buildAssetBundles;
        public List<BonfireBuildTarget> buildTargets = new();
        private Texture2D preview;
        public string previewPath;
        public Editor _editor;

        // Grab Point(s)
        public bool _useGrabPoint;
        public bool _useDifferentHands;
        public StringTuple _grabPointPrimary;
        public StringTuple _grabPointSecondary;

        [SerializeField] private string _metadataSerialized;

        public Texture2D Preview
        {
            get
            {
                if (!string.IsNullOrEmpty(previewPath) && preview == null)
                {
                    preview = AssetDatabase.LoadAssetAtPath<Texture2D>(previewPath);
                }

                return preview;
            }
            set => preview = value;
        }

        private SparkConfigData()
        {
            metadata = new Metadata
            {
                Guid = new Guid(),
                Name = string.Empty,
                Type = "Sparks.Entities.Entity",
                KeyVals = new Dictionary<string, string>()
            };

            prefab = null;
            supplementalFiles = new List<string>();
            preview = null;
            _metadataSerialized = string.Empty;
        }

        public void OnBeforeSerialize()
        {
            _metadataSerialized = JsonConvert.SerializeObject(metadata);
        }

        public void OnAfterDeserialize()
        {
            metadata = JsonConvert.DeserializeObject<Metadata>(_metadataSerialized);
        }
    }
}