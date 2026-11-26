using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

/// <summary>IsVisible rooted at a shown Window; Window Show/Hide/Visibility.</summary>
public class IsVisibleTests : WpfGtkTestBase
{
	[Fact]
	public void Detached_element_is_not_visible()
	{
		Assert.False(new VisElement().IsVisible);
		Assert.False(new WpfButton().IsVisible);
	}

	[Fact]
	public void Content_of_an_unshown_window_is_not_visible() => Run(() =>
	{
		var btn = new WpfButton();
		var win = new WpfWindow { Content = btn };
		Assert.False(win.IsVisible);
		Assert.False(btn.IsVisible);
	});

	[Fact]
	public void Content_of_a_shown_window_is_visible() => Run(() =>
	{
		var btn = new WpfButton();
		var win = new WpfWindow { Content = btn };
		win.Show();
		try
		{
			Assert.True(win.IsVisible);
			Assert.True(btn.IsVisible);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Content_attached_after_Show_becomes_visible() => Run(() =>
	{
		var win = new WpfWindow();
		win.Show();
		try
		{
			var btn = new WpfButton();
			win.Content = btn;
			Assert.True(btn.IsVisible);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Hide_makes_content_invisible_and_raises_IsVisibleChanged_parent_first() => Run(() =>
	{
		var btn = new WpfButton();
		var win = new WpfWindow { Content = btn };
		win.Show();
		var log = new List<string>();
		win.IsVisibleChanged += (s, e) => log.Add("win:" + e.NewValue);
		btn.IsVisibleChanged += (s, e) => log.Add("btn:" + e.NewValue);
		try
		{
			win.Hide();
			Assert.False(win.IsVisible);
			Assert.False(btn.IsVisible);
			Assert.Equal(new[] { "win:False", "btn:False" }, log);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Collapsed_and_Hidden_subtrees_are_not_visible() => Run(() =>
	{
		var inner = new WpfButton();
		var panel = new StackPanel();
		panel.Children.Add(inner);
		var win = new WpfWindow { Content = panel };
		win.Show();
		try
		{
			Assert.True(inner.IsVisible);
			panel.Visibility = Visibility.Collapsed;
			Assert.False(panel.IsVisible);
			Assert.False(inner.IsVisible);
			panel.Visibility = Visibility.Hidden;
			Assert.False(inner.IsVisible);
			panel.Visibility = Visibility.Visible;
			Assert.True(inner.IsVisible);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Window_Visibility_Visible_shows_and_Hide_sets_Hidden() => Run(() =>
	{
		var win = new WpfWindow();
		Assert.Equal(Visibility.Collapsed, win.Visibility);
		win.Visibility = Visibility.Visible;
		try
		{
			Assert.True(win.IsVisible);
			win.Hide();
			Assert.Equal(Visibility.Hidden, win.Visibility);
			Assert.False(win.IsVisible);
			win.Show();
			Assert.Equal(Visibility.Visible, win.Visibility);
			Assert.True(win.IsVisible);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Window_Collapsed_hides_without_closing() => Run(() =>
	{
		var win = new WpfWindow();
		win.Show();
		bool closed = false;
		win.Closed += (s, e) => closed = true;
		win.Visibility = Visibility.Collapsed;
		Assert.False(win.IsVisible);
		Assert.False(closed);
		win.Close();
	});
}
