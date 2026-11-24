using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>Lines <see cref="First"/> to <c>First + Count - 1</c> of a box, placed at <see cref="Y"/> in a column.</summary>
sealed class FlowSlice
{
	internal FlowSlice(FlowBox box, int first, int count, double y)
	{
		Box = box;
		First = first;
		Count = count;
		Y = y;
	}

	internal FlowBox Box { get; }

	internal int First { get; }

	internal int Count { get; set; }

	/// <summary>Top of the first line, relative to the top of the page content area.</summary>
	internal double Y { get; }

	internal double Height => Count * Box.Pitch;
}

sealed class FlowColumn
{
	internal List<FlowSlice> Slices { get; } = new List<FlowSlice>();
}

/// <summary>One page: <c>Columns.Count</c> columns of slices.</summary>
sealed class FlowPage
{
	internal FlowPage(int columns)
	{
		for (int i = 0; i < columns; i++) Columns.Add(new FlowColumn());
	}

	internal List<FlowColumn> Columns { get; } = new List<FlowColumn>();
}
