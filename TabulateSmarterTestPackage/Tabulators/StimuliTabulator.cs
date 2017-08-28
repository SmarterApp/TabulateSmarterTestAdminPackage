using System;
using System.Collections.Generic;
using System.Linq;
using ProcessSmarterTestPackage.Processors.Common.ItemPool.Passage;
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

                stimFields[(int) StimFieldNames.AssessmentName] = testInformation[ItemFieldNames.AssessmentName];
                stimFields[(int) StimFieldNames.AssessmentSubject] = testInformation[ItemFieldNames.AssessmentSubject];
                stimFields[(int) StimFieldNames.AssessmentGrade] = testInformation[ItemFieldNames.AssessmentGrade];
                stimFields[(int) StimFieldNames.AssessmentType] = testInformation[ItemFieldNames.AssessmentType];
                //stimFields[(int) StimFieldNames.StimuliId] =
                //    identifier.ValueForAttribute("uniqueid").Split('-').First();
                stimFields[(int)StimFieldNames.StimuliId] = identifier.ValueForAttribute("uniqueid").Split('-').Last();
                stimFields[(int) StimFieldNames.FileName] = passageProcessor.ValueForAttribute("filename");
                stimFields[(int) StimFieldNames.Version] = identifier.ValueForAttribute("version");

                resultList.Add(stimFields.ToList());
            }
            return resultList;
        }
    }
}