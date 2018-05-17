
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.Processors.Combined;
using ValidateSmarterTestPackage.Resources;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class CombinedItemTabulator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<IEnumerable<string>> ProcessResult(XPathNavigator navigator,
            CombinedTestProcessor testSpecificationProcessor, IDictionary<ItemFieldNames, string> testInformation)
        {
            var resultList = new List<List<string>>();

            var commonTestPackageItems = new SortedDictionary<int,string>();

            Logger.Debug($"I'm a AssessmentId {testInformation[ItemFieldNames.AssessmentId]}");
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentId, testInformation[ItemFieldNames.AssessmentId]);

            Logger.Debug($"I'm a AssessmentSubject {testInformation[ItemFieldNames.AssessmentSubject]}");
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentSubject, testInformation[ItemFieldNames.AssessmentSubject]);

            Logger.Debug($"I'm a AssessmentGrade {testInformation[ItemFieldNames.AssessmentGrade]}");
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentGrade, testInformation[ItemFieldNames.AssessmentGrade]);

            Logger.Debug($"I'm a AssessmentType {testInformation[ItemFieldNames.AssessmentType]}");
            commonTestPackageItems.Add((int)ItemFieldNames.AssessmentType, testInformation[ItemFieldNames.AssessmentType]);

            //Logger.Debug($"I'm a AssessmentVersion {testInformation[ItemFieldNames.AssessmentVersion]}");
            //commonTestPackageItems.Add((int)ItemFieldNames.AssessmentVersion, testInformation[ItemFieldNames.AssessmentVersion]);

            Logger.Debug($"I'm a AcademicYear {testInformation[ItemFieldNames.AcademicYear]}");
            commonTestPackageItems.Add((int)ItemFieldNames.AcademicYear, testInformation[ItemFieldNames.AcademicYear]);

            //commonTestPackageItems.Add((int)ItemFieldNames.FullItemKey, testInformation[ItemFieldNames.BankKey] + "-something");
            commonTestPackageItems.Add((int)ItemFieldNames.BankKey, testInformation[ItemFieldNames.BankKey]);


            
            

            var testPackage = testSpecificationProcessor.TestPackage;
            var tests = testPackage.Test;
            List<ItemGroupItem> testItems = new List<ItemGroupItem>();

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
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup.id);
                                }
                            }
                        }
                        else
                        {
                            var itemGroups = (segment.Item as TestSegmentPool)?.ItemGroup;
                            if (itemGroups != null)
                                foreach (var itemGroup in itemGroups)
                                {
                                    GetAssesmentItemList(test, itemGroup.Item.ToList(), commonTestPackageItems, testInformation, resultList, itemGroup.id);
                                }
                        }
                    }

                }
            }


            return resultList;
        }


        private void GetAssesmentItemList(Test test, List<ItemGroupItem> testItems, SortedDictionary<int, string> commonTestPackageItems, IDictionary<ItemFieldNames, string> testInformation, List<List<string>> resultList, String itemGroupId)
        {
            var subType = "summative";
            if (!testInformation[ItemFieldNames.AssessmentType].Equals("summative", StringComparison.OrdinalIgnoreCase))
            {
                if (test.id.Contains("ICA"))
                {
                    subType = "ICA";
                }
                else if (test.id.Contains("IAB"))
                {
                    subType = "IAB";
                }
                else
                {
                    subType = string.Empty;
                    
                }
            }
            
            foreach (var item in testItems)
            {
                var ids = GetStandardIDs(item, testInformation[ItemFieldNames.AssessmentSubject]);
                var newList = new SortedDictionary<int, string>(commonTestPackageItems)
                {
                    { (int)ItemFieldNames.AssessmentName, test.id},
                    { (int)ItemFieldNames.AssessmentLabel, test.label },
                    { (int)ItemFieldNames.AssessmentVersion, "what is this" },
                    { (int)ItemFieldNames.ItemId, item.id },
                    { (int)ItemFieldNames.FullItemKey, testInformation[ItemFieldNames.BankKey] + $"-{item.id}"},
                    { (int)ItemFieldNames.Filename,  $"item-{testInformation[ItemFieldNames.BankKey]}-{item.id}.xml"}, // item-200-21818.xml"
                    { (int)ItemFieldNames.Version, "what is this" },
                    { (int)ItemFieldNames.ItemType, item.type },
                    { (int)ItemFieldNames.AssessmentSubtype, subType },
                    { (int)ItemFieldNames.Grade, test.Grades[0].value },
                    { (int)ItemFieldNames.Standard, ids["Standard"]},
                    { (int)ItemFieldNames.Claim, ids["Claim"]},
                    { (int)ItemFieldNames.Target, ids["Target"]},
                    { (int)ItemFieldNames.PassageId, $"{testInformation[ItemFieldNames.BankKey]}-{itemGroupId}" },
                    { (int)ItemFieldNames.ASL, String.Empty } //wut?
                    //{ (int)ItemFieldNames.Braille,  }
                };
                resultList.Add(newList.Values.ToList());
            }
        }

        private Dictionary<string, string> GetStandardIDs(ItemGroupItem item, String subject)
        {
            var ids = new Dictionary<string, string>();
            foreach (var bpRef in item.BlueprintReferences)
            {
                if (subject.Equals("ELA", StringComparison.OrdinalIgnoreCase))
                {
                    if (bpRef.idRef.Contains("|"))
                    {
                        var parts = bpRef.idRef.Split('|');
                        ids.Add("Standard", $"SBAC-ELA-v1:{bpRef.idRef}");
                        ids.Add("Claim", parts[0] + "\t");
                        ids.Add("Target", parts[1] + "\t");
                       //return ids;
                    }
                } else if (subject.Equals("MATH", StringComparison.OrdinalIgnoreCase))
                {
                    var parts = bpRef.idRef.Split('|');
                    if (parts.Length == 4)
                    {
                        ids.Add("Standard", $"SBAC-MA-v6:{bpRef.idRef}");
                        ids.Add("Claim", parts[0] + "\t");
                        ids.Add("Target", parts[3] + "\t");
                        //return ids;
                    }
                }
            }

            if (ids.Count == 0)
            {
                ids.Add("Standard", String.Empty);
                ids.Add("Claim", String.Empty);
                ids.Add("Target", String.Empty);
            }

            return ids;
        }
    }
}