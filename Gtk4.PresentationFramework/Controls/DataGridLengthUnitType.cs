namespace System.Windows.Controls;

/// <summary>Unit types of a <see cref="DataGridLength"/>.</summary>
public enum DataGridLengthUnitType
{
	/// <summary>Size from the cell and header content.</summary>
	Auto = 0,
	/// <summary>An exact size in device-independent units.</summary>
	Pixel = 1,
	/// <summary>Size from the cell content.</summary>
	SizeToCells = 2,
	/// <summary>Size from the header content.</summary>
	SizeToHeader = 3,
	/// <summary>A weighted share of the remaining space.</summary>
	Star = 4
}
