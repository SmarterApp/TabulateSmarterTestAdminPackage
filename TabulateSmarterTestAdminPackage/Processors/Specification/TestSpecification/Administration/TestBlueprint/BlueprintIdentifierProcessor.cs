﻿using System.Xml.XPath;
using TabulateSmarterTestAdminPackage.Common.Enums;
using TabulateSmarterTestAdminPackage.Common.Utilities;
using TabulateSmarterTestAdminPackage.Common.Validators;

namespace TabulateSmarterTestAdminPackage.Processors.Specification.TestSpecification.Administration.TestBlueprint
{
    internal class BlueprintIdentifierProcessor : IdentifierProcessor
    {
        public BlueprintIdentifierProcessor(XPathNavigator navigator) : base(navigator) {}

        public override bool Process()
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

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_UniqueId.Expression,
                validators.GetMessage());
            return false;
        }

        internal new bool IsValidName()
        {
            var validators = new ValidatorCollection
            {
                new RequiredStringValidator(ErrorSeverity.Degraded),
                new MaxLengthValidator(ErrorSeverity.Degraded, 255)
            };
            Name = Navigator.Eval(sXp_Name);
            if (validators.IsValid(Name))
            {
                return true;
            }

            ReportingUtility.ReportSpecificationError(Navigator.NamespaceURI, sXp_Name.Expression,
                validators.GetMessage());
            return false;
        }
    }
}