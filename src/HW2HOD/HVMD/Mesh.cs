using CFHodEd.Math;
using HW2IFF;

namespace HW2HOD;

/// <summary>
/// HOD mesh vertex format.
/// </summary>
public struct HODVertex
{
    public Vector3 Position;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector2 TexCoords;

    public HODVertex(Vector3 pos, Vector3 normal, Vector2 tex)
    {
        Position = pos;
        Normal = normal;
        Tangent = Vector3.Zero;
        TexCoords = tex;
    }

    public readonly int Size => 44; // 3*4 + 3*4 + 3*4 + 2*4 = 44 bytes
}

/// <summary>
/// Represents a mesh LOD level.
/// </summary>
public sealed class MeshLOD
{
    private string _name = "Mesh";
    private string _parentJoint = "Root";
    private readonly List<HODVertex> _vertices = new();
    private readonly List<ushort> _indices = new();
    private int _materialIndex;
    private BoundingBox _bounds;

    public MeshLOD() { }

    public MeshLOD(MeshLOD m)
    {
        _name = m._name;
        _parentJoint = m._parentJoint;
        _vertices.AddRange(m._vertices);
        _indices.AddRange(m._indices);
        _materialIndex = m._materialIndex;
        _bounds = m._bounds;
    }

    public string Name
    {
        get => _name;
        set => _name = value ?? "";
    }

    public string ParentJoint
    {
        get => _parentJoint;
        set => _parentJoint = value ?? "";
    }

    public List<HODVertex> Vertices => _vertices;
    public List<ushort> Indices => _indices;

    public int MaterialIndex
    {
        get => _materialIndex;
        set => _materialIndex = value;
    }

    public BoundingBox Bounds
    {
        get => _bounds;
        set => _bounds = value;
    }

    public int TriangleCount => _indices.Count / 3;
    public int VertexCount => _vertices.Count;

    public override string ToString() => _name;

    public void RecalculateBounds()
    {
        if (_vertices.Count == 0)
        {
            _bounds = new BoundingBox(Vector3.Zero, Vector3.Zero);
            return;
        }

        var min = _vertices[0].Position;
        var max = _vertices[0].Position;

        for (int i = 1; i < _vertices.Count; i++)
        {
            var pos = _vertices[i].Position;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }

        _bounds = new BoundingBox(min, max);
    }

    internal void ReadIFF(IFFReader iff)
    {
        _name = iff.ReadString();
        _parentJoint = iff.ReadString();

        int vertexCount = iff.ReadInt32();
        int indexCount = iff.ReadInt32();
        _materialIndex = iff.ReadInt32();

        _vertices.Clear();
        for (int i = 0; i < vertexCount; i++)
        {
            var v = new HODVertex
            {
                Position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle()),
                Normal = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle()),
                Tangent = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle()),
                TexCoords = new Vector2(iff.ReadSingle(), iff.ReadSingle())
            };
            _vertices.Add(v);
        }

        _indices.Clear();
        for (int i = 0; i < indexCount; i++)
            _indices.Add((ushort)iff.ReadUInt16());

        // Read bounds
        var min = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        var max = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
        _bounds = new BoundingBox(min, max);
    }

    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(_name);
        iff.Write(_parentJoint);

        iff.WriteInt32(_vertices.Count);
        iff.WriteInt32(_indices.Count);
        iff.WriteInt32(_materialIndex);

        foreach (var v in _vertices)
        {
            iff.Write(v.Position.X);
            iff.Write(v.Position.Y);
            iff.Write(v.Position.Z);
            iff.Write(v.Normal.X);
            iff.Write(v.Normal.Y);
            iff.Write(v.Normal.Z);
            iff.Write(v.Tangent.X);
            iff.Write(v.Tangent.Y);
            iff.Write(v.Tangent.Z);
            iff.Write(v.TexCoords.X);
            iff.Write(v.TexCoords.Y);
        }

        foreach (var idx in _indices)
            iff.WriteUInt16(idx);

        iff.Write(_bounds.Min.X);
        iff.Write(_bounds.Min.Y);
        iff.Write(_bounds.Min.Z);
        iff.Write(_bounds.Max.X);
        iff.Write(_bounds.Max.Y);
        iff.Write(_bounds.Max.Z);
    }
}

/// <summary>
/// Represents a complete mesh with multiple LODs.
/// </summary>
public sealed class Mesh
{
    private string _name = "Mesh";
    private readonly EventList<MeshLOD> _lods = new();

    public Mesh() { }

    public Mesh(Mesh m)
    {
        _name = m._name;
        foreach (var lod in m._lods)
            _lods.Add(new MeshLOD(lod));
    }

    public string Name
    {
        get => _name;
        set => _name = value ?? "";
    }

    public IList<MeshLOD> LODs => _lods;

    public override string ToString() => _name;
}
