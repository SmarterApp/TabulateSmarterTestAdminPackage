namespace SmarterTestPackage.Common.Data
{
    public class CrossPackageValidationError : ProcessingError
    {
        public string PrimarySource { get; set; }
        public string SecondarySource { get; set; }

        public override string Message
            => $"{Location} attribute {Key} violates {GeneratedMessage} between {PrimarySource} and {SecondarySource}";
    }
}