using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>UIElement.ClipToBounds on real GTK widgets.</summary>
public class ClipToBoundsTests : WpfGtkTestBase
{
	[Fact]
	public void ClipToBounds_defaults_to_false() =>
		Assert.False(new Button().ClipToBounds);

	[Fact]
	public void ClipToBounds_maps_to_widget_overflow() => Run(() =>
	{
		var b = new StackPanel();
		Assert.Equal(Gtk.Overflow.Visible, b.Widget.Overflow);
		b.ClipToBounds = true;
		Assert.Equal(Gtk.Overflow.Hidden, b.Widget.Overflow);
		b.ClipToBounds = false;
		Assert.Equal(Gtk.Overflow.Visible, b.Widget.Overflow);
	});

	[Fact]
	public void ClipToBounds_set_before_realize_applies_on_creation() => Run(() =>
	{
		var b = new StackPanel { ClipToBounds = true };
		Assert.Equal(Gtk.Overflow.Hidden, b.Widget.Overflow);
	});

	[Fact]
	public void ClipToBounds_false_restores_the_widget_default_overflow() => Run(() =>
	{
		var border = new Border();
		var original = border.Widget.Overflow;
		border.ClipToBounds = true;
		Assert.Equal(Gtk.Overflow.Hidden, border.Widget.Overflow);
		border.ClipToBounds = false;
		Assert.Equal(original, border.Widget.Overflow);
	});

	[Fact]
	public void ClipToBounds_never_set_leaves_the_widget_default_overflow() => Run(() =>
	{
		var border = new Border();
		var original = border.Widget.Overflow;
		border.ClipToBounds = false;
		Assert.Equal(original, border.Widget.Overflow);
	});
}
