using System.Diagnostics;
using CFHodEd.Math;
using HW2IFF;

namespace HW2MAD;

/// <summary>
/// Class representing a Homeworld2 animation curve.
/// </summary>
internal sealed class AnimationCurve
{
    /// <summary>Animation channels.</summary>
    public enum AnimationChannel
    {
        Invalid = 0,
        TranslateX,
        TranslateY,
        TranslateZ,
        RotateX,
        RotateY,
        RotateZ,
        ScaleX,
        ScaleY,
        ScaleZ
    }

    private string _name = "AnimationCurve";
    private readonly EventList<Keyframe> _keyframes = new();
    private InfinityType _preInfinity = InfinityType.Constant;
    private InfinityType _postInfinity = InfinityType.Constant;

    /// <summary>Default constructor.</summary>
    public AnimationCurve()
    {
        _keyframes.AddItem += () => _keyframes.Sort();
        _keyframes.InsertItem += _ => _keyframes.Sort();
        _keyframes.ModifiedItem += _ => _keyframes.Sort();
    }

    /// <summary>Copy constructor.</summary>
    public AnimationCurve(AnimationCurve other) : this()
    {
        _name = other._name;
        foreach (var k in other._keyframes)
            _keyframes.Add(new Keyframe(k));
        _preInfinity = other._preInfinity;
        _postInfinity = other._postInfinity;
    }

    /// <summary>Gets or sets the name.</summary>
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

    /// <summary>Gets the keyframes list.</summary>
    public IList<Keyframe> Keyframes => _keyframes;

    /// <summary>Gets or sets pre-infinity behavior.</summary>
    public InfinityType PreInfinity
    {
        get => _preInfinity;
        set => _preInfinity = value;
    }

    /// <summary>Gets or sets post-infinity behavior.</summary>
    public InfinityType PostInfinity
    {
        get => _postInfinity;
        set => _postInfinity = value;
    }

    /// <summary>Gets the channel animated by this curve.</summary>
    internal AnimationChannel Channel
    {
        get
        {
            if (string.IsNullOrEmpty(_name) || !_name.Contains('_'))
            {
                Debug.Assert(false, "Invalid animation curve name");
                return AnimationChannel.Invalid;
            }

            string lastPart = _name[((_name.LastIndexOf('_')) + 1)..];

            return lastPart switch
            {
                "translateX" => AnimationChannel.TranslateX,
                "translateY" => AnimationChannel.TranslateY,
                "translateZ" => AnimationChannel.TranslateZ,
                "rotateX" => AnimationChannel.RotateX,
                "rotateY" => AnimationChannel.RotateY,
                "rotateZ" => AnimationChannel.RotateZ,
                "scaleX" => AnimationChannel.ScaleX,
                "scaleY" => AnimationChannel.ScaleY,
                "scaleZ" => AnimationChannel.ScaleZ,
                _ => AnimationChannel.Invalid
            };
        }
    }

    /// <summary>Gets interpolated value at time (linear, ignores tangents).</summary>
    internal float At(float time)
    {
        if (_keyframes.Count == 0)
        {
            Debug.Assert(false, "No keyframes");
            return 0;
        }

        if (time <= _keyframes[0].Time)
            return (float)_keyframes[0].Value;

        if (time >= _keyframes[^1].Time)
            return (float)_keyframes[^1].Value;

        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            var k0 = _keyframes[i];
            var k1 = _keyframes[i + 1];

            if (time >= k0.Time && time <= k1.Time)
            {
                double t = (time - k0.Time) / (k1.Time - k0.Time);
                return (float)(k0.Value + t * (k1.Value - k0.Value));
            }
        }

        return (float)_keyframes[^1].Value;
    }

    public override string ToString() => _name;

    /// <summary>Reads from IFF.</summary>
    internal void ReadIFF(IFFReader iff, Func<int, string> nameFromStri)
    {
        _name = nameFromStri(iff.ReadInt32());

        int keyframeCount = iff.ReadInt32();
        _keyframes.Clear();

        for (int i = 0; i < keyframeCount; i++)
        {
            var k = new Keyframe();
            k.ReadIFF(iff);
            _keyframes.Add(k);
        }

        _preInfinity = (InfinityType)iff.ReadInt32();
        _postInfinity = (InfinityType)iff.ReadInt32();
    }

    /// <summary>Writes to IFF.</summary>
    internal void WriteIFF(IFFWriter iff, ref int striPosition)
    {
        CalculateTangents();

        iff.Write(striPosition);
        striPosition += _name.Length + 1;

        iff.Write(_keyframes.Count);

        for (int i = 0; i < _keyframes.Count; i++)
            _keyframes[i].WriteIFF(iff);

        iff.WriteInt32((int)_preInfinity);
        iff.WriteInt32((int)_postInfinity);
    }

    /// <summary>Calculates linear tangents for all keyframes.</summary>
    internal void CalculateTangents()
    {
        if (_keyframes.Count == 0)
            return;

        _keyframes[0] = _keyframes[0].SetInTangent(new Vector2(1, 0));

        for (int i = 0; i < _keyframes.Count - 1; i++)
        {
            var k0 = _keyframes[i];
            var k1 = _keyframes[i + 1];

            float dx = (float)(k1.Time - k0.Time);
            float dy = (float)(k1.Value - k0.Value);
            var tangent = new Vector2(dx, dy);

            _keyframes[i] = k0.SetOutTangent(tangent);
            _keyframes[i + 1] = k1.SetInTangent(tangent);
        }

        _keyframes[^1] = _keyframes[^1].SetOutTangent(new Vector2(1, 0));
    }
}
