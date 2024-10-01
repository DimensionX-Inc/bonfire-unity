using System;

namespace DimX.SparkSDK
{
    public static class GenerateCause
    {
        public static string Content =>
            "using DimX.Common.Assets.Types.Sparks;\n" +
            "using DimX.Common.Assets.Types.Sparks.Elements.Common;\n" +
            "using DimX.Common.Utilities;\n" +
            "\n" +
            "namespace Sparks.TODO\n" +
            "{\n" +
            "    /// <summary>\n" +
            "    /// TODO\n" +
            "    /// </summary>\n" + 
            "    [DisplayName(\"TODO Cause Name\")] \n" +
           $"    [Guid(\"{Guid.NewGuid()}\")] \n" +
            "    public class CauseTODO : Cause\n" +
            "    {\n" +
            "        private readonly Spark _spark;\n" +
            "\n" +
            "        public CauseTODO(Spark spark) : base(spark)\n" +
            "        {\n" +
            "            _spark = spark;\n" +
            "\n" +
            "            // TODO\n" +
            "            InvokeOutput(Spark);\n" +
            "        }\n" + 
            "    }\n" +
            "}\n";
    }
}