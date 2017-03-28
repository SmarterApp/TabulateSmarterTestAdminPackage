using System.Collections.Generic;

namespace SmarterTestPackage.Common.Data
{
    public class OptionalStringOrErrorList
    {
        public OptionalStringOrErrorList()
        {
            Errors = new List<string>();
        }

        public string Value { get; set; }
        public IList<string> Errors { get; set; }
    }
}