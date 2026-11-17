using System;

namespace System.Windows.Media.Animation;

/// <summary>Discriminator for <see cref="RepeatBehavior"/>.</summary>
public enum RepeatBehaviorType
{
	/// <summary>Repeat a fixed number of times.</summary>
	Count,
	/// <summary>Repeat for a fixed duration.</summary>
	Duration,
	/// <summary>Repeat forever.</summary>
	Forever
}

/// <summary>
/// Specifies how a <see cref="Timeline"/> repeats.
/// Mirrors WPF's <c>System.Windows.Media.Animation.RepeatBehavior</c>.
/// </summary>
public struct RepeatBehavior : IEquatable<RepeatBehavior>
{
	readonly RepeatBehaviorType _type;
	readonly double _count;
	readonly TimeSpan _duration;

	/// <summary>Repeat forever.</summary>
	public static readonly RepeatBehavior Forever =
		new RepeatBehavior(RepeatBehaviorType.Forever, 0, TimeSpan.Zero);

	RepeatBehavior(RepeatBehaviorType type, double count, TimeSpan duration)
	{
		_type = type; _count = count; _duration = duration;
	}

	/// <summary>Creates a count-based repeat.</summary>
	public RepeatBehavior(double count)
	{
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		_type = RepeatBehaviorType.Count;
		_count = count;
		_duration = TimeSpan.Zero;
	}

	/// <summary>Creates a duration-based repeat.</summary>
	public RepeatBehavior(TimeSpan duration)
	{
		if (duration < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(duration));
		_type = RepeatBehaviorType.Duration;
		_count = 0;
		_duration = duration;
	}

	/// <summary>Returns the type of this repeat behaviour.</summary>
	public RepeatBehaviorType Type => _type;

	/// <summary>True when this is a count-based repeat.</summary>
	public bool HasCount => _type == RepeatBehaviorType.Count;

	/// <summary>True when this is a duration-based repeat.</summary>
	public bool HasDuration => _type == RepeatBehaviorType.Duration;

	/// <summary>The repeat count; valid only when <see cref="HasCount"/> is true.</summary>
	public double Count
	{
		get
		{
			if (!HasCount) throw new InvalidOperationException("RepeatBehavior is not Count-based.");
			return _count;
		}
	}

	/// <summary>The repeat duration; valid only when <see cref="HasDuration"/> is true.</summary>
	public TimeSpan Duration
	{
		get
		{
			if (!HasDuration) throw new InvalidOperationException("RepeatBehavior is not Duration-based.");
			return _duration;
		}
	}

	/// <inheritdoc/>
	public bool Equals(RepeatBehavior other) =>
		_type == other._type && _count == other._count && _duration == other._duration;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is RepeatBehavior r && Equals(r);
	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked
		{
			int h = (_type.GetHashCode() << 5) + _type.GetHashCode() ^ _count.GetHashCode();
			return (h << 5) + h ^ _duration.GetHashCode();
		}
	}
	/// <inheritdoc/>
	public static bool operator ==(RepeatBehavior a, RepeatBehavior b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(RepeatBehavior a, RepeatBehavior b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() =>
		_type == RepeatBehaviorType.Forever ? "Forever" :
		_type == RepeatBehaviorType.Count ? $"{_count}x" : _duration.ToString();
}