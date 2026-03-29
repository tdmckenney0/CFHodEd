namespace CFHodEd.Tests.Godot;

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

    /// <summary>Returns the full path to a named fixture, or null if the file is not present.</summary>
    public static string? FindHod(string filename) =>
        _hodFilesDir is not null
            ? Directory.GetFiles(_hodFilesDir, filename, SearchOption.TopDirectoryOnly).FirstOrDefault()
            : null;
}
