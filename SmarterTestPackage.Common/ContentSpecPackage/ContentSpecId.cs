using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
---
name: ContentSpecId.cs
description: Class for Constructing, Parsing, and Converting Smarter Balanced Content Specification IDs
url: https://raw.githubusercontent.com/SmarterApp/SC_ContentSpecId/master/ContentSpecId.cs
version: 1.1
keywords: CodeBit
dateModified: 2018-08-29
license: https://opensource.org/licenses/ECL-2.0
# This is a CodeBit; see https://www.filemeta.org/CodeBit.html
# Metadata in Yaml format using Schema.org properties. See https://schema.org.
...
*/

/*
Copyright 2018 Regents of the University of California.

Licensed under the Educational Community License, Version 2.0
(the "License"); you may not use this file except in compliance
with the License. You may obtain a copy of the License at

https://opensource.org/licenses/ECL-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an "AS IS"
BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express
or implied. See the License for the specific language governing
permissions and limitations under the License.
*/

/*
See http://www.smarterapp.org/documents/ContentSpecificationIdFormats.pdf for
details about Content Specification Identifiers in Enhanced and Legacy formats including
definitions of all properties.

This class may be used to represent a content specficiation identifier, to
generate IDs in Ehanced or Legacy formats, and to parse IDs in both formats.

The primary properties of a Smarter Balanced Content Specification
Identifier are Subject, Grade, Claim, and Target. Domain is also
frequently used but may be derived from the other attributes.
Legacy identifiers in certain formats include Emphasis, Target Set,
and Content Category properties all of which can be derived from
the primary properties.
*/


namespace SmarterApp
{
    public enum ContentSpecIdFormat : int
    {
        Unknown = 0,    // Unknown or not applicable
        ElaV1 = 1,      // SBAC-ELA-v1
        MathV4 = 4,     // SBAC-MA-v4
        MathV5 = 5,     // SBAC-MA-v5
        MathV6 = 6,     // SBAC-MA-v6
        Enhanced = 10,  // New format - works for all subjects
    }

    public enum ContentSpecSubject : int
    {
        Unspecified = 0,
        ELA = 1,
        Math = 2
    }

    public enum ContentSpecGrade : int
    {
        Unspecified = -1,
        GK = 0,   // Kindergarten
        G1 = 1,
        G2 = 2,
        G3 = 3,
        G4 = 4,
        G5 = 5,
        G6 = 6,
        G7 = 7,
        G8 = 8,
        GHS = 11 // High School
    }

    public enum ContentSpecClaim : int
    {
        Unspecified = 0,
        C1 = 1,
        C2 = 2,
        C3 = 3,
        C4 = 4
    }

    public enum ContentSpecEmphasis : int
    {
        Unknown = -1,
        NotApplicable = 0,  // Value is only applicable to Math Claim 1
        M = 1,      // Major work of the grade (also known as "P" for Priority Cluster)
        AS = 2      // Additional/Supporting (also known as "a/s", or S)
    }

    public enum ErrorSeverity : int
    {
        NoError = 0,
        Corrected = 1,  // An error that was corrected by the parser such as Domain mapping or Emphasis mismatch
        Invalid = 2     // The identifier is invalid and cannot be corrected
    }

    /// <summary>
    /// Represents a Smarter Balanced Content Specification Identifier
    /// </summary>
    /// <remarks>
    /// <para>See http://www.smarterapp.org/documents/ContentSpecificationIdFormats.pdf for
    /// details about Content Specification Identifiers in Enhanced and Legacy formats including
    /// definitions of all properties.
    /// </para>
    /// <para>This class may be used to represent a content specficiation identifier, to
    /// generate IDs in Ehanced or Legacy formats, and to parse IDs in both formats.
    /// </para>
    /// <para>The primary properties of a Smarter Balanced Content Specification
    /// Identifier are Subject, Grade, Claim, and Target. Domain is also
    /// frequently used but may be derived from the other attributes.
    /// Legacy identifiers in certain formats include Emphasis, Target Set,
    /// and Content Category properties all of which can be derived from
    /// the primary properties.
    /// </para>
    /// </remarks>
    public class ContentSpecId : IComparable, IComparable<ContentSpecId>, IEquatable<ContentSpecId>
    {
        const string c_Main = "m";
        const string c_AdditionalSupporting = "a/s";
        const string c_NotApplicable = "NA";
        const string c_PriorityCluster = "P";
        const string c_SupportingCluster = "S";

        // Primary properties
        ContentSpecSubject m_subject = ContentSpecSubject.Unspecified;
        ContentSpecGrade m_grade = ContentSpecGrade.Unspecified; // K = 0, 11 = High School, -1 = unknown
        ContentSpecClaim m_claim = ContentSpecClaim.Unspecified;
        string m_target = string.Empty;
        string m_ccss = string.Empty; // Common core state standard
        string m_domain = null;
        ContentSpecIdFormat m_parseFormat = ContentSpecIdFormat.Unknown;

        // Validation features
        bool m_modifiedSinceLastValidation = false;
        string m_parseError = null;
        ErrorSeverity m_parseErrorSeverity = ErrorSeverity.NoError;
        string m_validationError = null;
        ErrorSeverity m_validationErrorSeverity = ErrorSeverity.NoError;

        #region Properties

        /// <summary>
        /// Subject: Math or ELA
        /// </summary>
        public ContentSpecSubject Subject
        {
            get { return m_subject; }
            set { m_subject = value; ResetStatus(); }
        }

        /// <summary>
        /// Grade level
        /// </summary>
        public ContentSpecGrade Grade
        {
            get { return m_grade; }
            set { m_grade = value; ResetStatus(); }
        }

        /// <summary>
        /// Legacy grade is the grade number without the "G" prefix. High school is 11.
        /// </summary>
        public string LegacyGrade
        {
            get
            {
                if (m_grade == ContentSpecGrade.Unspecified) return string.Empty;
                return ((int)m_grade).ToString();
            }
        }

        /// <summary>
        /// Claim number
        /// </summary>
        public ContentSpecClaim Claim
        {
            get { return m_claim; }
            set { m_claim = value; ResetStatus(); }
        }

        /// <summary>
        /// Legacy claim is the claim number in string form without the "C" prefix.
        /// </summary>
        public string LegacyClaim
        {
            get
            {
                if (m_claim == ContentSpecClaim.Unspecified) return string.Empty;
                return ((int)m_claim).ToString();
            }
        }

        /// <summary>
        /// Target number or letter
        /// </summary>
        public string Target
        {
            get { return m_target; }
            set { m_target = value ?? string.Empty; ResetStatus(); }
        }

        /// <summary>
        /// The associated Common Core State Standard identifier
        /// </summary>
        public string CCSS
        {
            get { return m_ccss; }
            set { m_ccss = value ?? string.Empty; ResetStatus(); }
        }

