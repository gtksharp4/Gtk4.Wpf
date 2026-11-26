using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Creating a ComboBox's GTK widget must not raise SelectionChanged (WPF has no such event there).</summary>
public class ComboBoxWidgetCreationTests : WpfGtkTestBase
{
	[Fact]
	public void Creating_the_widget_raises_no_SelectionChanged_for_the_initial_index() => Run(() =>
	{
		var cb = new ComboBox();
		cb.Items.Add("a");
		cb.Items.Add("b");
		cb.SelectedIndex = 1;
		int raised = 0;
		cb.SelectionChanged += (_, _) => raised++;

		_ = cb.Widget;

		Assert.Equal(0, raised);
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Equal("b", cb.SelectedItem);
	});

	[Fact]
	public void A_handler_that_rebuilds_the_tree_does_not_recurse_through_widget_creation() => Run(() =>
	{
		var panel = new DockPanel();
		var cb = new ComboBox();
		cb.Items.Add("a");
		cb.Items.Add("b");
		cb.SelectedIndex = 1;
		int raised = 0;
		cb.SelectionChanged += (_, _) =>
		{
			raised++;
			var host = new StackPanel();
			host.Children.Add(cb);
			panel.Children.Clear();
			panel.Children.Add(host);
			_ = panel.Widget;
		};
		panel.Children.Add(cb);

		_ = panel.Widget;
		cb.SelectedIndex = 0;

		Assert.True(raised < 5, "SelectionChanged re-entered " + raised + " times");
	});
}
