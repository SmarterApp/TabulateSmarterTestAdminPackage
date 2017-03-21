using System.Collections.Generic;
using SmarterTestPackage.Common.Data;

namespace ProcessSmarterTestPackage.PostProcessors
{
    public interface IPostProcessor
    {
        IList<ValidationError> GenerateErrors();
    }
}