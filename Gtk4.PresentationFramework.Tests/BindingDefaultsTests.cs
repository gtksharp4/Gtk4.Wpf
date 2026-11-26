using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Pure tests (no GTK widgets) for Mode/UpdateSourceTrigger default resolution and the marked DPs.</summary>
public class BindingDefaultsTests
{
	static readonly DependencyProperty PlainDp = DependencyProperty.Register(
		"BdPlain", typeof(string), typeof(BindingDefaultsTests), new PropertyMetadata(""));

	static readonly DependencyProperty OneWayFpmDp = DependencyProperty.Register(
		"BdOneWay", typeof(string), typeof(BindingDefaultsTests), new FrameworkPropertyMetadata(""));

	static readonly DependencyProperty TwoWayFpmDp = DependencyProperty.Register(
		"BdTwoWay", typeof(string), typeof(BindingDefaultsTests),
		new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	static readonly DependencyProperty LostFocusFpmDp = DependencyProperty.Register(
		"BdLostFocus", typeof(string), typeof(BindingDefaultsTests),
		new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, null, null, false,
			UpdateSourceTrigger.LostFocus));

	public static IEnumerable<object[]> ModeRows()
	{
		foreach (var dpName in new[] { "plain", "fpm", "twoway" })
		{
			yield return new object[] { BindingMode.Default, dpName, dpName == "twoway" ? BindingMode.TwoWay : BindingMode.OneWay };
			foreach (var explicitMode in new[]
				{ BindingMode.OneWay, BindingMode.TwoWay, BindingMode.OneTime, BindingMode.OneWayToSource })
				yield return new object[] { explicitMode, dpName, explicitMode };
		}
	}

	static DependencyProperty Pick(string name) => name switch
	{
		"plain" => PlainDp,
		"fpm" => OneWayFpmDp,
		_ => TwoWayFpmDp,
	};

	[Theory]
	[MemberData(nameof(ModeRows))]
	public void Resolve_mode(BindingMode mode, string dp, BindingMode expected)
	{
		var (resolved, _) = BindingDefaults.Resolve(
			mode, UpdateSourceTrigger.Default, Pick(dp), typeof(BindingDefaultsTests));
		Assert.Equal(expected, resolved);
	}

	[Theory]
	[InlineData(UpdateSourceTrigger.Default, UpdateSourceTrigger.PropertyChanged)]
	[InlineData(UpdateSourceTrigger.PropertyChanged, UpdateSourceTrigger.PropertyChanged)]
	[InlineData(UpdateSourceTrigger.LostFocus, UpdateSourceTrigger.LostFocus)]
	[InlineData(UpdateSourceTrigger.Explicit, UpdateSourceTrigger.Explicit)]
	public void Resolve_trigger_for_plain_dp(UpdateSourceTrigger trigger, UpdateSourceTrigger expected)
	{
		var (_, resolved) = BindingDefaults.Resolve(
			BindingMode.Default, trigger, PlainDp, typeof(BindingDefaultsTests));
		Assert.Equal(expected, resolved);
	}

	[Fact]
	public void Resolve_trigger_default_uses_metadata_default_and_explicit_wins()
	{
		Assert.Equal(UpdateSourceTrigger.LostFocus, BindingDefaults.Resolve(
			BindingMode.Default, UpdateSourceTrigger.Default, LostFocusFpmDp, typeof(BindingDefaultsTests)).Trigger);
		Assert.Equal(UpdateSourceTrigger.Explicit, BindingDefaults.Resolve(
			BindingMode.Default, UpdateSourceTrigger.Explicit, LostFocusFpmDp, typeof(BindingDefaultsTests)).Trigger);
	}

	[Fact]
	public void TextBox_Text_defaults_to_LostFocus_and_others_to_PropertyChanged()
	{
		Assert.Equal(UpdateSourceTrigger.LostFocus, BindingDefaults.Resolve(
			BindingMode.Default, UpdateSourceTrigger.Default, WpfTextBox.TextProperty, typeof(WpfTextBox)).Trigger);
		Assert.Equal(UpdateSourceTrigger.PropertyChanged, BindingDefaults.Resolve(
			BindingMode.Default, UpdateSourceTrigger.Default, CheckBox.IsCheckedProperty, typeof(CheckBox)).Trigger);
		Assert.Equal(UpdateSourceTrigger.PropertyChanged, BindingDefaults.Resolve(
			BindingMode.Default, UpdateSourceTrigger.Default, TextBlock.TextProperty, typeof(TextBlock)).Trigger);
	}

