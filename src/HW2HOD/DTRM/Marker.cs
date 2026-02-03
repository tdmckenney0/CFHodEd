using CFHodEd.Math;
using HW2IFF;

namespace HW2HOD;

/// <summary>
/// Class representing a Homeworld2 marker (hardpoint).
/// </summary>
public sealed class Marker
{
    private string _name = "Marker";
    private string _parentName = "Root";
    private Vector3 _position;
    private Vector3 _rotation;

    public Marker() { }

    public Marker(Marker m)
    {
        _name = m._name;
        _parentName = m._parentName;
        _position = m._position;
        _rotation = m._rotation;
    }

    /// <summary>Gets or sets the marker name.</summary>
    public string Name
    {
        get => _name;
        set => _name = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>Gets or sets the parent joint name.</summary>
    public string ParentName
    {
        get => _parentName;
        set => _parentName = value ?? "";
    }

    /// <summary>Gets or sets the marker position.</summary>
    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>Gets or sets the marker rotation.</summary>
    public Vector3 Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    /// <summary>Gets the marker transform matrix.</summary>
    public Matrix Transform =>
        Matrix.RotationX(_rotation.X) *
        Matrix.RotationY(_rotation.Y) *
        Matrix.RotationZ(_rotation.Z) *
        Matrix.Translation(_position);

    public override string ToString() => _name;

    /// <summary>Reads marker from IFF.</summary>
    internal void ReadIFF(IFFReader iff)
    {
        _name = iff.ReadString();
        _parentName = iff.ReadString();
        _position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _rotation = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
    }

    /// <summary>Writes marker to IFF.</summary>
    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_name);
        iff.Write(_parentName);
        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);
        iff.Write(_rotation.X);
        iff.Write(_rotation.Y);
        iff.Write(_rotation.Z);
    }
}
