using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Xunit;
using System.Windows.Controls;
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

	sealed class FuncConverter : IValueConverter
	{
		public Func<object?, object?> Func = v => v;
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => Func(value);
		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value;
	}

	sealed class FuncMultiConverter : IMultiValueConverter
	{
		public Func<object?[], object?> Func = v => v[0];
		public object? Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
			=> Func(values);
		public object?[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
			=> null;
	}

	sealed class IntVm
	{
		public int W { get; set; } = 40;
	}

	static FuncConverter Unset() => new FuncConverter { Func = _ => DependencyProperty.UnsetValue };

	[Fact]
	public void Converter_returning_UnsetValue_on_Width_uses_the_default() => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm { Size = 5 } };
		tb.SetBinding(FrameworkElement.WidthProperty, new Binding("Size") { Converter = Unset() });

		Assert.True(double.IsNaN(tb.Width));
	});

	[Theory]
	[InlineData(30.0)]
	[InlineData("30")]
	public void Converter_returning_UnsetValue_uses_the_FallbackValue(object fallback) => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm { Size = 5 } };
		tb.SetBinding(FrameworkElement.WidthProperty,
			new Binding("Size") { Converter = Unset(), FallbackValue = fallback });

		Assert.Equal(30, tb.Width);
	});

	[Fact]
	public void Converter_returning_DoNothing_leaves_a_typed_target_unchanged() => Run(() =>
	{
		var vm = new NullableVm { Size = 50 };
		var tb = new WpfTextBlock { DataContext = vm };
		var c = new FuncConverter { Func = v => v is double d && d < 0 ? Binding.DoNothing : v };
		tb.SetBinding(FrameworkElement.WidthProperty, new Binding("Size") { Converter = c });
		Assert.Equal(50, tb.Width);

		vm.Size = -1;

		Assert.Equal(50, tb.Width);
	});

	[Fact]
	public void DoNothing_on_an_object_DP_is_not_stored() => Run(() =>
	{
		var vm = new NullableVm { Text = "a" };
		var cc = new ContentControl { DataContext = vm };
		var c = new FuncConverter { Func = v => v == null ? Binding.DoNothing : v };
		cc.SetBinding(ContentControl.ContentProperty, new Binding("Text") { Converter = c });
		Assert.Equal("a", cc.Content);

		vm.Text = null;

		Assert.Equal("a", cc.Content);
	});

	[Fact]
	public void UnsetValue_is_not_string_formatted() => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm { Text = "a" } };
		var def = WpfTextBlock.TextProperty.GetMetadata(typeof(WpfTextBlock)).DefaultValue;
		tb.SetBinding(WpfTextBlock.TextProperty, new Binding("Text") { Converter = Unset(), StringFormat = "x{0}" });

		Assert.Equal(def, tb.GetValue(WpfTextBlock.TextProperty));
	});

	static MultiBinding Multi(NullableVm vm, Func<object?[], object?> func, object? fallback = null)
	{
		var mb = new MultiBinding { Converter = new FuncMultiConverter { Func = func } };
		mb.Bindings.Add(new Binding("Size") { Source = vm });
		if (fallback != null) mb.FallbackValue = fallback;
		return mb;
	}

	[Fact]
	public void MultiBinding_UnsetValue_gives_the_default_or_FallbackValue() => Run(() =>
	{
		var vm = new NullableVm { Size = 1 };
		var a = new WpfTextBlock();
		a.SetBinding(FrameworkElement.WidthProperty, Multi(vm, _ => DependencyProperty.UnsetValue));
		var b = new WpfTextBlock();
		b.SetBinding(FrameworkElement.WidthProperty, Multi(vm, _ => DependencyProperty.UnsetValue, 25.0));

		Assert.True(double.IsNaN(a.Width));
		Assert.Equal(25, b.Width);
	});

	[Fact]
	public void MultiBinding_DoNothing_leaves_the_target_unchanged() => Run(() =>
	{
		var vm = new NullableVm { Size = 40 };
		var tb = new WpfTextBlock();
		tb.SetBinding(FrameworkElement.WidthProperty,
			Multi(vm, v => v[0] is double d && d < 0 ? Binding.DoNothing : v[0]));
		Assert.Equal(40, tb.Width);

		vm.Size = -1;

		Assert.Equal(40, tb.Width);
	});

	[Fact]
	public void PriorityBinding_int_source_on_Width_is_converted() => Run(() =>
	{
		var tb = new WpfTextBlock();
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("W") { Source = new IntVm() });

		tb.SetBinding(FrameworkElement.WidthProperty, pb);

		Assert.Equal(40.0, tb.Width);
	});

	[Fact]
	public void PriorityBinding_string_FallbackValue_on_Width_is_converted() => Run(() =>
	{
		var tb = new WpfTextBlock();
		var pb = new PriorityBinding { FallbackValue = "20" };
		pb.Bindings.Add(new Binding("Missing") { Source = new object() });

		tb.SetBinding(FrameworkElement.WidthProperty, pb);

		Assert.Equal(20.0, tb.Width);
	});

	[Fact]
	public void PriorityBinding_unconvertible_value_uses_fallback() => Run(() =>
	{
		var tb = new WpfTextBlock { DataContext = new NullableVm { Text = "abc" } };
		var pb = new PriorityBinding { FallbackValue = 15.0 };
		pb.Bindings.Add(new Binding("Text"));

		tb.SetBinding(FrameworkElement.WidthProperty, pb);

		Assert.Equal(15, tb.Width);
	});
}
