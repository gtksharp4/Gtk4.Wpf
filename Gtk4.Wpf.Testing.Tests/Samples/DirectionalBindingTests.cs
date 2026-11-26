using System.Linq;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Data Binding/DirectionalBinding": OneTime, OneWay and TwoWay bindings on one source.</summary>
public class DirectionalBindingTests : WpfUiWindowTest<DirectionalBinding.MainWindow>
{
	[WpfUiFact]
	public void The_initial_values_come_from_the_source()
	{
		Assert.NotNull(App.TryFind(By.Name("5000").And(By.ClassName("TextBlock"))));
		Assert.NotNull(App.TryFind(By.Name("2000").And(By.ClassName("TextBlock"))));
		Assert.NotNull(App.TryFind(By.Name("3000").And(By.ClassName("TextBlock"))));
	}

	[WpfUiFact]
	public void A_TwoWay_PropertyChanged_edit_updates_the_one_way_savings_text()
	{
		var food = App.Find(By.AutomationId("FoodText"));
		food.Type("300");
		App.WaitUntil(() => App.TryFind(By.Name("2700")) != null, description: "Savings to show 2700");
		Assert.Equal("300", food.GetText());
	}

	[WpfUiFact]
	public void Invoking_the_button_raises_the_rent_and_the_savings_follow()
	{
		// The sample raises the rent by a random 0-9 percent, so only the invariant is stable.
		App.Find(By.Name("Raise the Rent!")).Click();
		App.WaitForIdle();
		// The numeric TextBlocks, in order: total income, rent, savings.
		var numbers = App.FindAll(By.ClassName("TextBlock"))
			.Select(t => t.Current.Name).Where(n => int.TryParse(n, out _)).Select(int.Parse).ToArray();
		Assert.Equal(3, numbers.Length);
		int rent = numbers[1], savings = numbers[2];
		Assert.InRange(rent, 2000, 2180);
		Assert.Equal(5000, rent + savings);
	}
}
