using System;
using System.Collections.Generic;

namespace System.Windows.Controls
{
	/// <summary>Defines the width of a column in a <see cref="Grid"/>.</summary>
	public class ColumnDefinition
	{
		/// <summary>Width of the column. Default is <c>1*</c> (one star).</summary>
		public GridLength Width { get; set; } = new GridLength(1.0, GridUnitType.Star);

		/// <summary>Minimum column width in pixels.</summary>
		public double MinWidth { get; set; } = 0;

		/// <summary>Maximum column width in pixels.</summary>
		public double MaxWidth { get; set; } = double.PositiveInfinity;
	}

	/// <summary>Defines the height of a row in a <see cref="Grid"/>.</summary>
	public class RowDefinition
	{
		/// <summary>Height of the row. Default is <c>1*</c> (one star).</summary>
		public GridLength Height { get; set; } = new GridLength(1.0, GridUnitType.Star);

		/// <summary>Minimum row height in pixels.</summary>
		public double MinHeight { get; set; } = 0;

		/// <summary>Maximum row height in pixels.</summary>
		public double MaxHeight { get; set; } = double.PositiveInfinity;
	}

	/// <summary>Collection of <see cref="ColumnDefinition"/> items.</summary>
	public sealed class ColumnDefinitionCollection : List<ColumnDefinition> { }

	/// <summary>Collection of <see cref="RowDefinition"/> items.</summary>
	public sealed class RowDefinitionCollection : List<RowDefinition> { }
}
