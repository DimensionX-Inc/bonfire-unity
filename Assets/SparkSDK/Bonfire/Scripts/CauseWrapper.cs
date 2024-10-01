using System.Collections.Generic;
using UnityEngine;

namespace DimX.SparkSDK.Scripts
{
    public class CauseWrapper : MonoBehaviour
    {
        public string Type;
        public List<ModifierWrapper> ModifierOutputs = new();
        public List<EffectWrapper> EffectOutputs = new();
    }
}