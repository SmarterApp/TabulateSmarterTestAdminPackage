using System.Collections.Generic;
using NLog;
using SmarterTestPackage.Common.Data;
using ValidateSmarterTestPackage.Resources;

namespace ValidateSmarterTestPackage.Validators.Combined
{

    public class TestPackageValidator : ITestPackageValidator
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly HashSet<string> _recognizedToolOptions = new HashSet<string>
        {
            "TDS_BrailleTrans0",
            "TDS_BrailleTrans1",
            "TDS_PS_L0",
            "TDS_PS_L1",
            "TDS_PS_L2",
            "TDS_PS_L3",
            "TDS_PS_L4",
            "TDS_ITTC0",
            "TDS_ITTC_Pitch",
            "TDS_ITTC_Rate",
            "TDS_ITTC_Volume",
            "TDS_ASL0",
            "TDS_ASL1",
            "TDS_APC_PSP",
            "TDS_APC_SCRUBBER",
            "TDS_BT0",
            "TDS_BT_ECN",
            "TDS_BT_ECT",
            "TDS_BT_EXN",
            "TDS_BT_EXT",
            "TDS_BT_UCN",
            "TDS_BT_UCT",
            "TDS_BT_UXN",
            "TDS_BT_UXT",
            "TDS_ClosedCap0",
            "TDS_ClosedCap1",
            "TDS_CC0",
            "TDS_CCInvert",
            "TDS_CCMagenta",
            "TDS_CCMedGrayLtGray",
            "TDS_CCYellowB",
            "TDS_Emboss0",
            "TDS_Emboss_Stim&TDS_Emboss_Item",
            "TDS_ERT0",
            "TDS_ERT_OR",
            "TDS_ERT_OR&TDS_ERT_Auto",
            "TDS_ExpandablePassages0",
            "TDS_ExpandablePassages1",
            "TDS_FT_Serif",
            "TDS_HWPlayback",
            "TDS_Highlight0",
            "TDS_Highlight1",
            "TDS_ILG0",
            "TDS_ILG1",
            "TDS_IF_S14",
            "TDS_ITM1",
            "TDS_MfR0",
            "TDS_MfR1",
            "TDS_Masking0",
            "TDS_Masking1",
            "TDS_Mute0",
            "TDS_Mute1",
            "TDS_Mute2",
            "TDS_Mute3",
            "NEA0",
            "NEA_AR",
            "NEA_NoiseBuf",
            "NEA_RA_Stimuli",
            "NEA_SC_WritItems",
            "NEA_STT",
            "NEDS0",
            "NEDS_CC",
            "NEDS_CO",
            "NEDS_Mag",
            "NEDS_RA_Items",
            "NEDS_SC_Items",
            "NEDS_SS",
            "NEDS_TransDirs",
            "TDS_F_S14",
            "TDS_PM0",
            "TDS_PM1",
            "TDS_PoD0",
            "TDS_PoD_Item",
            "TDS_PoD_Stim",
            "TDS_PoD_Stim&TDS_PoD_Item",
            "TDS_RSL_ListView",
            "TDS_SLM0",
            "TDS_SLM1",
            "TDS_ST0",
            "TDS_ST1",
            "TDS_SC0",
            "TDS_SCNotepad",
            "TDS_SVC1",
            "TDS_TPI_ResponsesFix",
            "TDS_TS_Universal",
            "TDS_TTS0",
            "TDS_TTS_Item",
            "TDS_TTS_Stim",
            "TDS_TTS_Stim&TDS_TTS_Item",
            "TDS_TTSAA0",
            "TDS_TTSAA_Volume&TDS_TTSAA_Pitch&TDS_TTSAA_Rate&TDS_TTSAA_SelectVP",
            "TDS_TTSPause0",
            "TDS_TTSPause1",
            "TDS_TTX_A203",
            "TDS_TTX_A206",
            "TDS_T0",
            "TDS_T1",
            "TDS_WL0",
            "TDS_WL_Glossary",
            "ENU-Braille",
            "TDS_BT_NM",
            "TDS_Calc0",
            "TDS_CalcSciInv",
            "TDS_FT_Verdana",
            "NEA_Abacus",
            "NEA_Calc",
            "NEA_MT",
            "NEDS_RA_Stimuli",
            "NEDS_TArabic",
            "NEDS_TCantonese",
            "NEDS_TFilipino",
            "NEDS_TKorean",
            "NEDS_TMandarin",
            "NEDS_TPunjabi",
            "NEDS_TRussian",
            "NEDS_TSpanish",
            "NEDS_TUkrainian",
            "NEDS_TVietnamese",
            "TDS_WL_ArabicGloss",
            "TDS_WL_ArabicGloss&TDS_WL_Glossary",
            "TDS_WL_CantoneseGloss",
            "TDS_WL_CantoneseGloss&TDS_WL_Glossary",
            "TDS_WL_ESNGlossary",
            "TDS_WL_ESNGlossary&TDS_WL_Glossary",
            "TDS_WL_KoreanGloss",
            "TDS_WL_KoreanGloss&TDS_WL_Glossary",
            "TDS_WL_MandarinGloss",
            "TDS_WL_MandarinGloss&TDS_WL_Glossary",
            "TDS_WL_PunjabiGloss",
            "TDS_WL_PunjabiGloss&TDS_WL_Glossary",
            "TDS_WL_RussianGloss",
            "TDS_WL_RussianGloss&TDS_WL_Glossary",
            "TDS_WL_TagalGloss",
            "TDS_WL_TagalGloss&TDS_WL_Glossary",
            "TDS_WL_UkrainianGloss",
            "TDS_WL_UkrainianGloss&TDS_WL_Glossary",
            "TDS_WL_VietnameseGloss",
            "TDS_WL_VietnameseGloss&TDS_WL_Glossary",
            "TDS_BT_G1",
            "TDS_BT_G2",
            "NEDS_BD",
            "TDS_CalcBasic",
            "TDS_CalcSciInv&TDS_CalcGraphingInv&TDS_CalcRegress",
            "TDS_GN0",
            "TDS_GN1",
            "TDS_Dict0",
            "TDS_Dict_SD4",
            "TDS_DO_ALL",
            "TDS_TH0",
            "TDS_TH_TA",
            "TDS_TO_ALL",
            "TDS_Dict_SD2",
            "TDS_Dict_SD3"
        };

