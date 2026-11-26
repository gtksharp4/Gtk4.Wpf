using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Bindings never throw for a value the target DP rejects: they fall back to FallbackValue or the default.</summary>
public class BindingTargetSafetyTests : BindingTargetSafetyTestBase
{
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
