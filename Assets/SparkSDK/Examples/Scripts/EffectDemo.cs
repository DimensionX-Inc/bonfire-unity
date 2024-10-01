using DimX.Common.Architecture.Entities;
using DimX.Common.Assets.Types.Sparks.Elements.Common;
using DimX.Common.Utilities;
using System;
using UnityEngine;

namespace Sparks.Demo
{
    /// <summary>
    /// This is a demo Effect that writes a parameter to the console.
    /// </summary>
    [DisplayName("Demo Effect Ext.")]
    [Guid("75a101ff-cc82-4b40-abd1-4426ac0c5641")]
    public class EffectDemo : Effect
    {
        public EffectDemo(ISpark spark) : base(spark, 1, new Param<float>("Parameter", 5f))
        {    
            // NO-OP
        }
        
        protected override void OnEffect(Param[] @params, ISpark invoker)
        {
            var value = @params[0].GetValue<float>();
            Debug.Log($"{nameof(EffectDemo)}.{nameof(OnEffect)} = {value}");
            
            // Propagate Invoke
            InvokeOutput(invoker);
        }
    }
}
