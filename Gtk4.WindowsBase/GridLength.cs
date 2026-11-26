using System;

namespace System.Windows;

/// <summary>Units used by <see cref="GridLength"/>.</summary>
public enum GridUnitType
{
	/// <summary>Fixed pixel size.</summary>
	Pixel = 0,
	/// <summary>Content-driven size.</summary>
	Auto = 1,
	/// <summary>Star (proportional) size.</summary>
	Star = 2
}

/// <summary>
/// Describes the width or height of a Grid row/column.
/// Mirrors <c>System.Windows.GridLength</c> in WPF.
/// </summary>
public struct GridLength : IEquatable<GridLength>
{
	/// <summary>Auto-sized <see cref="GridLength"/>.</summary>
	public static readonly GridLength Auto = new GridLength(1.0, GridUnitType.Auto);

	readonly double _value;
	readonly GridUnitType _type;

	/// <summary>Creates a pixel <see cref="GridLength"/>.</summary>
	public GridLength(double pixels) : this(pixels, GridUnitType.Pixel) { }

	/// <summary>Creates a <see cref="GridLength"/> with a given value and unit type.</summary>
	public GridLength(double value, GridUnitType type)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
			throw new ArgumentException("GridLength value must be finite.", nameof(value));
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value), "GridLength value must be non-negative.");
		_value = value;
		_type = type;
	}

	/// <summary>The numeric component of this length.</summary>
	public double Value => _value;

	/// <summary>The unit type.</summary>
	public GridUnitType GridUnitType => _type;

	/// <summary>True if this is an Auto length.</summary>
	public bool IsAuto => _type == GridUnitType.Auto;

	/// <summary>True if this is a Star (proportional) length.</summary>
	public bool IsStar => _type == GridUnitType.Star;

	/// <summary>True if this is a fixed pixel length.</summary>
	public bool IsAbsolute => _type == GridUnitType.Pixel;

	/// <inheritdoc/>
	public bool Equals(GridLength other) => _type == other._type && _value == other._value;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is GridLength g && Equals(g);
	/// <inheritdoc/>
	public override int GetHashCode() => PrimitivesHashHelper.CombineHash(_value.GetHashCode(), (int)_type);
	/// <inheritdoc/>
	public static bool operator ==(GridLength a, GridLength b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(GridLength a, GridLength b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() =>
		_type == GridUnitType.Auto ? "Auto" :
		_type == GridUnitType.Star ? (_value == 1.0 ? "*" : $"{_value}*") :
		_value.ToString();
}