
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using NLog;
using ProcessSmarterTestPackage.Processors.Combined;
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

            var items = new SortedDictionary<int,string>();

            Logger.Debug($"I'm a AssessmentId {testInformation[ItemFieldNames.AssessmentId]}");
            items.Add((int)ItemFieldNames.AssessmentId, testInformation[ItemFieldNames.AssessmentId]);

            Logger.Debug($"I'm a AssessmentName {testInformation[ItemFieldNames.AssessmentName]}");
            items.Add((int)ItemFieldNames.AssessmentName, testInformation[ItemFieldNames.AssessmentName]);

            Logger.Debug($"I'm a AssessmentSubject {testInformation[ItemFieldNames.AssessmentSubject]}");
            items.Add((int)ItemFieldNames.AssessmentSubject, testInformation[ItemFieldNames.AssessmentSubject]);

            Logger.Debug($"I'm a AssessmentGrade {testInformation[ItemFieldNames.AssessmentGrade]}");
            items.Add((int)ItemFieldNames.AssessmentGrade, testInformation[ItemFieldNames.AssessmentGrade]);

            Logger.Debug($"I'm a AssessmentType {testInformation[ItemFieldNames.AssessmentType]}");
            items.Add((int)ItemFieldNames.AssessmentType, testInformation[ItemFieldNames.AssessmentType]);

            Logger.Debug($"I'm a AssessmentSubtype {testInformation[ItemFieldNames.AssessmentSubtype]}");
            items.Add((int)ItemFieldNames.AssessmentSubtype, testInformation[ItemFieldNames.AssessmentSubtype]);

            Logger.Debug($"I'm a AssessmentLabel {testInformation[ItemFieldNames.AssessmentLabel]} {(int)ItemFieldNames.AssessmentLabel} items count is {items.Count}");
            items.Add((int)ItemFieldNames.AssessmentLabel, testInformation[ItemFieldNames.AssessmentLabel]);

            Logger.Debug($"I'm a AssessmentVersion {testInformation[ItemFieldNames.AssessmentVersion]}");
            items.Add((int)ItemFieldNames.AssessmentVersion, testInformation[ItemFieldNames.AssessmentVersion]);

            Logger.Debug($"I'm a AcademicYear {testInformation[ItemFieldNames.AcademicYear]}");
            items.Add((int)ItemFieldNames.AcademicYear, testInformation[ItemFieldNames.AcademicYear]);


            var thing = items.Values;
            Logger.Debug(thing.ToList());

            resultList.Add(thing.ToList());
            return resultList;
        }

        private IEnumerable<IEnumerable<string>> GetAssesmentItems()
        {
            var asItems = new List<List<string>>();


            return asItems;
        }
    }
}