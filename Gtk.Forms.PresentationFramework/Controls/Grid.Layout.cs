using System;
using System.Collections.Generic;

namespace System.Windows.Controls
{
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
		/// Distribute remaining space to Star columns/rows proportionally.
		/// Auto sizes have already been set by UpdateAutoFromChildren.
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

			// Sum fixed + auto.
			double usedNonStar = 0;
			double totalStarWeight = 0;
			for (int i = 0; i < count; i++)
			{
				var gl = getLength(i);
				if (gl.IsStar)
					totalStarWeight += gl.Value;
				else
					usedNonStar += sizes[i];
			}

			double remaining = Math.Max(0, available - usedNonStar);

			if (totalStarWeight > 0)
			{
				for (int i = 0; i < count; i++)
				{
					var gl = getLength(i);
					if (gl.IsStar)
					{
						double val = remaining * gl.Value / totalStarWeight;
						sizes[i] = Math.Max(getMin(i), Math.Min(val, getMax(i)));
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

		/// <summary>Update Auto dimension sizes to fit child desired sizes (span=1 only).</summary>
		void UpdateAutoFromChildren(double[] sizes, int count, bool isColumn)
		{
			foreach (UIElement child in Children)
			{
				int idx = isColumn ? Math.Max(0, Math.Min(GetColumn(child), count - 1))
					: Math.Max(0, Math.Min(GetRow(child), count - 1));
				int span = isColumn ? Math.Max(1, GetColumnSpan(child))
					: Math.Max(1, GetRowSpan(child));

				if (span != 1) continue; // span > 1: skip for now (WPF defers this too)

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
}
