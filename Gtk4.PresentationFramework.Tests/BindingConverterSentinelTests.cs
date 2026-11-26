using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Converters and MultiBindings returning UnsetValue or DoNothing leave the target at its default or unchanged.</summary>
public class BindingConverterSentinelTests : BindingTargetSafetyTestBase
{
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
}