        readonly Dictionary<string, string> _toolDefaultsMap = new Dictionary<string, string>
        {
            {"American Sign Language", "TDS_Acc-ASL"},
            {"Audio Playback Controls", "TDSAcc-AudioPlaybackControls"},
            {"Braille Transcript", "TDSAcc-BrailleTranscript"},
            {"Braille Type", "TDSAcc-BrailleType"},
            {"Calculator", "TDSAcc-Calculator"},
            {"Closed Captioning", "TDSACC-NFCLOSEDCAP"},
            {"Color Choices", "TDSAcc-ColorChoices"},
            {"Dictionary", "TDSAcc-Dictionary"},
            {"Emboss", "TDSAcc-Emboss"},
            {"Emboss Request Type", "TDSAcc-EmbossRequestType"},
            {"Expandable Passages", "TDSAcc-ExpandablePassages"},
            {"Font Type", "TDSAcc-FontType"},
            {"Global Notes", "TDSAcc-GlobalNotes"},
            {"Hardware Checks", "TDSAcc-HWCheck"},
            {"Highlight", "TDSAcc-Highlight"},
            {"Item Font Size", "TDSAcc-ItemFontSize"},
            {"Item Tools Menu", "TDSAcc-ITM"},
            {"Language", "TDSAcc-Language"},
            {"Mark for Review", "TDSAcc-MarkForReview"},
            {"Masking", "TDSAcc-Masking"},
            {"Mute System Volume", "TDSAcc-Mute"},
            {"Non-Embedded Accommodations", "TDSAcc-NonEmbedAcc"},
            {"Non-Embedded Designated Supports", "TDSAcc-DesigSup"},
            {"Passage Font Size", "TDSAcc-FontSize"},
            {"Print on Request", "TDSAcc-PrintOnRequest"},
            {"Print Size", "TDSAcc-PrintSize"},
            {"Review Screen Layout", "TDSAcc-RvScrn"},
            {"Streamlined Mode", "TDSAcc-EAM"},
            {"Strikethrough", "TDSAcc-Strikethrough"},
            {"Student Comments", "TDSAcc-StudentComments"},
            {"System Volume Control", "TDSAcc-SVC"},
            {"Test Progress Indicator", "TDSAcc-TPI"},
            {"Test Shell", "TDSAcc-TestShell"},
            {"Thesaurus", "TDSAcc-Thesaurus"},
            {"TTS", "TDSAcc-TTS"},
            {"TTS Audio Adjustments", "TDSAcc-TTSAdjust"},
            {"TTS Pausing", "TDSAcc-TTSPausing"},
            {"TTX Business Rules", "TDSAcc-TTXBusinessRules"},
            {"Tutorial", "TDSAcc-Tutorial"},
            {"Word List", "TDSAcc-WordList"}
        };
        

