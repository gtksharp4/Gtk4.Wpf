using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>A Window's own Visibility is its show state, never content suppression of its widget.</summary>
public class WindowShowStateGtkTests : WpfGtkTestBase
{
	static Gtk.Window Native(Window w) => (Gtk.Window)w.Widget;

	[Fact]
	public void Widget_touched_before_Show_stays_focusable_and_targetable() => Run(() =>
	{
		var w = new Window { Content = new TextBox() };
		_ = w.Widget;
		w.Show();
		Assert.True(Native(w).CanFocus);
		Assert.True(Native(w).CanTarget);
		w.Close();
	});

	[Fact]
	public void Opacity_set_while_hidden_is_applied_on_Show() => Run(() =>
	{
		var w = new Window();
		w.Show();
		w.Hide();
		w.Opacity = 0.8;
		w.Show();
		Assert.Equal(0.8, Native(w).Opacity, 2);
		w.Close();
	});

	[Fact]
	public void Opacity_set_before_the_widget_exists_is_applied_on_creation() => Run(() =>
	{
		var w = new Window { Opacity = 0.5 };
		_ = w.Widget;
		Assert.Equal(0.5, Native(w).Opacity, 2);
		w.Close();
	});

	[Fact]
	public void HitTest_and_Focusable_toggled_before_Show_end_true() => Run(() =>
	{
		var w = new Window();
		w.IsHitTestVisible = false;
		w.IsHitTestVisible = true;
		w.Focusable = false;
		w.Focusable = true;
		w.Show();
		Assert.True(Native(w).CanTarget);
		Assert.True(Native(w).CanFocus);
		w.Close();
	});

	[Fact]
	public void Show_ShowDialog_and_Visibility_Visible_throw_after_Close() => Run(() =>
	{
		var w = new Window();
		w.Show();
		w.Close();
		Assert.Throws<InvalidOperationException>(() => w.Show());
		Assert.Throws<InvalidOperationException>(() => w.ShowDialog());
		Assert.Throws<InvalidOperationException>(() => w.Visibility = Visibility.Visible);
		Assert.False(w.IsVisible);
	});
}
