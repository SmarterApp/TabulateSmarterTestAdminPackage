using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml.XPath;
using ProcessSmarterTestPackage.Processors.Common;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Validators.Combined;
using ValidateSmarterTestPackage.RestrictedValues.Enums;
using NLog;
using ProcessSmarterTestPackage.Processors.Administration.AdminSegment;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.TestItem;

namespace ProcessSmarterTestPackage.Processors.Combined
{
    public class CombinedTestProcessor : Processor
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public TestPackage TestPackage { get; set; }

        public CombinedTestProcessor(XPathNavigator navigator, PackageType packageType)
            : base(navigator, packageType)
        {
            XmlDocument validateDocument = new XmlDocument();
            validateDocument.LoadXml(Navigator.OuterXml);
            validateDocument.Schemas.Add(null, "Resources/TestPackageSchema.xsd");
            ValidationEventHandler validation = SchemaValidationHandler;
            validateDocument.Validate(validation);
            Logger.Info("New format xml file loaded and validated against XML schema");

            //deserialize into class
            XmlSerializer serializer = new XmlSerializer(typeof(TestPackage));
            var testPackage = (TestPackage)serializer.Deserialize(XmlReader.Create(new StringReader(Navigator.OuterXml)));
            TestPackage = testPackage;
        }

        public List<ItemGroupItem> GetItems()
        {
            var tests = TestPackage.Test;
            List<ItemGroupItem> items = new List<ItemGroupItem>();
            foreach (var test in tests)
            {
                foreach (var segment in test.Segments)
                {
                    var segmentForms =
                        segment.Item is TestSegmentSegmentForms forms ? forms.SegmentForm : null;
                    if (segmentForms != null)
                    {
                        if (segment.algorithmType.Equals(Algorithm.FIXEDFORM,
                            StringComparison.CurrentCultureIgnoreCase))
                        {
                            foreach (var segmentForm in segmentForms)
                            {
                                foreach (var itemGroup in segmentForm.ItemGroup)
                                {
                                    items.AddRange(itemGroup.Item.ToList());
                                }
                            }
                        }
                        else
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    items.AddRange(itemGroup.Item.ToList());
                                }
                        }
                    }
                }
            }

            return items;
        }

        public List<Processor> GetItemsAsProcessors()
        {
            var items = this.GetItems();
            var processors = new List<Processor>();
            foreach (var item in items)
            {
                var node = Navigator.SelectSingleNode($"//Item[@id='{item.id}']");
                processors.Add(new TestItemProcessor(node, PackageType.Combined));
            }

            return processors;
        }

        protected override List<ValidationError> AdditionalValidations()
        {
            //load and validate with XML schema
            try
            {
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
                ToolsValidator toolsValidator = new ToolsValidator();

                List<ValidationError> valErrs = new List<ValidationError>();

                itemGroupValidator.Validate(TestPackage, valErrs);
                assessmentValidator.Validate(TestPackage, valErrs);
                blueprintValidator.Validate(TestPackage, valErrs);
                itemScoreDimensionValidator.Validate(TestPackage, valErrs);
                itemValidator.Validate(TestPackage, valErrs);
                segmentBlueprintValidator.Validate(TestPackage, valErrs);
                segmentFormValidator.Validate(TestPackage, valErrs);
                segmentValidator.Validate(TestPackage, valErrs);
                testPackageRootValidator.Validate(TestPackage, valErrs);
                toolsValidator.Validate(TestPackage, valErrs);

                if (valErrs.Count > 0)
                {
                    Logger.Error("Post-schema validation issues found:");
                    foreach (var error in valErrs)
                    {
                        Logger.Error(error.GeneratedMessage);
                    }
                }
                else
                {
                    Logger.Info("No post-schema validation issues found:");
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
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    Logger.Error("Schema Validation Error: {0}", e.Message);
                    throw new ArgumentException("XML Validation Failure");
                case XmlSeverityType.Warning:
                    Logger.Error("Schema Validation Warning: {0}", e.Message);
                    throw new ArgumentException("XML Validation Warning");
                default:
                    Logger.Error("Schema Validation Error: {0}", e.Message);
                    throw new ArgumentException("XML Validation Failure");
            }
        }

        public new string GetUniqueId()
        {
            return Navigator.SelectSingleNode("//Test")?.GetAttribute("id", string.Empty);
        }
    }
}