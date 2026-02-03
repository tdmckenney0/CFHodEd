using HW2IFF;

namespace HW2MAD;

/// <summary>
/// Class representing a Homeworld2 animation marker.
/// </summary>
public class Animation
{
    private string _name = "animation";
    private float _startTime;
    private float _endTime = 1f;
    private float _loopStartTime;
    private float _loopEndTime = 1f;
    private readonly EventList<AnimatedJoint> _joints = new();

    public Animation() { }

    /// <summary>Copy constructor.</summary>
    public Animation(Animation other)
    {
        _name = other._name;
        _startTime = other._startTime;
        _endTime = other._endTime;
        _loopStartTime = other._loopStartTime;
        _loopEndTime = other._loopEndTime;
        
        foreach (var j in other._joints)
            _joints.Add(new AnimatedJoint(j));
    }

    /// <summary>Gets or sets the animation name.</summary>
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

    /// <summary>Gets or sets the start time.</summary>
    public float StartTime
    {
        get => _startTime;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _startTime = value;
        }
    }

    /// <summary>Gets or sets the end time.</summary>
    public float EndTime
    {
        get => _endTime;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _endTime = value;
        }
    }

    /// <summary>Gets or sets the loop start time.</summary>
    public float LoopStartTime
    {
        get => _loopStartTime;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _loopStartTime = value;
        }
    }

    /// <summary>Gets or sets the loop end time.</summary>
    public float LoopEndTime
    {
        get => _loopEndTime;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            _loopEndTime = value;
        }
    }

    /// <summary>Gets the list of animated joints.</summary>
    public IList<AnimatedJoint> Joints => _joints;

    public override string ToString() => _name;

    /// <summary>Reads from IFF.</summary>
    internal void ReadIFF(IFFReader iff, Func<int, string> nameFromStri, IHodFile hod)
    {
        _name = nameFromStri(iff.ReadInt32());
        _startTime = iff.ReadSingle();
        _endTime = iff.ReadSingle();
        _loopStartTime = iff.ReadSingle();
        _loopEndTime = iff.ReadSingle();

        int numJoints = iff.ReadInt32();
        _joints.Clear();

        for (int i = 0; i < numJoints; i++)
        {
            var j = new AnimatedJoint();
            j.ReadIFF(iff, nameFromStri, hod);
            _joints.Add(j);
        }
    }

    /// <summary>Writes to IFF.</summary>
    internal void WriteIFF(IFFWriter iff, ref int striPosition, ref int index)
    {
        iff.WriteInt32(striPosition);
        striPosition += _name.Length + 1;

        iff.Write(_startTime);
        iff.Write(_endTime);
        iff.Write(_loopStartTime);
        iff.Write(_loopEndTime);
        iff.Write(_joints.Count);

        for (int i = 0; i < _joints.Count; i++)
            _joints[i].WriteIFF(iff, ref striPosition, ref index);
    }

    /// <summary>Prepares animation for export.</summary>
    internal void PrepareAnimationBeforeExport()
    {
        for (int i = _joints.Count - 1; i >= 0; i--)
        {
            if (_joints[i].Joint == null || _joints[i].ChannelCount == 0)
                _joints.RemoveAt(i);
        }
    }

    /// <summary>Resets all joint transforms.</summary>
    public void Reset()
    {
        foreach (var j in _joints)
            j.Reset();
    }

    /// <summary>Updates all joint transforms at time.</summary>
    public void Update(float time)
    {
        foreach (var j in _joints)
            j.Update(time);
    }

    /// <summary>Updates references from animation curves list.</summary>
    internal void UpdateReferences(IList<AnimationCurve> animationCurves)
    {
        foreach (var j in _joints)
            j.UpdateReferences(animationCurves);
    }
}
