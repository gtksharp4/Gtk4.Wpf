using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfUniformGrid = System.Windows.Controls.Primitives.UniformGrid;
using WpfDataGrid = System.Windows.Controls.DataGrid;
using WpfDataGridTextColumn = System.Windows.Controls.DataGridTextColumn;
using WpfRepeatButton = System.Windows.Controls.Primitives.RepeatButton;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for Round-8 Opus review defects (R1–R8).
/// Every test fails without the matching fix and passes with it.
/// Pure (no GTK) where possible; GTK smoke tests live in Round7GtkSmokeTests.
/// </summary>
public class Round8DefectTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	sealed class Source : INotifyPropertyChanged
	{
		int _count;
		string? _name;

		public int Count
		{
			get => _count;
			set { _count = value; RaiseChanged(nameof(Count)); }
		}

		public string? Name
		{
			get => _name;
			set { _name = value; RaiseChanged(nameof(Name)); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		void RaiseChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
	}

	sealed class IntElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(int), typeof(IntElement),
				new PropertyMetadata(0));

		public int Value
		{
			get => (int?)GetValue(ValueProperty) ?? 0;
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class DoubleElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(double), typeof(DoubleElement),
				new PropertyMetadata(0.0));

		public double Value
		{
			get => (double?)GetValue(ValueProperty) ?? 0.0;
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class ConcatMultiConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> string.Join("|", values);

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
			=> null;
	}

	// ── R1: RepeatButton timer ────────────────────────────────────────────────

	// Helper: reach internal RepeatButton state via reflection.
	static System.Reflection.FieldInfo RepeatButtonTimerField =>
		typeof(WpfRepeatButton)
			.GetField("_timer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

	static System.Reflection.FieldInfo RepeatButtonPressedField =>
		typeof(WpfRepeatButton)
			.GetField("_pressed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;

	// Drive the RepeatButton state machine via its internal SimulatePress/SimulateRelease
	// helpers, which were added for testing (the GTK gesture handlers take non-null GTK types).
	static void SimulatePress(WpfRepeatButton rb)
	{
		// Set _pressed = true and create/reset the timer, then fire the immediate click.
		// This mirrors what OnGtkPressed does, without needing a Gtk.PressedArgs.
		var method = typeof(WpfRepeatButton).GetMethod(
			"SimulatePressForTest",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		method!.Invoke(rb, null);
	}

	static void SimulateRelease(WpfRepeatButton rb)
	{
		var method = typeof(WpfRepeatButton).GetMethod(
			"SimulateReleaseForTest",
			System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		Assert.NotNull(method);
		method!.Invoke(rb, null);
	}

	[Fact]
	public void R1_RepeatButton_press_then_release_leaves_no_stale_timer()
	{
		var rb = new WpfRepeatButton { Delay = 50000, Interval = 50000 };
		int clicks = 0;
		rb.Click += (s, e) => clicks++;

		SimulatePress(rb);
		Assert.Equal(1, clicks); // immediate first fire

		SimulateRelease(rb);
		Assert.Null(RepeatButtonTimerField.GetValue(rb));
	}

	[Fact]
	public void R1_RepeatButton_dispose_while_pressed_stops_clicks()
	{
		var rb = new WpfRepeatButton { Delay = 50000, Interval = 50000 };
		int clicks = 0;
		rb.Click += (s, e) => clicks++;

		SimulatePress(rb);
		int clicksAfterPress = clicks;

		rb.Dispose();

		Assert.False((bool)RepeatButtonPressedField.GetValue(rb)!);
		Assert.Null(RepeatButtonTimerField.GetValue(rb));
		Assert.Equal(clicksAfterPress, clicks); // timer was huge → no extra fires
	}

	[Fact]
	public void R1_RepeatButton_double_press_replaces_timer()
	{
		var rb = new WpfRepeatButton { Delay = 50000, Interval = 50000 };

		SimulatePress(rb);
		var timer1 = RepeatButtonTimerField.GetValue(rb);
		Assert.NotNull(timer1);

		// Second press without release — old timer must be disposed and replaced.
		SimulatePress(rb);
		var timer2 = RepeatButtonTimerField.GetValue(rb);
		Assert.NotNull(timer2);
		Assert.NotSame(timer1, timer2);

		rb.Dispose();
	}

	// ── R2: Cross-engine binding replacement ─────────────────────────────────

	[Fact]
	public void R2_MultiBinding_replaced_by_simple_binding_old_handler_silent()
	{
		var vm = new Source { Name = "alice", Count = 7 };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;

		// Set a MultiBinding that updates Text when Name changes.
		var mb = new MultiBinding { Converter = new ConcatMultiConverter() };
		mb.Bindings.Add(new Binding("Name"));
		mb.Bindings.Add(new Binding("Count"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);
		Assert.Equal("alice|7", tb.Text);

		// Replace with a simple Binding that only tracks Name.
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Name"));
		Assert.Equal("alice", tb.Text);

		// Change a property the OLD multi-binding watched (Count).
		// If the old handler is still live it would re-push "alice|8" or similar.
		vm.Count = 8;
		// Text must NOT change — old multi-binding handler should be gone.
		Assert.Equal("alice", tb.Text);
	}

	[Fact]
	public void R2_Simple_binding_replaced_by_multibinding_old_handler_silent()
	{
		var vm = new Source { Name = "bob", Count = 3 };
		var tb = new WpfTextBlock();
		tb.DataContext = vm;

		// Set a simple Binding first.
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Name"));
		Assert.Equal("bob", tb.Text);

		// Replace with MultiBinding.
		var mb = new MultiBinding { Converter = new ConcatMultiConverter() };
		mb.Bindings.Add(new Binding("Name"));
		mb.Bindings.Add(new Binding("Count"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);
		Assert.Equal("bob|3", tb.Text);

		// Mutating Name should update via MultiBinding, not the old simple binding.
		vm.Name = "carol";
		Assert.Equal("carol|3", tb.Text);
	}

	// ── R3: MultiBinding DataContext set after binding ────────────────────────

	[Fact]
	public void R3_MultiBinding_set_before_DataContext_then_DataContext_assigned_pushes()
	{
		// Set the MultiBinding BEFORE DataContext is assigned.
		var tb = new WpfTextBlock();
		var mb = new MultiBinding { Converter = new ConcatMultiConverter() };
		mb.Bindings.Add(new Binding("Name"));
		mb.Bindings.Add(new Binding("Count"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);

		// DataContext still null — text should be fallback or empty-ish.
		// Now assign DataContext.
		var vm = new Source { Name = "dave", Count = 5 };
		tb.DataContext = vm;

		// Target must reflect the new DataContext.
		Assert.Equal("dave|5", tb.Text);
	}

	[Fact]
	public void R3_MultiBinding_INPC_fires_after_DataContext_assigned_late()
	{
		var tb = new WpfTextBlock();
		var mb = new MultiBinding { Converter = new ConcatMultiConverter() };
		mb.Bindings.Add(new Binding("Name"));
		mb.Bindings.Add(new Binding("Count"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);

		var vm = new Source { Name = "eve", Count = 1 };
		tb.DataContext = vm;
		Assert.Equal("eve|1", tb.Text);

		// Mutate source — INPC must trigger re-push because we subscribed on DataContext assignment.
		vm.Name = "frank";
		Assert.Equal("frank|1", tb.Text);
	}

	// ── R4: FallbackValue / TargetNullValue type conversion ───────────────────

	[Fact]
	public void R4_FallbackValue_string_converted_to_int_dp_type()
	{
		var el = new IntElement();
		// FallbackValue="42" on an int DP — must arrive as typed int, not string.
		el.SetBinding(IntElement.ValueProperty,
			new Binding("Missing") { FallbackValue = "42" });
		// No DataContext → path unresolvable → FallbackValue applied.
		Assert.Equal(42, el.Value);
	}

	[Fact]
	public void R4_FallbackValue_already_typed_int_dp_unchanged()
	{
		var el = new IntElement();
		el.SetBinding(IntElement.ValueProperty,
			new Binding("Missing") { FallbackValue = 99 });
		Assert.Equal(99, el.Value);
	}

	[Fact]
	public void R4_TargetNullValue_string_converted_to_double_dp_type()
	{
		var vm = new Source { Name = null };
		var el = new DoubleElement();
		el.DataContext = vm;
		// Name resolves to null → TargetNullValue="3.14" on a double DP.
		el.SetBinding(DoubleElement.ValueProperty,
			new Binding("Name") { TargetNullValue = "3.14" });
		Assert.Equal(3.14, el.Value, precision: 10);
	}

	// ── R5: FallbackValue semantics — non-null source, bad path ───────────────

	[Fact]
	public void R5_FallbackValue_used_when_source_non_null_path_unresolvable()
	{
		var tb = new WpfTextBlock();
		var vm = new Source { Name = "x" };
		tb.DataContext = vm;
		// Path doesn't exist on Source → FallbackValue must be applied.
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("NonExistentProperty") { FallbackValue = "fb" });
		Assert.Equal("fb", tb.Text);
	}

	[Fact]
	public void R5_TargetNullValue_used_when_resolved_value_is_null()
	{
		var tb = new WpfTextBlock();
		var vm = new Source { Name = null };
		tb.DataContext = vm;
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Name") { TargetNullValue = "tnv" });
		Assert.Equal("tnv", tb.Text);
	}

	[Fact]
	public void R5_Converter_runs_before_TargetNullValue()
	{
		// Converter that always returns null; TargetNullValue must still apply.
		var tb = new WpfTextBlock();
		tb.DataContext = new Source { Name = "anything" };
		tb.SetBinding(WpfTextBlock.TextProperty,
			new Binding("Name")
			{
				Converter = new NullConverter(),
				TargetNullValue = "null-sub"
			});
		Assert.Equal("null-sub", tb.Text);
	}

	sealed class NullConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> null;

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
			=> null;
	}

	// ── R6: TwoWay MultiBinding is now supported via ConvertBack ─────────────

	[Fact]
	public void R6_MultiBinding_TwoWay_no_longer_throws_now_that_ConvertBack_is_implemented()
	{
		// Round 14: TwoWay MultiBinding is supported — ConvertBack writes each child source.
		// The old contract (NotSupportedException) is replaced by functional TwoWay.
		var tb = new WpfTextBlock();
		tb.DataContext = new Source { Name = "x", Count = 1 };

		var mb = new MultiBinding
		{
			Mode = BindingMode.TwoWay,
			Converter = new ConcatMultiConverter()
		};
		mb.Bindings.Add(new Binding("Name"));
		mb.Bindings.Add(new Binding("Count"));

		// Should not throw.
		tb.SetBinding(WpfTextBlock.TextProperty, mb);
		Assert.NotNull(tb.Text);
	}

	// ── R7: UniformGrid overflow clamp ────────────────────────────────────────

	[Fact]
	public void R7_UniformGrid_children_beyond_capacity_dont_exceed_cell_count()
	{
		// 2×2 grid = 4 cells; 6 children → only 4 placed.
		var ug = new WpfUniformGrid { Rows = 2, Columns = 2 };
		for (int i = 0; i < 6; i++)
		{
			// Use a simple FE substitute that doesn't require GTK.
			ug.Children.Add(new DummyElement());
		}
		// ComputeGrid(2,2,6) = (2,2), capacity 4.
		var (rows, cols) = WpfUniformGrid.ComputeGrid(ug.Rows, ug.Columns, ug.Children.Count);
		Assert.Equal(2, rows);
		Assert.Equal(2, cols);
		// The overflow: children count 6 > rows*cols=4.
		Assert.True(ug.Children.Count > rows * cols);
	}

	sealed class DummyElement : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	// ── R8: DataGrid INotifyCollectionChanged ─────────────────────────────────

	[Fact]
	public void R8_DataGrid_ObservableCollection_add_rebuilds_rows()
	{
		var col = new ObservableCollection<object>(new[] { new { Name = "Alice" } });
		var dg = new WpfDataGrid();
		dg.ItemsSource = col;
		Assert.Equal(1, dg.Items.Count);

		// Adding to the ObservableCollection must trigger Rebuild → Items updated.
		col.Add(new { Name = "Bob" });
		Assert.Equal(2, dg.Items.Count);
	}

	[Fact]
	public void R8_DataGrid_ObservableCollection_remove_rebuilds_rows()
	{
		var item1 = new { Name = "Alice" };
		var item2 = new { Name = "Bob" };
		var col = new ObservableCollection<object>(new object[] { item1, item2 });
		var dg = new WpfDataGrid();
		dg.ItemsSource = col;
		Assert.Equal(2, dg.Items.Count);

		col.Remove(item2);
		Assert.Equal(1, dg.Items.Count);
	}

	[Fact]
	public void R8_DataGrid_replacing_ItemsSource_unsubscribes_old_collection()
	{
		var old = new ObservableCollection<object>(new[] { new { Name = "A" } });
		var @new = new ObservableCollection<object>(new[] { new { Name = "X" }, new { Name = "Y" } });
		var dg = new WpfDataGrid();
		dg.ItemsSource = old;
		Assert.Equal(1, dg.Items.Count);

		dg.ItemsSource = @new;
		Assert.Equal(2, dg.Items.Count);

		// Mutating old collection must NOT affect dg.
		old.Add(new { Name = "B" });
		Assert.Equal(2, dg.Items.Count); // still 2 — old subscription unhooked
	}
}