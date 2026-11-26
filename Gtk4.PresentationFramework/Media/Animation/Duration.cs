using System;

namespace System.Windows.Media.Animation;

/// <summary>Discriminator for <see cref="Duration"/>.</summary>
public enum DurationType
{
	/// <summary>Finite time span.</summary>
	TimeSpan,
	/// <summary>Duration runs forever.</summary>
	Forever,
	/// <summary>Duration is not set / automatic.</summary>
	Automatic
}

/// <summary>
/// Represents the duration of a <see cref="Timeline"/>.
/// Mirrors WPF's <c>System.Windows.Duration</c>.
/// </summary>
public struct Duration : IEquatable<Duration>
{
	readonly DurationType _type;
	readonly TimeSpan _timeSpan;

	/// <summary>A duration that runs forever.</summary>
	public static readonly Duration Forever = new Duration(DurationType.Forever, TimeSpan.Zero);

	/// <summary>An automatic (unset) duration.</summary>
	public static readonly Duration Automatic = new Duration(DurationType.Automatic, TimeSpan.Zero);

	Duration(DurationType type, TimeSpan ts) { _type = type; _timeSpan = ts; }

	/// <summary>Creates a finite duration from a <see cref="TimeSpan"/>.</summary>
	public Duration(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero)
			throw new ArgumentOutOfRangeException(nameof(timeSpan), "Duration must be non-negative.");
		_type = DurationType.TimeSpan;
		_timeSpan = timeSpan;
	}

	/// <summary>Returns the <see cref="DurationType"/> of this instance.</summary>
	public DurationType DurationType => _type;

	/// <summary>True when this is a finite time-span duration.</summary>
	public bool HasTimeSpan => _type == DurationType.TimeSpan;

	/// <summary>The finite duration, valid only when <see cref="HasTimeSpan"/> is true.</summary>
	public TimeSpan TimeSpan
	{
		get
		{
			if (!HasTimeSpan)
				throw new InvalidOperationException("Duration does not have a TimeSpan.");
			return _timeSpan;
		}
	}

	/// <summary>Implicit conversion from <see cref="System.TimeSpan"/>.</summary>
	public static implicit operator Duration(TimeSpan ts) => new Duration(ts);

	/// <inheritdoc/>
	public bool Equals(Duration other) => _type == other._type && _timeSpan == other._timeSpan;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Duration d && Equals(d);
	/// <inheritdoc/>
	public override int GetHashCode()
	{
		unchecked { return (_type.GetHashCode() << 5) + _type.GetHashCode() ^ _timeSpan.GetHashCode(); }
	}
	/// <inheritdoc/>
	public static bool operator ==(Duration a, Duration b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(Duration a, Duration b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() =>
		_type == DurationType.TimeSpan ? _timeSpan.ToString() :
		_type == DurationType.Forever ? "Forever" : "Automatic";
}