using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace ValidateSmarterTestPackage.Validators.Combined
{
    public class AssessmentValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            ValidateAssessmentIdLength(testPackage, errors);
            ValidateAssessmentsHaveAssociatedBlueprintElement(testPackage, errors);
        }

        // Validates that <Test id=?> ? does not exceed 250 chars and is not null
        private void ValidateAssessmentsHaveAssociatedBlueprintElement(TestPackage testPackage, List<ValidationError> errors)
        {
            var maxLengthValidator = new MaxLengthValidator(ErrorSeverity.Severe, 250);
            foreach (var test in testPackage.Test)
            {                
                Logger.Debug("length is " + ((string)test.id).Length);
                if (test.id != null && !maxLengthValidator.IsValid(test.id))
                {
                    var errStr =
                        $"Cannot have a test id ({test.id}) that exceeds 250 characters.";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Severe,
                        Location = "Test",
                        GeneratedMessage = errStr,
                        ItemId = test.id,
                        Key = "Test",
                        PackageType = PackageType.Combined,
                        Value = test.id
                    });
                }
            }
        }

        private void ValidateAssessmentIdLength(TestPackage testPackage, List<ValidationError> errors)
        {
            
            bool isMultiTestPackage = testPackage.Test.Length > 1;
            Logger.Debug($"isMultiTestPackage={isMultiTestPackage}");
            if (isMultiTestPackage)
            {
                
                foreach (var test in testPackage.Test)
                {
                    var maybeTestBpel = false;
                    foreach (var bpEl in testPackage.Blueprint)
                    {
                        if (bpEl.type.Equals(CombinedBlueprintElementTypes.PACKAGE,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var testBpEl in bpEl.BlueprintElement1)
                            {
                                if (testBpEl.type.Equals(CombinedBlueprintElementTypes.TEST,
                                        StringComparison.CurrentCultureIgnoreCase) && testBpEl.id.Equals(test.id,
                                        StringComparison.CurrentCultureIgnoreCase))
                                {
                                    maybeTestBpel = true;
                                }
                            }
                        }
                    }

                    if (!maybeTestBpel)
                    {
                        var errStr =
                            $"A 'test' BlueprintElement was not found in the test blueprint for the test with id {test.id}";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "Test",
                            GeneratedMessage = errStr,
                            ItemId = test.id,
                            Key = "Test",
                            PackageType = PackageType.Combined,
                            Value = test.id
                        });
                    }
                }
            }
            else
            {
                foreach(var test in testPackage.Test)
                {
                    var maybeTestBpel = false;
                    foreach (var bpEl in testPackage.Blueprint)
                    {
                        if (bpEl.type.Equals(CombinedBlueprintElementTypes.TEST,
                                StringComparison.CurrentCultureIgnoreCase) &&
                            bpEl.id.Equals(test.id, StringComparison.CurrentCultureIgnoreCase))
                        {
                            maybeTestBpel = true;
                        }
                    }
                    if (!maybeTestBpel)
                    {
                        var errStr =
                            $"A 'test' BlueprintElement was not found in the test blueprint for the test with id {test.id}";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Severe,
                            Location = "Test",
                            GeneratedMessage = errStr,
                            ItemId = test.id,
                            Key = "Test",
                            PackageType = PackageType.Combined,
                            Value = test.id
                        });
                    }
                }
            }
        }
    }
}
