using System;

namespace SmarterTestPackage.Common.Data
{
    public abstract class ProcessingError
    {
        public string AssessmentId { get; set; }
        public string Value { get; set; }
        public ErrorSeverity ErrorSeverity { get; set; }
        public string ItemId { get; set; }
        public abstract string Message { get; }
        public string Location { get; set; }
        public string Key { get; set; }
        public string GeneratedMessage { get; set; }
        public PackageType PackageType { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is ProcessingError))
            {
                return false;
            }
            var other = (ProcessingError) obj;
            return AssessmentId.Equals(other.AssessmentId, StringComparison.OrdinalIgnoreCase) &&
                   Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase) &&
                   ErrorSeverity == other.ErrorSeverity && PackageType == other.PackageType &&
                   Message.Equals(other.Message, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}