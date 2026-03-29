namespace CFHodEd.Tests;

/// <summary>
/// Resolves paths to HOD test fixtures in test/hod-files/ without copying them to the output.
/// </summary>
internal static class TestFixtures
{
    private static readonly string? _hodFilesDir = FindHodFilesDir();

    private static string? FindHodFilesDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "test", "hod-files");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }
        return null;
    }

    /// <summary>Enumerates all .hod files in test/hod-files/. Empty when the directory is absent.</summary>
    public static IEnumerable<string> HodFiles()
    {
        if (_hodFilesDir is null)
            yield break;
        foreach (var file in Directory.GetFiles(_hodFilesDir, "*.hod"))
            yield return file;
    }

    /// <summary>Returns the full path to a named fixture, or null if the file is not present.</summary>
    public static string? FindHod(string filename) =>
        _hodFilesDir is not null
            ? Directory.GetFiles(_hodFilesDir, filename, SearchOption.TopDirectoryOnly).FirstOrDefault()
            : null;
}
