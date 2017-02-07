using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint
{
    internal class BlueprintElementProcessor : Processor
    {
        internal static readonly XPathExpression sXp_ElementType = XPathExpression.Compile("@elementtype");
        internal static readonly XPathExpression sXp_MinOpItems = XPathExpression.Compile("@minopitems");
        internal static readonly XPathExpression sXp_MaxOpItems = XPathExpression.Compile("@maxopitems");
        internal static readonly XPathExpression sXp_MinFtItems = XPathExpression.Compile("@minftitems");
        internal static readonly XPathExpression sXp_MaxFtItems = XPathExpression.Compile("@maxftitems");
        internal static readonly XPathExpression sXp_OpItemCount = XPathExpression.Compile("@opitemcount");
        internal static readonly XPathExpression sXp_FtItemCount = XPathExpression.Compile("@ftitemcount");
        internal static readonly XPathExpression sXp_ParentId = XPathExpression.Compile("@parentid");

        private readonly XPathNavigator _navigator;

        internal BlueprintElementProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
            IdentifierProcessor = new BlueprintIdentifierProcessor(navigator.SelectSingleNode("identifier"));
        }

        private BlueprintIdentifierProcessor IdentifierProcessor { get; }
        private string ElementType { get; set; }
        private string MinOpItems { get; set; }
        private string MaxOpItems { get; set; }
        private string MinFtItems { get; set; }
        private string MaxFtItems { get; set; }
        private string OpItemCount { get; set; }
        private string FtItemCount { get; set; }
        private string ParentId { get; set; }

        public override bool Process()
        {
            return IsValidElementType()
                   && IsValidMinOpItems()
                   && IsValidMaxOpItems()
                   && IsMinOpItemsLessThanMaxOpItems()
                   && IsValidMinFtItems()
                   && IsValidMaxFtItems()
                   && IsMinFtItemsLessThanMaxFtItems()
                   && IsValidOpItemCount()
                   && IsValidFtItemCount()
                   && IsValidParentId()
                   && IdentifierProcessor.Process();
        }

        //TODO: Restrict to enum values
        internal bool IsValidElementType()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 100)
            };
            ElementType = _navigator.Eval(sXp_ElementType);
            if (validators.IsValid(ElementType))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ElementType.Expression, validators.GetMessage());
            return false;
        }

        // Not Required
        //TODO: Check to make sure that items with this property reference actual parent items that exist
        internal bool IsValidParentId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 150)
            };
            ParentId = _navigator.Eval(sXp_ParentId);
            if (ParentId == null || validators.IsValid(ParentId))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ParentId.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidMinOpItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 4),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            MinOpItems = _navigator.Eval(sXp_MinOpItems);
            if (validators.IsValid(MinOpItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MinOpItems.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidMaxOpItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 4),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            MaxOpItems = _navigator.Eval(sXp_MaxOpItems);
            if (validators.IsValid(MaxOpItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxOpItems.Expression, validators.GetMessage());
            return false;
        }

        // Ensure that MaxOpItems is >= MinOpItems
        // TODO: Establish and evaluate Min/Max hierarchy
        internal bool IsMinOpItemsLessThanMaxOpItems()
        {
            var validators = new ValidatorCollection
            {
                new MinIntValueValidator(ErrorSeverity.Degraded, MinOpItems)
            };
            if (validators.IsValid(MaxOpItems))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxOpItems.Expression, validators.GetMessage());
            return false;
        }

        // Not Required
        internal bool IsValidMinFtItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 4),
                new MinIntValueValidator(ErrorSeverity.Benign, 0)
            };
            MinFtItems = _navigator.Eval(sXp_MinFtItems);
            if (MinFtItems == null || validators.IsValid(MinFtItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MinFtItems.Expression, validators.GetMessage());
            return false;
        }

        // Not Required
        internal bool IsValidMaxFtItems()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Benign),
                new MaxLengthValidator(ErrorSeverity.Benign, 4),
                new MinIntValueValidator(ErrorSeverity.Benign, 1)
            };
            MaxFtItems = _navigator.Eval(sXp_MaxFtItems);
            if (MaxFtItems == null || validators.IsValid(MaxFtItems))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxFtItems.Expression, validators.GetMessage());
            return false;
        }

        // Not Required
        // Ensure that MaxFtItems is >= MinFtItems
        // TODO: Establish and evaluate Min/Max hierarchy
        internal bool IsMinFtItemsLessThanMaxFtItems()
        {
            var validators = new ValidatorCollection
            {
                new MinIntValueValidator(ErrorSeverity.Degraded, MinFtItems)
            };
            // If they're both null, they weren't included and are valid
            if ((string.IsNullOrEmpty(MinFtItems)
                 && string.IsNullOrEmpty(MaxFtItems))
                || validators.IsValid(MaxFtItems))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_MaxFtItems.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidOpItemCount()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 4),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            OpItemCount = _navigator.Eval(sXp_OpItemCount);
            if (validators.IsValid(OpItemCount))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_OpItemCount.Expression, validators.GetMessage());
            return false;
        }

        // TODO: This value is different from spec
        internal bool IsValidFtItemCount()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 4),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            FtItemCount = _navigator.Eval(sXp_FtItemCount);
            if (validators.IsValid(FtItemCount))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_FtItemCount.Expression, validators.GetMessage());
            return false;
        }
    }
}