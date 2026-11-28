using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>Defines the width of a column in a <see cref="Grid"/>.</summary>
public class ColumnDefinition
{
	/// <summary>Width of the column. Default is <c>1*</c> (one star).</summary>
	public GridLength Width
	{
		get => _width;
		set
		{
			if (_width.Equals(value)) return;
			_width = value;
			Owner?.OnTrackLengthChanged();
		}
	}

	GridLength _width = new GridLength(1.0, GridUnitType.Star);

	// The grid whose children's slot caps read this track; told when the length changes.
	internal Grid? Owner { get; set; }

	/// <summary>Minimum column width in pixels.</summary>
	public double MinWidth { get; set; } = 0;

	/// <summary>Maximum column width in pixels.</summary>
	public double MaxWidth { get; set; } = double.PositiveInfinity;

	/// <summary>Gets the rendered width, set by the grid's last arrange pass (0 before it).</summary>
	public double ActualWidth { get; internal set; }

	/// <summary>Gets or sets the shared-size group name. Stored only: sizes are not shared across columns.</summary>
	public string? SharedSizeGroup { get; set; }
}

/// <summary>Defines the height of a row in a <see cref="Grid"/>.</summary>
public class RowDefinition
{
	/// <summary>Height of the row. Default is <c>1*</c> (one star).</summary>
	public GridLength Height
	{
		get => _height;
		set
		{
			if (_height.Equals(value)) return;
			_height = value;
			Owner?.OnTrackLengthChanged();
		}
	}

	GridLength _height = new GridLength(1.0, GridUnitType.Star);

	// The grid whose children's slot caps read this track; told when the length changes.
	internal Grid? Owner { get; set; }

	/// <summary>Minimum row height in pixels.</summary>
	public double MinHeight { get; set; } = 0;

	/// <summary>Maximum row height in pixels.</summary>
	public double MaxHeight { get; set; } = double.PositiveInfinity;

	/// <summary>Gets the rendered height, set by the grid's last arrange pass (0 before it).</summary>
	public double ActualHeight { get; internal set; }

	/// <summary>Gets or sets the shared-size group name. Stored only: sizes are not shared across rows.</summary>
	public string? SharedSizeGroup { get; set; }
}

/// <summary>Collection of <see cref="ColumnDefinition"/> items.</summary>
public sealed class ColumnDefinitionCollection : List<ColumnDefinition> { }

/// <summary>Collection of <see cref="RowDefinition"/> items.</summary>
public sealed class RowDefinitionCollection : List<RowDefinition> { }
