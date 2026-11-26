using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.FlowDirectionTests;

public class FlowDirectionGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Realized_widget_follows_FlowDirection() => Run(() =>
	{
		var b = new Button();
		var w = b.Widget;
		b.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(Gtk.TextDirection.Rtl, w.Direction);
		b.FlowDirection = FlowDirection.LeftToRight;
		Assert.Equal(Gtk.TextDirection.Ltr, w.Direction);
	});

	[Fact]
	public void Value_set_before_realization_is_applied_on_create() => Run(() =>
	{
		var tb = new TextBlock { Text = "x", FlowDirection = FlowDirection.RightToLeft };
		Assert.Equal(Gtk.TextDirection.Rtl, tb.Widget.Direction);
	});

	[Fact]
	public void Inherited_value_reaches_realized_descendants_and_late_children() => Run(() =>
	{
		var panel = new StackPanel();
		var early = new Button();
		panel.Children.Add(early);
		_ = panel.Widget;
		_ = early.Widget;
		panel.FlowDirection = FlowDirection.RightToLeft;
		Assert.Equal(Gtk.TextDirection.Rtl, early.Widget.Direction);
		var late = new Button();
		panel.Children.Add(late);
		Assert.Equal(Gtk.TextDirection.Rtl, late.Widget.Direction);
		early.FlowDirection = FlowDirection.LeftToRight;
		Assert.Equal(Gtk.TextDirection.Ltr, early.Widget.Direction);
	});

	[Fact]
	public void Window_direction_applies_to_shown_window() => Run(() =>
	{
		var text = new TextBlock { Text = "a" };
		var win = new Window { FlowDirection = FlowDirection.RightToLeft, Content = text };
		win.Show();
		try
		{
			Assert.Equal(Gtk.TextDirection.Rtl, win.Widget.Direction);
			Assert.Equal(Gtk.TextDirection.Rtl, text.Widget.Direction);
		}
		finally { win.Close(); }
	});
}
