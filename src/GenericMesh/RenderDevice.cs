namespace GenericMesh;

/// <summary>
/// Abstract interface for a rendering device.
/// Replaces Direct3D Device for cross-platform rendering.
/// </summary>
public interface IRenderDevice
{
    /// <summary>
    /// Sets a render state.
    /// </summary>
    void SetRenderState(RenderState state, int value);

    /// <summary>
    /// Sets a texture stage state.
    /// </summary>
    void SetTextureStageState(int stage, TextureStageState state, int value);

    /// <summary>
    /// Sets a texture on a stage.
    /// </summary>
    void SetTexture(int stage, ITexture? texture);

    /// <summary>
    /// Sets the material.
    /// </summary>
    void SetMaterial(MaterialProperties material);
}

/// <summary>
/// Abstract texture interface.
/// </summary>
public interface ITexture : IDisposable
{
    /// <summary>
    /// Width of the texture in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Height of the texture in pixels.
    /// </summary>
    int Height { get; }
}

/// <summary>
/// Render state enumeration - replaces Direct3D RenderState.
/// </summary>
public enum RenderState
{
    ZEnable,
    FillMode,
    ShadeMode,
    ZWriteEnable,
    AlphaTestEnable,
    LastPixel,
    SourceBlend,
    DestinationBlend,
    CullMode,
    ZFunc,
    AlphaRef,
    AlphaFunc,
    DitherEnable,
    AlphaBlendEnable,
    FogEnable,
    SpecularEnable,
    FogColor,
    FogTableMode,
    FogStart,
    FogEnd,
    FogDensity,
    RangeFogEnable,
    StencilEnable,
    StencilFail,
    StencilZFail,
    StencilPass,
    StencilFunc,
    StencilRef,
    StencilMask,
    StencilWriteMask,
    TextureFactor,
    Wrap0,
    Wrap1,
    Wrap2,
    Wrap3,
    Wrap4,
    Wrap5,
    Wrap6,
    Wrap7,
    Clipping,
    Lighting,
    Ambient,
    FogVertexMode,
    ColorVertex,
    LocalViewer,
    NormalizeNormals,
    DiffuseMaterialSource,
    SpecularMaterialSource,
    AmbientMaterialSource,
    EmissiveMaterialSource,
    VertexBlend,
    ClipPlaneEnable,
    PointSize,
    PointSizeMin,
    PointSpriteEnable,
    PointScaleEnable,
    PointScaleA,
    PointScaleB,
    PointScaleC,
    MultiSampleAntiAlias,
    MultiSampleMask,
    PatchEdgeStyle,
    DebugMonitorToken,
    PointSizeMax,
    IndexedVertexBlendEnable,
    ColorWriteEnable,
    TweenFactor,
    BlendOperation,
    PositionDegree,
    NormalDegree,
    ScissorTestEnable,
    SlopeScaleDepthBias,
    AntiAliasedLineEnable,
    MinTessellationLevel,
    MaxTessellationLevel,
    AdaptiveTessX,
    AdaptiveTessY,
    AdaptiveTessZ,
    AdaptiveTessW,
    EnableAdaptiveTessellation,
    TwoSidedStencilMode,
    CcwStencilFail,
    CcwStencilZFail,
    CcwStencilPass,
    CcwStencilFunc,
    ColorWriteEnable1,
    ColorWriteEnable2,
    ColorWriteEnable3,
    BlendFactor,
    SrgbWriteEnable,
    DepthBias,
    Wrap8,
    Wrap9,
    Wrap10,
    Wrap11,
    Wrap12,
    Wrap13,
    Wrap14,
    Wrap15,
    SeparateAlphaBlendEnable,
    SourceBlendAlpha,
    DestinationBlendAlpha,
    BlendOperationAlpha,
}

/// <summary>
/// Texture stage state enumeration.
/// </summary>
public enum TextureStageState
{
    ColorOperation,
    ColorArg1,
    ColorArg2,
    AlphaOperation,
    AlphaArg1,
    AlphaArg2,
    BumpEnvMat00,
    BumpEnvMat01,
    BumpEnvMat10,
    BumpEnvMat11,
    TexCoordIndex,
    BumpEnvLScale,
    BumpEnvLOffset,
    TextureTransformFlags,
    ColorArg0,
    AlphaArg0,
    ResultArg,
    Constant,
}

/// <summary>
/// Material properties for a surface.
/// </summary>
public struct MaterialProperties
{
    public CFHodEd.Math.ColorValue Diffuse;
    public CFHodEd.Math.ColorValue Ambient;
    public CFHodEd.Math.ColorValue Specular;
    public CFHodEd.Math.ColorValue Emissive;
    public float SpecularSharpness;
}
