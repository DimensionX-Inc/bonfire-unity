using System;
using UnityEngine;

namespace DimX.SparkUtils.SO
{
    public interface IConfig
    {
        public GameObject Prefab { get; set; }
        public Guid Guid { get; set; }
    }
}