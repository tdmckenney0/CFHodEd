using CFHodEd.Math;
using HW2IFF;

namespace HW2HOD;

/// <summary>
/// Class representing a Homeworld2 engine glow effect.
/// </summary>
public sealed class EngineGlow
{
    private string _parentName = "Root";
    private Vector3 _position;
    private Vector3 _direction = new(0, 0, 1);
    private float _size = 1.0f;

    public EngineGlow() { }

    public EngineGlow(EngineGlow e)
    {
        _parentName = e._parentName;
        _position = e._position;
        _direction = e._direction;
        _size = e._size;
    }

    public string ParentName
    {
        get => _parentName;
        set => _parentName = value ?? "";
    }

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector3 Direction
    {
        get => _direction;
        set => _direction = value;
    }

    public float Size
    {
        get => _size;
        set => _size = Math.Max(0, value);
    }

    internal void ReadIFF(IFFReader iff)
    {
        _parentName = iff.ReadString();
        _position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _direction = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _size = iff.ReadSingle();
    }

    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_parentName);
        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);
        iff.Write(_direction.X);
        iff.Write(_direction.Y);
        iff.Write(_direction.Z);
        iff.Write(_size);
    }
}

/// <summary>
/// Class representing a Homeworld2 engine burn effect.
/// </summary>
public sealed class EngineBurn
{
    private string _parentName = "Root";
    private Vector3 _position;
    private Vector3 _direction = new(0, 0, 1);
    private float _size = 1.0f;

    public EngineBurn() { }

    public EngineBurn(EngineBurn e)
    {
        _parentName = e._parentName;
        _position = e._position;
        _direction = e._direction;
        _size = e._size;
    }

    public string ParentName
    {
        get => _parentName;
        set => _parentName = value ?? "";
    }

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector3 Direction
    {
        get => _direction;
        set => _direction = value;
    }

    public float Size
    {
        get => _size;
        set => _size = Math.Max(0, value);
    }

    internal void ReadIFF(IFFReader iff)
    {
        _parentName = iff.ReadString();
        _position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _direction = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _size = iff.ReadSingle();
    }

    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_parentName);
        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);
        iff.Write(_direction.X);
        iff.Write(_direction.Y);
        iff.Write(_direction.Z);
        iff.Write(_size);
    }
}

/// <summary>
/// Class representing a Homeworld2 engine shape mesh reference.
/// </summary>
public sealed class EngineShape
{
    private string _parentName = "Root";
    private string _meshName = "";
    private Vector3 _position;
    private Vector3 _rotation;
    private float _size = 1.0f;

    public EngineShape() { }

    public EngineShape(EngineShape e)
    {
        _parentName = e._parentName;
        _meshName = e._meshName;
        _position = e._position;
        _rotation = e._rotation;
        _size = e._size;
    }

    public string ParentName
    {
        get => _parentName;
        set => _parentName = value ?? "";
    }

    public string MeshName
    {
        get => _meshName;
        set => _meshName = value ?? "";
    }

    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector3 Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    public float Size
    {
        get => _size;
        set => _size = Math.Max(0, value);
    }

    internal void ReadIFF(IFFReader iff)
    {
        _parentName = iff.ReadString();
        _meshName = iff.ReadString();
        _position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _rotation = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _size = iff.ReadSingle();
    }

    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_parentName);
        iff.Write(_meshName);
        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);
        iff.Write(_rotation.X);
        iff.Write(_rotation.Y);
        iff.Write(_rotation.Z);
        iff.Write(_size);
    }
}

/// <summary>
/// Class representing a Homeworld2 navigation light.
/// </summary>
public sealed class NavLight
{
    private string _name = "NavLight";
    private string _parentName = "Root";
    private Vector3 _position;
    private Vector3 _direction = new(0, 1, 0);
    private ColorValue _color = new(1, 1, 1, 1);
    private float _size = 1.0f;
    private float _frequency = 1.0f;
    private float _phase;
    private int _style;

    public NavLight() { }

    public NavLight(NavLight n)
    {
        _name = n._name;
        _parentName = n._parentName;
        _position = n._position;
        _direction = n._direction;
        _color = n._color;
        _size = n._size;
        _frequency = n._frequency;
        _phase = n._phase;
        _style = n._style;
    }

    public string Name { get => _name; set => _name = value ?? ""; }
    public string ParentName { get => _parentName; set => _parentName = value ?? ""; }
    public Vector3 Position { get => _position; set => _position = value; }
    public Vector3 Direction { get => _direction; set => _direction = value; }
    public ColorValue Color { get => _color; set => _color = value; }
    public float Size { get => _size; set => _size = Math.Max(0, value); }
    public float Frequency { get => _frequency; set => _frequency = value; }
    public float Phase { get => _phase; set => _phase = value; }
    public int Style { get => _style; set => _style = value; }

    public override string ToString() => _name;

    internal void ReadIFF(IFFReader iff)
    {
        _name = iff.ReadString();
        _parentName = iff.ReadString();
        _position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _direction = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _color = new ColorValue(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _size = iff.ReadSingle();
        _frequency = iff.ReadSingle();
        _phase = iff.ReadSingle();
        _style = iff.ReadInt32();
    }

    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_name);
        iff.Write(_parentName);
        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);
        iff.Write(_direction.X);
        iff.Write(_direction.Y);
        iff.Write(_direction.Z);
        iff.Write(_color.R);
        iff.Write(_color.G);
        iff.Write(_color.B);
        iff.Write(_color.A);
        iff.Write(_size);
        iff.Write(_frequency);
        iff.Write(_phase);
        iff.WriteInt32(_style);
    }
}
