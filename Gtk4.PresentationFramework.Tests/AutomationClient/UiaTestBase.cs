using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

/// <summary>A shown window with the controls the client API tests find and drive.</summary>
public abstract class UiaTestBase : WpfGtkTestBase, IDisposable
{
	// Subscriptions and listener counts are static; no test may leave any behind for the next.
	public void Dispose()
	{
		if (WpfGtkFixture.Instance.UnavailableReason == null)
			Run(() => System.Windows.Automation.Automation.RemoveAllEventHandlers());
	}

	protected sealed class Form
	{
		public Application App = null!;
		public Window Window = null!;
		public StackPanel Panel = null!;
		public TextBox Name = null!;
		public Button Ok = null!;
		public Button Off = null!;
		public CheckBox Agree = null!;
		public Slider Slider = null!;
		public ComboBox Combo = null!;
		public ListBox List = null!;
		public int Clicks;
	}

	// A panel has no peer of its own, as in WPF; this one shows up in the automation tree.
	sealed class PaneStack : StackPanel
	{
		protected override System.Windows.Automation.Peers.AutomationPeer? OnCreateAutomationPeer() =>
			new System.Windows.Automation.Peers.FrameworkElementAutomationPeer(this);
	}

	protected static T Id<T>(T element, string id) where T : DependencyObject
	{
		AutomationProperties.SetAutomationId(element, id);
		return element;
	}

	protected static Form OpenForm()
	{
		var f = new Form { App = new Application() };
		f.Name = Id(new TextBox(), "Name");
		f.Ok = Id(new Button { Content = "OK" }, "Ok");
		f.Ok.Click += (s, e) => f.Clicks++;
		f.Off = Id(new Button { Content = "Off", IsEnabled = false }, "Off");
		f.Agree = Id(new CheckBox { Content = "Agree", IsChecked = false }, "Agree");
		f.Slider = Id(new Slider { Minimum = 0, Maximum = 10, Value = 2 }, "Level");
		f.Combo = Id(new ComboBox(), "Choice");
		f.Combo.Items.Add("one");
		f.Combo.Items.Add("two");
		f.List = Id(new ListBox(), "Items");
		f.List.Items.Add("a");
		f.List.Items.Add("b");
		f.List.Items.Add("c");
		f.Panel = Id(new PaneStack(), "Panel");
		foreach (var child in new UIElement[] { f.Name, f.Ok, f.Off, f.Agree, f.Slider, f.Combo, f.List })
			f.Panel.Children.Add(child);
		f.Window = Id(new Window { Title = "Main", Content = f.Panel, Width = 400, Height = 500 }, "MainWindow");
		f.Window.Show();
		GtkPump.Until(() => f.Window.Widget.IsMapped && f.Ok.Widget.Width > 0);
		return f;
	}

	protected static AutomationElement Find(string id) =>
		AutomationElement.RootElement.FindFirst(TreeScope.Descendants,
			new PropertyCondition(AutomationElement.AutomationIdProperty, id));
}
