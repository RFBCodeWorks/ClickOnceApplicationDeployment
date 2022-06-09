using System.Runtime.InteropServices;
using Syroot.Windows.IO;

// Go through each KnownFolderType enumeration member.
foreach (KnownFolderType type in Enum.GetValues<KnownFolderType>())
{
    Console.WriteLine(type);
    KnownFolder knownFolder = new(type);
    writeInfo("Display Name", () => knownFolder.DisplayName);
    writeInfo("Current Path", () => knownFolder.Path);
    writeInfo("Default Path", () => knownFolder.DefaultPath);
    Console.WriteLine();
}

static void writeInfo(object header, Func<string> property)
{
    try
    {
        Console.Write($"{header}: ");
        Console.WriteLine(property());
    }
    catch (ExternalException ex)
    {
        Console.WriteLine($"<Exception> {ex.InnerException?.Message ?? ex.Message}");
    }
}
