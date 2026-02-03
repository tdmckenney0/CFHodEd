namespace GenericMesh;

/// <summary>
/// Defines the generalized properties that a material implements to
/// create a suitable material type for the generic mesh class.
/// </summary>
public interface IMaterial : IEquatable<IMaterial>
{
    /// <summary>
    /// Sets the default properties.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Applies the material to a render device.
    /// </summary>
    /// <param name="device">The device whose states are set to apply the material.</param>
    void Apply(IRenderDevice device);

    /// <summary>
    /// Removes the effect of the material; i.e. sets the 
    /// device to its normal state (only for the states changed).
    /// </summary>
    /// <param name="device">The device whose states are set to normal states.</param>
    void Reset(IRenderDevice device);
}
