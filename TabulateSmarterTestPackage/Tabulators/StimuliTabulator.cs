using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
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
                stimFields[(int) StimFieldNames.StimuliId] =
                    identifier.ValueForAttribute("uniqueid").Split('-').Last();
                stimFields[(int) StimFieldNames.FileName] = passageProcessor.ValueForAttribute("filename");
                stimFields[(int) StimFieldNames.Version] = identifier.ValueForAttribute("version");

                resultList.Add(stimFields.ToList());
            }
            return resultList;
        }

        public IEnumerable<IEnumerable<string>> ProcessResult (XPathNodeIterator stimuliNodes, IDictionary<ItemFieldNames, string> testInformation)
        {
            var resultList = new List<List<string>>();
            foreach (XPathNavigator stimulus in stimuliNodes)
            {
                var stimFields = new string[Enum.GetNames(typeof(StimFieldNames)).Length];

                stimFields[(int)StimFieldNames.AssessmentName] = testInformation[ItemFieldNames.AssessmentName];
                stimFields[(int)StimFieldNames.AssessmentSubject] = testInformation[ItemFieldNames.AssessmentSubject];
                stimFields[(int)StimFieldNames.AssessmentGrade] = testInformation[ItemFieldNames.AssessmentGrade];
                stimFields[(int)StimFieldNames.AssessmentType] = testInformation[ItemFieldNames.AssessmentType];
                stimFields[(int) StimFieldNames.StimuliId] = stimulus.GetAttribute("id", "");
                stimFields[(int) StimFieldNames.FileName] =
                    $"stim-{testInformation[ItemFieldNames.BankKey]}-{stimFields[(int) StimFieldNames.StimuliId]}.xml";
                stimFields[(int)StimFieldNames.Version] = testInformation[ItemFieldNames.Version];

                resultList.Add(stimFields.ToList());
            }
            return resultList;
        }
    }
}