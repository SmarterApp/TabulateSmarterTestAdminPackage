using System;

namespace TabulateSmarterTestAdminPackage.Exceptions
{
    public class IncorrectPackageTypeException : Exception
    {
        public IncorrectPackageTypeException(string message) : base(message) {}
    }
}