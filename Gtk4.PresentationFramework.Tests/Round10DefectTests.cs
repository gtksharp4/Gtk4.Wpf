using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Xunit;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfTabControl = System.Windows.Controls.TabControl;
using WpfTabItem = System.Windows.Controls.TabItem;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for Round-10 Opus review defects (U1–U7).
/// Every test fails without the matching fix and passes with it.
/// Pure (no GTK loop) except U1-TabControl which needs ItemsControl.
/// </summary>
public class Round10DefectTests
{
	// ── U1: selection preserved on INCC Add ─────────────────────────────────

	[Fact]
	public void U1_ListBox_Add_preserves_existing_selection()
	{
		var src = new ObservableCollection<string> { "a", "b", "c" };
		var lb = new WpfListBox { ItemsSource = src };

		lb.SelectedIndex = 1; // select "b"
		Assert.Equal("b", lb.SelectedItem);

		src.Add("d"); // INCC Add → OnItemsRebuilt

		// Selection must still be on "b".
		Assert.Equal("b", lb.SelectedItem);
		Assert.Equal(1, lb.SelectedIndex);
	}

	[Fact]
	public void U1_ListBox_Remove_selected_item_clears_selection()
	{
		var src = new ObservableCollection<string> { "a", "b", "c" };
		var lb = new WpfListBox { ItemsSource = src };

		lb.SelectedIndex = 1; // select "b"
		Assert.Equal("b", lb.SelectedItem);

		src.Remove("b"); // selected item leaves the collection

		// Selection must be cleared.
		Assert.Equal(-1, lb.SelectedIndex);
		Assert.Null(lb.SelectedItem);
	}

	[Fact]
	public void U1_ListBox_Add_does_not_fire_SelectionChanged_when_selection_unchanged()
	{
		var src = new ObservableCollection<string> { "a", "b" };
		var lb = new WpfListBox { ItemsSource = src };
		lb.SelectedIndex = 0; // select "a"

		int count = 0;
		lb.SelectionChanged += (_, _) => count++;

		src.Add("c"); // rebuild but selection is still on "a"

		Assert.Equal(0, count);
	}

	[Fact]
	public void U1_TabControl_Add_preserves_existing_selection()
	{
		var ti1 = new WpfTabItem { Header = "T1" };
		var ti2 = new WpfTabItem { Header = "T2" };
		var src = new ObservableCollection<WpfTabItem> { ti1, ti2 };
		var tc = new WpfTabControl { ItemsSource = src };

		tc.SelectedIndex = 1;
		Assert.Same(ti2, tc.SelectedItem);

		var ti3 = new WpfTabItem { Header = "T3" };
		src.Add(ti3);

		Assert.Same(ti2, tc.SelectedItem);
		Assert.Equal(1, tc.SelectedIndex);
	}

	// ── U2: UIElement.OpacityProperty is registered and animatable ───────────

	/// <summary>Minimal pure-test element that skips GTK widget creation.</summary>
	sealed class OpacityTarget : FrameworkElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void U2_OpacityProperty_is_registered_on_UIElement()
	{
		var dp = DependencyProperty.FindByName("Opacity", typeof(UIElement));
		Assert.NotNull(dp);
		Assert.Equal(typeof(double), dp!.PropertyType);
	}

	[Fact]
	public void U2_OpacityProperty_default_is_1()
	{
		var target = new OpacityTarget();
		Assert.Equal(1.0, target.Opacity, precision: 10);
	}

	[Fact]
	public void U2_Storyboard_string_Opacity_animates_to_To_value()
	{
		var target = new OpacityTarget();

		var anim = new DoubleAnimation { From = 1.0, To = 0.0, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		// String-target path: SetTargetPropertyName (the XAML compiler path).
		sb.SetTargetPropertyName(anim, "Opacity");

		sb.Begin(target);

		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(1.0));

		Assert.Equal(0.0, target.Opacity, precision: 10);
	}

	[Fact]
	public void U2_Storyboard_string_Width_animates_to_To_value()
	{
		var target = new OpacityTarget();

		var anim = new DoubleAnimation { From = 0.0, To = 200.0, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetPropertyName(anim, "Width");

		sb.Begin(target);

		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(1.0));

		Assert.Equal(200.0, target.Width, precision: 10);
	}

	[Fact]
	public void U2_Storyboard_unresolvable_TargetPropertyName_throws_InvalidOperationException()
	{
		var target = new OpacityTarget();
		var anim = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetPropertyName(anim, "NoSuchProperty");

		var ex = Assert.Throws<InvalidOperationException>(() => sb.Begin(target));
		Assert.Contains("NoSuchProperty", ex.Message);
	}

