using System.Collections.Generic;
using UnityEngine;

namespace DimX.SparkSDK.Scripts
{
    public class EffectWrapper : MonoBehaviour
    {
        public string Type;
        public List<CauseWrapper> CauseInputs = new();
        public List<ModifierWrapper> ModifierInputs = new();
    }
}