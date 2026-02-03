// TODO: Port WavefrontObject translator - requires full GBasicMesh implementation
// This module provides import/export of Wavefront OBJ mesh format

namespace GenericMesh.Translators;

/// <summary>
/// Wavefront Object translator for reading and writing OBJ files.
/// </summary>
/// <remarks>
/// NOTE: The reader/writer are loosely implemented. Not all features of the 
/// wavefront object specification are implemented, and some are added as convenience.
/// </remarks>
public static class WavefrontObject
{
    /// <summary>Formatting for input/output (invariant culture).</summary>
    internal static readonly IFormatProvider FormatProvider = 
        System.Globalization.CultureInfo.CreateSpecificCulture("en-US");

    // TODO: Implement ReadFile and WriteFile once GBasicMesh is fully ported
}
