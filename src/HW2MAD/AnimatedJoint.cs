using System.Diagnostics;
using CFHodEd.Math;
using HW2IFF;

namespace HW2MAD;

/// <summary>
/// Represents a Homeworld2 MAD file animated joint.
/// </summary>
public class AnimatedJoint
{
    private IJoint? _joint;
    private Vector3 _defaultPosition;
    private Vector3 _defaultRotation;
    private Vector3 _defaultScale;

    private AnimationCurve _translateX;
    private AnimationCurve _translateY;
    private AnimationCurve _translateZ;
    private AnimationCurve _rotateX;
    private AnimationCurve _rotateY;
    private AnimationCurve _rotateZ;
    private AnimationCurve _scaleX;
    private AnimationCurve _scaleY;
    private AnimationCurve _scaleZ;

    private readonly List<int> _indices = new();

    public AnimatedJoint()
    {
        _translateX = new AnimationCurve { Name = "unknown_translateX" };
        _translateY = new AnimationCurve { Name = "unknown_translateY" };
        _translateZ = new AnimationCurve { Name = "unknown_translateZ" };
        _rotateX = new AnimationCurve { Name = "unknown_rotateX" };
        _rotateY = new AnimationCurve { Name = "unknown_rotateY" };
        _rotateZ = new AnimationCurve { Name = "unknown_rotateZ" };
        _scaleX = new AnimationCurve { Name = "unknown_scaleX" };
        _scaleY = new AnimationCurve { Name = "unknown_scaleY" };
        _scaleZ = new AnimationCurve { Name = "unknown_scaleZ" };
        _defaultScale = new Vector3(1, 1, 1);
    }

    public AnimatedJoint(AnimatedJoint other) : this()
    {
        _joint = other._joint;
        _defaultPosition = other._defaultPosition;
        _defaultRotation = other._defaultRotation;
        _defaultScale = other._defaultScale;

        _translateX = new AnimationCurve(other._translateX);
        _translateY = new AnimationCurve(other._translateY);
        _translateZ = new AnimationCurve(other._translateZ);
        _rotateX = new AnimationCurve(other._rotateX);
        _rotateY = new AnimationCurve(other._rotateY);
        _rotateZ = new AnimationCurve(other._rotateZ);
        _scaleX = new AnimationCurve(other._scaleX);
        _scaleY = new AnimationCurve(other._scaleY);
        _scaleZ = new AnimationCurve(other._scaleZ);
    }