        /// <summary>
        /// The Domain to which the target belongs.
        /// </summary>
        /// <remarks>
        /// <para>For targets that are in-range (ELA targets 1-14 and Math targets
        /// A-P) the domain can be derived by setting the claim and target. Setting
        /// the domain, either by setting this property or by parsing an ID, will
        /// override any derivation. See <see cref="DeriveDomain"/>.
        /// </para>
        /// <para>For Math Claims 2-4, the domain defaults to NA but it may contain
        /// the domain from the associated Claim 1 alignment on the same item.
        /// <see cref="SetDomainFromClaimOneId(ContentSpecId)"/>.
        /// </para>
        /// <para>Math v6 formatted IDs do not include a domain. Instead, they include
        /// the related <see cref="ContentCategory"/>. When an ID of this format is
        /// parsed, the domain will be derived if the target is in range and empty
        /// string if unknown.
        /// </para>
        /// <para>Setting the value to null will cause it to be derived from the claim
        /// and target.
        /// </para>
        /// <seealso cref="LegacyDomain"/>
        /// <seealso cref="LegacyContentCategory"/>
        /// <seealso cref="SetDomainFromClaimOneId(ContentSpecId)"/>
        /// </remarks>
        public string Domain
        {
            get
            {
                if (m_domain != null)
                {
                    return m_domain;
                }

                // Doing it this way without setting m_domain allows the
                // domain to be sensitive to future changes to claim and target.
                return DeriveDomain(m_subject, m_grade, m_claim, m_target);
            }

            set
            {
                m_domain = value;
            }
        }

        /// <summary>
        /// Domain value for the Legacy format
        /// </summary>
        /// <remarks>
        /// <para>Certain ELA domains in the legacy format are different from the Enhanced
        /// format. This property uses the translated values in those cases.
        /// </para>
        /// <para>For High School Math, the legacy domain includes the CCSS Content Category
        /// plus the domain (e.g. "G-SRT") whereas the new domain only includes the CCSS
        /// Content Category (e.g. "G").</para>
        /// <seealso cref="Domain"/>
        /// <seealso cref="LegacyContentCategory"/>
        /// <seealso cref="SetDomainFromClaimOneId(ContentSpecId)"/>
        /// </remarks>
        public string LegacyDomain
        {
            get
            {
                string domain = Domain;

                // If Claim 1 and high school, and the domain matches the the beginning
                // of the ccss then return the extended domain from the CCSS standard.
                // For example: if (Domain == "G" and CCSS == "G-SRT.6") return "G-SRT";
                if (m_subject == ContentSpecSubject.Math && m_grade == ContentSpecGrade.GHS)
                {
                    int dash = m_ccss.IndexOf('-');
                    int dot = m_ccss.IndexOf('.');
                    if (dash > 0 && dot > dash &&
                        m_ccss.Substring(0, dash).Equals(domain, StringComparison.Ordinal))
                    {
                        return m_ccss.Substring(0, dot);
                    }
                }

                // Translate ELA domains
                if (m_subject == ContentSpecSubject.ELA)
                {
                    switch (domain)
                    {
                        case "RL":
                            return "LT"; // Reading Literary Texts
                        case "RI":
                            return "IT"; // Reading Informational Texts
                        case "WN":
                        case "WI":
                        case "WO":
                        case "WE":
                        case "WA":
                        case "WG":
                            return "W"; // Writing
                        case "SL":
                            return "L"; // Listening
                        case "R":
                            return "CR"; // Research & Inquiry - Communicating Reasoning
                        default:
                            return domain;
                    }
                }

                return domain;
            }

            set
            {
                if (value == null)
                {
                    m_domain = null;
                    return;
                }

                // Reduce high school math domain to just the Content Category
                int dash = value.IndexOf('-');
                if (dash > 0)
                {
                    m_domain = value.Substring(0, dash);
                    return;
                }

                // Translate ELA domains
                if (m_subject == ContentSpecSubject.ELA)
                {
                    switch (value)
                    {
                        case "LT":
                            m_domain = "RL"; // Reading Literary Texts
                            break;
                        case "IT":
                            m_domain = "RI"; // Reading Informational Texts
                            break;
                        case "W":
                            {
                                // If grade, claim, and target results in a writing domain
                                // then set the derived value, else set the value we got
                                var d = DeriveElaDomain(m_grade, m_claim, m_target);
                                m_domain = (d[0] == 'W') ? d : value;
                            }
                            break;
                        case "L":
                            m_domain = "SL"; // Speaking and Listening
                            break;
                        case "CR":
                            m_domain = "R"; // Research & Inquiry - Communicating Reasoning
                            break;
                        default:
                            m_domain = value;
                            break;
                    }
                    return;
                }

                m_domain = value;
            }
        }

        /// <summary>
        /// The emphasis - whether Primary or Additional/Supporting.
        /// Only valid for math claim 1. Otherwise NotApplicable.
        /// </summary>
        public ContentSpecEmphasis Emphasis
        {
            get
            {
                if (m_subject != ContentSpecSubject.Math || m_claim != ContentSpecClaim.C1)
                {
                    return ContentSpecEmphasis.NotApplicable;
                }

                int grade = (int)m_grade;
                int target = ParseMathTarget(m_target);

                if (grade >= c_MathEmphasis3to8GradeMin
                    && grade < c_MathEmphasis3to8GradeMin + c_MathEmphasis3to8GradeCount
                    && target >= c_MathEmphasis3to8TargetMin
                    && target < c_MathEmphasis3to8TargetMin + c_MathEmphasis3to8TargetCount)
                {
                    return s_MathEmphasis3to8[grade - c_MathEmphasis3to8GradeMin, target - c_MathEmphasis3to8TargetMin]
                        ? ContentSpecEmphasis.M : ContentSpecEmphasis.AS;
                }

                if (m_grade == ContentSpecGrade.GHS
                    && target > 0 && target <= s_MathEmphasisHighSchool.Length)
                {
                    /* This is an odd case. Sometimes when recording secondary alignment to a CCSS
                    standard that's not referenced in teh content specification, staff have used
                    a target of "O" for "Other". Usually, and preferably, they have used a target
                    of "X". The trouble with "O" is that it's a legitimate target and is "m"
                    whereas other alignments should be "a/s". We solve this by looking at the CCSS
                    alignment. If it does not start with "G-SRT" which is the proper standard for
                    target "O" then we set the emphasis to "a/s".
                    */
                    if (target == ('O' - 'A') + 1   // Target 'O'
                        && !string.IsNullOrEmpty(m_ccss)    // CCSS has a value
                        && !m_ccss.StartsWith("G-SRT", StringComparison.Ordinal))
                    {
                        return ContentSpecEmphasis.AS;
                    }

                    return s_MathEmphasisHighSchool[target - 1]
                        ? ContentSpecEmphasis.M : ContentSpecEmphasis.AS;
                }

                return ContentSpecEmphasis.AS;
            }
        }

        /// <summary>
        /// Returns the legacy emphasis value in string form.
        /// </summary>
        /// <remarks>
        /// Possible return values are: "m", "a/s", and "NA".
        /// </remarks>
        public string LegacyEmphasis
        {
            get
            {
                switch (Emphasis)
                {
                    case ContentSpecEmphasis.M:
                        return c_Main;
                    case ContentSpecEmphasis.AS:
                        return c_AdditionalSupporting;
                    default:
                        return c_NotApplicable;
                }
            }
        }

