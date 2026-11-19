using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Bindings never throw for a value the target DP rejects: they fall back to FallbackValue or the default.</summary>
public class BindingTargetSafetyTests : WpfGtkTestBase
{
	sealed class NullableVm : INotifyPropertyChanged
	{
		double? _size;
		string? _text;

		public double? Size
		{
			get => _size;
			set { _size = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Size))); }
		}

		public string? Text
		{
			get => _text;
			set { _text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class JoinConverter : IMultiValueConverter
	{
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture) => null;
		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => null;
	}

	static MultiBinding NoSourceMultiBinding()
	{
		var mb = new MultiBinding { Converter = new JoinConverter() };
		mb.Bindings.Add(new Binding("Missing"));
		return mb;
	}

	[Fact]
	public void MultiBinding_on_IsEnabled_without_DataContext_keeps_the_default() => Run(() =>
	{
		var tb = new WpfTextBlock();

		tb.SetBinding(UIElement.IsEnabledProperty, NoSourceMultiBinding());

		Assert.True(tb.IsEnabled);
	});

	[Fact]
	public void MultiBinding_on_Width_without_DataContext_keeps_NaN() => Run(() =>
	{
		var tb = new WpfTextBlock();

		tb.SetBinding(FrameworkElement.WidthProperty, NoSourceMultiBinding());

		Assert.True(double.IsNaN(tb.Width));
	});

	[Fact]
	public void Null_nullable_source_into_Width_gives_the_default_then_follows_the_source() => Run(() =>
	{
		var vm = new NullableVm();
		var tb = new WpfTextBlock { DataContext = vm };

		tb.SetBinding(FrameworkElement.WidthProperty, new Binding("Size"));
		Assert.True(double.IsNaN(tb.Width));

		vm.Size = 50;
		Assert.Equal(50, tb.Width);
	});

	[Fact]
	public void Null_nullable_source_into_FontSize_uses_FallbackValue() => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm() };

		tb.SetBinding(WpfTextBlock.FontSizeProperty, new Binding("Size") { FallbackValue = 20.0 });

		Assert.Equal(20, tb.FontSize);
	});

	[Fact]
	public void Failed_conversion_reverts_to_the_default_instead_of_keeping_a_stale_value() => Run(() =>
	{
		var vm = new NullableVm { Text = "50" };
		var tb = new WpfTextBlock { DataContext = vm };
		tb.SetBinding(FrameworkElement.WidthProperty, new Binding("Text"));
		Assert.Equal(50, tb.Width);

		vm.Text = "abc";

		Assert.True(double.IsNaN(tb.Width));
	});

	[Fact]
	public void Unconvertible_FallbackValue_does_not_throw_and_falls_to_the_default() => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm { Text = "abc" } };

		tb.SetBinding(FrameworkElement.WidthProperty, new Binding("Text") { FallbackValue = "also bad" });

		Assert.True(double.IsNaN(tb.Width));
	});
}
