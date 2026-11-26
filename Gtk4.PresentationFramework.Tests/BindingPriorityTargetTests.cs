using System.Windows;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>PriorityBinding converts its value or FallbackValue to the target type.</summary>
public class BindingPriorityTargetTests : BindingTargetSafetyTestBase
{
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
