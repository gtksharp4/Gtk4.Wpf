using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>Visibility / IsEnabled / IsHitTestVisible on real GTK widgets, and the input gates.</summary>
public class VisibilityGtkTests : WpfGtkTestBase
{
	public VisibilityGtkTests() => Run(() =>
	{
		Mouse.Capture(null);
		(Keyboard.FocusedElement as UIElement)?.SetFocusedInternal(false);
	});

	[Fact]
	public void Collapsed_sets_widget_Visible_false_and_Visible_restores_it() => Run(() =>
	{
		var btn = new WpfButton();
		var w = btn.Widget;
		btn.Visibility = Visibility.Collapsed;
		Assert.False(w.Visible);
		btn.Visibility = Visibility.Visible;
		Assert.True(w.Visible);
	});

	[Fact]
	public void Collapsed_before_widget_creation_is_applied_on_creation() => Run(() =>
	{
		var btn = new WpfButton { Visibility = Visibility.Collapsed };
		Assert.False(btn.Widget.Visible);
	});

	[Fact]
	public void Hidden_zeroes_opacity_and_blocks_targeting_but_keeps_the_allocation() => Run(() =>
	{
		var btn = new WpfButton { Content = "x" };
		var win = new WpfWindow { Content = btn };
		win.Show();
		try
		{
			btn.Visibility = Visibility.Hidden;
			Assert.True(btn.Widget.Visible);
			Assert.Equal(0.0, btn.Widget.Opacity);
			Assert.False(btn.Widget.CanTarget);
			Assert.False(btn.Widget.CanFocus);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Hidden_widget_keeps_a_non_zero_size_request() => Run(() =>
	{
		var btn = new WpfButton { Content = "wide label", Visibility = Visibility.Hidden };
		btn.Widget.Measure(Gtk.Orientation.Horizontal, -1, out var min, out var nat, out _, out _);
		Assert.True(nat > 0);
	});

	[Fact]
	public void Opacity_set_while_Hidden_stays_zero_and_is_restored_on_Visible() => Run(() =>
	{
		var btn = new WpfButton { Visibility = Visibility.Hidden };
		var w = btn.Widget;
		btn.Opacity = 0.5;
		Assert.Equal(0.0, w.Opacity);
		btn.Visibility = Visibility.Visible;
		Assert.Equal(0.5, w.Opacity, 2);
		Assert.True(w.CanTarget);
	});

	[Fact]
	public void IsEnabled_false_sets_Sensitive_false_on_the_widget() => Run(() =>
	{
		var btn = new WpfButton();
		var w = btn.Widget;
		btn.IsEnabled = false;
		Assert.False(w.Sensitive);
		btn.IsEnabled = true;
		Assert.True(w.Sensitive);
	});

	[Fact]
	public void Button_CanExecute_false_makes_the_widget_insensitive() => Run(() =>
	{
		var cmd = new SwitchCommand { Can = false };
		var btn = new WpfButton { Command = cmd };
		Assert.False(btn.Widget.Sensitive);
		cmd.Can = true;
		cmd.Raise();
		Assert.True(btn.Widget.Sensitive);
	});

	[Fact]
	public void IsHitTestVisible_false_sets_CanTarget_false() => Run(() =>
	{
		var btn = new WpfButton();
		var w = btn.Widget;
		btn.IsHitTestVisible = false;
		Assert.False(w.CanTarget);
		btn.IsHitTestVisible = true;
		Assert.True(w.CanTarget);
	});

	[Fact]
	public void Disabling_the_captor_releases_capture_and_raises_LostMouseCapture() => Run(() =>
	{
		var btn = new WpfButton();
		bool lost = false;
		btn.LostMouseCapture += (s, e) => lost = true;
		Assert.True(btn.CaptureMouse());
		btn.IsEnabled = false;
		Assert.Null(Mouse.Captured);
		Assert.False(btn.IsMouseCaptured);
		Assert.True(lost);
	});

	[Fact]
	public void Collapsing_an_ancestor_of_the_captor_releases_capture() => Run(() =>
	{
		var btn = new WpfButton();
		var panel = new StackPanel();
		panel.Children.Add(btn);
		Assert.True(btn.CaptureMouse());
		panel.Visibility = Visibility.Collapsed;
		Assert.Null(Mouse.Captured);
	});

	[Fact]
	public void Focus_returns_false_when_disabled_or_collapsed() => Run(() =>
	{
		var btn = new WpfButton { IsEnabled = false };
		Assert.False(btn.Focus());
		btn.IsEnabled = true;
		btn.Visibility = Visibility.Collapsed;
		Assert.False(btn.Focus());
		btn.Visibility = Visibility.Hidden;
		Assert.False(btn.Focus());
		btn.Visibility = Visibility.Visible;
		Assert.True(btn.Focus());
	});

	[Fact]
	public void Focus_returns_false_under_a_collapsed_ancestor() => Run(() =>
	{
		var btn = new WpfButton();
		var panel = new StackPanel { Visibility = Visibility.Collapsed };
		panel.Children.Add(btn);
		Assert.False(btn.Focus());
	});

	[Fact]
	public void Focused_element_that_becomes_disabled_or_collapsed_drops_focus() => Run(() =>
	{
		var a = new WpfButton();
		Assert.True(a.Focus());
		a.IsEnabled = false;
		Assert.False(a.IsFocused);

		var b = new WpfButton();
		Assert.True(b.Focus());
		b.Visibility = Visibility.Collapsed;
		Assert.False(b.IsFocused);
		Assert.Null(Keyboard.FocusedElement);
	});
}
