using System;
using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class BlueprintValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly string[] Groups = new string[]
        {
            CombinedBlueprintElementTypes.AFFINITY_GROUP, CombinedBlueprintElementTypes.CLAIM, CombinedBlueprintElementTypes.PACKAGE, CombinedBlueprintElementTypes.SOCK, CombinedBlueprintElementTypes.TEST
        };
        private static HashSet<string> ROOT_BLUEPRINT_ELEMENT_TYPES = new HashSet<string>(Groups);

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            ValidateTopLevelBlueprintElements(testPackage, errors);
            ValidateOnlyOneTopLevelTestOrPackageBlueprintElement(testPackage, errors);
            validateEachBlueprintElement(testPackage, errors, testPackage.Blueprint);
        }

        private void validateEachBlueprintElement(TestPackage testPackage, List<ValidationError> errors, BlueprintElement[] blueprint)
        {
            string subject = testPackage.subject;
            string grade = testPackage.Test[0].Grades[0].value;

            foreach (var bpEl in blueprint)
            {
                string errStr;
                switch (bpEl.type)
                {

                    case CombinedBlueprintElementTypes.PACKAGE:
                        // Validate that a "package" blueprint has only child test blueprint elements
                        if (bpEl.BlueprintElement1 != null)
                            foreach (var childBlueprintElement in bpEl.BlueprintElement1)
                            {
                                if (!childBlueprintElement.type.Equals(CombinedBlueprintElementTypes.TEST,
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    errStr =
                                        "Child elements of a 'package' blueprint element should be of a 'test' blueprint element type";
                                    errors.Add(new ValidationError
                                    {
                                        ErrorSeverity = ErrorSeverity.Severe,
                                        Location = "TestPackage/Blueprint/BlueprintElement",
                                        GeneratedMessage = errStr,
                                        ItemId = childBlueprintElement.id,
                                        Key = "BlueprintElement",
                                        PackageType = PackageType.Combined,
                                        Value = childBlueprintElement.type
                                    });
                                }
                            }

                        break;

                    case CombinedBlueprintElementTypes.TEST:
                        // Validate that a "test" blueprint element has a corresponding Test element
                        bool hasTest = false;
                        foreach (var test in testPackage.Test)
                        {
                            if (test.id.Equals(bpEl.id, StringComparison.CurrentCultureIgnoreCase))
                            {
                                hasTest = true;
                                break;
                            }
                        }

                        if (!hasTest)
                        {
                            errStr =
                                $"A 'test' type blueprint element with the id {bpEl.id} was identified, but no corresponding " +
                                "<Test> element was identified with a matching id.";
                            errors.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Severe,
                                Location = "TestPackage/Blueprint/BlueprintElement",
                                GeneratedMessage = errStr,
                                ItemId = bpEl.id,
                                Key = "BlueprintElement",
                                PackageType = PackageType.Combined,
                                Value = bpEl.id
                            });
                        }

                        // Validate that a "test" blueprint has only child segment
                        if (bpEl.BlueprintElement1 != null)
                            foreach (var segBpel in bpEl.BlueprintElement1)
                            {
                                if (!segBpel.type.Equals(CombinedBlueprintElementTypes.SEGMENT,
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    errStr =
                                        "Child elements of a 'test' blueprint element should be of a 'segment' blueprint element type.";
                                    errors.Add(new ValidationError
                                    {
                                        ErrorSeverity = ErrorSeverity.Severe,
                                        Location = "TestPackage/Blueprint/BlueprintElement",
                                        GeneratedMessage = errStr,
                                        ItemId = segBpel.id,
                                        Key = "BlueprintElement",
                                        PackageType = PackageType.Combined,
                                        Value = segBpel.id
                                    });
                                }
                            }

                        break;

                    case CombinedBlueprintElementTypes.SEGMENT:
                        // Validate that a "segment" blueprint element has a corresponding Segment element
                        bool hasSegment = false;
                        foreach (var test in testPackage.Test)
                        {
                            foreach (var segment in test.Segments)
                            {
                                if (segment.id.Equals(bpEl.id, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    hasSegment = true;
                                }
                            }
                        }

                        if (!hasSegment)
                        {
                            errStr = $"A 'segment' type blueprint element with the id '{bpEl.id}' was identified, but no corresponding " +
                                     "<Segment> element was identified with a matching id";
                            errors.Add(new ValidationError
                            {
                                ErrorSeverity = ErrorSeverity.Severe,
                                Location = "TestPackage/Blueprint/BlueprintElement",
                                GeneratedMessage = errStr,
                                ItemId = bpEl.id,
                                Key = "BlueprintElement",
                                PackageType = PackageType.Combined,
                                Value = bpEl.id
                            });
                        }
                        break;

                    case CombinedBlueprintElementTypes.CLAIM:
                    case CombinedBlueprintElementTypes.TARGET:
                        // Validate content spec IDs contained in this claim blueprint element, and all the nested target blueprint elements.
                        validateContentSpecId(bpEl, subject, grade, errors);

                        // Validate that a "claim" blueprint has only child claims or targets
                        if (bpEl.BlueprintElement1 != null)
                        {
                            foreach (var childBpel in bpEl.BlueprintElement1)
                            {
                                if (!childBpel.type.Equals(CombinedBlueprintElementTypes.TARGET,
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    errStr =
                                        "Child elements of a 'claim' or 'target' blueprint element should be of a 'target' blueprint element type.";
                                    errors.Add(new ValidationError
                                    {
                                        ErrorSeverity = ErrorSeverity.Severe,
                                        Location = "TestPackage/Blueprint/BlueprintElement",
                                        GeneratedMessage = errStr,
                                        ItemId = bpEl.id,
                                        Key = "Label",
                                        PackageType = PackageType.Combined,
                                        Value = bpEl.id
                                    });
                                }
                            }
                        }

                        break;

                    case CombinedBlueprintElementTypes.STRAND:
                        errStr = $"A blueprint type of 'strand' was detected for the blueprint element '{bpEl.id}'. It is recommended that the blueprint " +
                                  "element type is renamed to 'claim'";
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            Location = "TestPackage/Blueprint/BlueprintElement",
                            GeneratedMessage = errStr,
                            ItemId = bpEl.id,
                            Key = "BlueprintElement",
                            PackageType = PackageType.Combined,
                            Value = bpEl.id
                        });
                        break;

                    case CombinedBlueprintElementTypes.CONTENT_LEVEL:
                        errStr = $"A blueprint type of 'contentlevel' was detected for the blueprint element '{bpEl.id}'. It is recommended that the blueprint " +
                                 "element type is renamed to 'target'";
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            Location = "TestPackage/Blueprint/BlueprintElement",
                            GeneratedMessage = errStr,
                            ItemId = bpEl.id,
                            Key = "BlueprintElement",
                            PackageType = PackageType.Combined,
                            Value = bpEl.type
                        });
                        break;

                    case CombinedBlueprintElementTypes.AFFINITY_GROUP:
                    case CombinedBlueprintElementTypes.SOCK:
                        //Nothing to validate
                        break;
                    default:
                        errStr = $"Unknown blueprint element type '{bpEl.type}' detected for id {bpEl.id}.";
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign, 
                            Location = "TestPackage/Blueprint/BlueprintElement",
                            GeneratedMessage = errStr,
                            ItemId = bpEl.id,
                            Key = "BlueprintElement",
                            PackageType = PackageType.Combined,
                            Value = bpEl.type
                        });
                        break;
                }
            }
        }

        private void validateContentSpecId(BlueprintElement bpEl, string subject, string grade, List<ValidationError> errors)
        {
            var defaultGrade = SmarterApp.ContentSpecId.ParseGrade(grade);
            string fullId = bpEl.id;

            if (subject.Equals("ELA", StringComparison.OrdinalIgnoreCase))
            {
                fullId = $"SBAC-ELA-v1:{bpEl.id}";
            }
            else if (subject.Equals("MATH", StringComparison.OrdinalIgnoreCase))
            {
                fullId = $"SBAC-MA-v6:{bpEl.id}";
            }

            SmarterApp.ContentSpecId csid = SmarterApp.ContentSpecId.TryParse(fullId, defaultGrade);
            if (csid.ParseErrorSeverity == SmarterApp.ErrorSeverity.Invalid)
            {
                // Blueprint element's id cannot be parsed via content spec library
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = $"ContentSpec rule: invalid blueprint id '{bpEl.id}' ({fullId}) : {csid.ParseErrorDescription}",
                    ItemId = bpEl.id,
                    Key = "id",
                    PackageType = PackageType.Combined,
                    Value = bpEl.id
                });
            }

            if (string.IsNullOrEmpty(bpEl.label))
            {
                // Blueprint element's label field has not been set
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = $"ContentSpec rule: label not populated for blueprint id '{bpEl.id}'",
                    ItemId = bpEl.id,
                    Key = "label",
                    PackageType = PackageType.Combined,
                    Value = bpEl.label
                });
            }
            else if (csid.ParseErrorSeverity == SmarterApp.ErrorSeverity.Invalid)
            {
                // Blueprint element's label field not consistent with id because id field is not parsable as a content spec Id
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = $"ContentSpec rule: blueprint label does not match id for blueprint id '{bpEl.id}'. Label is '{bpEl.label}', but id cannot be parsed: {csid.ParseErrorDescription}",
                    ItemId = bpEl.id,
                    Key = "label",
                    PackageType = PackageType.Combined,
                    Value = bpEl.label
                });
            }
            else if (!string.Equals(bpEl.label, csid.ToString(SmarterApp.ContentSpecIdFormat.Enhanced), StringComparison.Ordinal))
            {
                // Blueprint element's label field not consistent with id because label does not match converted value of the id
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = $"ContentSpec rule: blueprint label does not match id for blueprint id '{bpEl.id}'. Label is '{bpEl.label}', " +
                                        $"but id is equivalent to '{csid.ToString(SmarterApp.ContentSpecIdFormat.Enhanced)}'",
                    ItemId = bpEl.id,
                    Key = "label",
                    PackageType = PackageType.Combined,
                    Value = bpEl.label
                });
            }

            // Recurse through nested target elements
            if (bpEl.BlueprintElement1 != null)
            {
                foreach (var childBpel in bpEl.BlueprintElement1)
                {
                    if (childBpel.type.Equals(CombinedBlueprintElementTypes.TARGET,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        validateContentSpecId(childBpel, subject, grade, errors);
                    }
                }
            }
        }
         
        // <Blueprint type=?> ? must be one of the types in ROOT_BLUEPRINT_ELEMENT_TYPES
        private void ValidateTopLevelBlueprintElements(TestPackage testPackage, List<ValidationError> errors)
        {
            foreach (var blueprintElement in testPackage.Blueprint)
            {
                if (!ROOT_BLUEPRINT_ELEMENT_TYPES.Contains(blueprintElement.type))
                {
                    var errStr =
                        $"A top-level blueprint element type of {blueprintElement.type} was detected for blueprint element with id of '{blueprintElement.id}'." + 
                            $" Recognized top-level blueprint element types include: '{String.Join(", ", ROOT_BLUEPRINT_ELEMENT_TYPES)}'.";
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "TestPackage/Blueprint/BlueprintElement",
                        GeneratedMessage = errStr,
                        ItemId = blueprintElement.id,
                        Key = "BlueprintElement",
                        PackageType = PackageType.Combined,
                        Value = blueprintElement.id
                    });
                }
            }
        }

        // Validate that there is either a single "package" or "test" blueprint element type at the top level
        private void ValidateOnlyOneTopLevelTestOrPackageBlueprintElement(TestPackage testPackage,
            List<ValidationError> errors)
        {
            List<BlueprintElement> testOrPackageBlueprintElements = new List<BlueprintElement>();
            foreach (var blueprintElement in testPackage.Blueprint)
            {
                if (blueprintElement.type.Equals(CombinedBlueprintElementTypes.TEST, StringComparison.CurrentCultureIgnoreCase) ||
                    blueprintElement.type.Equals(CombinedBlueprintElementTypes.PACKAGE, StringComparison.CurrentCultureIgnoreCase))
                {
                    testOrPackageBlueprintElements.Add(blueprintElement);
                } 
            }

            if (testOrPackageBlueprintElements.Count <= 0)
            {
                var errStr =
                    "The test package did not contain a 'package' or 'test' top-level blueprint element. For " +
                    "combined multi-test packages, a 'package' blueprint element containing the 'test' blueprint elements is needed. " +
                    "For single-test packages, a top level 'test' blueprint element is required.";
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = errStr,
                    ItemId = "none",
                    Key = "BlueprintElement",
                    PackageType = PackageType.Combined,
                    Value = "none"
                });
            } else if (testOrPackageBlueprintElements.Count > 1)
            {
                var errStr =
                    "The test package contained more than one top-level 'test' or 'package' blueprint element. " +
                    "For combined multi-test packages, a 'package' blueprint element containing the 'test' blueprint elements is needed. " +
                    "For single-test packages, a top level 'test' blueprint element is required.";
                errors.Add(new ValidationError
                {
                    ErrorSeverity = ErrorSeverity.Severe,
                    Location = "TestPackage/Blueprint/BlueprintElement",
                    GeneratedMessage = errStr,
                    ItemId = "none",
                    Key = "BlueprintElement",
                    PackageType = PackageType.Combined,
                    Value = "none"
                });
            }
        }
    }
}