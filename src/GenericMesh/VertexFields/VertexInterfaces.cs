using CFHodEd.Math;

namespace GenericMesh.VertexFields;

/// <summary>
/// Enables a structure implementing IVertex to have a position (3D) field.
/// </summary>
public interface IVertexPosition3 : IVertex
{
    /// <summary>
    /// Retrieves 3D coordinates of the vertex.
    /// </summary>
    Vector3 GetPosition3();

    /// <summary>
    /// Sets 3D coordinates of the vertex.
    /// </summary>
    /// <param name="v">The coordinates.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetPosition3(Vector3 v);
}

/// <summary>
/// Enables a structure implementing IVertex to have a 4D position field.
/// </summary>
public interface IVertexPosition4 : IVertex
{
    /// <summary>
    /// Retrieves 4D coordinates of the vertex.
    /// </summary>
    Vector4 GetPosition4();

    /// <summary>
    /// Sets 4D coordinates of the vertex.
    /// </summary>
    /// <param name="v">The coordinates.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetPosition4(Vector4 v);
}

/// <summary>
/// Enables a structure implementing IVertex to have a normal field.
/// </summary>
public interface IVertexNormal3 : IVertex
{
    /// <summary>
    /// Retrieves normal of the vertex.
    /// </summary>
    Vector3 GetNormal3();

    /// <summary>
    /// Sets normal of the vertex.
    /// </summary>
    /// <param name="v">The normal.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetNormal3(Vector3 v);
}

/// <summary>
/// Enables a structure implementing IVertex to have a diffuse color field.
/// </summary>
public interface IVertexDiffuse : IVertex
{
    /// <summary>
    /// Retrieves diffuse color of the vertex.
    /// </summary>
    ColorValue GetDiffuse();

    /// <summary>
    /// Sets diffuse color of the vertex.
    /// </summary>
    /// <param name="v">The color.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetDiffuse(ColorValue v);
}

/// <summary>
/// Enables a structure implementing IVertex to have a specular color field.
/// </summary>
public interface IVertexSpecular : IVertex
{
    /// <summary>
    /// Retrieves specular color of the vertex.
    /// </summary>
    ColorValue GetSpecular();

    /// <summary>
    /// Sets specular color of the vertex.
    /// </summary>
    /// <param name="v">The color.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetSpecular(ColorValue v);
}

/// <summary>
/// Enables a structure implementing IVertex to have a point size field.
/// </summary>
public interface IVertexPointSize : IVertex
{
    /// <summary>
    /// Retrieves point size of the vertex.
    /// </summary>
    float GetPointSize();

    /// <summary>
    /// Sets point size of the vertex.
    /// </summary>
    /// <param name="v">The point size.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetPointSize(float v);
}

/// <summary>
/// Enables a structure implementing IVertex to be transformed by a matrix.
/// </summary>
public interface IVertexTransformable : IVertex
{
    /// <summary>
    /// Transforms the vertex by a matrix.
    /// </summary>
    /// <param name="m">The transforming matrix.</param>
    /// <returns>Modified vertex.</returns>
    IVertex Transform(Matrix m);
}
