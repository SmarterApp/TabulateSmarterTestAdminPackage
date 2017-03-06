using System.Collections.Generic;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public abstract class PostProcessor : IPostProcessor
    {
        protected PostProcessor(PackageType packageType, Processor processor)
        {
            Processor = processor;
            PackageType = packageType;
        }

        public Processor Processor { get; set; }

        public PackageType PackageType { get; }
        public abstract IList<ValidationError> GenerateErrors();
    }
}