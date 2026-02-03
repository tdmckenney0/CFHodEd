using System.Diagnostics;
using CFHodEd.Math;
using HW2IFF;
using HW2MAD;

namespace HW2HOD;

/// <summary>
/// Class representing a Homeworld2 joint (bone) in a HOD file.
/// </summary>
public sealed class Joint : IJoint
{
    private string _name = "Root";
    private bool _visible = true;
    private Vector3 _position;
    private Vector3 _rotation;
    private Vector3 _scale = new(1, 1, 1);
    private Vector3 _axis;
    private Vector3 _degreeOfFreedom;
    private readonly EventList<Joint> _children = new();

    public Joint() { }

    /// <summary>Copy constructor.</summary>
    public Joint(Joint j)
    {
        _name = j._name;
        _visible = j._visible;
        _position = j._position;
        _rotation = j._rotation;
        _scale = j._scale;
        _axis = j._axis;
        _degreeOfFreedom = j._degreeOfFreedom;

        foreach (var child in j._children)
            _children.Add(new Joint(child));
    }

    /// <summary>Gets or sets the joint name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            _name = value;
        }
    }

    /// <summary>Gets or sets visibility.</summary>
    public bool Visible
    {
        get => _visible;
        set => _visible = value;
    }

    /// <summary>Gets the children list.</summary>
    public IList<Joint> Children => _children;

    /// <summary>Gets or sets the joint position.</summary>
    public Vector3 Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>Gets or sets the joint rotation (Euler angles in radians).</summary>
    public Vector3 Rotation
    {
        get => _rotation;
        set => _rotation = value;
    }

    /// <summary>Gets or sets the joint scale.</summary>
    public Vector3 Scale
    {
        get => _scale;
        set => _scale = value;
    }

    /// <summary>Gets or sets the joint axis.</summary>
    public Vector3 Axis
    {
        get => _axis;
        set => _axis = value;
    }

    /// <summary>Gets or sets degree of freedom.</summary>
    public Vector3 DegreeOfFreedom
    {
        get => _degreeOfFreedom;
        set
        {
            _degreeOfFreedom.X = value.X != 0 ? 1 : 0;
            _degreeOfFreedom.Y = value.Y != 0 ? 1 : 0;
            _degreeOfFreedom.Z = value.Z != 0 ? 1 : 0;
        }
    }

    /// <summary>Gets the local transform matrix.</summary>
    public Matrix LocalTransform
    {
        get
        {
            return Matrix.Scaling(_scale) *
                   Matrix.RotationX(_rotation.X + _axis.X) *
                   Matrix.RotationY(_rotation.Y + _axis.Y) *
                   Matrix.RotationZ(_rotation.Z + _axis.Z) *
                   Matrix.Translation(_position);
        }
    }

    /// <summary>Gets the world transform matrix (up to root).</summary>
    public Matrix Transform
    {
        get
        {
            // For now, return local transform
            // Full implementation requires parent tracking
            return LocalTransform;
        }
    }

    public override string ToString() => _name;

    /// <summary>Returns the total count of this joint and all descendants.</summary>
    public int Count()
    {
        var queue = new Queue<Joint>();
        int count = 0;

        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var j = queue.Dequeue();
            count++;
            foreach (var child in j._children)
                queue.Enqueue(child);
        }

        return count;
    }

    /// <summary>Returns all joints as a flat array.</summary>
    public Joint[] ToArray()
    {
        var queue = new Queue<Joint>();
        var result = new List<Joint>();

        queue.Enqueue(this);
        while (queue.Count > 0)
        {
            var j = queue.Dequeue();
            result.Add(j);
            foreach (var child in j._children)
                queue.Enqueue(child);
        }

        return result.ToArray();
    }

    /// <summary>Gets a joint by name.</summary>
    public Joint? GetJointByName(string name)
    {
        if (_name == name)
            return this;

        foreach (var child in _children)
        {
            var result = child.GetJointByName(name);
            if (result != null)
                return result;
        }

        return null;
    }

    /// <summary>Reads joint from IFF.</summary>
    internal void ReadIFF(IFFReader iff, out string parentName)
    {
        _name = iff.ReadString();
        parentName = iff.ReadString();

        _position.X = iff.ReadSingle();
        _position.Y = iff.ReadSingle();
        _position.Z = iff.ReadSingle();

        _rotation.X = iff.ReadSingle();
        _rotation.Y = iff.ReadSingle();
        _rotation.Z = iff.ReadSingle();

        _scale.X = iff.ReadSingle();
        _scale.Y = iff.ReadSingle();
        _scale.Z = iff.ReadSingle();

        _axis.X = iff.ReadSingle();
        _axis.Y = iff.ReadSingle();
        _axis.Z = iff.ReadSingle();

        _degreeOfFreedom.X = iff.ReadInt32();
        _degreeOfFreedom.Y = iff.ReadInt32();
        _degreeOfFreedom.Z = iff.ReadInt32();

        _visible = iff.ReadInt32() != 0;
    }

    /// <summary>Writes joint to IFF.</summary>
    internal void WriteIFF(IFFWriter iff, string parentName)
    {
        iff.Write(_name);
        iff.Write(parentName);

        iff.Write(_position.X);
        iff.Write(_position.Y);
        iff.Write(_position.Z);

        iff.Write(_rotation.X);
        iff.Write(_rotation.Y);
        iff.Write(_rotation.Z);

        iff.Write(_scale.X);
        iff.Write(_scale.Y);
        iff.Write(_scale.Z);

        iff.Write(_axis.X);
        iff.Write(_axis.Y);
        iff.Write(_axis.Z);

        iff.WriteInt32((int)_degreeOfFreedom.X);
        iff.WriteInt32((int)_degreeOfFreedom.Y);
        iff.WriteInt32((int)_degreeOfFreedom.Z);

        iff.WriteInt32(_visible ? 1 : 0);
    }

    /// <summary>Reads HIER chunk from IFF.</summary>
    internal static void ReadHIERChunk(IFFReader iff, Joint root)
    {
        int count = iff.ReadInt32();
        var joints = new Dictionary<string, Joint>();
        var parentNames = new Dictionary<Joint, string>();

        root.ReadIFF(iff, out string rootParent);
        joints[root.Name] = root;
        parentNames[root] = rootParent;

        for (int i = 1; i < count; i++)
        {
            var joint = new Joint();
            joint.ReadIFF(iff, out string parentName);
            joints[joint.Name] = joint;
            parentNames[joint] = parentName;
        }

        // Build hierarchy
        foreach (var kvp in parentNames)
        {
            if (!string.IsNullOrEmpty(kvp.Value) && joints.TryGetValue(kvp.Value, out var parent))
            {
                if (kvp.Key != root)
                    parent._children.Add(kvp.Key);
            }
        }
    }

    /// <summary>Writes HIER chunk to IFF.</summary>
    internal static void WriteHIERChunk(IFFWriter iff, Joint root)
    {
        var stack = new Stack<(Joint joint, string parent)>();
        var processed = new List<(Joint joint, string parent)>();

        stack.Push((root, ""));

        while (stack.Count > 0)
        {
            var (joint, parent) = stack.Pop();
            processed.Add((joint, parent));

            for (int i = joint._children.Count - 1; i >= 0; i--)
                stack.Push((joint._children[i], joint.Name));
        }

        iff.Push("HIER");
        iff.WriteInt32(processed.Count);

        foreach (var (joint, parent) in processed)
            joint.WriteIFF(iff, parent);

        iff.Pop();
    }

    /// <summary>Initializes to default state.</summary>
    public void Initialize()
    {
        _name = "Root";
        _visible = true;
        _position = Vector3.Zero;
        _rotation = Vector3.Zero;
        _scale = new Vector3(1, 1, 1);
        _axis = Vector3.Zero;
        _degreeOfFreedom = Vector3.Zero;
        _children.Clear();
    }
}
