using System.Linq;
using System.Windows.Automation;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>WPF-Samples "Data Binding/MasterDetail": three lists synchronised through the current item.</summary>
public class MasterDetailTests : WpfUiWindowTest<MasterDetail.MainWindow>
{
	AutomationElement[] Lists => App.FindAll(By.ControlType(ControlType.List));

	static string[] Items(AutomationElement list) =>
		list.FindAll(By.ControlType(ControlType.ListItem)).Select(i => i.Current.Name).ToArray();

	[WpfUiFact]
	public void The_first_league_and_division_are_current_at_start()
	{
		Assert.Equal(new[] { "League A", "League B" }, Items(Lists[0]));
		Assert.Equal(new[] { "Division A", "Division B", "Division C" }, Items(Lists[1]));
		Assert.Equal(new[] { "Team I", "Team II", "Team III", "Team IV", "Team V" }, Items(Lists[2]));
		Assert.NotNull(App.TryFind(By.Name("League A").And(By.ControlType(ControlType.Text))));
	}

	[WpfUiFact]
	public void Selecting_a_league_reloads_the_divisions_and_teams()
	{
		App.Find(By.Name("League B").And(By.ControlType(ControlType.ListItem))).Select();
		App.WaitUntil(() => Items(Lists[2]).Contains("Team 1"), description: "the teams of League B");
		Assert.NotNull(App.TryFind(By.Name("League B").And(By.ControlType(ControlType.Text))));
		Assert.Equal(new[] { "Team 1", "Team 2", "Team 3", "Team 4", "Team 5" }, Items(Lists[2]));
	}

	[WpfUiFact]
	public void Selecting_a_division_reloads_the_teams_and_its_label()
	{
		Lists[1].FindAll(By.Name("Division B"))[0].Select();
		App.WaitUntil(() => Items(Lists[2]).Contains("Team Blue"), description: "the teams of Division B");
		Assert.NotNull(App.TryFind(By.Name("Division B").And(By.ControlType(ControlType.Text))));
	}
}