        public void Validate(TestPackage testPackage, List<ValidationError> errors)
        {
            var toolTypes = new List<ToolsTool>();
            foreach (var test in testPackage.Test)
            {
                if (test.Tools != null)
                {
                    toolTypes.AddRange(test.Tools);
                }
                
                foreach (var segment in test.Segments)
                {
                    if (segment.Tools != null)
                    {
                        toolTypes.AddRange(segment.Tools);
                    }
                    
                }
            }
            ValidateTestToolNamesAreRecognized(toolTypes, errors);
            ValidateToolARTFieldNamesAreRecognized(toolTypes, errors);
            ValidateKnownToolCodes(toolTypes, errors);
        }

        private void ValidateKnownToolCodes(List<ToolsTool> toolTypes, List<ValidationError> errors)
        {
            foreach (var tool in toolTypes)
            {
                foreach (var option in tool.Options)
                {
                    if (!_recognizedToolOptions.Contains(option.code))
                    {
                        var errStr =
                            $"A tool with an unrecognized ISAAP code was detected: {option.code}";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            Location = "TestPackage/Test/Tools/Tool/Options/Option *or* TestPackage/Test/Segments/Segment/Tools/Tool/Options/Option",
                            GeneratedMessage = errStr,
                            ItemId = tool.name,
                            Key = "Tool",
                            PackageType = PackageType.Combined,
                            Value = option.code
                        });
                    }
                }
            }
        }

        private void ValidateToolARTFieldNamesAreRecognized(List<ToolsTool> toolTypes, List<ValidationError> errors)
        {
            foreach (var tool in toolTypes)
            {
                if (tool.studentPackageFieldName != null)
                {
                    if (!_toolDefaultsMap.ContainsValue(tool.studentPackageFieldName))
                    {
                        var errStr =
                            $"The tool {tool.name} contained an unrecognized ART field name {tool.studentPackageFieldName}.";
                        Logger.Debug(errStr);
                        errors.Add(new ValidationError
                        {
                            ErrorSeverity = ErrorSeverity.Benign,
                            Location = "TestPackage/Test/Tools/Tool *or* TestPackage/Test/Segments/Segment/Tools/Tool",
                            GeneratedMessage = errStr,
                            ItemId = tool.name,
                            Key = "Tool",
                            PackageType = PackageType.Combined,
                            Value = tool.studentPackageFieldName
                        });
                    }
                }
            }
        }

        private void ValidateTestToolNamesAreRecognized(List<ToolsTool> toolTypes, List<ValidationError> errors)
        {
            List<string> knownToolTypes = new List<string>(_toolDefaultsMap.Keys);
            foreach (var tool in toolTypes)
            {
                if (! knownToolTypes.Contains(tool.name))
                {
                    var errStr =
                        $"An unrecognized test tool type with the tool name \"{tool.name}\" was detected.";
                    Logger.Debug(errStr);
                    errors.Add(new ValidationError
                    {
                        ErrorSeverity = ErrorSeverity.Benign,
                        Location = "TestPackage/Test/Tools/Tool *or* TestPackage/Test/Segments/Segment/Tools/Tool",
                        GeneratedMessage = errStr,
                        ItemId = tool.name,
                        Key = "Tool",
                        PackageType = PackageType.Combined,
                        Value = tool.studentPackageFieldName
                    });
                }
            }
        }
    }
}