	[Fact]
	public void UpdateSourceTrigger_resolves_through_PresentationFramework_forward()
	{
		Assert.Equal(UpdateSourceTrigger.LostFocus, new Binding("x") { UpdateSourceTrigger = UpdateSourceTrigger.LostFocus }
			.UpdateSourceTrigger);
		Assert.Equal(typeof(FrameworkPropertyMetadata).Assembly, typeof(UpdateSourceTrigger).Assembly);
	}

	public static IEnumerable<object[]> MarkedDps()
	{
		DependencyProperty[] dps =
		{
			WpfTextBox.TextProperty, ToggleButton.IsCheckedProperty, CheckBox.IsCheckedProperty,
			RadioButton.IsCheckedProperty, MenuItem.IsCheckedProperty, ListBox.SelectedIndexProperty,
			ComboBox.SelectedIndexProperty, TabControl.SelectedIndexProperty, ListBoxItem.IsSelectedProperty,
			Expander.IsExpandedProperty, Slider.ValueProperty, ScrollBar.ValueProperty, ProgressBar.ValueProperty,
		};
		var owners = new[]
		{
			typeof(WpfTextBox), typeof(ToggleButton), typeof(CheckBox), typeof(RadioButton), typeof(MenuItem),
			typeof(ListBox), typeof(ComboBox), typeof(TabControl), typeof(ListBoxItem), typeof(Expander),
			typeof(Slider), typeof(ScrollBar), typeof(ProgressBar),
		};
		for (var i = 0; i < dps.Length; i++)
			yield return new object[] { owners[i].Name + "." + dps[i].Name, dps[i], owners[i], true };

		DependencyProperty[] noJournal =
		{
			ListBox.SelectedItemProperty, ListBox.SelectedValueProperty, ComboBox.SelectedItemProperty,
			ComboBox.SelectedValueProperty, TabControl.SelectedItemProperty, DataGrid.SelectedItemProperty,
			TreeViewItem.IsSelectedProperty, Popup.IsOpenProperty, ContextMenu.IsOpenProperty,
			ToolTip.IsOpenProperty, Calendar.SelectedDateProperty, Calendar.DisplayDateProperty,
			DatePicker.SelectedDateProperty,
		};
		var noJournalOwners = new[]
		{
			typeof(ListBox), typeof(ListBox), typeof(ComboBox), typeof(ComboBox), typeof(TabControl), typeof(DataGrid),
			typeof(TreeViewItem), typeof(Popup), typeof(ContextMenu), typeof(ToolTip), typeof(Calendar),
			typeof(Calendar), typeof(DatePicker),
		};
		for (var i = 0; i < noJournal.Length; i++)
			yield return new object[] { noJournalOwners[i].Name + "." + noJournal[i].Name, noJournal[i], noJournalOwners[i], false };
	}

	[Theory]
	[MemberData(nameof(MarkedDps))]
	public void Real_two_way_dps_are_marked_as_in_WPF(string name, DependencyProperty dp, Type owner, bool journal)
	{
		var meta = Assert.IsType<FrameworkPropertyMetadata>(dp.GetMetadata(owner));
		Assert.True(meta.BindsTwoWayByDefault, name);
		Assert.Equal(journal, meta.Journal);
	}

	[Fact]
	public void TreeView_SelectedItem_and_TreeViewItem_IsExpanded_are_not_two_way_by_default()
	{
		Assert.False(dpTwoWay(TreeView.SelectedItemProperty, typeof(TreeView)));
		Assert.False(dpTwoWay(TreeViewItem.IsExpandedProperty, typeof(TreeViewItem)));
		Assert.False(dpTwoWay(TextBlock.TextProperty, typeof(TextBlock)));

		static bool dpTwoWay(DependencyProperty dp, Type t)
			=> dp.GetMetadata(t) is FrameworkPropertyMetadata m && m.BindsTwoWayByDefault;
	}

	[Fact]
	public void TextBox_Text_metadata_has_LostFocus_default_trigger()
	{
		var meta = (FrameworkPropertyMetadata)WpfTextBox.TextProperty.GetMetadata(typeof(WpfTextBox));
		Assert.Equal(UpdateSourceTrigger.LostFocus, meta.DefaultUpdateSourceTrigger);
	}
}
