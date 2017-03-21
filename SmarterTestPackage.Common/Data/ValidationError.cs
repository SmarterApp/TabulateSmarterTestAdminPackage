namespace SmarterTestPackage.Common.Data
{
    public class ValidationError : ProcessingError
    {
        public override string Message => $"{Location} attribute {Key} violates {GeneratedMessage}";
    }
}