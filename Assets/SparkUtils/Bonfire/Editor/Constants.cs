using System.IO;
using UnityEngine.Device;

namespace DimX.SparkUtils
{
    public static class Constants
    {
        /// <summary>
        /// 
        /// </summary>
        public static string AssetRoot => Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Bonfire"));

        /// <summary>
        /// 
        /// </summary>
        public static string BonfireRoot => Path.GetFullPath(Path.Combine(Application.persistentDataPath, "..", "..", "Dimension X", "Bonfire Builder", "Assets"));
    }
}