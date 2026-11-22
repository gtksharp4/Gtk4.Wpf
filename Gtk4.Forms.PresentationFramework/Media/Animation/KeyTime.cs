using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Animation;

/// <summary>How a <see cref="KeyTime"/> is expressed.</summary>
public enum KeyTimeType
{
	/// <summary>Evenly spaced between its neighbours.</summary>
	Uniform,

	/// <summary>Paced by the animated distance (treated as <see cref="Uniform"/> here).</summary>
	Paced,

	/// <summary>A fraction of the animation duration.</summary>
	Percent,

	/// <summary>An absolute time offset.</summary>
	TimeSpan,
}

/// <summary>When a key frame ends. Mirrors <c>System.Windows.Media.Animation.KeyTime</c>.</summary>
[TypeConverter(typeof(KeyTimeConverter))]
public struct KeyTime : IEquatable<KeyTime>
{
	readonly TimeSpan _timeSpan;
	readonly double _percent;
	readonly KeyTimeType _type;

	KeyTime(KeyTimeType type, double percent, TimeSpan timeSpan)
	{
		_type = type;
		_percent = percent;
		_timeSpan = timeSpan;
	}

	/// <summary>Evenly spaced between neighbouring key times.</summary>
	public static KeyTime Uniform => new KeyTime(KeyTimeType.Uniform, 0, TimeSpan.Zero);

	/// <summary>Paced by distance.</summary>
	public static KeyTime Paced => new KeyTime(KeyTimeType.Paced, 1, TimeSpan.Zero);

	/// <summary>A fraction (0 to 1) of the duration.</summary>
	public static KeyTime FromPercent(double percent)
	{
		if (percent < 0.0 || percent > 1.0) throw new ArgumentOutOfRangeException(nameof(percent));
		return new KeyTime(KeyTimeType.Percent, percent, TimeSpan.Zero);
	}

	/// <summary>An absolute offset from the start.</summary>
	public static KeyTime FromTimeSpan(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeSpan));
		return new KeyTime(KeyTimeType.TimeSpan, 0, timeSpan);
	}

	/// <summary>The kind of key time.</summary>
	public KeyTimeType Type => _type;

	/// <summary>The fraction of the duration; only for <see cref="KeyTimeType.Percent"/>.</summary>
	public double Percent => _type == KeyTimeType.Percent
		? _percent
		: throw new InvalidOperationException("The KeyTime is not a Percent.");

	/// <summary>The time offset; only for <see cref="KeyTimeType.TimeSpan"/>.</summary>
	public TimeSpan TimeSpan => _type == KeyTimeType.TimeSpan
		? _timeSpan
		: throw new InvalidOperationException("The KeyTime is not a TimeSpan.");

	/// <summary>Converts a time offset to a key time.</summary>
	public static implicit operator KeyTime(TimeSpan timeSpan) => FromTimeSpan(timeSpan);

	/// <summary>Compares two key times.</summary>
	public static bool Equals(KeyTime value1, KeyTime value2) =>
		value1._type == value2._type && value1._percent == value2._percent && value1._timeSpan == value2._timeSpan;

	/// <summary>Compares two key times.</summary>
	public static bool operator ==(KeyTime keyTime1, KeyTime keyTime2) => Equals(keyTime1, keyTime2);

	/// <summary>Compares two key times.</summary>
	public static bool operator !=(KeyTime keyTime1, KeyTime keyTime2) => !Equals(keyTime1, keyTime2);

	/// <inheritdoc/>
	public bool Equals(KeyTime value) => Equals(this, value);

	/// <inheritdoc/>
	public override bool Equals(object? value) => value is KeyTime k && Equals(this, k);

	/// <inheritdoc/>
	public override int GetHashCode() => _type.GetHashCode() ^ _percent.GetHashCode() ^ _timeSpan.GetHashCode();

	/// <inheritdoc/>
	public override string ToString()
	{
		switch (_type)
		{
			case KeyTimeType.Percent:
				return (_percent * 100.0).ToString(CultureInfo.InvariantCulture) + "%";
			case KeyTimeType.TimeSpan:
				return _timeSpan.ToString();
			default:
				return _type.ToString();
		}
	}
}

/// <summary>Converts "Uniform", "Paced", "50%" and time spans to <see cref="KeyTime"/>.</summary>
public class KeyTimeConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string) || sourceType == typeof(TimeSpan);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value is TimeSpan ts) return KeyTime.FromTimeSpan(ts);
		if (!(value is string text))
			throw new NotSupportedException("Cannot convert from " + (value?.GetType().FullName ?? "(null)") + ".");
		text = text.Trim();
		if (text == "Uniform") return KeyTime.Uniform;
		if (text == "Paced") return KeyTime.Paced;
		if (text.EndsWith("%", StringComparison.Ordinal))
		{
			double percent = Convert.ToDouble(text.Substring(0, text.Length - 1), CultureInfo.InvariantCulture);
			return KeyTime.FromPercent(percent / 100.0);
		}
		return KeyTime.FromTimeSpan(TimeSpan.Parse(text, CultureInfo.InvariantCulture));
	}

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is KeyTime k) return k.ToString();
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
