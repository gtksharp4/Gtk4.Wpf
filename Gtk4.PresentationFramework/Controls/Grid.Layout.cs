using System;
using System.Collections.Generic;

namespace System.Windows.Controls;

public partial class Grid
{
	/// <summary>
	/// WPF Grid measure: resolve Auto columns/rows by child natural sizes,
	/// then distribute Star proportionally. Place each child in its cell.
	/// </summary>
	protected override Size MeasureOverride(Size availableSize)
	{
		int colCount = Math.Max(1, ColumnDefinitions.Count);
		int rowCount = Math.Max(1, RowDefinitions.Count);

		double[] colWidths = ResolveColumns(colCount, availableSize.Width);
		double[] rowHeights = ResolveRows(rowCount, availableSize.Height);

		// Auto pass: measure children that occupy Auto columns/rows.
		MeasureAutoChildren(colCount, rowCount, colWidths, rowHeights);

		// Now re-resolve Auto sizes with measured child sizes.
		colWidths = ResolveColumnsWithAuto(colCount, availableSize.Width, colWidths);
		rowHeights = ResolveRowsWithAuto(rowCount, availableSize.Height, rowHeights);

		// Final measure of all children in their resolved cells.
		foreach (UIElement child in Children)
		{
			var cellSize = GetCellSize(child, colCount, rowCount, colWidths, rowHeights);
			child.Measure(cellSize);
		}

		double totalW = SumFinite(colWidths);
		double totalH = SumFinite(rowHeights);
		return new Size(totalW, totalH);
	}

	/// <summary>WPF Grid arrange: position each child in its computed cell rect.</summary>
	protected override Size ArrangeOverride(Size finalSize)
	{
		int colCount = Math.Max(1, ColumnDefinitions.Count);
		int rowCount = Math.Max(1, RowDefinitions.Count);

		double[] colWidths = ResolveColumnsArrange(colCount, finalSize.Width);
		double[] rowHeights = ResolveRowsArrange(rowCount, finalSize.Height);

		double[] colOffsets = BuildOffsets(colWidths);
		double[] rowOffsets = BuildOffsets(rowHeights);
		PublishActualSizes(colWidths, rowHeights);

		foreach (UIElement child in Children)
		{
			int col = Math.Max(0, Math.Min(GetColumn(child), colCount - 1));
			int row = Math.Max(0, Math.Min(GetRow(child), rowCount - 1));
			int colSpan = Math.Max(1, Math.Min(GetColumnSpan(child), colCount - col));
			int rowSpan = Math.Max(1, Math.Min(GetRowSpan(child), rowCount - row));

			double x = colOffsets[col];
			double y = rowOffsets[row];
			double w = SumRange(colWidths, col, colSpan);
			double h = SumRange(rowHeights, row, rowSpan);

			child.Arrange(new Rect(x, y, w, h));
		}

		return finalSize;
	}

	// ── Column resolution ───────────────────────────────────────────────────

	double[] ResolveColumns(int colCount, double available)
	{
		double[] widths = new double[colCount];
		if (ColumnDefinitions.Count == 0)
		{
			// Single implicit star column.
			widths[0] = double.IsPositiveInfinity(available) ? 0 : available;
			return widths;
		}
		return ResolveDefinitions(ColumnDefinitions.Count, available,
			i => ColumnDefinitions[i].Width,
			i => ColumnDefinitions[i].MinWidth,
			i => ColumnDefinitions[i].MaxWidth,
			widths);
	}

