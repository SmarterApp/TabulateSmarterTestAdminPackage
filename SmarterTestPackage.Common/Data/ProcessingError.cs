namespace SmarterTestPackage.Common.Data
{
    public abstract class ProcessingError
    {
        public string TestName { get; set; }
        public string Path { get; set; }
        public ErrorSeverity ErrorSeverity { get; set; }
        public string ItemId { get; set; }
        public abstract string Message { get; }
        public string Location { get; set; }
        public string Key { get; set; }
        public string GeneratedMessage { get; set; }
    }
}