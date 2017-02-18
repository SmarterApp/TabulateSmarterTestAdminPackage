using System;
using System.IO;
using TabulateSmarterTestAdminPackage.Common.RestrictedValues.Enums;

namespace TabulateSmarterTestAdminPackage.Common.Validators
{
    public class FilePathValidator : Validator
    {
        public FilePathValidator(ErrorSeverity errorSeverity, object parameter = null) : base(errorSeverity, parameter) {}
        //TODO: Validate w/JT that there isn't a better/easier way to do this
        public override bool IsValid(object value)
        {
            try
            {
                new FileInfo(value as string);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public override string GetMessage()
        {
            return "[ValidFilePathRequired]";
        }
    }
}