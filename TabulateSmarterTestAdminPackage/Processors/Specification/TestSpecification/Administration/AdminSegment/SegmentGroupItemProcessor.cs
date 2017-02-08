using System;
using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.AdminSegment
{
    internal class SegmentGroupItemProcessor : Processor
    {
        internal static readonly XPathExpression sXp_ItemId = XPathExpression.Compile("@itemid");
        internal static readonly XPathExpression sXp_GroupPosition = XPathExpression.Compile("@groupposition");
        internal static readonly XPathExpression sXp_AdminRequired = XPathExpression.Compile("@adminrequired");
        private static readonly XPathExpression sXp_ResponseRequired = XPathExpression.Compile("@responserequired");
        internal static readonly XPathExpression sXp_IsActive = XPathExpression.Compile("@isactive");
        internal static readonly XPathExpression sXp_IsFieldTest = XPathExpression.Compile("@isfieldtest");
        private static readonly XPathExpression sXp_BlockId = XPathExpression.Compile("@blockid");

        private readonly XPathNavigator _navigator;

        internal SegmentGroupItemProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string ItemId { get; set; }
        private string GroupPosition { get; set; }
        private string AdminRequired { get; set; }
        private string ResponseRequired { get; set; }
        private string IsActive { get; set; }
        private string IsFieldTest { get; set; }
        private string BlockId { get; set; }

        public override bool Process()
        {
            throw new NotImplementedException();
        }

        internal bool IsValidItemId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            ItemId = _navigator.Eval(sXp_ItemId);
            if (validators.IsValid(ItemId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ItemId.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidGroupPositionn()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 1)
            };
            GroupPosition = _navigator.Eval(sXp_GroupPosition);
            if (validators.IsValid(GroupPosition))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_GroupPosition.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidAdminRequired()
        {
            var validators = new ValidatorCollection
            {
                new RequiredBooleanValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 5)
            };
            AdminRequired = _navigator.Eval(sXp_AdminRequired);
            if (validators.IsValid(AdminRequired))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_AdminRequired.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidResponseRequired()
        {
            var validators = new ValidatorCollection
            {
                new RequiredBooleanValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 5)
            };
            ResponseRequired = _navigator.Eval(sXp_ResponseRequired);
            if (validators.IsValid(ResponseRequired))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ResponseRequired.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidIsActive()
        {
            var validators = new ValidatorCollection
            {
                new RequiredBooleanValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 5)
            };
            IsActive = _navigator.Eval(sXp_IsActive);
            if (validators.IsValid(IsActive))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_IsActive.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidIsFieldTest()
        {
            var validators = new ValidatorCollection
            {
                new RequiredBooleanValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 5)
            };
            IsFieldTest = _navigator.Eval(sXp_IsFieldTest);
            if (validators.IsValid(IsFieldTest))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_IsFieldTest.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidBlockId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10)
            };
            BlockId = _navigator.Eval(sXp_BlockId);
            if (validators.IsValid(BlockId))
            {
                return true;
            }
            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_BlockId.Expression, validators.GetMessage());
            return false;
        }
    }
}