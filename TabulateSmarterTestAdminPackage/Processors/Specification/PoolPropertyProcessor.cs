﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class PoolPropertyProcessor : Processor
    {
        private static readonly XPathExpression sXp_Property = XPathExpression.Compile("@property");
        private static readonly XPathExpression sXp_Value = XPathExpression.Compile("@value");
        private static readonly XPathExpression sXp_Label = XPathExpression.Compile("@label");
        private static readonly XPathExpression sXp_ItemCount = XPathExpression.Compile("@itemcount");

        private readonly XPathNavigator _navigator;

        internal PoolPropertyProcessor(XPathNavigator navigator)
        {
            _navigator = navigator;
        }

        private string Property { get; set; }
        private string Value { get; set; }
        private string Label { get; set; }
        private string ItemCount { get; set; }

        public override bool Process()
        {
            return IsValidProperty() 
                && IsValidValue() 
                && IsValidLabel() 
                && IsValidItemCount();
        }

        internal bool IsValidProperty()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 50)
            };
            Property = _navigator.Eval(sXp_Property);
            if (validators.IsValid(Property))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Property.Expression, validators.GetMessage());
            return false;
        }

        //TODO: Is there a defined restricted set here we could put into an enum?
        internal bool IsValidValue()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 128)
            };
            Value = _navigator.Eval(sXp_Value);
            if (validators.IsValid(Value))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Value.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidLabel()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            Label = _navigator.Eval(sXp_Label);
            if (validators.IsValid(Label))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_Label.Expression, validators.GetMessage());
            return false;
        }

        internal bool IsValidItemCount()
        {
            var validators = new ValidatorCollection
            {
                new RequiredIntValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 10),
                new MinIntValueValidator(ErrorSeverity.Degraded, 0)
            };
            ItemCount = _navigator.Eval(sXp_ItemCount);
            if (validators.IsValid(ItemCount))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(_navigator.NamespaceURI, sXp_ItemCount.Expression, validators.GetMessage());
            return false;
        }
    }
}