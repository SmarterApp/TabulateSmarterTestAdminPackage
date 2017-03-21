using System.Runtime.InteropServices;

namespace TabulateSmarterTestPackage
{
    internal static class ConsoleHelper
    {
        public static bool IsSoleConsoleOwner
        {
            get
            {
                var procIds = new uint[4];
                var count = GetConsoleProcessList(procIds, (uint) procIds.Length);
                return count <= 1;
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetConsoleProcessList(
            uint[] ProcessList,
            uint ProcessCount
        );
    }
}