    /// <summary>Gets or sets the joint animated by this object.</summary>
    public IJoint? Joint
    {
        get => _joint;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            bool makeNewCurves = value != _joint;
            _joint = value;

            _defaultPosition = value.Position;
            _defaultRotation = value.Rotation;
            _defaultScale = value.Scale;

            if (makeNewCurves)
            {
                _translateX = new AnimationCurve { Name = $"{value.Name}_translateX" };
                _translateY = new AnimationCurve { Name = $"{value.Name}_translateY" };
                _translateZ = new AnimationCurve { Name = $"{value.Name}_translateZ" };
                _rotateX = new AnimationCurve { Name = $"{value.Name}_rotateX" };
                _rotateY = new AnimationCurve { Name = $"{value.Name}_rotateY" };
                _rotateZ = new AnimationCurve { Name = $"{value.Name}_rotateZ" };
                _scaleX = new AnimationCurve { Name = $"{value.Name}_scaleX" };
                _scaleY = new AnimationCurve { Name = $"{value.Name}_scaleY" };
                _scaleZ = new AnimationCurve { Name = $"{value.Name}_scaleZ" };
            }
        }
    }

    // Animation curves
    internal AnimationCurve TranslateX => _translateX;
    internal AnimationCurve TranslateY => _translateY;
    internal AnimationCurve TranslateZ => _translateZ;
    internal AnimationCurve RotateX => _rotateX;
    internal AnimationCurve RotateY => _rotateY;
    internal AnimationCurve RotateZ => _rotateZ;
    internal AnimationCurve ScaleX => _scaleX;
    internal AnimationCurve ScaleY => _scaleY;
    internal AnimationCurve ScaleZ => _scaleZ;

    /// <summary>Gets the number of channels with keyframes.</summary>
    internal int ChannelCount
    {
        get
        {
            int count = 0;
            if (_translateX.Keyframes.Count != 0) count++;
            if (_translateY.Keyframes.Count != 0) count++;
            if (_translateZ.Keyframes.Count != 0) count++;
            if (_rotateX.Keyframes.Count != 0) count++;
            if (_rotateY.Keyframes.Count != 0) count++;
            if (_rotateZ.Keyframes.Count != 0) count++;
            if (_scaleX.Keyframes.Count != 0) count++;
            if (_scaleY.Keyframes.Count != 0) count++;
            if (_scaleZ.Keyframes.Count != 0) count++;
            return count;
        }
    }

    public override string ToString() => _joint?.Name ?? "unknown";

    /// <summary>Reads from IFF.</summary>
    internal void ReadIFF(IFFReader iff, Func<int, string> nameFromStri, IHodFile hod)
    {
        string name = nameFromStri(iff.ReadInt32());
        var joint = hod.GetJointByName(name);

        if (joint != null)
            Joint = joint;
        else
            Trace.TraceError($"Joint '{name}' referenced in MAD does not exist in HOD.");

        int indCount = iff.ReadInt32();
        _indices.Clear();
        for (int i = 0; i < indCount; i++)
            _indices.Add(iff.ReadInt32());
    }

    /// <summary>Writes to IFF.</summary>
    internal void WriteIFF(IFFWriter iff, ref int striPosition, ref int index)
    {
        iff.WriteInt32(striPosition);
        striPosition += ToString().Length + 1;
        iff.WriteInt32(ChannelCount);

        if (_translateX.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_translateY.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_translateZ.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_rotateX.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_rotateY.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_rotateZ.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_scaleX.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_scaleY.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
        if (_scaleZ.Keyframes.Count != 0) { iff.WriteInt32(index); index++; }
    }

    /// <summary>Resets joint to default transform.</summary>
    internal void Reset()
    {
        if (_joint == null) return;
        _joint.Position = _defaultPosition;
        _joint.Rotation = _defaultRotation;
        _joint.Scale = _defaultScale;
    }

    /// <summary>Updates joint transform at time.</summary>
    internal void Update(float time)
    {
        if (_joint == null) return;

        var position = _defaultPosition;
        var rotation = _defaultRotation;
        var scale = _defaultScale;

        if (_translateX.Keyframes.Count != 0) position.X = _translateX.At(time);
        if (_translateY.Keyframes.Count != 0) position.Y = _translateY.At(time);
        if (_translateZ.Keyframes.Count != 0) position.Z = _translateZ.At(time);
        if (_rotateX.Keyframes.Count != 0) rotation.X = _rotateX.At(time);
        if (_rotateY.Keyframes.Count != 0) rotation.Y = _rotateY.At(time);
        if (_rotateZ.Keyframes.Count != 0) rotation.Z = _rotateZ.At(time);
        if (_scaleX.Keyframes.Count != 0) scale.X = _scaleX.At(time);
        if (_scaleY.Keyframes.Count != 0) scale.Y = _scaleY.At(time);
        if (_scaleZ.Keyframes.Count != 0) scale.Z = _scaleZ.At(time);

        _joint.Position = position;
        _joint.Rotation = rotation;
        _joint.Scale = scale;
    }

    /// <summary>Updates references from animation curves list.</summary>
    internal void UpdateReferences(IList<AnimationCurve> animationCurves)
    {
        foreach (int ind in _indices)
        {
            if (ind < 0 || ind >= animationCurves.Count)
            {
                Trace.TraceError("Joint refers to invalid animation curve.");
                continue;
            }

            var anim = animationCurves[ind];
            var dest = anim.Channel switch
            {
                AnimationCurve.AnimationChannel.TranslateX => _translateX,
                AnimationCurve.AnimationChannel.TranslateY => _translateY,
                AnimationCurve.AnimationChannel.TranslateZ => _translateZ,
                AnimationCurve.AnimationChannel.RotateX => _rotateX,
                AnimationCurve.AnimationChannel.RotateY => _rotateY,
                AnimationCurve.AnimationChannel.RotateZ => _rotateZ,
                AnimationCurve.AnimationChannel.ScaleX => _scaleX,
                AnimationCurve.AnimationChannel.ScaleY => _scaleY,
                AnimationCurve.AnimationChannel.ScaleZ => _scaleZ,
                _ => null
            };

            if (dest == null)
            {
                Trace.TraceError($"Animation curve '{anim.Name}' refers to invalid channel.");
                continue;
            }

            dest.Keyframes.Clear();
            foreach (var k in anim.Keyframes)
                dest.Keyframes.Add(new Keyframe(k));
        }

        _indices.Clear();
    }
}
