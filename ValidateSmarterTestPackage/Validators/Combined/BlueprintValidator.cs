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
            //throw new System.NotImplementedException();
        }

        private void validateEachBlueprintElement(TestPackage testPackage, List<ValidationError> errors, BlueprintElement[] blueprint)
        {
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
                                    Logger.Debug(errStr);
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
                            Logger.Debug(errStr);
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
                                    Logger.Debug(errStr);
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
                            Logger.Debug(errStr);
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
                        // Validate that a "claim" blueprint has only child claims or targets
                        if (bpEl.BlueprintElement1 != null)
                            foreach (var childBpel in bpEl.BlueprintElement1)
                            {
                                if (!childBpel.type.Equals(CombinedBlueprintElementTypes.TARGET,
                                    StringComparison.CurrentCultureIgnoreCase))
                                {
                                    errStr =
                                        "Child elements of a 'claim' or 'target' blueprint element should be of a 'target' blueprint element type.";
                                    Logger.Debug(errStr);
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
                            }

                        break;

                    case CombinedBlueprintElementTypes.STRAND:
                        errStr = $"A blueprint type of 'strand' was detected for the blueprint element '{bpEl.id}'. It is recommended that the blueprint " +
                                  "element type is renamed to 'claim'";
                        Logger.Debug(errStr);
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
                        Logger.Debug(errStr);
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
                        Logger.Debug(errStr);
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
                    Logger.Debug(errStr);
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
                Logger.Debug(errStr);
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
                Logger.Debug(errStr);
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