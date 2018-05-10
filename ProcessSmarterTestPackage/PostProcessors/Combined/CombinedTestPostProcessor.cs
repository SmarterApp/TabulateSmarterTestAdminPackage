using System.Collections;
using System.Collections.Generic;
using NLog;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors.Combined
{
    public class CombinedTestPostProcessor : PostProcessor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CombinedTestPostProcessor(PackageType packageType, Processor processor) : base(packageType, processor)
        {
           
        }

        public override IList<ValidationError> GenerateErrors()
        {
            var errors = new List<ValidationError>();
            Logger.Debug("CombinedTestPostProcessor calling GenerateErrors().");
            //throw new System.NotImplementedException();
            return errors;
        }
    }
}