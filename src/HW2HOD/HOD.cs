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
        // Register handlers for the top-level FORM chunks (ID is "VERS", "HVMD", etc.)
        iff.AddHandler("VERS", ChunkType.Form, ReadVERSChunk);
        iff.AddHandler("NAME", ChunkType.Form, ReadNAMEChunk);
        iff.AddHandler("HVMD", ChunkType.Form, ReadHVMDChunk);
        iff.AddHandler("DTRM", ChunkType.Form, ReadDTRMChunk);
        iff.AddHandler("INFO", ChunkType.Form, ReadINFOChunk);
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
        // Add handlers for HVMD sub-chunks
        // VB.NET uses ChunkType.Normal with specific versions
        iff.AddHandler("STAT", ChunkType.Normal, ReadSTATChunk, 1001);
        iff.AddHandler("STAT", ChunkType.Default, ReadSTATChunk);  // Old format
        iff.AddHandler("MULT", ChunkType.Normal, ReadMULTChunk, 1400);
        // Skip other chunk types for now - they'll be ignored by the parser
        iff.Parse();
    }

    private void ReadSTATChunk(IFFReader iff, ChunkAttributes attrs)
    {
        var material = new Material();
        material.ReadIFF(iff, (int)attrs.Version);
        _materials.Add(material);
    }

    // Current mesh being read (for nested BMSH chunks)
    private Mesh? _currentMesh;
    private int _expectedLodCount;

    private void ReadMULTChunk(IFFReader iff, ChunkAttributes attrs)
    {
        // Create a new mesh
        var mesh = new Mesh();
        
        // Read name
        mesh.Name = iff.ReadString();
        
        // Read parent name
        mesh.ParentJoint = iff.ReadString();
        
        // Read LOD count
        _expectedLodCount = iff.ReadInt32();
        
        // Store current mesh for BMSH handler
        _currentMesh = mesh;
        
        // Add handlers for BMSH chunks (VB.NET uses versions 1400 and 1401)
        iff.AddHandler("BMSH", ChunkType.Normal, ReadBMSHChunk, 1400);
        iff.AddHandler("BMSH", ChunkType.Normal, ReadBMSHChunk, 1401);
        // TAGS will be ignored by parser
        
        // Parse nested chunks
        iff.Parse();
        
        // Add mesh to list if it has data
        if (mesh.LODs.Count > 0)
            _meshes.Add(mesh);
            
        _currentMesh = null;
    }

    private void ReadBMSHChunk(IFFReader iff, ChunkAttributes attrs)
    {
        if (_currentMesh == null) return;
        
        var lod = new MeshLOD();
        
        // Read LOD index
        int lodIndex = iff.ReadInt32();
        
        // Read part count
        int partCount = iff.ReadInt32();
        
        // Read all parts
        for (int p = 0; p < partCount; p++)
        {
            // Read material index
            int materialIndex = iff.ReadInt32();
            lod.MaterialIndex = materialIndex;
            
            // Read vertex mask (determines which vertex components are present)
            int vertexMask = iff.ReadInt32();
            
            // Read vertex count
            int vertexCount = iff.ReadInt32();
            
            // Determine vertex format from mask
            bool hasPosition = (vertexMask & 0x01) != 0;     // Position
            bool hasNormal = (vertexMask & 0x02) != 0;       // Normal
            bool hasTangent = (vertexMask & 0x04) != 0;      // Tangent
            bool hasBinormal = (vertexMask & 0x08) != 0;     // Binormal
            bool hasColor = (vertexMask & 0x10) != 0;        // Color
            bool hasTexCoord0 = (vertexMask & 0x20) != 0;    // TexCoord0
            bool hasTexCoord1 = (vertexMask & 0x40) != 0;    // TexCoord1
            
            // Read vertices
            for (int v = 0; v < vertexCount; v++)
            {
                var vertex = new HODVertex();
                
                if (hasPosition)
                    vertex.Position = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
                if (hasNormal)
                    vertex.Normal = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
                if (hasTangent)
                    vertex.Tangent = new Vector3(iff.ReadSingle(), iff.ReadSingle(), iff.ReadSingle());
                if (hasBinormal)
                {
                    // Skip binormal (3 floats)
                    iff.ReadSingle(); iff.ReadSingle(); iff.ReadSingle();
                }
                if (hasColor)
                    iff.ReadInt32(); // Skip color
                if (hasTexCoord0)
                    vertex.TexCoords = new Vector2(iff.ReadSingle(), iff.ReadSingle());
                if (hasTexCoord1)
                {
                    // Skip second UV (2 floats)
                    iff.ReadSingle(); iff.ReadSingle();
                }
                    
                lod.Vertices.Add(vertex);
            }
            
            // Read primitive group count
            short primGroupCount = iff.ReadInt16();
            
            // Read all primitive groups
            for (int g = 0; g < primGroupCount; g++)
            {
                // Read primitive type
                int primType = iff.ReadInt32();
                
                // Read index count
                int indexCount = iff.ReadInt32();
                
                // Read indices
                for (int i = 0; i < indexCount; i++)
                {
                    lod.Indices.Add(iff.ReadUInt16());
                }
            }
        }
        
        // Calculate bounds
        lod.RecalculateBounds();
        
        // Set LOD name based on parent mesh
        lod.Name = $"{_currentMesh.Name}_LOD{lodIndex}";
        lod.ParentJoint = _currentMesh.ParentJoint;
        
        // Add to current mesh
        _currentMesh.LODs.Add(lod);
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
