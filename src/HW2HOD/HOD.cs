using CFHodEd.Math;
using HW2IFF;
using HW2MAD;

namespace HW2HOD;

/// <summary>
/// Constants for HOD file format.
/// </summary>
internal static class HODConstants
{
    public const string Name_MultiMesh = "Homeworld2 Multi Mesh File";
    public const string Name_Lighting = "Homeworld2 Background Lighting File";
    public const int Version_Standard = 0x200;
    public const int Version_Background = 1000;
}

/// <summary>
/// Class representing a Homeworld2 HOD (model) file.
/// </summary>
public sealed class HOD : IHodFile
{
    private int _version = HODConstants.Version_Standard;
    private string _name = HODConstants.Name_MultiMesh;

    // DTRM data (data tree)
    private readonly Joint _root = new();
    private readonly EventList<Marker> _markers = new();
    private readonly EventList<EngineGlow> _engineGlows = new();
    private readonly EventList<EngineBurn> _engineBurns = new();
    private readonly EventList<EngineShape> _engineShapes = new();
    private readonly EventList<NavLight> _navLights = new();

    // HVMD data (mesh data)
    private readonly EventList<Material> _materials = new();
    private readonly EventList<Mesh> _meshes = new();

    // Rendering properties
    private ColorValue _teamColor = new(0.5f, 0.5f, 0.5f, 1f);
    private ColorValue _stripeColor = new(0.5f, 0.5f, 0.5f, 1f);
    private float _thrusterPower;
    private string _badge = "";

    public HOD() { }

    /// <summary>Gets or sets the HOD version.</summary>
    public int Version
    {
        get => _version;
        set
        {
            if (value != HODConstants.Version_Standard && value != HODConstants.Version_Background)
                throw new ArgumentException("Invalid version value.");
            
            _version = value;
            _name = value == HODConstants.Version_Standard 
                ? HODConstants.Name_MultiMesh 
                : HODConstants.Name_Lighting;
        }
    }

    /// <summary>Gets or sets the HOD name.</summary>
    public string Name
    {
        get => _name;
        set
        {
            if (value == HODConstants.Name_MultiMesh)
            {
                _name = value;
                _version = HODConstants.Version_Standard;
            }
            else if (value == HODConstants.Name_Lighting)
            {
                _name = value;
                _version = HODConstants.Version_Background;
            }
            else
            {
                throw new ArgumentException("Invalid name value.");
            }
        }
    }

    // DTRM accessors
    public Joint Root => _root;
    public IList<Marker> Markers => _markers;
    public IList<EngineGlow> EngineGlows => _engineGlows;
    public IList<EngineBurn> EngineBurns => _engineBurns;
    public IList<EngineShape> EngineShapes => _engineShapes;
    public IList<NavLight> NavLights => _navLights;

    // HVMD accessors
    public IList<Material> Materials => _materials;
    public IList<Mesh> Meshes => _meshes;

    // Rendering properties
    public ColorValue TeamColor { get => _teamColor; set => _teamColor = value; }
    public ColorValue StripeColor { get => _stripeColor; set => _stripeColor = value; }
    public float ThrusterPower 
    { 
        get => _thrusterPower; 
        set => _thrusterPower = System.Math.Clamp(value, 0f, 1f); 
    }
    public string Badge { get => _badge; set => _badge = value ?? ""; }

    /// <summary>Gets a joint by name (IHodFile implementation).</summary>
    public IJoint? GetJointByName(string name) => _root.GetJointByName(name);

    /// <summary>Reads a HOD file from a stream.</summary>
    public void Read(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead)
            throw new ArgumentException("Stream must be readable.", nameof(stream));

        Initialize();

