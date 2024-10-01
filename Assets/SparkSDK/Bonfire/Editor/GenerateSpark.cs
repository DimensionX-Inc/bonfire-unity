using System;

namespace DimX.SparkSDK
{
    public static class GenerateSpark
    {
        public static string Content =>
            "using DimX.Common.Assets.Types.Sparks.Entities;\n" +
            "\n" +
            "namespace Sparks.TODO\n" +
            "{\n" +
            "    /// <summary>\n" +
            "    /// TODO\n" +
            "    /// </summary>\n" +
            "    public class SparkTODO : Entity\n" +
            "    {\n" +
            "        protected override void Initialize()\n" +
            "        {\n" +
            "            base.Initialize();\n" +
            "\n" +
            "            // TODO Causes\n" +
            "            // AddCauseDefinition<CauseDemo>();\n" +
            "\n" +
            "            // TODO Effects\n" +
            "            // AddEffectDefinition<EffectDemo>();\n" +
            "\n" +
            "            // TODO Traits\n" +
            "            // AddTrait(typeof(SparkTODO), new TraitTODO(this));\n" +
            "        }\n" +
            "    }\n" +
            "}";
    }
}