	double[] ResolveColumnsWithAuto(int colCount, double available, double[] initial)
	{
		if (ColumnDefinitions.Count == 0) return initial;
		UpdateAutoFromChildren(initial, colCount, true);
		return ResolveStarDistribution(initial, colCount, available,
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].Width : new GridLength(1.0, GridUnitType.Star),
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].MinWidth : 0,
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].MaxWidth : double.PositiveInfinity);
	}

	double[] ResolveColumnsArrange(int colCount, double available)
	{
		double[] widths = ResolveColumns(colCount, available);
		UpdateAutoFromChildren(widths, colCount, true);
		return ResolveStarDistribution(widths, colCount, available,
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].Width : new GridLength(1.0, GridUnitType.Star),
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].MinWidth : 0,
			i => i < ColumnDefinitions.Count ? ColumnDefinitions[i].MaxWidth : double.PositiveInfinity);
	}

	// ── Row resolution ──────────────────────────────────────────────────────

	double[] ResolveRows(int rowCount, double available)
	{
		double[] heights = new double[rowCount];
		if (RowDefinitions.Count == 0)
		{
			heights[0] = double.IsPositiveInfinity(available) ? 0 : available;
			return heights;
		}
		return ResolveDefinitions(RowDefinitions.Count, available,
			i => RowDefinitions[i].Height,
			i => RowDefinitions[i].MinHeight,
			i => RowDefinitions[i].MaxHeight,
			heights);
	}

	double[] ResolveRowsWithAuto(int rowCount, double available, double[] initial)
	{
		if (RowDefinitions.Count == 0) return initial;
		UpdateAutoFromChildren(initial, rowCount, false);
		return ResolveStarDistribution(initial, rowCount, available,
			i => i < RowDefinitions.Count ? RowDefinitions[i].Height : new GridLength(1.0, GridUnitType.Star),
			i => i < RowDefinitions.Count ? RowDefinitions[i].MinHeight : 0,
			i => i < RowDefinitions.Count ? RowDefinitions[i].MaxHeight : double.PositiveInfinity);
	}

	double[] ResolveRowsArrange(int rowCount, double available)
	{
		double[] heights = ResolveRows(rowCount, available);
		UpdateAutoFromChildren(heights, rowCount, false);
		return ResolveStarDistribution(heights, rowCount, available,
			i => i < RowDefinitions.Count ? RowDefinitions[i].Height : new GridLength(1.0, GridUnitType.Star),
			i => i < RowDefinitions.Count ? RowDefinitions[i].MinHeight : 0,
			i => i < RowDefinitions.Count ? RowDefinitions[i].MaxHeight : double.PositiveInfinity);
	}

	// ── Core algorithm helpers ──────────────────────────────────────────────

	static double[] ResolveDefinitions(
		int count, double available,
		Func<int, GridLength> getLength,
		Func<int, double> getMin,
		Func<int, double> getMax,
		double[] sizes)
	{
		double usedFixed = 0;
		for (int i = 0; i < count; i++)
		{
			var gl = getLength(i);
			if (gl.IsAbsolute)
			{
				sizes[i] = Math.Max(getMin(i), Math.Min(gl.Value, getMax(i)));
				usedFixed += sizes[i];
			}
			// Auto and Star start at 0; resolved later.
		}
		return sizes;
	}

	/// <summary>
	/// Distribute remaining space to Star columns/rows using WPF's iterative Min/Max
	/// clamping algorithm. Tracks that hit their Min or Max are fixed; remaining space
	/// is redistributed among still-flexible tracks until no new track clamps.
	/// </summary>
	static double[] ResolveStarDistribution(
		double[] sizes, int count, double available,
		Func<int, GridLength> getLength,
		Func<int, double> getMin,
		Func<int, double> getMax)
	{
		if (double.IsPositiveInfinity(available))
		{
			// In infinite space Star = 0 (like WPF).
			for (int i = 0; i < count; i++)
			{
				var gl = getLength(i);
				if (gl.IsStar) sizes[i] = 0;
			}
			return sizes;
		}

		// Sum non-star (fixed + auto) used space.
		double usedNonStar = 0;
		for (int i = 0; i < count; i++)
		{
			var gl = getLength(i);
			if (!gl.IsStar) usedNonStar += sizes[i];
		}

		double remaining = Math.Max(0, available - usedNonStar);

		// Iterative WPF star distribution: each pass fixes tracks that hit Min/Max,
		// redistributes the remaining space among still-flexible tracks.
		bool[] clamped = new bool[count];
		bool anyNewClamp = true;
		while (anyNewClamp)
		{
			anyNewClamp = false;
			double flexWeight = 0;
			for (int i = 0; i < count; i++)
			{
				if (!clamped[i] && getLength(i).IsStar)
					flexWeight += getLength(i).Value;
			}

			if (flexWeight <= 0) break;

			// Assign unclamped stars proportionally.
			for (int i = 0; i < count; i++)
			{
				if (clamped[i] || !getLength(i).IsStar) continue;
				double val = remaining * getLength(i).Value / flexWeight;
				double min = getMin(i);
				double max = getMax(i);
				if (val <= min)
				{
					sizes[i] = min;
					remaining -= min;
					clamped[i] = true;
					anyNewClamp = true;
				}
				else if (val >= max)
				{
					sizes[i] = max;
					remaining -= max;
					clamped[i] = true;
					anyNewClamp = true;
				}
				else
				{
					sizes[i] = val; // tentative — will be overwritten next iteration if not clamped
				}
			}
		}

		// Final proportional pass for any remaining flexible tracks.
		{
			double flexWeight = 0;
			for (int i = 0; i < count; i++)
				if (!clamped[i] && getLength(i).IsStar)
					flexWeight += getLength(i).Value;

			if (flexWeight > 0)
			{
				for (int i = 0; i < count; i++)
				{
					if (!clamped[i] && getLength(i).IsStar)
						sizes[i] = remaining * getLength(i).Value / flexWeight;
				}
			}
		}

		return sizes;
	}

	void MeasureAutoChildren(int colCount, int rowCount, double[] colWidths, double[] rowHeights)
	{
		foreach (UIElement child in Children)
		{
			var cellSize = GetCellSize(child, colCount, rowCount, colWidths, rowHeights);
			child.Measure(cellSize);
		}
	}

	/// <summary>
	/// Update Auto dimension sizes to fit child desired sizes.
	/// Span=1 children set their Auto track directly.
	/// Span&gt;1 children: if the spanned Auto tracks cannot already contain the desired size,
	/// distribute the deficit equally among the Auto tracks in the span (matching WPF).
	/// </summary>
	void UpdateAutoFromChildren(double[] sizes, int count, bool isColumn)
	{
		// Pass 1: single-span Auto tracks.
		foreach (UIElement child in Children)
		{
			int idx = isColumn ? Math.Max(0, Math.Min(GetColumn(child), count - 1))
				: Math.Max(0, Math.Min(GetRow(child), count - 1));
			int span = isColumn ? Math.Max(1, GetColumnSpan(child))
				: Math.Max(1, GetRowSpan(child));

			if (span != 1) continue;

			bool isAuto = isColumn
				? (ColumnDefinitions.Count > idx && ColumnDefinitions[idx].Width.IsAuto)
				: (RowDefinitions.Count > idx && RowDefinitions[idx].Height.IsAuto);

			if (!isAuto) continue;

			double desired = isColumn ? child.DesiredSize.Width : child.DesiredSize.Height;
			double minVal = isColumn
				? (ColumnDefinitions.Count > idx ? ColumnDefinitions[idx].MinWidth : 0)
				: (RowDefinitions.Count > idx ? RowDefinitions[idx].MinHeight : 0);
			double maxVal = isColumn
				? (ColumnDefinitions.Count > idx ? ColumnDefinitions[idx].MaxWidth : double.PositiveInfinity)
				: (RowDefinitions.Count > idx ? RowDefinitions[idx].MaxHeight : double.PositiveInfinity);

			sizes[idx] = Math.Max(sizes[idx], Math.Max(minVal, Math.Min(desired, maxVal)));
		}

		// Pass 2: spanning children — grow Auto tracks in the span to cover desired size.
		foreach (UIElement child in Children)
		{
			int start = isColumn ? Math.Max(0, Math.Min(GetColumn(child), count - 1))
				: Math.Max(0, Math.Min(GetRow(child), count - 1));
			int span = isColumn ? Math.Max(1, GetColumnSpan(child))
				: Math.Max(1, GetRowSpan(child));
			int end = Math.Min(start + span, count);

			if (span <= 1) continue;

			// Collect Auto tracks in the span.
			var autoIdxs = new System.Collections.Generic.List<int>();
			for (int i = start; i < end; i++)
			{
				bool isAuto = isColumn
					? (ColumnDefinitions.Count > i && ColumnDefinitions[i].Width.IsAuto)
					: (RowDefinitions.Count > i && RowDefinitions[i].Height.IsAuto);
				if (isAuto) autoIdxs.Add(i);
			}

			if (autoIdxs.Count == 0) continue; // no Auto tracks to grow

			double currentSum = SumRange(sizes, start, end - start);
			double desired = isColumn ? child.DesiredSize.Width : child.DesiredSize.Height;
			double deficit = desired - currentSum;

			if (deficit <= 0) continue; // already fits

			// Distribute deficit equally among Auto tracks.
			double share = deficit / autoIdxs.Count;
			foreach (int ai in autoIdxs)
			{
				double minVal = isColumn
					? (ColumnDefinitions.Count > ai ? ColumnDefinitions[ai].MinWidth : 0)
					: (RowDefinitions.Count > ai ? RowDefinitions[ai].MinHeight : 0);
				double maxVal = isColumn
					? (ColumnDefinitions.Count > ai ? ColumnDefinitions[ai].MaxWidth : double.PositiveInfinity)
					: (RowDefinitions.Count > ai ? RowDefinitions[ai].MaxHeight : double.PositiveInfinity);
				sizes[ai] = Math.Max(minVal, Math.Min(sizes[ai] + share, maxVal));
			}
		}
	}

	Size GetCellSize(UIElement child, int colCount, int rowCount, double[] colWidths, double[] rowHeights)
	{
		int col = Math.Max(0, Math.Min(GetColumn(child), colCount - 1));
		int row = Math.Max(0, Math.Min(GetRow(child), rowCount - 1));
		int colSpan = Math.Max(1, Math.Min(GetColumnSpan(child), colCount - col));
		int rowSpan = Math.Max(1, Math.Min(GetRowSpan(child), rowCount - row));

		double w = SumRange(colWidths, col, colSpan);
		double h = SumRange(rowHeights, row, rowSpan);

		// Infinite for Auto/Star to allow natural size.
		if (w == 0 && colSpan == 1)
		{
			bool isAuto = ColumnDefinitions.Count > col && ColumnDefinitions[col].Width.IsAuto;
			bool isStar = ColumnDefinitions.Count == 0 ||
			              (ColumnDefinitions.Count > col && ColumnDefinitions[col].Width.IsStar);
			if (isAuto || isStar) w = double.PositiveInfinity;
		}
		if (h == 0 && rowSpan == 1)
		{
			bool isAuto = RowDefinitions.Count > row && RowDefinitions[row].Height.IsAuto;
			bool isStar = RowDefinitions.Count == 0 ||
			              (RowDefinitions.Count > row && RowDefinitions[row].Height.IsStar);
			if (isAuto || isStar) h = double.PositiveInfinity;
		}

		return new Size(w, h);
	}

	static double SumRange(double[] arr, int start, int count)
	{
		double sum = 0;
		for (int i = start; i < start + count && i < arr.Length; i++)
			sum += arr[i];
		return sum;
	}

	static double SumFinite(double[] arr)
	{
		double sum = 0;
		foreach (var v in arr) if (!double.IsPositiveInfinity(v)) sum += v;
		return sum;
	}

	static double[] BuildOffsets(double[] sizes)
	{
		var offsets = new double[sizes.Length];
		double pos = 0;
		for (int i = 0; i < sizes.Length; i++)
		{
			offsets[i] = pos;
			pos += sizes[i];
		}
		return offsets;
	}
}