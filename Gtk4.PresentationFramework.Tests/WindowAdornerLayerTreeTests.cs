using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;
using static Gtk4.PresentationFramework.Tests.WindowAdornerLayerTests;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Regression pins: the window's implicit adorner layer must not change the logical/visual tree.</summary>
public class WindowAdornerLayerTreeTests : WpfGtkTestBase
{
	[Fact]
	public void Layer_is_not_a_logical_or_visual_child_of_window() => Run(() =>
	{
		var (win, panel, _, _) = Build();
		_ = win.ImplicitAdornerLayer;
		var logical = LogicalTreeHelper.GetChildren(win).Cast<object>().ToList();
		Assert.Single(logical);
		Assert.Same(panel, logical[0]);
		Assert.Equal(1, VisualTreeHelper.GetChildrenCount(win));
		Assert.Same(panel, VisualTreeHelper.GetChild(win, 0));
	});

	[Fact]
	public void Content_parents_are_still_the_window() => Run(() =>
	{
		var (win, panel, _, _) = Build();
		Assert.Same(win, panel.LogicalParent);
		Assert.Same(win, panel.VisualParent);
	});

	[Fact]
	public void DataContext_still_inherits_from_window() => Run(() =>
	{
		var (win, _, target, _) = Build();
		win.DataContext = "ctx";
		Assert.Equal("ctx", target.DataContext);
	});

	[Fact]
	public void FindAncestor_window_binding_still_resolves() => Run(() =>
	{
		var (win, panel, _, _) = Build();
		win.Title = "bound-title";
		var probe = new TextBlock();
		panel.Children.Add(probe);
		probe.SetBinding(TextBlock.TextProperty, new Binding("Title")
		{
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(WpfWindow), 1)
		});
		Assert.Equal("bound-title", probe.Text);
	});

	[Fact]
	public void Loaded_fires_once_for_content() => Run(() =>
	{
		var (win, _, target, _) = Build();
		int loaded = 0;
		target.Loaded += (_, _) => loaded++;
		try
		{
			ShowAndLayout(win, target);
			Assert.Equal(1, loaded);
		}
		finally { win.Close(); }
	});
}
