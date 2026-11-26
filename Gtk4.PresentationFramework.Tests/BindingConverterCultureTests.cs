using System;
using System.Globalization;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>ConverterCulture reaches converters and StringFormat; en-US is the default.</summary>
public class BindingConverterCultureTests : BindingWriteBackTestBase
{
	sealed class CultureConverter : IValueConverter, IMultiValueConverter
	{
		public CultureInfo? Forward;
		public CultureInfo? Back;

		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			Forward = culture;
			return value;
		}

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		{
			Back = culture;
			return int.Parse((string)value!);
		}

		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
		{
			Forward = culture;
			return "x";
		}

		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
		{
			Back = culture;
			return new object?[] { 4 };
		}
	}

	sealed class DecVm
	{
		public decimal Price { get; set; } = 1234.5m;
	}

	[Fact]
	public void Defaults_to_null_and_converter_sees_en_US() => Run(() =>
	{
		var conv = new CultureConverter();
		var b = new Binding("Age") { Converter = conv };
		Assert.Null(b.ConverterCulture);
		var tb = new WpfTextBox { DataContext = new Vm() };
		tb.SetBinding(WpfTextBox.TextProperty, b);
		Assert.Equal("en-US", conv.Forward!.Name);
	});

	[Fact]
	public void Set_culture_reaches_Convert_and_ConvertBack() => Run(() =>
	{
		var conv = new CultureConverter();
		var de = CultureInfo.GetCultureInfo("de-DE");
		var tb = new WpfTextBox { DataContext = new Vm() };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Age") { Converter = conv, ConverterCulture = de });
		tb.Text = "5";
		MoveFocusAway(tb);
		Assert.Same(de, conv.Forward);
		Assert.Same(de, conv.Back);
	});

	[Fact]
	public void ConvertBack_sees_en_US_when_unset() => Run(() =>
	{
		var conv = new CultureConverter();
		var tb = new WpfTextBox { DataContext = new Vm() };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Age") { Converter = conv });
		tb.Text = "5";
		MoveFocusAway(tb);
		Assert.Equal("en-US", conv.Back!.Name);
	});

	[Fact]
	public void MultiBinding_converter_gets_the_culture() => Run(() =>
	{
		var conv = new CultureConverter();
		var de = CultureInfo.GetCultureInfo("de-DE");
		var tb = new WpfTextBox { DataContext = new Vm() };
		var mb = new MultiBinding { Converter = conv, ConverterCulture = de, Mode = BindingMode.TwoWay };
		mb.Bindings.Add(new Binding("Age"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);
		Assert.Same(de, conv.Forward);
		tb.Text = "y";
		MoveFocusAway(tb);
		Assert.Same(de, conv.Back);
	});

	[Fact]
	public void MultiBinding_defaults_to_en_US() => Run(() =>
	{
		var conv = new CultureConverter();
		var tb = new WpfTextBox { DataContext = new Vm() };
		var mb = new MultiBinding { Converter = conv };
		mb.Bindings.Add(new Binding("Age"));
		tb.SetBinding(WpfTextBox.TextProperty, mb);
		Assert.Equal("en-US", conv.Forward!.Name);
	});

	[Fact]
	public void StringFormat_uses_the_effective_culture() => Run(() =>
	{
		var tb = new WpfTextBox { DataContext = new DecVm() };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Price")
		{
			StringFormat = "{0:N2}",
			ConverterCulture = CultureInfo.GetCultureInfo("de-DE")
		});
		Assert.Equal("1.234,50", tb.Text);

		var tb2 = new WpfTextBox { DataContext = new DecVm() };
		tb2.SetBinding(WpfTextBox.TextProperty, new Binding("Price") { StringFormat = "{0:N2}" });
		Assert.Equal("1,234.50", tb2.Text);
	});

	[Fact]
	public void ValidatesOnNotifyDataErrors_defaults_to_true()
	{
		Assert.True(new Binding().ValidatesOnNotifyDataErrors);
		Assert.True(new MultiBinding().ValidatesOnNotifyDataErrors);
	}
}
