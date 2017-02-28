using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
using TabulateSmarterTestPackage.Utilities;
using ValidateSmarterTestPackage.RestrictedValues.Enums;

namespace TabulateSmarterTestPackage.Tabulators
{
    public class StimuliTabulator
    {
        public IEnumerable<IEnumerable<string>> ProcessResult(
            IList<PassageProcessor> passageProcessors, IDictionary<ItemFieldNames, string> testInformation)
        {
            var resultList = new List<List<string>>();

            foreach (var passageProcessor in passageProcessors)
            {
                var identifier = passageProcessor.ChildNodeWithName("identifier");
                var stimFields = new string[Enum.GetNames(typeof(StimFieldNames)).Length];

                stimFields[(int) StimFieldNames.TestName] = testInformation[ItemFieldNames.AssessmentName];
                stimFields[(int) StimFieldNames.TestSubject] = testInformation[ItemFieldNames.AssessmentSubject];
                stimFields[(int) StimFieldNames.TestGrade] = testInformation[ItemFieldNames.AssessmentGrade];
                stimFields[(int) StimFieldNames.TestType] = testInformation[ItemFieldNames.AssessmentType];
                stimFields[(int) StimFieldNames.StimId] = FormatHelper.Strip200(identifier.ValueForAttribute("uniqueid"));
                stimFields[(int) StimFieldNames.Filename] = passageProcessor.ValueForAttribute("filename");
                stimFields[(int) StimFieldNames.Version] = identifier.ValueForAttribute("version");

                resultList.Add(stimFields.ToList());
            }
            return resultList;
        }
    }
}