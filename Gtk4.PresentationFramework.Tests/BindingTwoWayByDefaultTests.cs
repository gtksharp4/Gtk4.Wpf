using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Behaviour of Binding Mode=Default on two-way-by-default targets, and DP validators.</summary>
public class BindingTwoWayByDefaultTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		bool _flag;
		double _number;
		object? _selected;
		string _name = "n";

		public bool Flag { get => _flag; set { _flag = value; Raise(nameof(Flag)); } }
		public double Number { get => _number; set { _number = value; Raise(nameof(Number)); } }
		public object? Selected { get => _selected; set { _selected = value; Raise(nameof(Selected)); } }
		public string Name { get => _name; set { _name = value; Raise(nameof(Name)); } }

		public event PropertyChangedEventHandler? PropertyChanged;

		void Raise(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
	}

	sealed class PassThrough : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) => values[0];

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
			=> new[] { value };
	}

	[Fact]
	public void CheckBox_IsChecked_default_mode_writes_back() => Run(() =>
	{
		var vm = new Vm();
		var cb = new CheckBox { DataContext = vm };
		cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("Flag"));
		cb.IsChecked = true;
		Assert.True(vm.Flag);
	});

	[Fact]
	public void Slider_Value_default_mode_writes_back() => Run(() =>
	{
		var vm = new Vm();
		var s = new Slider { DataContext = vm, Maximum = 100 };
		s.SetBinding(Slider.ValueProperty, new Binding("Number"));
		s.Value = 42;
		Assert.Equal(42, vm.Number);
	});

	[Fact]
	public void ListBox_SelectedItem_default_mode_writes_back() => Run(() =>
	{
		var vm = new Vm();
		var lb = new ListBox { DataContext = vm, ItemsSource = new[] { "a", "b" } };
		lb.SetBinding(ListBox.SelectedItemProperty, new Binding("Selected"));
		lb.SelectedItem = "b";
		Assert.Equal("b", vm.Selected);
	});

	[Fact]
	public void Source_changes_still_flow_to_a_two_way_default_target() => Run(() =>
	{
		var vm = new Vm();
		var cb = new CheckBox { DataContext = vm };
		cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("Flag"));
		vm.Flag = true;
		Assert.Equal(true, cb.IsChecked);
	});

	[Fact]
	public void Explicit_OneWay_on_TextBox_Text_does_not_write_back() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name") { Mode = BindingMode.OneWay });
		tb.Text = "typed";
		Assert.Equal("n", vm.Name);
	});

	[Fact]
	public void TextBlock_Text_default_mode_stays_one_way() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		tb.SetBinding(TextBlock.TextProperty, new Binding("Name"));
		tb.Text = "typed";
		Assert.Equal("n", vm.Name);
	});

	[Fact]
	public void MultiBinding_default_mode_resolves_to_two_way_on_two_way_default_target() => Run(() =>
	{
		var vm = new Vm();
		var cb = new CheckBox { DataContext = vm };
		var mb = new MultiBinding { Converter = new PassThrough() };
		mb.Bindings.Add(new Binding("Flag"));
		cb.SetBinding(CheckBox.IsCheckedProperty, mb);
		cb.IsChecked = true;
		Assert.True(vm.Flag);
	});

	[Fact]
	public void MultiBinding_default_mode_stays_one_way_on_plain_target() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		var mb = new MultiBinding { Converter = new PassThrough() };
		mb.Bindings.Add(new Binding("Name"));
		tb.SetBinding(TextBlock.TextProperty, mb);
		tb.Text = "typed";
		Assert.Equal("n", vm.Name);
	});

	[Fact]
	public void Visibility_rejects_undefined_enum_values() => Run(() =>
	{
		var tb = new TextBlock();
		Assert.Throws<ArgumentException>(() => tb.Visibility = (Visibility)42);
		Assert.Equal(Visibility.Visible, tb.Visibility);
	});

	[Theory]
	[InlineData(0.0)]
	[InlineData(-1.0)]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(40000.0)]
	public void FontSize_rejects_invalid_values(double value) => Run(() =>
	{
		var tb = new TextBlock();
		Assert.Throws<ArgumentException>(() => tb.FontSize = value);
	});

	[Fact]
	public void FontSize_accepts_valid_values() => Run(() =>
	{
		var tb = new TextBlock { FontSize = 20 };
		Assert.Equal(20, tb.FontSize);
	});
}
