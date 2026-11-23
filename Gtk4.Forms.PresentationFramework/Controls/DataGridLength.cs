using System.ComponentModel;
using System.Globalization;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>The width of a <see cref="DataGridColumn"/>: a unit type plus its value.</summary>
[TypeConverter(typeof(DataGridLengthConverter))]
public struct DataGridLength : IEquatable<DataGridLength>
{
	const double AutoValue = 1.0;

	static DataGridLength()
	{
		// The DP markup path has no TypeConverter fallback and the registry belongs to XamlValueConverter,
		// so the text form is registered here, before any DP that defaults to a DataGridLength is read.
		XamlValueConverter.Register<DataGridLength>(
			s => (DataGridLength)new DataGridLengthConverter().ConvertFromString(null, CultureInfo.InvariantCulture, s)!);
	}

	/// <summary>Sizes from the cell and header content.</summary>
	public static readonly DataGridLength Auto = new DataGridLength(AutoValue, DataGridLengthUnitType.Auto);

	/// <summary>Sizes from the cell content.</summary>
	public static readonly DataGridLength SizeToCells =
		new DataGridLength(AutoValue, DataGridLengthUnitType.SizeToCells);

	/// <summary>Sizes from the header content.</summary>
	public static readonly DataGridLength SizeToHeader =
		new DataGridLength(AutoValue, DataGridLengthUnitType.SizeToHeader);

	readonly double _unitValue;
	readonly DataGridLengthUnitType _unitType;
	readonly double _desiredValue;
	readonly double _displayValue;

	/// <summary>Creates a pixel length.</summary>
	public DataGridLength(double pixels) : this(pixels, DataGridLengthUnitType.Pixel) { }

	/// <summary>Creates a length of the given unit type.</summary>
	public DataGridLength(double value, DataGridLengthUnitType type)
		: this(value, type, type == DataGridLengthUnitType.Pixel ? value : double.NaN,
			type == DataGridLengthUnitType.Pixel ? value : double.NaN)
	{
	}

	/// <summary>Creates a length with a desired and a display value.</summary>
	public DataGridLength(double value, DataGridLengthUnitType type, double desiredValue, double displayValue)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
			throw new ArgumentException("DataGridLength value must be a finite number.", nameof(value));
		if (double.IsInfinity(desiredValue))
			throw new ArgumentException("DataGridLength desired value must not be infinite.", nameof(desiredValue));
		if (double.IsInfinity(displayValue))
			throw new ArgumentException("DataGridLength display value must not be infinite.", nameof(displayValue));
		if (type < DataGridLengthUnitType.Auto || type > DataGridLengthUnitType.Star)
			throw new ArgumentException("Undefined DataGridLengthUnitType.", nameof(type));
		var absoluteOrStar = type == DataGridLengthUnitType.Pixel || type == DataGridLengthUnitType.Star;
		_unitValue = absoluteOrStar ? value : AutoValue;
		_unitType = type;
		_desiredValue = desiredValue;
		_displayValue = displayValue;
	}

	/// <summary>True for a pixel length.</summary>
	public bool IsAbsolute => _unitType == DataGridLengthUnitType.Pixel;

	/// <summary>True for an Auto length.</summary>
	public bool IsAuto => _unitType == DataGridLengthUnitType.Auto;

	/// <summary>True for a star length.</summary>
	public bool IsStar => _unitType == DataGridLengthUnitType.Star;

	/// <summary>True for a SizeToCells length.</summary>
	public bool IsSizeToCells => _unitType == DataGridLengthUnitType.SizeToCells;

	/// <summary>True for a SizeToHeader length.</summary>
	public bool IsSizeToHeader => _unitType == DataGridLengthUnitType.SizeToHeader;

	/// <summary>The value, in pixels or star weight; 1.0 for the content-sized units.</summary>
	public double Value => _unitValue;

	/// <summary>The unit type.</summary>
	public DataGridLengthUnitType UnitType => _unitType;

	/// <summary>The size the content asked for; NaN when unknown.</summary>
	public double DesiredValue => _desiredValue;

	/// <summary>The size actually used; NaN when unknown.</summary>
	public double DisplayValue => _displayValue;

	/// <summary>Converts a pixel count to a length.</summary>
	public static implicit operator DataGridLength(double value) => new DataGridLength(value);

	/// <summary>Compares unit type, value, desired and display value; NaN equals NaN.</summary>
	public static bool operator ==(DataGridLength a, DataGridLength b) =>
		a._unitType == b._unitType && SameDouble(a._unitValue, b._unitValue)
		&& SameDouble(a._desiredValue, b._desiredValue) && SameDouble(a._displayValue, b._displayValue);

	/// <summary>Negation of <c>==</c>.</summary>
	public static bool operator !=(DataGridLength a, DataGridLength b) => !(a == b);

	/// <inheritdoc/>
	public bool Equals(DataGridLength other) => this == other;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is DataGridLength other && this == other;

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var hash = (int)_unitType;
		hash = hash * 31 + HashDouble(_unitValue);
		hash = hash * 31 + HashDouble(_desiredValue);
		return hash * 31 + HashDouble(_displayValue);
	}

	/// <inheritdoc/>
	public override string ToString() => DataGridLengthConverter.ConvertToString(this, CultureInfo.CurrentCulture);

	static bool SameDouble(double a, double b) => a == b || (double.IsNaN(a) && double.IsNaN(b));

	static int HashDouble(double d) => double.IsNaN(d) ? 0 : d.GetHashCode();
}