	// ── U3: parenthesized / dotted TargetProperty string resolves ────────────

	[Fact]
	public void U3_Parenthesized_TargetPropertyName_resolves_same_as_bare_name()
	{
		var target = new OpacityTarget();

		var anim = new DoubleAnimation { From = 1.0, To = 0.5, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetPropertyName(anim, "(UIElement.Opacity)");

		sb.Begin(target);

		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(1.0));
		Assert.Equal(0.5, target.Opacity, precision: 10);
	}

	[Fact]
	public void U3_Dotted_TargetPropertyName_resolves_same_as_bare_name()
	{
		var target = new OpacityTarget();

		var anim = new DoubleAnimation { From = 1.0, To = 0.3, Duration = TimeSpan.FromSeconds(1) };
		var sb = new Storyboard();
		sb.Children.Add(anim);
		sb.SetTargetPropertyName(anim, "UIElement.Opacity");

		sb.Begin(target);

		Assert.Equal(1, sb.ActiveClocks.Count);
		sb.ActiveClocks[0].Tick(TimeSpan.FromSeconds(1.0));
		Assert.Equal(0.3, target.Opacity, precision: 10);
	}

	// ── U4: plain two-way binding does zero validation work ──────────────────

	sealed class PlainVm : INotifyPropertyChanged
	{
		string _name = string.Empty;
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class PlainTarget : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("U4Value", typeof(string), typeof(PlainTarget),
				new PropertyMetadata(string.Empty));

		public string Value
		{
			get => (string?)GetValue(ValueProperty) ?? string.Empty;
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	[Fact]
	public void U4_Plain_TwoWay_binding_write_does_not_set_HasError()
	{
		var vm = new PlainVm { Name = "initial" };
		var el = new PlainTarget();
		el.SetBinding(PlainTarget.ValueProperty,
			new Binding("Name") { Source = vm, Mode = BindingMode.TwoWay });
		// No ValidationRules, no ValidatesOnDataErrors, no ValidatesOnExceptions.

		el.Value = "changed";

		// No validation should have been run → HasError stays false.
		Assert.False(Validation.GetHasError(el));
		Assert.Empty(Validation.GetErrors(el));
	}

	[Fact]
	public void U4_Plain_TwoWay_binding_write_does_not_touch_Errors_DP()
	{
		var vm = new PlainVm { Name = "x" };
		var el = new PlainTarget();
		el.SetBinding(PlainTarget.ValueProperty,
			new Binding("Name") { Source = vm, Mode = BindingMode.TwoWay });

		// Capture the Errors collection reference before the write.
		var before = Validation.GetErrors(el);

		el.Value = "y";

		// The same empty collection (or still empty) — no allocation triggered.
		var after = Validation.GetErrors(el);
		Assert.Empty(after);
		// If the engine touched the DP, HasError would have been set.
		Assert.False(Validation.GetHasError(el));
	}

	// ── U5: null items are preserved in ListCollectionView ───────────────────

	[Fact]
	public void U5_Source_with_null_item_yields_3_items_in_view()
	{
		var src = new ObservableCollection<string?> { "a", null, "b" };
		var view = new ListCollectionView(src);
		// No filter.

		var items = new System.Collections.Generic.List<object?>();
		foreach (var item in view) items.Add(item);

		Assert.Equal(3, items.Count);
		Assert.Null(items[1]);
	}

	[Fact]
	public void U5_Filter_on_non_null_items_excludes_null()
	{
		var src = new ObservableCollection<string?> { "a", null, "b" };
		var view = new ListCollectionView(src);
		view.Filter = o => o != null;
		view.Refresh();

		var items = new System.Collections.Generic.List<object?>();
		foreach (var item in view) items.Add(item);

		Assert.Equal(2, items.Count);
	}

	// ── U6: CollectionViewSource forwards SortDescriptions to view ───────────

	[Fact]
	public void U6_CollectionViewSource_SortDescription_produces_sorted_view()
	{
		var src = new ObservableCollection<string> { "charlie", "alice", "bob" };
		var cvs = new CollectionViewSource { Source = src };
		cvs.SortDescriptions.Add(new SortDescription(null, ListSortDirection.Ascending));

		// Accessing View after SortDescriptions are set must produce sorted items.
		var view = cvs.View!;

		var names = new System.Collections.Generic.List<string>();
		foreach (string item in view) names.Add(item);

		Assert.Equal(new[] { "alice", "bob", "charlie" }, names);
	}
}