        /// <summary>
        /// Returns the legacy content category if applicable or available.
        /// </summary>
        /// <remarks>
        /// <para>For Math Claim 1, the content category is "P" (Priority Cluster)
        /// if emphasis is "m" (main) and it is "S" (Supporting) if emphasis
        /// is "a/s" (additional/supporting).
        /// </para>
        /// <para>For Math Claims 2-4 the content category is the Domain
        /// from the associated Claim 1 alignment in the same item. If that value
        /// has not been set then this will return "NA". When constructing
        /// a <see cref="ContentSpecId"/> from parameters, you can use 
        /// <see cref="SetDomainFromClaimOneId"/> or set
        /// <see cref="LegacyDomain"/> to set the correct value.
        /// </para>
        /// <para>For subjects other than math, value is "NA".
        /// </para>
        /// </remarks>
        public string LegacyContentCategory
        {
            get
            {
                if (m_subject != ContentSpecSubject.Math)
                {
                    return c_NotApplicable;
                }
                if (m_claim == ContentSpecClaim.C1)
                {
                    switch (Emphasis)
                    {
                        case ContentSpecEmphasis.M:
                            return c_PriorityCluster;
                        case ContentSpecEmphasis.AS:
                            return c_SupportingCluster;
                        default:
                            return c_NotApplicable;
                    }
                }
                if (m_domain != null)
                {
                    return m_domain;
                }
                return c_NotApplicable;
            }
        }

        /// <summary>
        /// The target set in the range 1-10. Only relevant for Math Claim 1.
        /// Value of 0 indicates not applicable.
        /// </summary>
        public int TargetSet
        {
            get
            {
                if (m_subject != ContentSpecSubject.Math || m_claim != ContentSpecClaim.C1)
                {
                    return 0;
                }

                int grade = (int)m_grade;
                int target = ParseMathTarget(m_target);

                if (grade >= c_MathTargetSetGradeMin
                    && grade < c_MathTargetSetGradeMin + c_MathTargetSetGradeCount
                    && target >= c_mathTargetSetTargetMin
                    && target < c_mathTargetSetTargetMin + c_MathTargetSetTargetCount)
                {
                    return s_MathTargetSet[grade - c_MathTargetSetGradeMin, target - 1];
                }

                return 0;
            }
        }

        string LegacyTargetSet
        {
            get
            {
                int ts = TargetSet;
                return (ts == 0) ? c_NotApplicable : $"TS{ts:D2}";
            }
        }

