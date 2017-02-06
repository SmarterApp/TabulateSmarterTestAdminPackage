﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Validators;
using TabulateSmarterTestAdminPackage.Utility;

namespace TabulateSmarterTestAdminPackage.Processors.Specification
{
    internal class TestItemIdentifierProcessor : IdentifierProcessor
    {
        public TestItemIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        internal new bool Process()
        {
            return IsValidUniqueId()
                   && IsValidName()
                   && IsValidVersion();
        }

        internal new bool IsValidUniqueId()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 150)
            };
            UniqueId = Navigator.Eval(sXp_UniqueId);
            if (validators.IsValid(UniqueId))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression, validators.GetMessage());
            return false;
        }

        internal new bool IsValidName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 80)
            };
            Name = Navigator.Eval(sXp_Name);
            if (validators.IsValid(Name))
            {
                return true;
            }

            AdminPackageUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Name.Expression, validators.GetMessage());
            return false;
        }
    }
}