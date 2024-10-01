using System;
using DimX.Common.Assets.Types.Sparks;
using DimX.Common.Assets.Types.Sparks.Elements.Common;
using UnityEngine;

namespace Sparks.Demo
{
    /// <summary>
    /// This is an example Trait that supersizes the associated Spark.
    /// </summary>
    public class TraitDemo : Trait<float>
    {
        private readonly Spark _spark;
        
        public TraitDemo(Spark spark) : base("Demo Trait Ext.", 2f)
        {
            _spark = spark;
        }

        public override Guid Guid => new("01b7dd77-d205-4e6d-a7cc-1d339d4e7860");
        
        protected override float OnGet()
        {
            Debug.Log($"{nameof(TraitDemo)}.{nameof(OnGet)} = {_value}");
            return _value;
        }

        protected override void OnSet(float value)
        {
            _value = value;
            _spark.Actor.transform.localScale = Vector3.one * value;
            Debug.Log($"{nameof(TraitDemo)}.{nameof(OnSet)} = {_value}");
        }
    }
}
