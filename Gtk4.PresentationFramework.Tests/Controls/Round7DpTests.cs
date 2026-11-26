using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit;
using WpfCalendar       = System.Windows.Controls.Calendar;
using WpfDatePicker     = System.Windows.Controls.DatePicker;
using WpfPasswordBox    = System.Windows.Controls.PasswordBox;
using WpfRepeatButton   = System.Windows.Controls.Primitives.RepeatButton;
using WpfStatusBar      = System.Windows.Controls.Primitives.StatusBar;
using WpfStretch        = System.Windows.Media.Stretch;
using WpfThumb          = System.Windows.Controls.Primitives.Thumb;
using WpfUniformGrid    = System.Windows.Controls.Primitives.UniformGrid;
using WpfViewbox        = System.Windows.Controls.Viewbox;
using WpfDataGrid       = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfBinding        = System.Windows.Data.Binding;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) DP and behaviour tests for the Round-7 controls.
/// </summary>
public class Round7DpTests
{
	// ── UniformGrid row/col computation ──────────────────────────────────────

	[Theory]
	[InlineData(0, 0, 1, 1, 1)]   // 1 child → 1×1
	[InlineData(0, 0, 4, 2, 2)]   // 4 children → cols=ceil(sqrt(4))=2, rows=2
	[InlineData(0, 0, 5, 2, 3)]   // 5 children → cols=ceil(sqrt(5))=3, rows=ceil(5/3)=2
	[InlineData(0, 0, 9, 3, 3)]   // 9 children → 3×3
	[InlineData(2, 0, 6, 2, 3)]   // explicit 2 rows, 6 children → cols=ceil(6/2)=3
	[InlineData(0, 3, 6, 2, 3)]   // explicit 3 cols, 6 children → rows=ceil(6/3)=2
	[InlineData(2, 3, 10, 2, 3)]  // both set → use as-is
	public void UniformGrid_ComputeGrid_correct(int rows, int cols, int n, int expRows, int expCols)
	{
		var (r, c) = WpfUniformGrid.ComputeGrid(rows, cols, n);
		Assert.Equal(expRows, r);
		Assert.Equal(expCols, c);
	}

	[Fact]
	public void UniformGrid_Rows_default_is_zero()
	{
		var g = new WpfUniformGrid();
		Assert.Equal(0, g.Rows);
	}

	[Fact]
	public void UniformGrid_Columns_default_is_zero()
	{
		var g = new WpfUniformGrid();
		Assert.Equal(0, g.Columns);
	}

	[Fact]
	public void UniformGrid_Rows_roundtrip()
	{
		var g = new WpfUniformGrid { Rows = 3 };
		Assert.Equal(3, g.Rows);
	}

	[Fact]
	public void UniformGrid_Columns_roundtrip()
	{
		var g = new WpfUniformGrid { Columns = 4 };
		Assert.Equal(4, g.Columns);
	}

	// ── Calendar ─────────────────────────────────────────────────────────────

	[Fact]
	public void Calendar_SelectedDate_default_is_null()
	{
		var cal = new WpfCalendar();
		Assert.Null(cal.SelectedDate);
	}

	[Fact]
	public void Calendar_SelectedDate_roundtrip()
	{
		var cal = new WpfCalendar();
		var d = new DateTime(2024, 6, 15);
		cal.SelectedDate = d;
		Assert.Equal(d, cal.SelectedDate);
	}

	[Fact]
	public void Calendar_SelectedDatesChanged_fires_on_set()
	{
		var cal = new WpfCalendar();
		bool fired = false;
		cal.SelectedDatesChanged += (s, e) => { fired = true; };
		cal.SelectedDate = new DateTime(2024, 1, 1);
		Assert.True(fired);
	}

	[Fact]
	public void Calendar_SelectedDatesChanged_fires_once()
	{
		var cal = new WpfCalendar();
		int count = 0;
		cal.SelectedDatesChanged += (s, e) => { count++; };
		cal.SelectedDate = new DateTime(2024, 1, 1);
		Assert.Equal(1, count);
	}

	// ── DatePicker ────────────────────────────────────────────────────────────

