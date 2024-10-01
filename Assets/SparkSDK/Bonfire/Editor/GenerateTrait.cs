using System;

namespace DimX.SparkSDK
{
    public static class GenerateTrait
    {
        public static string Content =>
            "using System;\n" +
            "using DimX.Common.Assets.Types.Sparks;\n" +
            "using DimX.Common.Assets.Types.Sparks.Elements.Common;\n" +
            "\n" +
            "namespace Sparks.TODO\n" +
            "{\n" +
            "    /// <summary>\n" +
            "    /// TODO\n" +
            "    /// </summary>\n" +
            "    public class TraitTODO : Trait<float>\n" +
            "    {\n" +
            "        private readonly Spark _spark;\n" +
            "\n" +
            "        public TraitTODO(Spark spark) : base(\"TODO NAME\", 1f)\n" +
            "        {\n" +
            "            _spark = spark;\n" +
            "        }\n" +
            "\n" +
           $"        public override Guid Guid => new(\"{Guid.NewGuid()}\");\n" +
            "\n" +
            "        protected override float OnGet()\n" +
            "        {\n" +
            "            // TODO\n" +
            "            return _value;\n" +
            "        }\n" +
            "\n" +
            "        protected override void OnSet(float value)\n" +
            "        {\n" +
            "            // TODO\n" +
            "            _value = value;\n" +
            "        }\n" +
            "    }\n" +
            "}";
    }
}