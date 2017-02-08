using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.ItemPool.Passage
{
    internal class PassageProcessor : Processor
    {
        internal static readonly XPathExpression sXp_FileName = XPathExpression.Compile("@filename");

        internal PassageProcessor(XPathNavigator navigator) : base(navigator)
        {
            PassageIdentifierProcessor = new PassageIdentifierProcessor(navigator.SelectSingleNode("identifier"));
        }

        private string FileName { get; set; }
        private PassageIdentifierProcessor PassageIdentifierProcessor { get; }

        public override bool Process()
        {
            return IsValidFileName()
                   && PassageIdentifierProcessor.Process();
        }

        internal bool IsValidFileName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 50),
                new FilePathValidator(ErrorSeverity.Degraded)
            };
            FileName = Navigator.Eval(sXp_FileName);
            if (validators.IsValid(FileName))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_FileName.Expression,
                validators.GetMessage());
            return false;
        }
    }
}