using System.Text;
using HW2IFF;

namespace HW2MAD;

/// <summary>
/// Class representing a Homeworld2 MAD (animation) file.
/// </summary>
public sealed class MAD
{
    private const string MadName = "Homeworld2 MAD File";

    private int _fps = 30;
    private string _stri = "";
    private IHodFile? _hod;
    private readonly EventList<Animation> _animations = new();
    private readonly EventList<AnimationCurve> _animationCurves = new();

    public MAD() { }

    /// <summary>Copy constructor.</summary>
    public MAD(MAD other)
    {
        _fps = other._fps;
        _hod = other._hod;

        foreach (var a in other._animations)
            _animations.Add(new Animation(a));
    }

    /// <summary>Gets or sets frames per second.</summary>
    public int FPS
    {
        get => _fps;
        set
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _fps = value;
        }
    }

    /// <summary>Gets the animations list.</summary>
    public IList<Animation> Animations => _animations;

    /// <summary>Reads a MAD file from a stream.</summary>
    public void Read(Stream stream, IHodFile hod)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (hod == null)
            throw new ArgumentNullException(nameof(hod));

        var iff = new IFFReader(stream);
        Initialize();
        _hod = hod;

        iff.AddHandler("MAD ", ChunkType.Form, ReadMADChunk);
        iff.Parse();

        // Update references
        foreach (var anim in _animations)
            anim.UpdateReferences(_animationCurves);

        _animationCurves.Clear();
        _hod = null;
    }

    /// <summary>Writes a MAD file to a stream.</summary>
    public void Write(Stream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var iff = new IFFWriter(stream);
        int striPosition = 0;

        PrepareMADForExport();

        iff.Push("MAD ", ChunkType.Form);

        // VERS chunk
        iff.Push("VERS");
        iff.WriteInt32(0x104);
        iff.Pop();

        // NAME chunk
        iff.Push("NAME");
        iff.Write(MadName, MadName.Length);
        iff.Pop();

        // INFO, STRI, MARK, CURV chunks
        WriteINFOChunk(iff);
        WriteSTRIChunk(iff);
        WriteMARKChunk(iff, ref striPosition);
        WriteCURVChunk(iff, ref striPosition);

        iff.Pop(); // MAD 

        _stri = "";
        _animationCurves.Clear();
    }

    private void PrepareMADForExport()
    {
        var sb = new StringBuilder();

        foreach (var anim in _animations)
        {
            anim.PrepareAnimationBeforeExport();
            sb.Append(anim.ToString());
            sb.Append('\0');

            foreach (var joint in anim.Joints)
            {
                sb.Append(joint.ToString());
                sb.Append('\0');

                if (joint.TranslateX.Keyframes.Count != 0) _animationCurves.Add(joint.TranslateX);
                if (joint.TranslateY.Keyframes.Count != 0) _animationCurves.Add(joint.TranslateY);
                if (joint.TranslateZ.Keyframes.Count != 0) _animationCurves.Add(joint.TranslateZ);
                if (joint.RotateX.Keyframes.Count != 0) _animationCurves.Add(joint.RotateX);
                if (joint.RotateY.Keyframes.Count != 0) _animationCurves.Add(joint.RotateY);
                if (joint.RotateZ.Keyframes.Count != 0) _animationCurves.Add(joint.RotateZ);
                if (joint.ScaleX.Keyframes.Count != 0) _animationCurves.Add(joint.ScaleX);
                if (joint.ScaleY.Keyframes.Count != 0) _animationCurves.Add(joint.ScaleY);
                if (joint.ScaleZ.Keyframes.Count != 0) _animationCurves.Add(joint.ScaleZ);
            }
        }

        foreach (var curve in _animationCurves)
        {
            sb.Append(curve.ToString());
            sb.Append('\0');
        }

        _stri = sb.ToString();
    }

    private void ReadMADChunk(IFFReader iff, ChunkAttributes attrs)
    {
        iff.AddHandler("VERS", ChunkType.Default, ReadIDChunk);
        iff.AddHandler("NAME", ChunkType.Default, ReadIDChunk);
        iff.AddHandler("INFO", ChunkType.Default, ReadINFOChunk);
        iff.AddHandler("STRI", ChunkType.Default, ReadSTRIChunk);
        iff.AddHandler("MARK", ChunkType.Default, ReadMARKChunk);
        iff.AddHandler("CURV", ChunkType.Default, ReadCURVChunk);
        iff.Parse();
    }

    private void ReadIDChunk(IFFReader iff, ChunkAttributes attrs)
    {
        if (attrs.ID == "VERS")
        {
            int version = iff.ReadInt32();
            System.Diagnostics.Debug.Assert(version == 0x104, "VERS chunk ID test failed.");
        }
        else if (attrs.ID == "NAME")
        {
            string name = iff.ReadString(attrs.Size);
            System.Diagnostics.Debug.Assert(name == MadName, "NAME chunk ID test failed.");
        }
    }

    private void ReadINFOChunk(IFFReader iff, ChunkAttributes attrs)
    {
        _fps = iff.ReadInt32();
        int animationCount = iff.ReadInt32();
        int animationCurveCount = iff.ReadInt32();
        int jointMapCount = iff.ReadInt32();
        int indiceCount = iff.ReadInt32();

        _animations.Clear();
        for (int i = 0; i < animationCount; i++)
            _animations.Add(new Animation());

        _animationCurves.Clear();
        for (int i = 0; i < animationCurveCount; i++)
            _animationCurves.Add(new AnimationCurve());
    }

    private void WriteINFOChunk(IFFWriter iff)
    {
        int jointMapCount = 0;
        int indiceCount = 0;

        foreach (var anim in _animations)
        {
            jointMapCount += anim.Joints.Count;
            foreach (var joint in anim.Joints)
                indiceCount += joint.ChannelCount;
        }

        iff.Push("INFO");
        iff.WriteInt32(_fps);
        iff.WriteInt32(_animations.Count);
        iff.WriteInt32(_animationCurves.Count);
        iff.WriteInt32(jointMapCount);
        iff.WriteInt32(indiceCount);
        iff.Pop();
    }

    private void ReadSTRIChunk(IFFReader iff, ChunkAttributes attrs)
    {
        _stri = iff.ReadString(attrs.Size);
    }

    private void WriteSTRIChunk(IFFWriter iff)
    {
        iff.Push("STRI");
        iff.Write(_stri, _stri.Length);
        iff.Pop();
    }

    private string GetNameFromSTRI(int pos)
    {
        if (pos < 0 || pos >= _stri.Length)
            return "Unknown";

        int nullPos = _stri.IndexOf('\0', pos);
        if (nullPos == -1)
            return _stri[pos..];

        return _stri[pos..nullPos];
    }

    private void ReadMARKChunk(IFFReader iff, ChunkAttributes attrs)
    {
        foreach (var anim in _animations)
            anim.ReadIFF(iff, GetNameFromSTRI, _hod!);
    }

    private void WriteMARKChunk(IFFWriter iff, ref int striPosition)
    {
        int index = 0;
        iff.Push("MARK");
        foreach (var anim in _animations)
            anim.WriteIFF(iff, ref striPosition, ref index);
        iff.Pop();
    }

    private void ReadCURVChunk(IFFReader iff, ChunkAttributes attrs)
    {
        foreach (var curve in _animationCurves)
            curve.ReadIFF(iff, GetNameFromSTRI);
    }

    private void WriteCURVChunk(IFFWriter iff, ref int striPosition)
    {
        iff.Push("CURV");
        foreach (var curve in _animationCurves)
            curve.WriteIFF(iff, ref striPosition);
        iff.Pop();
    }

    /// <summary>Initializes the MAD.</summary>
    public void Initialize()
    {
        _fps = 30;
        _stri = "";
        _hod = null;
        _animations.Clear();
        _animationCurves.Clear();
    }

    /// <summary>Resets all animations.</summary>
    public void Reset()
    {
        foreach (var anim in _animations)
            anim.Reset();
    }
}
