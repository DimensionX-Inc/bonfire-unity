using System;
using DimX.Common.Assets.Types.Sparks;
using DimX.Common.Assets.Types.Sparks.Elements.Common;
using DimX.Common.Assets.Types.Sparks.Elements.Libraries.Traits;
using DimX.Common.Utilities;
using UnityEngine;

namespace Sparks.Demo
{
    /// <summary>
    /// This is a demo Cause that fires when its Spark exceeds a given Altitude.
    /// </summary>
    [DisplayName("Demo Cause Ext.")]
    [Guid("f9a30fde-9cfd-427f-a209-859be209d446")]
    public class CauseDemo : Cause
    {
        private readonly TraitAltitude _traitAltitude;

        private float _altitudeLast = float.MinValue;
        private float AltitudeCurrent => _traitAltitude?.GetValue<float>() ?? 0f;
        private float AltitudeThreshold => GetParams()[0].GetValue<float>();
        
        public CauseDemo(Spark spark) : base(spark, new Param<float>("Delay", 5f))
        {
            // Register Update Delegate
            Lifecycle.OnUpdate(OnUpdate);

            // Get Reference to this Spark's Altitude Trait
            _traitAltitude = spark.GetTrait<TraitAltitude>();
            if (_traitAltitude == null)
            {
                Debug.LogError($"Failed to locate Altitude trait in Spark: {spark.Name}");
            }
        }

        private void OnUpdate()
        {
            // Check altitude for changes and invoke output on transition above threshold
            if ((AltitudeCurrent >= AltitudeThreshold) && (_altitudeLast < AltitudeThreshold))
            {
                InvokeOutput(Spark);
            }

            _altitudeLast = AltitudeCurrent;
        }
    }
}
