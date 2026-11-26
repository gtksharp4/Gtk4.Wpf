using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

/// <summary>FrameworkElement.LogicalChildren is null by default (as in WPF); consumers treat null as empty.</summary>
public class LogicalChildrenNullTests : WpfGtkTestBase
{
	class Bare : FrameworkElement { }

	class ReturnsNull : FrameworkElement
	{
		protected internal override IEnumerator? LogicalChildren => null;
	}

	class Forwards : FrameworkElement
	{
		protected internal override IEnumerator? LogicalChildren => base.LogicalChildren;
	}

	static bool LoadedRaises(FrameworkElement fe)
	{
		bool loaded = false;
		fe.Loaded += (s, e) => loaded = true;
		LifecycleBroadcaster.BroadcastLoaded(fe);
		return loaded;
	}

	[Fact]
	public void The_default_is_null_like_WPF() => Run(() => Assert.Null(new FrameworkElementProbe().Probe()));

	class FrameworkElementProbe : FrameworkElement
	{
		public IEnumerator? Probe() => LogicalChildren;
	}

	[Fact]
	public void A_subclass_without_an_override_enumerates_as_empty() => Run(() =>
		Assert.Empty(LogicalTreeHelper.GetChildren(new Bare()).Cast<object>()));

	[Fact]
	public void An_override_returning_null_or_forwarding_base_does_not_throw_and_still_raises_Loaded() => Run(() =>
	{
		foreach (FrameworkElement fe in new FrameworkElement[] { new ReturnsNull(), new Forwards(), new Bare() })
		{
			Assert.Empty(LogicalTreeHelper.GetChildren(fe).Cast<object>());
			Assert.True(LoadedRaises(fe), fe.GetType().Name);
		}
	});

	[Fact]
	public void The_other_consumers_accept_a_null_enumerator() => Run(() =>
	{
		var host = new StackPanel();
		host.Children.Add(new ReturnsNull());
		LifecycleBroadcaster.BroadcastLoaded(host);
		LifecycleBroadcaster.BroadcastUnloaded(host);
		_ = host.Children[0].MoveFocus(new System.Windows.Input.TraversalRequest(
			System.Windows.Input.FocusNavigationDirection.Next));
		new Window { Content = new ReturnsNull() }.Close();
	});

	[Fact]
	public void Panel_ContentControl_and_ItemsControl_return_their_children_unchanged() => Run(() =>
	{
		var panel = new StackPanel();
		var a = new Bare();
		panel.Children.Add(a);
		Assert.Equal(new object[] { a }, LogicalTreeHelper.GetChildren(panel).Cast<object>());

		var cc = new ContentControl { Content = a };
		Assert.Equal(new object[] { a }, LogicalTreeHelper.GetChildren(cc).Cast<object>());
		Assert.Empty(LogicalTreeHelper.GetChildren(new ContentControl { Content = "text" }).Cast<object>());

		var items = new ItemsControl();
		items.Items.Add(new Bare());
		_ = items.Widget;
		Assert.Single(LogicalTreeHelper.GetChildren(items).Cast<object>());
	});
}
