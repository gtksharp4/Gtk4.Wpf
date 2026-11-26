using System.Windows.Automation;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Data Binding/SimpleBinding": a TextBox and a TextBlock bound to one Person.</summary>
public class SimpleBindingTests : WpfUiWindowTest<SimpleBinding.MainWindow>
{
	AutomationElement Box => App.Find(By.ControlType(ControlType.Edit));

	AutomationElement Echo => App.Find(By.ClassName("TextBlock"));

	[WpfUiFact]
	public void Both_controls_start_with_the_source_value()
	{
		Assert.Equal("Joe", Box.GetText());
		Assert.Equal("Joe", Echo.Current.Name);
	}

	[WpfUiFact]
	public void Typing_updates_the_source_on_each_change_and_the_other_control()
	{
		Box.Type("Ada");
		App.WaitUntil(() => Echo.Current.Name == "Ada", description: "the TextBlock to show Ada");
		Assert.NotNull(App.TryFind(By.Name("Ada").And(By.ClassName("TextBlock"))));
	}

	[WpfUiFact]
	public void The_labels_come_from_the_XAML_content()
	{
		Assert.NotNull(App.TryFind(By.Name("Enter a Name:")));
		Assert.NotNull(App.TryFind(By.Name("The name you entered:")));
	}
}
