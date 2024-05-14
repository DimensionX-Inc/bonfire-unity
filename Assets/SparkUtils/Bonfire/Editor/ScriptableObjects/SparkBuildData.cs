using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DimX.SparkUtils.SO
{
    [CreateAssetMenu(fileName = "NEWSparkBuild", menuName = "DimX/Sparks/BuildData")]
    public class SparkBuildData : ScriptableObject
    {
        public List<SparkConfigData> configs;
        public bool doDebug;
        public bool autoDeploy;
        public string outputPath;

        private SparkBuildData()
        {
            configs = new List<SparkConfigData>();
            doDebug = false;
            autoDeploy = true;
            outputPath = Path.Combine(
                Path.GetFullPath(
                    Path.Combine(Application.dataPath, "..", "Bonfire")
                    ),
                "Sparks");
        }
    }
}