using System;
using DimX.Common.Architecture.Entities;
using DimX.Common.Assets.Types.Sparks.Elements.Common;
using DimX.Common.Assets.Types.Sparks.Elements.Libraries.Traits;
using DimX.Common.Assets.Types.Sparks.Entities;
using DimX.Common.Utilities;
using UnityEngine;

namespace Sparks.Demo
{
    /// <summary>
    /// This is an example Spark that contains internal and external elements.
    /// </summary>
    public class SparkDemo : Entity
    {
        private int valueA = 2;
        private int valueB = 5;
        
        protected override void Initialize()
        {            
            // Base Class
            base.Initialize();
            
            // General-Purpose Elements
            AddTrait(typeof(SparkDemo), new TraitDemo(this));
            AddCauseDefinition<CauseDemo>();
            AddEffectDefinition<EffectDemo>();

            // Spark-Specific Elements
            AddTrait(typeof(SparkDemo), new TraitInternal(this));
            AddCauseDefinition<CauseInternal>();
            AddEffectDefinition<EffectInternal>();
            
            // Anonymous Elements
            AddTrait(typeof(SparkDemo), new TraitAnonymous<int>("Demo Trait Anonymous", OnAnonymousGet, OnAnonymousSet, valueA));
        }

        private int OnAnonymousGet()
        {
            Debug.Log($"{nameof(TraitInternal)}.{nameof(OnAnonymousGet)} = {valueA}");
            return valueA;
        }

        private void OnAnonymousSet(int value)
        {
            valueA = value;
            Debug.Log($"{nameof(TraitInternal)}.{nameof(OnAnonymousSet)} = {valueA}");
        }

        /// <summary>
        /// Example Trait that's unique to this Spark.
        /// </summary>
        private class TraitInternal : Trait<int>
        {
            private readonly SparkDemo _spark;
            
            public TraitInternal(ISpark spark) : base("Demo Trait Int.", (spark as SparkDemo).valueB)
            {
                _spark = spark as SparkDemo;
            }

            public override Guid Guid => new("4407497e-d807-424d-92ad-a26c09d399c9");
            
            protected override int OnGet()
            {
                Debug.Log($"{nameof(TraitInternal)}.{nameof(OnGet)} = {_spark.valueB}");
                return _spark.valueB;
            }

            protected override void OnSet(int value)
            {
                _spark.valueB = value;
                Debug.Log($"{nameof(TraitInternal)}.{nameof(OnSet)} = {_spark.valueB}");
            }
        }

        /// <summary>
        /// Example Cause that's unique to this Spark.
        /// </summary>
        [DisplayName("Demo Cause Int.")]
        [Guid("592fe749-ad07-4209-8c3a-f0ee7e5fa68f")]
        private class CauseInternal : Cause
        {
            private int _lastA = -1;
            private int _lastB = -1;
            private readonly SparkDemo _spark;
            
            public CauseInternal(ISpark spark) : base(spark)
            {
                _spark = spark as SparkDemo;
                Lifecycle.OnUpdate(OnUpdate);
            }

            private void OnUpdate()
            {
                if (_lastA != _spark.valueA ||
                    _lastB != _spark.valueB)
                {
                    _lastA = _spark.valueA;
                    _lastB = _spark.valueB;

                    if (_lastA == _lastB)
                    {
                        InvokeOutput(Spark);
                    }
                }
            }
        }

        /// <summary>
        /// Example Effect that's unique to this Spark.
        /// </summary>
        [DisplayName("Demo Effect Int.")]
        [Guid("0a41601c-890b-459e-8a46-64bf20ac4eba")]
        private class EffectInternal : Effect
        {
            private SparkDemo _spark;
            
            public EffectInternal(ISpark spark) : base(spark, 1)
            {
                _spark = spark as SparkDemo;
            }
            
            protected override void OnEffect(Param[] @params, ISpark invoker)
            {
                Debug.Log($"{nameof(TraitInternal)}.{nameof(OnEffect)}");
                
                // Propagate Invoke
                InvokeOutput(invoker);
            }
        }
    }
}