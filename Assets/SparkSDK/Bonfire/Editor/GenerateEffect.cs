using System;

namespace DimX.SparkSDK
{
    public static class GenerateEffect
    {
        public static string Content =>
            "using DimX.Common.Architecture.Entities;\n" +
            "using DimX.Common.Assets.Types.Sparks;\n" +
            "using DimX.Common.Assets.Types.Sparks.Elements.Common;\n" +
            "using DimX.Common.Utilities;\n" +
            "\n" +
            "namespace Sparks.TODO\n" +
            "{\n" +
            "    /// <summary>\n" +
            "    /// TODO\n" +
            "    /// </summary>\n" + 
            "    [DisplayName(\"TODO Effect Name\")] \n" +
           $"    [Guid(\"{Guid.NewGuid()}\")] \n" +
            "    public class EffectTODO : Effect\n" +
            "    {\n" +
            "        private readonly Spark _spark;\n" +
            "\n" +
            "        public EffectTODO(Spark spark) : base(spark, 1)\n" +
            "        {\n" +
            "            _spark = spark;\n" +
            "        }\n" +
            "\n"+
            "        protected override void OnEffect(Param[] @params, ISpark invoker)\n" +
            "        {\n" +
            "            // TODO\n" +
            "\n" +
            "            // Propagate Invoke\n"+
            "            InvokeOutput(invoker);\n"+
            "        }\n"+
            "    }\n"+
            "}";
    }
}