using CFHodEd.Math;

namespace GenericMesh.VertexFields;

/// <summary>
/// Enables a structure implementing IVertex to have 1 set of texture coordinates.
/// </summary>
public interface IVertexTex1 : IVertex
{
    /// <summary>
    /// Retrieves texture coordinates.
    /// </summary>
    /// <param name="index">The texture coordinate set to retrieve (0).</param>
    float GetTexCoords1(int index = 0);

    /// <summary>
    /// Sets texture coordinates.
    /// </summary>
    /// <param name="v">The texture coordinate.</param>
    /// <param name="index">The texture coordinate set to set.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetTexCoords1(float v, int index = 0);
}

/// <summary>
/// Enables a structure implementing IVertex to have texture coordinates (2D).
/// </summary>
public interface IVertexTex2 : IVertex
{
    /// <summary>
    /// Retrieves texture coordinates.
    /// </summary>
    /// <param name="index">The texture coordinate set to retrieve.</param>
    Vector2 GetTexCoords2(int index = 0);

    /// <summary>
    /// Sets texture coordinates.
    /// </summary>
    /// <param name="v">The texture coordinates.</param>
    /// <param name="index">The texture coordinate set to set.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetTexCoords2(Vector2 v, int index = 0);
}

/// <summary>
/// Enables a structure implementing IVertex to have 3D texture coordinates.
/// </summary>
public interface IVertexTex3 : IVertex
{
    /// <summary>
    /// Retrieves texture coordinates.
    /// </summary>
    /// <param name="index">The texture coordinate set to retrieve.</param>
    Vector3 GetTexCoords3(int index = 0);

    /// <summary>
    /// Sets texture coordinates.
    /// </summary>
    /// <param name="v">The texture coordinates.</param>
    /// <param name="index">The texture coordinate set to set.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetTexCoords3(Vector3 v, int index = 0);
}

/// <summary>
/// Enables a structure implementing IVertex to have 4D texture coordinates.
/// </summary>
public interface IVertexTex4 : IVertex
{
    /// <summary>
    /// Retrieves texture coordinates.
    /// </summary>
    /// <param name="index">The texture coordinate set to retrieve.</param>
    Vector4 GetTexCoords4(int index = 0);

    /// <summary>
    /// Sets texture coordinates.
    /// </summary>
    /// <param name="v">The texture coordinates.</param>
    /// <param name="index">The texture coordinate set to set.</param>
    /// <returns>The modified vertex.</returns>
    IVertex SetTexCoords4(Vector4 v, int index = 0);
}
