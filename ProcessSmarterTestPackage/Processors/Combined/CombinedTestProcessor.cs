using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using ProcessSmarterTestPackage.PostProcessors.Combined;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.Validators.Combined;
using NLog;

namespace ProcessSmarterTestPackage.Processors.Combined
{
    public class CombinedTestProcessor : Processor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public CombinedTestProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            //Processors.Add(new CombinedTestProcessor(Navigator, packageType));
        }

        public override List<ValidationError> AdditionalValidations()
        {
            //load and validate with XML schema
            try
            {
                XmlDocument validateDocument = new XmlDocument();
                validateDocument.LoadXml(Navigator.OuterXml);
                validateDocument.Schemas.Add(null, "Resources/TestPackageSchema.xsd"); //TODO can I put this in a setting or something?
                ValidationEventHandler validation = new ValidationEventHandler(SchemaValidationHandler);
                validateDocument.Validate(validation);
                Logger.Debug("New package type xml file loaded and validated against XML schema");

                //deserialize into class?
                XmlSerializer serializer = new XmlSerializer(typeof(TestPackage));
                var testPackage = (TestPackage)serializer.Deserialize(XmlReader.Create(new StringReader(Navigator.OuterXml)));

                //all the validators for the new format
                ItemGroupValidator itemGroupValidator = new ItemGroupValidator();
                AssessmentValidator assessmentValidator = new AssessmentValidator();
                BlueprintValidator blueprintValidator = new BlueprintValidator();
                ItemScoreDimensionValidator itemScoreDimensionValidator = new ItemScoreDimensionValidator();
                ItemValidator itemValidator = new ItemValidator();
                SegmentBlueprintValidator segmentBlueprintValidator = new SegmentBlueprintValidator();
                SegmentFormValidator segmentFormValidator = new SegmentFormValidator();
                SegmentValidator segmentValidator = new SegmentValidator();
                TestPackageRootValidator testPackageRootValidator = new TestPackageRootValidator();
                TestPackageValidator testPackageValidator = new TestPackageValidator();

                List<ValidationError> valErrs = new List<ValidationError>();

                itemGroupValidator.Validate(testPackage, valErrs);
                assessmentValidator.Validate(testPackage, valErrs);
                blueprintValidator.Validate(testPackage, valErrs);
                itemScoreDimensionValidator.Validate(testPackage, valErrs);
                itemValidator.Validate(testPackage, valErrs);
                segmentBlueprintValidator.Validate(testPackage, valErrs);
                segmentFormValidator.Validate(testPackage, valErrs);
                segmentValidator.Validate(testPackage, valErrs);
                testPackageRootValidator.Validate(testPackage, valErrs);
                testPackageValidator.Validate(testPackage, valErrs);

                if (valErrs.Count > 0)
                {
                    Logger.Debug("Post-schema validation issues found:");
                    foreach (var error in valErrs)
                    {
                        Logger.Debug(error.GeneratedMessage);
                    }
                }
                else
                {
                    Logger.Debug("No post-schema validation issues found:");
                }

                return valErrs;

            }
            catch (XmlException e)
            {
                Logger.Error("Schema Validation Error: {0}", e.Message);
                throw new ArgumentException("XML Validation Failure");
            }
           
        }

        static void SchemaValidationHandler(object sender, ValidationEventArgs e)
        {
            Logger.Debug("VALIDATION ERROR!!");
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Logger.Error("Schema Validation Error: {0}", e.Message);
                    throw new ArgumentException("XML Validation Failure");
                case XmlSeverityType.Warning:
                    Logger.Error("Schema Validation Warning: {0}", e.Message);
                    throw new ArgumentException("XML Validation Warning");
                default:
                    throw new ArgumentException("XML Validation Failure");
            }
        }

        public new string GetUniqueId()
        {
            return Navigator.SelectSingleNode("//Test")?.GetAttribute("id", string.Empty);
        }
    }
}