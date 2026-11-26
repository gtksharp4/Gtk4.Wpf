using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class AppearanceInheritanceTests
{
	static (Window win, StackPanel panel, TextBlock tb, Button btn) Tree()
	{
		var win = new Window();
		var panel = new StackPanel();
		var tb = new TextBlock();
		var btn = new Button();
		panel.Children.Add(tb);
		panel.Children.Add(btn);
		win.Content = panel;
		return (win, panel, tb, btn);
	}

	[Fact]
	public void Window_font_size_and_foreground_flow_to_descendants()
	{
		System.Windows.Themes.Generic.Reset();
		var (win, _, tb, btn) = Tree();
		win.FontSize = 16;
		var red = Brushes.Red;
		win.Foreground = red;
		Assert.Equal(16.0, tb.FontSize);
		Assert.Equal(16.0, btn.FontSize);
		Assert.Same(red, tb.Foreground);
		// WPF themes colour Button text with ControlTextBrush: Window.Foreground reaches TextBlocks only.
		Assert.Same(SystemColors.ControlTextBrush, btn.Foreground);
	}

	[Fact]
	public void Value_set_before_attach_flows_on_attach()
	{
		var win = new Window { FontWeight = FontWeights.Bold };
		var tb = new TextBlock();
		win.Content = new StackPanel { Children = { tb } };
		Assert.Equal(FontWeights.Bold, tb.FontWeight);
	}

	[Fact]
	public void Local_value_on_child_wins()
	{
		var (win, _, tb, btn) = Tree();
		win.FontSize = 16;
		tb.FontSize = 30;
		Assert.Equal(30.0, tb.FontSize);
		Assert.Equal(16.0, btn.FontSize);
		win.FontSize = 18;
		Assert.Equal(30.0, tb.FontSize);
	}

	[Fact]
	public void Clearing_parent_value_reverts_child()
	{
		var (win, _, tb, btn) = Tree();
		win.FontSize = 16;
		win.ClearValue(Control.FontSizeProperty);
		Assert.Equal(12.0, tb.FontSize);
		Assert.Equal(12.0, btn.FontSize);
	}

	[Fact]
	public void Background_does_not_inherit()
	{
		var (win, _, tb, btn) = Tree();
		win.Background = Brushes.Yellow;
		Assert.Null(tb.Background);
		Assert.Null(btn.Background);
	}

	[Fact]
	public void Intermediate_panel_does_not_block_inheritance_and_font_family_flows()
	{
		var (win, _, tb, _) = Tree();
		win.FontFamily = new FontFamily("Arial");
		Assert.Equal("Arial", tb.FontFamily.Source);
	}
}
