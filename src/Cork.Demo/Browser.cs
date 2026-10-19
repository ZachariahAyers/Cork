using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Cork.Demo;

[SupportedOSPlatform("browser")]
internal static partial class Browser
{
    [JSExport]
    internal static string Plan(string input) => Planning.Run(input);

    [JSExport]
    internal static double Floor(double pickedZ) => Planning.Floor(pickedZ);

    private static void Main()
    {
    }
}