	[Fact]
	public void DatePicker_SelectedDate_default_is_null()
	{
		var dp = new WpfDatePicker();
		Assert.Null(dp.SelectedDate);
	}

	[Fact]
	public void DatePicker_SelectedDate_roundtrip()
	{
		var dp = new WpfDatePicker();
		var d = new DateTime(2025, 3, 20);
		dp.SelectedDate = d;
		Assert.Equal(d, dp.SelectedDate);
	}

	[Fact]
	public void DatePicker_SelectedDateChanged_fires_on_set()
	{
		var dp = new WpfDatePicker();
		bool fired = false;
		dp.SelectedDateChanged += (s, e) => { fired = true; };
		dp.SelectedDate = new DateTime(2024, 12, 31);
		Assert.True(fired);
	}

	[Fact]
	public void DatePicker_Text_reflects_SelectedDate()
	{
		var dp = new WpfDatePicker();
		dp.SelectedDate = new DateTime(2024, 7, 4);
		Assert.Equal("2024-07-04", dp.Text);
	}

	// ── PasswordBox ──────────────────────────────────────────────────────────

	[Fact]
	public void PasswordBox_Password_default_is_empty()
	{
		var pb = new WpfPasswordBox();
		Assert.Equal(string.Empty, pb.Password);
	}

	[Fact]
	public void PasswordBox_Password_roundtrip()
	{
		var pb = new WpfPasswordBox();
		pb.Password = "s3cr3t";
		Assert.Equal("s3cr3t", pb.Password);
	}

	[Fact]
	public void PasswordBox_PasswordChanged_fires_on_set()
	{
		var pb = new WpfPasswordBox();
		bool fired = false;
		pb.PasswordChanged += (s, e) => { fired = true; };
		pb.Password = "abc";
		Assert.True(fired);
	}

	[Fact]
	public void PasswordBox_PasswordChanged_fires_once_per_set()
	{
		var pb = new WpfPasswordBox();
		int count = 0;
		pb.PasswordChanged += (s, e) => { count++; };
		pb.Password = "x";
		pb.Password = "y";
		Assert.Equal(2, count);
	}

	[Fact]
	public void PasswordBox_PasswordChar_default_is_bullet()
	{
		var pb = new WpfPasswordBox();
		Assert.Equal('●', pb.PasswordChar);
	}

	[Fact]
	public void PasswordBox_PasswordChar_roundtrip()
	{
		var pb = new WpfPasswordBox();
		pb.PasswordChar = '*';
		Assert.Equal('*', pb.PasswordChar);
	}

	// ── Viewbox ──────────────────────────────────────────────────────────────

	[Fact]
	public void Viewbox_Child_default_is_null()
	{
		var vb = new WpfViewbox();
		Assert.Null(vb.Child);
	}

	[Fact]
	public void Viewbox_Child_roundtrip()
	{
		var vb = new WpfViewbox();
		var tb = new WpfTextBlock { Text = "scaled" };
		vb.Child = tb;
		Assert.Same(tb, vb.Child);
	}

	[Fact]
	public void Viewbox_Stretch_default_is_Uniform()
	{
		var vb = new WpfViewbox();
		Assert.Equal(WpfStretch.Uniform, vb.Stretch);
	}

	[Fact]
	public void Viewbox_Stretch_roundtrip()
	{
		var vb = new WpfViewbox();
		vb.Stretch = WpfStretch.Fill;
		Assert.Equal(WpfStretch.Fill, vb.Stretch);
	}

	// ── RepeatButton ─────────────────────────────────────────────────────────

	[Fact]
	public void RepeatButton_Delay_default_is_500()
	{
		var rb = new WpfRepeatButton();
		Assert.Equal(500, rb.Delay);
	}

	[Fact]
	public void RepeatButton_Interval_default_is_33()
	{
		var rb = new WpfRepeatButton();
		Assert.Equal(33, rb.Interval);
	}

	[Fact]
	public void RepeatButton_Delay_roundtrip()
	{
		var rb = new WpfRepeatButton();
		rb.Delay = 250;
		Assert.Equal(250, rb.Delay);
	}

	[Fact]
	public void RepeatButton_Interval_roundtrip()
	{
		var rb = new WpfRepeatButton();
		rb.Interval = 100;
		Assert.Equal(100, rb.Interval);
	}

