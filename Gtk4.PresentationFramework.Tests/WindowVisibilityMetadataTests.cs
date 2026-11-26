using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Merged metadata chains the base callback once; closed windows reject Visible before storing it.</summary>
public class WindowVisibilityMetadataTests : WpfGtkTestBase
{
	sealed class CountingWindow : Window
	{
		public int VisibilityCallbacks;

		internal override void OnVisibilityChangedCore(Visibility visibility)
		{
			VisibilityCallbacks++;
			base.OnVisibilityChangedCore(visibility);
		}
	}

	[Fact]
	public void Window_Visibility_metadata_chains_one_callback() =>
		Assert.Single(UIElement.VisibilityProperty.GetMetadata(typeof(Window)).PropertyChangedCallback!.GetInvocationList());

	[Fact]
	public void AddOwner_appearance_metadata_chains_one_callback()
	{
		Assert.Single(Control.BackgroundProperty.GetMetadata(typeof(Control)).PropertyChangedCallback!.GetInvocationList());
		Assert.Single(Border.BackgroundProperty.GetMetadata(typeof(Border)).PropertyChangedCallback!.GetInvocationList());
		Assert.Single(WpfTextBlock.BackgroundProperty.GetMetadata(typeof(WpfTextBlock)).PropertyChangedCallback!.GetInvocationList());
		Assert.Single(Control.BorderBrushProperty.GetMetadata(typeof(Control)).PropertyChangedCallback!.GetInvocationList());
		Assert.Single(Control.BorderThicknessProperty.GetMetadata(typeof(Control)).PropertyChangedCallback!.GetInvocationList());
	}

	[Fact]
	public void Show_runs_the_visibility_change_once() => Run(() =>
	{
		var w = new CountingWindow();

		w.Show();

		Assert.Equal(1, w.VisibilityCallbacks);
		w.Hide();
		Assert.Equal(2, w.VisibilityCallbacks);
	});

	[Fact]
	public void Setting_Visible_after_close_throws_without_storing_the_value() => Run(() =>
	{
		var w = new Window();
		w.Show();
		w.Close();

		Assert.Throws<InvalidOperationException>(() => w.Visibility = Visibility.Visible);
		Assert.Equal(Visibility.Hidden, w.Visibility);
		Assert.Throws<InvalidOperationException>(() => w.Visibility = Visibility.Visible);
		Assert.Equal(Visibility.Hidden, w.Visibility);
	});

	[Fact]
	public void Button_like_controls_use_theme_text_colours_not_the_window_foreground() => Run(() =>
	{
		System.Windows.Themes.Generic.Reset();
		var win = new Window { Foreground = Brushes.Red };
		var panel = new StackPanel();
		var toggle = new ToggleButton();
		var repeat = new RepeatButton();
		var password = new PasswordBox();
		panel.Children.Add(toggle);
		panel.Children.Add(repeat);
		panel.Children.Add(password);
		win.Content = panel;

		Assert.Same(SystemColors.ControlTextBrush, toggle.Foreground);
		Assert.Same(SystemColors.ControlTextBrush, repeat.Foreground);
		Assert.Same(SystemColors.WindowTextBrush, password.Foreground);

		var blue = new SolidColorBrush(Colors.Blue);
		toggle.Foreground = blue;
		Assert.Same(blue, toggle.Foreground);
	});
}