        /// <summary>
        /// Indicates whether the all primary properties are at their default (Unknown) values.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return m_subject == ContentSpecSubject.Unspecified
                    && m_grade == ContentSpecGrade.Unspecified
                    && m_claim == ContentSpecClaim.Unspecified
                    && string.IsNullOrEmpty(m_target)
                    && string.IsNullOrEmpty(m_ccss);
            }
        }

        /// <summary>
        /// The format from which the ID was parsed. "Unknown" if no ID was parsed.
        /// </summary>
        public ContentSpecIdFormat ParseFormat
        {
            get { return m_parseFormat; }
        }

        /// <summary>
        /// True if the last parse succeeded or no parse has been attempted. False if the parse failed.
        /// </summary>
        /// <remarks>Also consider <see cref="ParseErrorSeverity"/> == <see cref="ErrorSeverity.NoError"/></remarks>
        public bool ParseSucceeded
        {
            get { return m_parseErrorSeverity == ErrorSeverity.NoError; }
        }

        /// <summary>
        /// The severity of the any parse error
        /// </summary>
        public ErrorSeverity ParseErrorSeverity
        {
            get { return m_parseErrorSeverity; }
        }

        /// <summary>
        /// A description of any error from the last parse attempt. Null if no error or no parse attempt.
        /// </summary>
        public string ParseErrorDescription
        {
            get { return m_parseError; }
        }

        /// <summary>
        /// Detail from the last call to <see cref="Validate"/> or <see cref="ValidateFor"/>.
        /// If either of the above calls returns false then this property will contain a detailed
        /// description of the validation error. Null if neither validation call has been made
        /// or if validation succeeded.
        /// </summary>
        public string ValidationErrorDescription
        {
            get { return m_validationError; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Attempts to parse a Smarter Balanced Standard ID. 
        /// </summary>
        /// <param name="value">The ID to parse</param>
        /// <param name="grade">Optional grade value</param>
        /// <returns>A Standard ID. Check IsValid on the ID to determine wehther the parse succeeded.</returns>
        /// <remarks>
        /// <para>In the legacy formats, the grade is frequently, but not always, specified as a suffix
        /// to the target. Specifying a grade will ensure that the grade value is set when it's omitted
        /// from the legacy ID.
        /// </para>
        /// <para>If the parse succeeds then IsValid will be true on the resulting standard ID. If the parse
        /// false than IsValid will be false and ValidityError will be set to a description of what failed.
        /// Regardless of errors, the parser attempts to extract as much valid informaiton as possible.
        /// </para>
        /// </remarks>
        public static ContentSpecId TryParse(string value, ContentSpecGrade grade = ContentSpecGrade.Unspecified)
        {
            ContentSpecId id = new ContentSpecId();

            if (value.StartsWith("SBAC-ELA-v1:", StringComparison.Ordinal))
            {
                id.m_parseFormat = ContentSpecIdFormat.ElaV1;
                id.m_subject = ContentSpecSubject.ELA;

                string[] parts = value.Substring(12).Split('|');

                string claim = string.Empty;
                string domain = string.Empty;
                string target = string.Empty;
                string parseGrade = null;

                if (parts.Length > 0)
                {
                    int dash = parts[0].IndexOf('-');
                    if (dash >= 0)
                    {
                        claim = parts[0].Substring(0, dash);
                        domain = parts[0].Substring(dash + 1);
                    }
                    else
                    {
                        claim = parts[0];
                    }
                }

                if (parts.Length > 1)
                {
                    int dash = parts[1].IndexOf('-');
                    if (dash >= 0)
                    {
                        target = parts[1].Substring(0, dash);
                        parseGrade = parts[1].Substring(dash + 1);
                    }
                    else
                    {
                        target = parts[1];
                    }
                }

                // The TryParse methods append to m_parseError if an error is found.
                if (parseGrade != null)
                    id.TrySetGrade(parseGrade);
                else
                    id.m_grade = grade;
                id.TrySetClaim(claim);
                id.TrySetTarget(target);
                if (parts.Length > 2) id.TrySetCcss(parts[2]);
                id.TrySetLegacyDomain(domain);
            }

            else if (value.StartsWith("SBAC-MA-v4:", StringComparison.Ordinal)
                || value.StartsWith("SBAC-MA-v5:", StringComparison.Ordinal))
            {
                id.m_parseFormat = (value[9] == '4') ? ContentSpecIdFormat.MathV4 : ContentSpecIdFormat.MathV5;
                id.m_subject = ContentSpecSubject.Math;

                string[] parts = value.Substring(11).Split('|');

                string target = string.Empty;
                string parseGrade = null;

                if (parts.Length > 2)
                {
                    int dash = parts[2].IndexOf('-');
                    if (dash >= 0)
                    {
                        target = parts[2].Substring(0, dash);
                        parseGrade = parts[2].Substring(dash + 1);
                    }
                    else
                    {
                        target = parts[2];
                    }
                }

                if (parseGrade != null)
                    id.TrySetGrade(parseGrade);
                else
                    id.m_grade = grade;
                if (parts.Length > 0) id.TrySetClaim(parts[0]);
                id.TrySetTarget(target);
                if (parts.Length > 4) id.TrySetCcss(parts[4]);
                if (parts.Length > 1) id.TrySetLegacyDomain(parts[1]);
                if (parts.Length > 3) id.ValidateEmphasis(parts[3]);
            }

            else if (value.StartsWith("SBAC-MA-v6:", StringComparison.Ordinal))
            {
                id.m_parseFormat = ContentSpecIdFormat.MathV6;
                id.m_subject = ContentSpecSubject.Math;

                string[] parts = value.Substring(11).Split('|');

                string target = string.Empty;
                string parseGrade = null;

                if (parts.Length > 3)
                {
                    int dash = parts[3].IndexOf('-');
                    if (dash >= 0)
                    {
                        target = parts[3].Substring(0, dash);
                        parseGrade = parts[3].Substring(dash + 1);
                    }
                    else
                    {
                        target = parts[3];
                    }
                }

                if (parseGrade != null)
                    id.TrySetGrade(parseGrade);
                else
                    id.m_grade = grade;
                if (parts.Length > 0) id.TrySetClaim(parts[0]);
                id.TrySetTarget(target);
                if (parts.Length > 1) id.TrySetLegacyContentCategory(parts[1]);
                if (parts.Length > 2) id.ValidateTargetSet(parts[2]);
            }

            // Enhanced format
            else if (value[0] == 'E' || value[0] == 'M')
            {
                id.m_parseFormat = ContentSpecIdFormat.Enhanced;
                string[] parts = value.Split('.');

                id.TrySetSubject(parts[0]);

                if (parts.Length > 1)
                {
                    if (parts[1][0] != 'G')
                    {
                        id.AppendParseError(ErrorSeverity.Invalid, $"Expected grade in second part of ID but found '{parts[1]}'.");
                    }
                    id.TrySetGrade(parts[1].Substring(1));
                }

                string domain = string.Empty;
                if (parts.Length > 2)
                {
                    if (parts[2][0] != 'C')
                    {
                        id.AppendParseError(ErrorSeverity.Invalid, $"Expected claim in third part of ID but found '{parts[2]}'.");
                    }
                    id.TrySetClaim(parts[2].Substring(1, 1));
                    domain = parts[2].Substring(2);
                }

                if (parts.Length > 3)
                {
                    if (parts[3][0] != 'T')
                    {
                        id.AppendParseError(ErrorSeverity.Invalid, $"Expected target in third part of ID but found '{parts[3]}'.");
                    }
                    id.TrySetTarget(parts[3].Substring(1));
                }

                if (parts.Length > 4)
                {
                    id.TrySetCcss(string.Join(".", parts, 4, parts.Length - 4));
                }

                id.TrySetDomain(domain);
            }

            else
            {
                id.AppendParseError(ErrorSeverity.Invalid, $"Unknown ID format: '{value}'.");
                return id;
            }

            return id;
        }

        /// <summary>
        /// Attempts to parse a Smarter Balanced Standard ID. Throws an exception if the parse fails.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>A valid Standard ID.</returns>
        public static ContentSpecId Parse(string value, ContentSpecGrade grade = ContentSpecGrade.Unspecified)
        {
            var id = TryParse(value);
            if (!id.ParseSucceeded)
            {
                throw new FormatException("Invalid SmarterBalanced.StandardId format: " + id.ParseErrorDescription);
            }
            return id;
        }

        /// <summary>
        /// Parses a grade string into a ContentSpecGrade
        /// </summary>
        /// <param name="grade">A string to be parsed.</param>
        /// <returns>A <see cref="ContentSpecGrade"/> if successful or <see cref="ContentSpecGrade.Unspecified"/> if not.</returns>
        public static ContentSpecGrade ParseGrade(string grade)
        {
            if (grade.Equals("HS"))
            {
                return ContentSpecGrade.GHS;
            }
            int n;
            if (int.TryParse(grade, out n))
            {
                return ConvertGrade(n);
            }
            return ContentSpecGrade.Unspecified;
        }

        /// <summary>
        /// Convert a grade from int into ContentSpecGrade
        /// </summary>
        /// <param name="grade">An integer grade - typically 3-12.</param>
        /// <returns>A <see cref="ContentSpecGrade"/> if successful or <see cref="ContentSpecGrade.Unspecified"/> if not.</returns>
        public static ContentSpecGrade ConvertGrade(int grade)
        {
            if (grade >= 3 && grade <= 8)
            {
                return (ContentSpecGrade)grade;
            }
            if (grade >= 9 && grade <= 12)
            {
                return ContentSpecGrade.GHS;
            }
            return ContentSpecGrade.Unspecified;
        }

        /// <summary>
        /// Indicates whether the ID is valid for formatting into the specified format.
        /// </summary>
        /// <param name="format">The <see cref="ContentSpecIdFormat"/> for which formatting is being tested.</param>
        /// <returns>True if item is valid for the specified format. False otherwise.</returns>
        /// <remarks>If the method returns false, <see cref="FormatError"/> will be set to the
        /// error that prevents formatting into the specified format.</remarks>
        public ErrorSeverity ValidateFor(ContentSpecIdFormat format)
        {
            // Check for general validity
            var validity = Validate();
            if (validity == ErrorSeverity.Invalid)
            {
                return validity;
            }
            m_validationError = null;

            // Check for valid format and for subject match
            bool requiresDetail = false;
            switch (format)
            {
                case ContentSpecIdFormat.Enhanced:
                    if (m_subject == ContentSpecSubject.Unspecified)
                    {
                        m_validationError = "Subject is Unspecified.";
                        return ErrorSeverity.Invalid;
                    }
                    break;

                case ContentSpecIdFormat.ElaV1:
                    if (m_subject != ContentSpecSubject.ELA)
                    {
                        m_validationError = $"For format ElaV1, Subject must be ELA.";
                        return ErrorSeverity.Invalid;
                    }
                    requiresDetail = true;
                    break;

                case ContentSpecIdFormat.MathV4:
                case ContentSpecIdFormat.MathV5:
                case ContentSpecIdFormat.MathV6:
                    if (m_subject != ContentSpecSubject.Math)
                    {
                        m_validationError = $"For format {format}, Subject must be Math.";
                        return ErrorSeverity.Invalid;
                    }
                    requiresDetail = true;
                    break;

                default:
                    m_validationError = $"Invalid format '{format}'.";
                    return ErrorSeverity.Invalid;
            }

            if (requiresDetail)
            {
                if (m_grade == ContentSpecGrade.Unspecified)
                {
                    m_validationError = $"For format {format}, Grade must be specified.";
                    return ErrorSeverity.Invalid;
                }
                if (m_claim == ContentSpecClaim.Unspecified)
                {
                    m_validationError = $"For format {format}, Claim must be specified.";
                    return ErrorSeverity.Invalid;
                }
                if (string.IsNullOrEmpty(m_target))
                {
                    m_validationError = $"For format {format}, Target must be specified.";
                    return ErrorSeverity.Invalid;
                }
            }

            return ErrorSeverity.NoError;
        }

        /// <summary>
        /// Formats a Smarter Balanced Standard ID.
        /// </summary>
        /// <param name="format">The format to use. <see cref="ContentSpecIdFormat"/> for values.</param>
        /// <returns>The ID formatted into the specified string format.</returns>
        /// <remarks>Throws an exception if any property is invalid.</remarks>
        public string ToString(ContentSpecIdFormat format)
        {
            // Enhanced format can handle partially-correct values.
            if (format == ContentSpecIdFormat.Enhanced)
            {
                return ToStringEnhanced();
            }

            if (ValidateFor(format) == ErrorSeverity.Invalid)
            {
                return string.Empty;
            }

            switch (format)
            {
                case ContentSpecIdFormat.Enhanced:
                    return ToStringEnhanced();

                case ContentSpecIdFormat.ElaV1:
                    {
                        string fmt = $"SBAC-ELA-v1:{LegacyClaim}-{LegacyDomain}|{Target}-{LegacyGrade}";
                        if (!string.IsNullOrEmpty(m_ccss))
                        {
                            fmt = $"{fmt}|{m_ccss}";
                        }
                        return fmt;
                    }

                case ContentSpecIdFormat.MathV4:
                case ContentSpecIdFormat.MathV5:
                    {
                        string fmt = $"SBAC-MA-v{(int)format}:{LegacyClaim}|{LegacyDomain}|{Target}-{LegacyGrade}|{LegacyEmphasis}";
                        if (!string.IsNullOrEmpty(m_ccss))
                        {
                            fmt = $"{fmt}|{m_ccss}";
                        }
                        return fmt;
                    }

                case ContentSpecIdFormat.MathV6:
                    return $"SBAC-MA-v6:{LegacyClaim}|{LegacyContentCategory}|{LegacyTargetSet}|{Target}-{LegacyGrade}";

                default:
                    throw new ArgumentException($"Invalid format");
            }
        }

        /// <summary>
        /// Formats the ID as a string in the format from which it was parsed
        /// or in Enhanced format if it was not parsed.
        /// </summary>
        /// <returns>A formatted ID</returns>
        /// <remarks>Use the overload that accepts a format identifier to control the output format.</remarks>
        public override string ToString()
        {
            return ToString((m_parseFormat == ContentSpecIdFormat.Unknown)
                ? ContentSpecIdFormat.Enhanced
                : m_parseFormat);
        }

        /// <summary>
        /// Validates the ID.
        /// </summary>
        /// <returns>True the ID is valid, false if not.</returns>
        /// <remarks>If the ID is not valid, <see cref="ValidationErrorDescription"/> will
        /// be set to a description of the problems with the ID.</remarks>
        public ErrorSeverity Validate()
        {
            if (m_modifiedSinceLastValidation)
            {
                m_validationError = null;
                m_modifiedSinceLastValidation = false;

                if (m_subject == ContentSpecSubject.Unspecified
                    || !Enum.IsDefined(typeof(ContentSpecSubject), m_subject))
                {
                    AppendValidationError(ErrorSeverity.Invalid, $"Subject '{m_subject}' is not valid.");
                }

                if (m_grade == ContentSpecGrade.Unspecified
                    || !Enum.IsDefined(typeof(ContentSpecGrade), m_grade))
                {
                    AppendValidationError(ErrorSeverity.Invalid, $"Grade '{m_grade}' is not valid.");
                }

                if (m_claim == ContentSpecClaim.Unspecified
                    || !Enum.IsDefined(typeof(ContentSpecClaim), m_claim))
                {
                    AppendValidationError(ErrorSeverity.Invalid, $"Claim '{m_claim}' is not valid.");
                }

                if (string.IsNullOrEmpty(m_target))
                {
                    AppendValidationError(ErrorSeverity.Invalid, $"Target has not been specified.");
                }

                if (
                    (m_subject == ContentSpecSubject.ELA
                        || (m_subject == ContentSpecSubject.Math && m_claim == ContentSpecClaim.C1))
                    && string.IsNullOrEmpty(m_ccss))
                {
                    AppendValidationError(ErrorSeverity.Invalid, $"CCSS has not been specified.");
                }
            }

            return m_validationErrorSeverity;
        }

        /// <summary>
        /// Sets the <see cref="Domain"/>, <see cref="LegacyDomain"/>,
        /// and the <see cref="LegacyContentCategory"/>
        /// for a Math Claim 2-4 identifier from a Claim 1 identifier.
        /// </summary>
        /// <param name="otherId">A Claim 1 <see cref="ContentSpecId"/>.</param>
        /// <returns>True if successful, False otherwise</returns>
        /// <remarks>
        /// <para>Targets in Math Claim 2-4 do not have a domain. However, the identifier
        /// may include the domain from the Claim 1 alignment of the same item. This method
        /// will set the <see cref="LegacyDomain"/> and <see cref="LegacyContentCategory"/>
        /// from an associated Claim 1 identifier.
        /// </para>
        /// <para>To succeed, the subject identifier must be Math claim 2, 3, or 4, and the
        /// "otherId" from which the value is retrieved must be Math claim 1. If these
        /// conditions are not true then the method returns false.
        /// </para>
        /// </remarks>
        public bool SetDomainFromClaimOneId(ContentSpecId otherId)
        {
            if (m_subject != ContentSpecSubject.Math
                || m_claim < ContentSpecClaim.C2 || m_claim > ContentSpecClaim.C4
                || otherId.m_subject != ContentSpecSubject.Math
                || otherId.m_claim != ContentSpecClaim.C1)
            {
                return false;
            }
            m_domain = otherId.Domain;
            return true;
        }

        #endregion Methods

        #region Supporting Methods

        void ResetStatus()
        {
            m_modifiedSinceLastValidation = true;
            m_validationError = null;
            m_parseError = null;
            m_parseErrorSeverity = ErrorSeverity.NoError;
        }

        bool TrySetGrade(string grade)
        {
            if (string.Equals(grade, "HS", StringComparison.OrdinalIgnoreCase))
            {
                m_grade = ContentSpecGrade.GHS;
                return true;
            }

            int intGrade;
            if (int.TryParse(grade, out intGrade))
            {
                if (intGrade >= 1 && intGrade <= 8)
                {
                    m_grade = (ContentSpecGrade)intGrade;    // The grade IDs are compatible with integers when in range
                    return true;
                }
                if (intGrade >= 9 && intGrade <= 12)
                {
                    m_grade = ContentSpecGrade.GHS;
                    return true;
                }
            }
            AppendParseError(ErrorSeverity.Invalid, $"Grade must be integer in range 1-12 or 'HS'. Found '{grade}'.");
            return false;
        }

        bool TrySetSubject(string subject)
        {
            switch (subject)
            {
                case "E":
                    m_subject = ContentSpecSubject.ELA;
                    return true;

                case "M":
                    m_subject = ContentSpecSubject.Math;
                    return true;

                default:
                    AppendParseError(ErrorSeverity.Invalid, $"Subject must be 'E' or 'M'; found '{subject}'");
                    return false;
            }
        }

        bool TrySetClaim(string claim)
        {
            int intClaim;
            if (int.TryParse(claim, out intClaim))
            {
                if (intClaim >= 1 && intClaim <= 4)
                {
                    m_claim = (ContentSpecClaim)intClaim;    // The claim IDs are compatible with integers when in range
                    return true;
                }
            }
            AppendParseError(ErrorSeverity.Invalid, $"Claim must be integer in range 1-4. Found '{claim}'.");
            return false;
        }

        bool TrySetTarget(string target)
        {
            /* Removing this validation for now because it breaks the ability to parse claim level IDs from the test package BlueprintElements.
             * These are of the form SBAC-ELA-v1:1 or SBAC-MA-v6:1, and so do not contain target information.
            
            // So far, this is the only validation we do.
            if (String.IsNullOrEmpty(target))
            {
                AppendParseError(ErrorSeverity.Invalid, "Target must not be empty.");
                return false;
            }
            */
            m_target = target;
            return true;
        }

        bool TrySetCcss(string ccss)
        {
            // So far, this is the only validation we do.
            if (String.IsNullOrEmpty(ccss))
            {
                AppendParseError(ErrorSeverity.Invalid, "CCSS must not be empty.");
                return false;
            }
            m_ccss = ccss;
            return true;
        }

        bool TrySetDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain)
                || domain.Equals(c_NotApplicable, StringComparison.Ordinal))
            {
                m_domain = null;
                return true;
            }

            // If no target has been set (typically a truncated new-format ID) then just keep the specified domain
            if (string.IsNullOrEmpty(m_target))
            {
                m_domain = domain;
            }

            // If Math claim 2-4 accept the inbound domain (it's from an associated Claim 1 ID)
            if (m_subject == ContentSpecSubject.Math && m_claim != ContentSpecClaim.C1)
            {
                m_domain = domain;
                return true;
            }

            m_domain = null;
            var expectedDomain = Domain;

            // If domain cannot be derived, then set the value
            if (string.IsNullOrEmpty(expectedDomain))
            {
                m_domain = domain;
                return true;
            }

            // Otherwise, check that the value is what we expect it to be.
            if (!string.Equals(domain, expectedDomain, StringComparison.Ordinal))
            {
                AppendParseError(ErrorSeverity.Corrected, $"Incorrect domain in parsed ID. Found '{domain}', expected '{expectedDomain}'.");
                return false;
            }
            return true;
        }

        bool TrySetLegacyDomain(string legacyDomain)
        {
            if (string.IsNullOrEmpty(legacyDomain)
                || legacyDomain.Equals(c_NotApplicable, StringComparison.Ordinal))
            {
                m_domain = null;
                return true;
            }

            // If Math claim 2-4 accept the inbound domain (it's from an associated Claim 1 ID)
            if (m_subject == ContentSpecSubject.Math && m_claim != ContentSpecClaim.C1)
            {
                m_domain = legacyDomain;
                return true;
            }

            m_domain = null;
            var expectedDomain = LegacyDomain;

            // If domain cannot be derived, then set the value
            if (string.IsNullOrEmpty(expectedDomain))
            {
                // Strip the extension if this is a hyphenated domain and grade is High School
                if (m_grade == ContentSpecGrade.GHS)
                {
                    int dash = legacyDomain.IndexOf('-');
                    if (dash > 0 && m_ccss.StartsWith(legacyDomain))
                    {
                        m_domain = legacyDomain.Substring(0, dash);
                        return true;
                    }
                }

                m_domain = legacyDomain;
                return true;
            }

            // Otherwise, check the value
            if (!string.Equals(legacyDomain, LegacyDomain, StringComparison.Ordinal))
            {
                AppendParseError(ErrorSeverity.Corrected, $"Invalid Domain in parsed ID. Found '{legacyDomain}', expected '{expectedDomain}'.");
                return false;
            }

            return true;
        }

        bool TrySetLegacyContentCategory(string legacyContentCategory)
        {
            if (m_subject == ContentSpecSubject.Math && m_claim != ContentSpecClaim.C1)
            {
                m_domain = legacyContentCategory;
                return true;
            }
            m_domain = null;
            if (!string.Equals(legacyContentCategory, LegacyContentCategory, StringComparison.Ordinal))
            {
                AppendParseError(ErrorSeverity.Corrected, $"Incorrect Content Category in parsed ID. Found '{legacyContentCategory}', expected '{LegacyContentCategory}'.");
                return false;
            }
            return true;
        }

        bool ValidateTargetSet(string targetSet)
        {
            if (!string.Equals(targetSet, LegacyTargetSet, StringComparison.Ordinal))
            {
                AppendParseError(ErrorSeverity.Corrected, $"Incorrect TargetSet in parsed ID. Found '{targetSet}', expected '{LegacyTargetSet}'.");
                return false;
            }
            return true;
        }

        bool ValidateEmphasis(string emphasis)
        {
            if (!string.Equals(emphasis, LegacyEmphasis, StringComparison.Ordinal))
            {
                // Only report the error if it's not due to a previous problem
                if (string.IsNullOrEmpty(m_parseError))
                {
                    AppendParseError(ErrorSeverity.Corrected, $"Incorrect emphasis in parsed ID. Found '{emphasis}', expected '{LegacyEmphasis}'.");
                }
                return false;
            }
            return true;
        }

        void AppendParseError(ErrorSeverity severity, string error)
        {
            if (m_parseErrorSeverity < severity)
            {
                m_parseErrorSeverity = severity;
            }
            if (string.IsNullOrEmpty(m_parseError))
            {
                m_parseError = error;
            }
            else
            {
                m_parseError = string.Concat(m_parseError, " | ", error);
            }
        }

        void AppendValidationError(ErrorSeverity severity, string error)
        {
            if (m_validationErrorSeverity < severity)
            {
                m_validationErrorSeverity = severity;
            }
            if (string.IsNullOrEmpty(m_validationError))
            {
                m_validationError = error;
            }
            else
            {
                m_validationError = string.Concat(m_validationError, " | ", error);
            }
        }

        string ToStringEnhanced()
        {
            var v = new StringBuilder();
            switch (m_subject)
            {
                case ContentSpecSubject.ELA:
                    v.Append('E');
                    break;

                case ContentSpecSubject.Math:
                    v.Append('M');
                    break;

                default:
                    return string.Empty;
            }

            if (m_grade != ContentSpecGrade.Unspecified)
            {
                v.Append('.');
                v.Append(m_grade.ToString()); // Includes the 'G' prefix

                if (m_claim != ContentSpecClaim.Unspecified)
                {
                    v.Append('.');
                    v.Append(m_claim.ToString()); // Includes the 'G' prefix

                    // Either Conceptual category (High School) or Domain (Grades 3-8)
                    var domain = Domain;
                    if (!string.IsNullOrEmpty(domain)
                        && !string.Equals(domain, c_NotApplicable, StringComparison.Ordinal))
                    {
                        v.Append(domain.ToString());
                    }

                    if (!string.IsNullOrEmpty(m_target))
                    {
                        v.Append(".T");
                        v.Append(m_target);

                        if (!string.IsNullOrEmpty(m_ccss))
                        {
                            v.Append('.');
                            v.Append(m_ccss);
                        }
                    }
                }
            }
            return v.ToString();
        }

        #endregion

        #region Derived Propoerties

        public static string DeriveDomain(ContentSpecSubject subject, ContentSpecGrade grade, ContentSpecClaim claim, string target)
        {
            switch (subject)
            {
                case ContentSpecSubject.ELA:
                    return DeriveElaDomain(grade, claim, target);

                case ContentSpecSubject.Math:
                    return DeriveMathDomain(grade, claim, target);

                default:
                    return string.Empty; // Unknown
            }
        }

        static string DeriveElaDomain(ContentSpecGrade grade, ContentSpecClaim claim, string target)
        {
            int nTarget = ParseElaTarget(target);
            if (nTarget == 0) return string.Empty;

            D domain;
            switch (claim)
            {
                case ContentSpecClaim.C1:
                    domain = (nTarget < 8) ? D.RL : D.RI;
                    break;

                case ContentSpecClaim.C2:
                    if (grade <= ContentSpecGrade.G5)
                    {
                        if (nTarget <= 2) domain = D.WN;
                        else if (nTarget <= 5) domain = D.WI;
                        else if (nTarget <= 7) domain = D.WO;
                        else domain = D.WG;
                    }
                    else
                    {
                        if (nTarget <= 2) domain = D.WN;
                        else if (nTarget <= 5) domain = D.WE;
                        else if (nTarget <= 7) domain = D.WA;
                        else domain = D.WG;
                    }
                    break;

                case ContentSpecClaim.C3:
                    domain = D.SL;
                    break;

                case ContentSpecClaim.C4:
                    domain = D.R;
                    break;

                default:
                    domain = D.UNK;
                    break;
            }

            return (domain == D.UNK) ? string.Empty : domain.ToString();
        }

        // Returns "NA" if the domain is not applicable and empty string if it is unknown.
        static string DeriveMathDomain(ContentSpecGrade grade, ContentSpecClaim claim, string target)
        {
            // Must be claim 1
            if (claim != ContentSpecClaim.C1)
            {
                return D.NA.ToString();
            }

            int nTarget = ParseMathTarget(target);

            D domain = D.UNK;

            if ((int)grade >= c_MathDomains3to8GradeMin
                && (int)grade < c_MathDomains3to8GradeMin + c_MathDomains3to8GradeCount
                && nTarget >= c_MathDomains3to8TargetMin
                && nTarget < c_MathDomains3to8TargetMin + c_MathDomains3to8TargetCount)
            {
                domain = s_MathDomains3to8[(int)grade - c_MathDomains3to8GradeMin, nTarget - c_MathDomains3to8TargetMin];
            }

            else if (grade == ContentSpecGrade.GHS && nTarget > 0 && nTarget <= s_MathDomainsHighSchool.Length)
            {
                domain = s_MathDomainsHighSchool[nTarget - 1];
            }

            return (domain == D.UNK) ? string.Empty : domain.ToString();
        }

        #endregion Derived Properties

        #region Collection-Friendliness

        public override int GetHashCode()
        {
            int result = (int)m_subject
                ^ (int)m_grade
                ^ (int)m_claim;
            if (m_target != null)
                result ^= m_target.GetHashCode();
            if (m_ccss != null)
                result ^= m_ccss.GetHashCode();
            return result;
        }

        public int CompareTo(ContentSpecId other)
        {
            int result = (int)m_subject - (int)other.m_subject;
            if (result != 0) return result;

            result = (int)m_grade - (int)other.m_grade;
            if (result != 0) return result;

            result = (int)m_claim - (int)other.m_claim;
            if (result != 0) return result;

            result = string.CompareOrdinal(m_target, other.m_target);
            if (result != 0) return result;

            result = string.CompareOrdinal(m_ccss, other.m_ccss);
            if (result != 0) return result;

            return string.CompareOrdinal(m_domain, other.m_domain);
        }

        public bool Equals(ContentSpecId other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(object obj)
        {
            ContentSpecId other = obj as ContentSpecId;
            if (other == null) return -1;
            return CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            ContentSpecId other = obj as ContentSpecId;
            if (other == null) return false;
            return CompareTo(other) == 0;
        }

        #endregion Collection-Friendliness

        #region Static Parsing Methods

        const int c_maxElaTarget = 14;

        /// <summary>
        /// Parses an ELA target into an integer.
        /// </summary>
        /// <param name="target">The target in string form.</param>
        /// <returns>If successful, an integer in the range 1-14. If the value is not in a
        /// recognized format, returns 0.</returns>
        /// <remarks>
        /// <para>ELA targets are integers in the range 1-14. They may have a single lower-case
        /// letter suffix (e.g. 1a, 1b, 3a, 3b). When used in Enhanced format identifiers they
        /// may have a "T" prefix. This method strips the prefix and suffix and only
        /// parses the integer part of the target.</para>
        /// </remarks>
        static int ParseElaTarget(string target)
        {
            if (string.IsNullOrEmpty(target)) return 0;

            // Strip the prefix (if any)
            if (target.StartsWith("C"))
            {
                target = target.Substring(1);
            }

            // Strip the suffix (if any)
            int len = target.Length;
            if (target[len - 1] >= 'a' && target[len - 1] <= 'z')
            {
                target = target.Substring(0, len - 1);
            }

            // Parse the remaining integer
            int n;
            if (!int.TryParse(target, out n)) return 0;

            if (n < 1 || n > c_maxElaTarget) return 0;

            return n;
        }

        const int c_maxMathTarget = 16;

        /// <summary>
        /// Parses an Math target into an integer.
        /// </summary>
        /// <param name="target">The target in string form.</param>
        /// <returns>If successful, an integer in the range 1-16 If the value is not in a
        /// recognized format, returns 0.</returns>
        /// <remarks>
        /// <para>Math targets are capital letters in the range A-P. When used in Enhanced
        /// format identifiers they may have a "T" prefix. This method strips the prefix
        /// converts the letter part into an integer between 1 and 16 inclusive where 'A' = 1.</para>
        /// </remarks>
        static int ParseMathTarget(string target)
        {
            int value;
            if (target.Length == 2 && target[0] == 'T')
            {
                value = (target[1] - ('A' - 1));
            }
            else if (target.Length == 1)
            {
                value = (target[0] - ('A' - 1));
            }
            else
            {
                value = 0;
            }

            if (value < 0 || value > c_maxMathTarget) return 0;
            return value;
        }

        #endregion Static Parsing Methods

        #region Standard Targets and Domains plus Lookup Tables

        const MathTarget c_firstMathTarget = MathTarget.TA;
        const MathTarget c_lastMathTarget = MathTarget.TP;

        // Target Type Enum
        private enum MathTarget : int
        {
            Unspecified = 0,

            // Math
            TA = 21,
            TB = 22,
            TC = 23,
            TD = 24,
            TE = 25,
            TF = 26,
            TG = 27,
            TH = 28,
            TI = 29,
            TJ = 30,
            TK = 31,
            TL = 32,
            TM = 33,
            TN = 34,
            TO = 35,
            TP = 36
        }

        // Standard Domains (others may be used)
        // Name is short because these constants are used in tables below.
        public enum D : int
        {
            UNK = -1,   // Unknown / Unspecified
            NA = 0,     // Not applicable

            // ELA Domains
            RL = 1,     // Reading Literary Texts
            RI = 2,     // Reading Informational Texts
            WN = 3,     // Writing Narrative
            WI = 4,     // Writing Informational (Grades 3-5)
            WO = 5,     // Writing Opinion (Grades 3-5)
            WE = 6,     // Writing Explanatory (Grades 6-HS)
            WA = 7,     // Writing Argumentative (Grades 6-HS)
            WG = 8,     // Writing General
            SL = 9,     // Speaking and Listening
            R = 10,    // Research / Inquiry

            // Math Domains
            OA = 21,   // Operations and Algebraic Thinking
            NBT = 22,   // Number and Operations - Base 10
            NF = 23,   // Number and Operations - Fractions
            MD = 24,   // Measurement and Data
            G = 25,   // Geometry
            F = 26,   // Functions
            RP = 27,   // Ratios and Proportional Relationships
            NS = 28,   // The Number System
            EE = 29,   // Expressions & Equations
            SP = 20,   // Statistics & Probability

            // High School Math Conceptual Categories
            N = 31,      // Number and Quantity
            A = 32,      // Algebra
            S = 33       // Statistics and Probability

            // High School Math Conceptual Categories already included in the domains
            // F           // Functions
            // G           // Geometry

        }

        // Math claim 1 targets map to a Domain in grades 3-8.
        // This two-dimensional array maps grade and target to (Conceptual Category)-Domain
        const int c_MathDomains3to8GradeMin = 3;
        const int c_MathDomains3to8GradeCount = 6;
        const int c_MathDomains3to8TargetMin = 1;
        const int c_MathDomains3to8TargetCount = 12;
        static D[,] s_MathDomains3to8 = new D[6, 12]
        {
            // Target:          A     B     C     D     E     F     G     H     I     J     K     L
            /* Grade 03 */ { D.OA, D.OA, D.OA, D.OA, D.NBT,D.NF, D.MD, D.MD, D.MD, D.MD, D.G,  D.UNK },
            /* Grade 04 */ { D.OA, D.OA, D.OA, D.NBT,D.NBT,D.NF, D.NF, D.NF, D.MD, D.MD, D.MD, D.G   },
            /* Grade 05 */ { D.OA, D.OA, D.NBT,D.NBT,D.NF, D.NF, D.MD, D.MD, D.MD, D.G,  D.G,  D.UNK },
            /* Grade 06 */ { D.RP, D.NS, D.NS, D.NS, D.EE, D.EE, D.EE, D.G,  D.SP, D.SP, D.UNK,D.UNK },
            /* Grade 07 */ { D.RP, D.NS, D.EE, D.EE, D.G,  D.G,  D.SP, D.SP, D.SP, D.UNK,D.UNK,D.UNK },
            /* Grade 08 */ { D.NS, D.EE, D.EE, D.EE, D.F,  D.F,  D.G,  D.G,  D.G,  D.SP, D.UNK,D.UNK }
        };

        // Math claim 1 targets map to a Conceptual Category in High School.
        static D[] s_MathDomainsHighSchool = new D[]
        {
        //   A    B    C    D    E    F    G    H    I    J    K    L    M    N    O    P
            D.N, D.N, D.N, D.A, D.A, D.A, D.A, D.A, D.A, D.A, D.F, D.F, D.F, D.F, D.G, D.S
        };

        // Math claim 1 targets are either Main (m) or Additional/Supporting (a/s)
        // In these arrays, "true" indicates Main.
        const int c_MathEmphasis3to8GradeMin = 3;
        const int c_MathEmphasis3to8GradeCount = 6;
        const int c_MathEmphasis3to8TargetMin = 1;
        const int c_MathEmphasis3to8TargetCount = 12;
        static bool[,] s_MathEmphasis3to8 = new bool[6, 12]
        {
            // Target:          A      B      C      D      E      F      G      H      I      J      K      L
            /* Grade 03 */ {  true,  true,  true,  true, false,  true,  true, false,  true, false, false, false },
            /* Grade 04 */ {  true, false, false,  true,  true,  true,  true,  true, false, false, false, false },
            /* Grade 05 */ { false, false,  true,  true,  true,  true, false, false,  true, false, false, false },
            /* Grade 06 */ {  true,  true, false,  true,  true,  true,  true, false, false, false, false, false },
            /* Grade 07 */ {  true,  true,  true,  true, false, false, false, false, false, false, false, false },
            /* Grade 08 */ { false,  true,  true,  true,  true,  true,  true,  true, false, false, false, false },
        };

        static bool[] s_MathEmphasisHighSchool = new bool[]
        {
        //    A      B      C     D     E     F     G     H     I     J     K     L     M     N      O      P
            false, false, false, true, true, true, true, true, true, true, true, true, true, true, false, false
        };

        const int c_MathTargetSetGradeMin = 3;
        const int c_MathTargetSetGradeCount = 9;
        const int c_mathTargetSetTargetMin = 1;
        const int c_MathTargetSetTargetCount = 16;
        static int[,] s_MathTargetSet = new int[9, 16]
        {
            // Target:       A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P
            /* Grade  3 */ { 3 ,1 ,1 ,2 ,4 ,2 ,1 ,5 ,1 ,4 ,4 ,0 ,0 ,0 ,0 ,0 },
            /* Grade  4 */ { 1 ,6 ,6 ,3 ,1 ,1 ,2 ,4 ,5 ,6 ,5 ,7 ,0 ,0 ,0 ,0 },
            /* Grade  5 */ { 5 ,5 ,3 ,3 ,1 ,2 ,5 ,5 ,1 ,4 ,4 ,0 ,0 ,0 ,0 ,0 },
            /* Grade  6 */ { 2 ,3 ,5 ,4 ,1 ,1 ,3 ,5 ,5 ,5 ,0 ,0 ,0 ,0 ,0 ,0 },
            /* Grade  7 */ { 1 ,2 ,2 ,1 ,3 ,3 ,4 ,4 ,4 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            /* Grade  8 */ { 4 ,2 ,1 ,1 ,2 ,3 ,2 ,3 ,4 ,4 ,0 ,0 ,0 ,0 ,0 ,0 },
            /* N/A      */ { 0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            /* N/A      */ { 0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 ,0 },
            /* Grade HS */ { 9 ,9,10 ,1 ,1 ,2 ,3 ,3 ,3 ,4 ,5 ,6 ,6 ,6 ,7 ,8 }
        };

        #endregion Lookup Tables

    } // Class StandardId
}