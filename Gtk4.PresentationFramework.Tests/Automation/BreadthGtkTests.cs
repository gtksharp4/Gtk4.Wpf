using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

/// <summary>The Phase 4 pushes into GTK's accessible tree, read back through GTK itself.</summary>
public class BreadthGtkTests : WpfGtkTestBase
{
	static AutomationPeer PeerOf(UIElement e) => UIElementAutomationPeer.CreatePeerForElement(e)!;

	[Fact]
	public void LabeledBy_reaches_the_gtk_labelled_by_relation_and_clears() => Run(() =>
	{
		var label = new Label { Content = "Name" };
		var box = new TextBox();
		var target = box.FocusWidget;
		_ = label.Widget;
		PeerOf(label);
		PeerOf(box);
		Assert.False(GtkA11yProbe.HasRelation(target, Gtk.AccessibleRelation.LabelledBy));
		AutomationProperties.SetLabeledBy(box, label);
		Assert.True(GtkA11yProbe.HasRelation(target, Gtk.AccessibleRelation.LabelledBy));
		Assert.True(GtkA11yProbe.RelationTargetEquals(
			target, Gtk.AccessibleRelation.LabelledBy, ((UIElementAutomationPeer)PeerOf(label)).Target(label.Widget)));
		AutomationProperties.SetLabeledBy(box, null);
		Assert.False(GtkA11yProbe.HasRelation(target, Gtk.AccessibleRelation.LabelledBy));
	});

	[Fact]
	public void LabeledBy_set_before_the_widget_exists_is_pushed_at_creation() => Run(() =>
	{
		var label = new Label { Content = "Name" };
		var box = new TextBox();
		AutomationProperties.SetLabeledBy(box, label);
		_ = label.Widget;
		Assert.True(GtkA11yProbe.HasRelation(box.FocusWidget, Gtk.AccessibleRelation.LabelledBy));
	});

	sealed class Observer : System.IDisposable
	{
		public readonly List<(string Message, Gtk.AccessibleAnnouncementPriority Priority)> Seen =
			new List<(string, Gtk.AccessibleAnnouncementPriority)>();

		public Observer() => PeerAccessible_Observer(Add);

		void Add(string m, Gtk.AccessibleAnnouncementPriority p) => Seen.Add((m, p));

		public void Dispose() => PeerAccessible_Observer(null);

		static void PeerAccessible_Observer(System.Action<string, Gtk.AccessibleAnnouncementPriority>? observer)
		{
			var type = typeof(AutomationPeer).Assembly.GetType("System.Windows.Automation.Peers.PeerAccessible")!;
			type.GetField("AnnouncementObserver", System.Reflection.BindingFlags.Static
				| System.Reflection.BindingFlags.NonPublic)!.SetValue(null, observer);
		}
	}

	[Fact]
	public void LiveRegionChanged_announces_by_the_live_setting() => Run(() =>
	{
		var block = new TextBlock { Text = "3 results" };
		_ = block.Widget;
		var peer = PeerOf(block);
		using var observer = new Observer();
		peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
		Assert.Empty(observer.Seen);
		AutomationProperties.SetLiveSetting(block, AutomationLiveSetting.Polite);
		peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
		AutomationProperties.SetLiveSetting(block, AutomationLiveSetting.Assertive);
		peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
		Assert.Equal(new[]
		{
			("3 results", Gtk.AccessibleAnnouncementPriority.Medium),
			("3 results", Gtk.AccessibleAnnouncementPriority.High),
		}, observer.Seen);
	});

	[Fact]
	public void Notification_events_are_announced_with_a_priority_from_their_processing() => Run(() =>
	{
		var block = new TextBlock { Text = "status" };
		_ = block.Widget;
		var peer = PeerOf(block);
		using var observer = new Observer();
		peer.RaiseNotificationEvent(AutomationNotificationKind.Other,
			AutomationNotificationProcessing.ImportantAll, "Saved", "save");
		peer.RaiseNotificationEvent(AutomationNotificationKind.Other,
			AutomationNotificationProcessing.All, "Syncing", "sync");
		peer.RaiseNotificationEvent(AutomationNotificationKind.Other,
			AutomationNotificationProcessing.All, "", "none");
		Assert.Equal(new[]
		{
			("Saved", Gtk.AccessibleAnnouncementPriority.High),
			("Syncing", Gtk.AccessibleAnnouncementPriority.Medium),
		}, observer.Seen);
	});

	[Fact]
	public void Progress_bar_and_tree_node_push_their_value_and_level() => Run(() =>
	{
		Assert.SkipUnless(GtkA11yProbe.CanCheckValues, "variadic GTK test API unavailable on this ABI");
		var bar = new ProgressBar { Minimum = 0, Maximum = 10, Value = 4 };
		_ = bar.Widget;
		PeerOf(bar);
		AssertRole(bar.Widget, Gtk.AccessibleRole.ProgressBar);
		Assert.True(GtkA11yProbe.HasProperty(bar.Widget, Gtk.AccessibleProperty.ValueNow));

		var leaf = new TreeViewItem { Header = "leaf" };
		var root = new TreeViewItem { Header = "root" };
		root.Items.Add(leaf);
		var tree = new TreeView();
		tree.Items.Add(root);
		_ = tree.Widget;
		PeerOf(tree).GetChildren()![0].GetChildren();
		AssertRole(root.Widget, Gtk.AccessibleRole.TreeItem);
		Assert.True(GtkA11yProbe.HasProperty(root.Widget, Gtk.AccessibleProperty.Level));
	});

	static void AssertRole(Gtk.Widget widget, Gtk.AccessibleRole role) =>
		Assert.True(GtkA11yProbe.HasRole(widget, role), "GTK reports role " + role);

	[Fact]
	public void Text_box_expander_and_menu_item_roles_and_states() => Run(() =>
	{
		var rich = new RichTextBox();
		_ = rich.Widget;
		PeerOf(rich);
		Assert.Equal(Gtk.AccessibleRole.TextBox, rich.FocusWidget.AccessibleRole);
		var check = new MenuItem { Header = "Wrap", IsCheckable = true };
		_ = check.Widget;
		PeerOf(check);
		Assert.Equal(Gtk.AccessibleRole.MenuItemCheckbox, check.Widget.AccessibleRole);
		var expander = new Expander { Header = "More" };
		_ = expander.Widget;
		PeerOf(expander);
		expander.IsExpanded = true;
		Assert.True(GtkA11yProbe.HasState(expander.Widget, Gtk.AccessibleState.Expanded));
	});
}