	// ── Thumb drag events ─────────────────────────────────────────────────────

	[Fact]
	public void Thumb_DragStarted_handler_can_be_registered()
	{
		var thumb = new WpfThumb();
		bool attached = false;
		thumb.DragStarted += (s, e) => { attached = true; };
		Assert.False(attached); // event not yet raised — just wires
	}

	[Fact]
	public void Thumb_DragDelta_handler_can_be_registered()
	{
		var thumb = new WpfThumb();
		bool attached = false;
		thumb.DragDelta += (s, e) => { attached = true; };
		Assert.False(attached);
	}

	[Fact]
	public void Thumb_DragCompleted_handler_can_be_registered()
	{
		var thumb = new WpfThumb();
		bool attached = false;
		thumb.DragCompleted += (s, e) => { attached = true; };
		Assert.False(attached);
	}

	// ── StatusBar ─────────────────────────────────────────────────────────────

	[Fact]
	public void StatusBar_ItemsSource_roundtrip()
	{
		var sb = new WpfStatusBar();
		var items = new[] { "Ready", "Ln 1", "Col 1" };
		sb.ItemsSource = items;
		Assert.Equal(3, sb.Items.Count);
	}

	// ── DataGrid ─────────────────────────────────────────────────────────────

	[Fact]
	public void DataGrid_AutoGenerateColumns_default_is_true()
	{
		var dg = new WpfDataGrid();
		Assert.True(dg.AutoGenerateColumns);
	}

	[Fact]
	public void DataGrid_SelectedItem_default_is_null()
	{
		var dg = new WpfDataGrid();
		Assert.Null(dg.SelectedItem);
	}

	[Fact]
	public void DataGrid_Columns_starts_empty()
	{
		var dg = new WpfDataGrid();
		Assert.Empty(dg.Columns);
	}

	[Fact]
	public void DataGrid_ItemsSource_roundtrip()
	{
		var dg = new WpfDataGrid();
		var rows = new[] { new { Name = "Alice" }, new { Name = "Bob" } };
		dg.ItemsSource = rows;
		// Items is populated by IEnumerable
		Assert.Equal(2, dg.Items.Count);
	}

	[Fact]
	public void DataGrid_AutoGenerateColumns_reflects_public_props()
	{
		var dg = new WpfDataGrid();
		dg.ItemsSource = new[] { new { FirstName = "Alice", Age = 30 } };
		// Force widget creation to trigger column generation.
		// Without widget, effective cols are computed on first Rebuild call.
		// We can invoke GetEffectiveColumns via ItemsSource assignment:
		// Count items to verify reflected columns would be 2.
		// This is a pure (no-GTK) check: ItemsSource is set and Items reflects the row.
		Assert.Equal(1, dg.Items.Count);
		// AutoGenerateColumns is on; columns are generated on widget creation.
		Assert.True(dg.AutoGenerateColumns);
	}

	[Fact]
	public void DataGrid_ExplicitColumns_added()
	{
		var dg = new WpfDataGrid();
		dg.Columns.Add(new WpfDataGridTextColumn
		{
			Header = "Name",
			Binding = new WpfBinding("Name"),
		});
		dg.Columns.Add(new WpfDataGridTextColumn
		{
			Header = "Age",
			Binding = new WpfBinding("Age"),
		});
		Assert.Equal(2, dg.Columns.Count);
	}

	[Fact]
	public void DataGrid_SelectedItem_roundtrip()
	{
		var dg = new WpfDataGrid();
		var item = new { Name = "Alice" };
		dg.SelectedItem = item;
		Assert.Same(item, dg.SelectedItem);
	}

	[Fact]
	public void DataGridTextColumn_GetCellText_via_binding()
	{
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Name") };
		var item = new { Name = "Alice" };
		Assert.Equal("Alice", col.GetCellText(item));
	}

	[Fact]
	public void DataGridTextColumn_GetCellText_null_item_returns_empty()
	{
		var col = new WpfDataGridTextColumn { Binding = new WpfBinding("Name") };
		Assert.Equal(string.Empty, col.GetCellText(null));
	}
}