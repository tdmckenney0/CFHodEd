using CFHodEd.Math;
using HW2IFF;

namespace HW2MAD;

/// <summary>
/// Structure representing a Homeworld2 animation curve keyframe.
/// </summary>
public struct Keyframe : IComparable<Keyframe>, IEquatable<Keyframe>
{
    /// <summary>Time of keyframe.</summary>
    public double Time;

    /// <summary>Value at keyframe.</summary>
    public double Value;

    /// <summary>In tangent at keyframe.</summary>
    internal Vector2 InTangent;

    /// <summary>Out tangent at keyframe.</summary>
    internal Vector2 OutTangent;

    /// <summary>Copy constructor.</summary>
    public Keyframe(Keyframe k)
    {
        Time = k.Time;
        Value = k.Value;
        InTangent = k.InTangent;
        OutTangent = k.OutTangent;
    }

    public override string ToString() => $"{{ {Time:F3}, {Value:F3} }}";

    public override bool Equals(object? obj) => obj is Keyframe k && this == k;

    public bool Equals(Keyframe other) => this == other;

    public override int GetHashCode() => HashCode.Combine(Time, Value);

    public static bool operator ==(Keyframe left, Keyframe right) =>
        left.Time == right.Time && left.Value == right.Value;

    public static bool operator !=(Keyframe left, Keyframe right) => !(left == right);

    /// <summary>Reads keyframe from IFF reader.</summary>
    internal void ReadIFF(IFFReader iff)
    {
        Time = iff.ReadDouble();
        Value = iff.ReadDouble();
        InTangent = new Vector2(iff.ReadSingle(), iff.ReadSingle());
        OutTangent = new Vector2(iff.ReadSingle(), iff.ReadSingle());
    }

    /// <summary>Writes keyframe to IFF writer.</summary>
    internal void WriteIFF(IFFWriter iff)
    {
        iff.Write(Time);
        iff.Write(Value);
        iff.Write(InTangent.X);
        iff.Write(InTangent.Y);
        iff.Write(OutTangent.X);
        iff.Write(OutTangent.Y);
    }

    /// <summary>Sets the in tangent.</summary>
    internal Keyframe SetInTangent(Vector2 v)
    {
        InTangent = v;
        return this;
    }

    /// <summary>Sets the out tangent.</summary>
    internal Keyframe SetOutTangent(Vector2 v)
    {
        OutTangent = v;
        return this;
    }

    /// <summary>Compares keyframes by time.</summary>
    public int CompareTo(Keyframe other) => Time.CompareTo(other.Time);
}
