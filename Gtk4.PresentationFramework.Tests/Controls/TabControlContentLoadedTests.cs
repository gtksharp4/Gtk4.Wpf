using System.Collections.Generic;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// WPF hosts only the selected tab's content, so an unselected tab's content is not loaded (a Loaded-started
/// animation on a hidden tab does not run); switching tabs unloads the old content and loads the new.
/// </summary>
public class TabControlContentLoadedTests : WpfGtkTestBase
{
	static (WpfWindow Window, TabControl Tabs, Border First, Border Second, TextBlock Header, List<string> Log) Open()
	{
		var log = new List<string>();
		var first = new Border { Name = "first" };
		var second = new Border { Name = "second" };
		var header = new TextBlock { Text = "Two" };
		foreach (var b in new[] { first, second })
		{
			b.Loaded += (s, _) => log.Add("Loaded:" + ((Border)s!).Name);
			b.Unloaded += (s, _) => log.Add("Unloaded:" + ((Border)s!).Name);
		}
		var tabs = new TabControl();
		tabs.Items.Add(new TabItem { Header = "One", Content = first });
		tabs.Items.Add(new TabItem { Header = header, Content = second });
		var window = new WpfWindow { Width = 300, Height = 200, Content = tabs };
		window.Show();
		Assert.True(GtkPump.Until(() => tabs.IsLoaded && first.IsLoaded), "the first tab never loaded");
		GtkPump.For(100);
		return (window, tabs, first, second, header, log);
	}

	[Fact]
	public void Only_the_selected_tabs_content_is_loaded() => Run(() =>
	{
		var (window, _, first, second, header, log) = Open();
		try
		{
			Assert.True(first.IsLoaded);
			Assert.False(second.IsLoaded);
			Assert.True(header.IsLoaded, "a tab's header is in the tab strip, so it is loaded");
			Assert.Equal(new[] { "Loaded:first" }, log);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Switching_tabs_unloads_the_old_content_and_loads_the_new() => Run(() =>
	{
		var (window, tabs, first, second, _, log) = Open();
		try
		{
			log.Clear();
			tabs.SelectedIndex = 1;
			Assert.True(GtkPump.Until(() => second.IsLoaded && !first.IsLoaded), string.Join(",", log));
			Assert.Equal(new[] { "Unloaded:first", "Loaded:second" }, log);

			log.Clear();
			tabs.SelectedIndex = 0;
			Assert.True(GtkPump.Until(() => first.IsLoaded && !second.IsLoaded), string.Join(",", log));
			Assert.Equal(new[] { "Unloaded:second", "Loaded:first" }, log);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Closing_the_window_unloads_only_the_loaded_content() => Run(() =>
	{
		var (window, _, first, second, _, log) = Open();
		log.Clear();
		window.Close();
		Assert.True(GtkPump.Until(() => !first.IsLoaded), "the selected content stayed loaded");
		Assert.Equal(new[] { "Unloaded:first" }, log);
		Assert.False(second.IsLoaded);
	});
}
