using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Automation;

public partial class PatternTests
{
	[Fact]
	public void Window_pattern_reports_and_changes_chrome_state() => Run(() =>
	{
		var window = new Window { Title = "Main", Topmost = true };
		var peer = UIElementAutomationPeer.CreatePeerForElement(window)!;
		Assert.Equal("Main", peer.GetName());
		Assert.Equal(AutomationControlType.Window, peer.GetAutomationControlType());
		var provider = (IWindowProvider)peer.GetPattern(PatternInterface.Window)!;
		Assert.True(provider.IsTopmost);
		Assert.False(provider.IsModal);
		Assert.True(provider.Maximizable);
		Assert.Equal(WindowVisualState.Normal, provider.VisualState);
		provider.SetVisualState(WindowVisualState.Maximized);
		Assert.Equal(WindowState.Maximized, window.WindowState);
		Assert.Equal(WindowVisualState.Maximized, provider.VisualState);
		window.ResizeMode = ResizeMode.NoResize;
		Assert.Throws<InvalidOperationException>(() => provider.SetVisualState(WindowVisualState.Minimized));
	});

	[Fact]
	public void Window_Close_closes_the_window() => Run(() =>
	{
		var window = new Window();
		bool closed = false;
		window.Closed += (s, e) => closed = true;
		window.Show();
		Pattern<IWindowProvider>(window, PatternInterface.Window).Close();
		Assert.True(GtkPump.Until(() => closed));
	});

	[Fact]
	public void Slider_orientation_and_text_peers_report_their_types() => Run(() =>
	{
		var slider = UIElementAutomationPeer.CreatePeerForElement(new Slider { Orientation = Orientation.Vertical })!;
		Assert.Equal(AutomationOrientation.Vertical, slider.GetOrientation());
		var text = UIElementAutomationPeer.CreatePeerForElement(new TextBlock { Text = "Hi" })!;
		Assert.Equal(AutomationControlType.Text, text.GetAutomationControlType());
		Assert.Equal("Hi", text.GetName());
		var label = UIElementAutomationPeer.CreatePeerForElement(new Label { Content = "Name" })!;
		Assert.Equal("Name", label.GetName());
		Assert.Equal("Label", label.GetClassName());
	});
}