        var iff = new IFFReader(stream);
        iff.AddHandler("FORM", ChunkType.Form, ReadFormChunk);
        iff.Parse();
    }

    /// <summary>Writes a HOD file to a stream.</summary>
    public void Write(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (!stream.CanWrite)
            throw new ArgumentException("Stream must be writable.", nameof(stream));

        var iff = new IFFWriter(stream);

        iff.Push("FORM", ChunkType.Form);

        // Write VERS chunk
        iff.Push("VERS");
        iff.WriteInt32(_version);
        iff.Pop();

        // Write NAME chunk
        iff.Push("NAME");
        iff.Write(_name, _name.Length);
        iff.Pop();

        // Write HVMD, DTRM, INFO chunks for standard HODs
        if (_version == HODConstants.Version_Standard)
        {
            WriteHVMDChunk(iff);
            WriteDTRMChunk(iff);
            WriteINFOChunk(iff);
        }

        iff.Pop(); // FORM
    }

    private void ReadFormChunk(IFFReader iff, ChunkAttributes attrs)
    {
        iff.AddHandler("VERS", ChunkType.Default, ReadVERSChunk);
        iff.AddHandler("NAME", ChunkType.Default, ReadNAMEChunk);
        iff.AddHandler("HVMD", ChunkType.Form, ReadHVMDChunk);
        iff.AddHandler("DTRM", ChunkType.Form, ReadDTRMChunk);
        iff.AddHandler("INFO", ChunkType.Form, ReadINFOChunk);
        iff.Parse();
    }

    private void ReadVERSChunk(IFFReader iff, ChunkAttributes attrs)
    {
        _version = iff.ReadInt32();
    }

    private void ReadNAMEChunk(IFFReader iff, ChunkAttributes attrs)
    {
        _name = iff.ReadString(attrs.Size);
    }

    private void ReadHVMDChunk(IFFReader iff, ChunkAttributes attrs)
    {
        // HVMD reading - simplified for now
        iff.Parse();
    }

    private void ReadDTRMChunk(IFFReader iff, ChunkAttributes attrs)
    {
        iff.AddHandler("HIER", ChunkType.Default, (r, a) => Joint.ReadHIERChunk(r, _root));
        iff.AddHandler("MARK", ChunkType.Default, ReadMARKChunk);
        iff.AddHandler("ENGN", ChunkType.Form, ReadENGNChunk);
        iff.AddHandler("NAVL", ChunkType.Default, ReadNAVLChunk);
        iff.Parse();
    }

    private void ReadMARKChunk(IFFReader iff, ChunkAttributes attrs)
    {
        int count = iff.ReadInt32();
        _markers.Clear();
        for (int i = 0; i < count; i++)
        {
            var m = new Marker();
            m.ReadIFF(iff);
            _markers.Add(m);
        }
    }

    private void ReadENGNChunk(IFFReader iff, ChunkAttributes attrs)
    {
        iff.AddHandler("GLOW", ChunkType.Default, (r, a) =>
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var e = new EngineGlow();
                e.ReadIFF(r);
                _engineGlows.Add(e);
            }
        });
        iff.AddHandler("BURN", ChunkType.Default, (r, a) =>
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var e = new EngineBurn();
                e.ReadIFF(r);
                _engineBurns.Add(e);
            }
        });
        iff.AddHandler("SHAP", ChunkType.Default, (r, a) =>
        {
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var e = new EngineShape();
                e.ReadIFF(r);
                _engineShapes.Add(e);
            }
        });
        iff.Parse();
    }

    private void ReadNAVLChunk(IFFReader iff, ChunkAttributes attrs)
    {
        int count = iff.ReadInt32();
        _navLights.Clear();
        for (int i = 0; i < count; i++)
        {
            var n = new NavLight();
            n.ReadIFF(iff);
            _navLights.Add(n);
        }
    }

    private void ReadINFOChunk(IFFReader iff, ChunkAttributes attrs)
    {
        iff.Parse();
    }

    private void WriteHVMDChunk(IFFWriter iff)
    {
        iff.Push("HVMD", ChunkType.Form);
        
        // Write materials
        iff.Push("MATL");
        iff.WriteInt32(_materials.Count);
        foreach (var m in _materials)
            m.WriteIFF(iff, _version);
        iff.Pop();

        // Write meshes - simplified
        iff.Push("MESH");
        iff.WriteInt32(_meshes.Count);
        foreach (var mesh in _meshes)
        {
            iff.Write(mesh.Name);
            iff.WriteInt32(mesh.LODs.Count);
            foreach (var lod in mesh.LODs)
                lod.WriteIFF(iff);
        }
        iff.Pop();

        iff.Pop(); // HVMD
    }

    private void WriteDTRMChunk(IFFWriter iff)
    {
        iff.Push("DTRM", ChunkType.Form);

        // Write HIER
        Joint.WriteHIERChunk(iff, _root);

        // Write MARK
        iff.Push("MARK");
        iff.WriteInt32(_markers.Count);
        foreach (var m in _markers)
            m.WriteIFF(iff);
        iff.Pop();

        // Write ENGN
        iff.Push("ENGN", ChunkType.Form);
        
        iff.Push("GLOW");
        iff.WriteInt32(_engineGlows.Count);
        foreach (var e in _engineGlows)
            e.WriteIFF(iff);
        iff.Pop();

        iff.Push("BURN");
        iff.WriteInt32(_engineBurns.Count);
        foreach (var e in _engineBurns)
            e.WriteIFF(iff);
        iff.Pop();

        iff.Push("SHAP");
        iff.WriteInt32(_engineShapes.Count);
        foreach (var e in _engineShapes)
            e.WriteIFF(iff);
        iff.Pop();

        iff.Pop(); // ENGN

        // Write NAVL
        iff.Push("NAVL");
        iff.WriteInt32(_navLights.Count);
        foreach (var n in _navLights)
            n.WriteIFF(iff);
        iff.Pop();

        iff.Pop(); // DTRM
    }

    private void WriteINFOChunk(IFFWriter iff)
    {
        iff.Push("INFO", ChunkType.Form);
        // INFO chunk contents - boundary box, etc.
        iff.Pop();
    }

    /// <summary>Initializes the HOD to default state.</summary>
    public void Initialize()
    {
        _version = HODConstants.Version_Standard;
        _name = HODConstants.Name_MultiMesh;
        _root.Initialize();
        _markers.Clear();
        _engineGlows.Clear();
        _engineBurns.Clear();
        _engineShapes.Clear();
        _navLights.Clear();
        _materials.Clear();
        _meshes.Clear();
        _teamColor = new ColorValue(0.5f, 0.5f, 0.5f, 1f);
        _stripeColor = new ColorValue(0.5f, 0.5f, 0.5f, 1f);
        _thrusterPower = 0;
        _badge = "";
    }
}
