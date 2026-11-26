using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Gtk4.Wpf.Testing.Tests.Driver;

/// <summary>A small login-style form the driver tests find and drive.</summary>
public sealed class FormWindow : Window
{
	public readonly TextBox User = new TextBox();
	public readonly Button Login = new Button { Content = "Log in", IsEnabled = false };
	public readonly CheckBox Remember = new CheckBox { Content = "Remember me" };
	public readonly TextBlock Status = new TextBlock { Text = "Signed out" };
	public readonly ListBox Fruit = new ListBox();
	public readonly ListBox Veg = new ListBox();
	public int Clicks;
	public int KeyDowns;
	public int MouseDowns;

	public FormWindow()
	{
		Title = "Form";
		Width = 400;
		Height = 500;
		AutomationProperties.SetAutomationId(User, "User");
		AutomationProperties.SetAutomationId(Login, "Login");
		AutomationProperties.SetAutomationId(Remember, "Remember");
		AutomationProperties.SetAutomationId(Status, "Status");
		AutomationProperties.SetAutomationId(Fruit, "Fruit");
		AutomationProperties.SetAutomationId(Veg, "Veg");
		foreach (var item in new[] { "Apple", "Pear" }) Fruit.Items.Add(item);
		foreach (var item in new[] { "Leek", "Pear" }) Veg.Items.Add(item);
		User.TextChanged += (s, e) => Login.IsEnabled = User.Text.Length > 0;
		Login.Click += (s, e) =>
		{
			Clicks++;
			Status.Text = "Welcome, " + User.Text;
		};
		User.KeyDown += (s, e) => KeyDowns++;
		Login.PreviewMouseDown += (s, e) => MouseDowns++;
		var panel = new StackPanel();
		foreach (UIElement child in new UIElement[] { User, Login, Remember, Status, Fruit, Veg })
			panel.Children.Add(child);
		Content = panel;
	}
}
