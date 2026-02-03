using CFHodEd.Math;

namespace HW2MAD;

/// <summary>
/// Interface for joints that can be animated by MAD files.
/// </summary>
/// <remarks>
/// This interface abstracts the HOD.Joint dependency so HW2MAD can be
/// compiled independently of HW2HOD during the porting process.
/// </remarks>
public interface IJoint
{
    /// <summary>Gets the joint name.</summary>
    string Name { get; }
    
    /// <summary>Gets or sets the joint position.</summary>
    Vector3 Position { get; set; }
    
    /// <summary>Gets or sets the joint rotation (Euler angles).</summary>
    Vector3 Rotation { get; set; }
    
    /// <summary>Gets or sets the joint scale.</summary>
    Vector3 Scale { get; set; }
}

/// <summary>
/// Interface for HOD files that contain joints to animate.
/// </summary>
public interface IHodFile
{
    /// <summary>Gets a joint by name.</summary>
    IJoint? GetJointByName(string name);
}
