namespace TabulateSmarterTestAdminPackage.Common.AttributeValidation
{
    public static class Version
    {
        public static bool IsValidVersionDecimal(string versionString)
        {
            decimal version;
            return versionString.Length < 20
                   && decimal.TryParse(versionString, out version)
                   && version >= 0;
        }

        public static bool IsValidVersionInt(string versionString)
        {
            int version;
            return versionString.Length < 10
                   && int.TryParse(versionString, out version)
                   && version >= 0;
        }
    }
}
