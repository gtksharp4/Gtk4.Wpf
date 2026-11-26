using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Samples;

/// <summary>
/// Snapshots of the automation tree of each sample the driver tests, so a change in what the peers report
/// (type, name, class, nesting) shows up as one readable diff. The enabled state is left out: it follows
/// app logic (a command's CanExecute), not the peers.
/// </summary>
static class TreeSnapshot
{
	internal static string Of(WpfAppHost app) =>
		TreeDump.Of(app.MainWindow).Replace("\r\n", "\n").Replace(" [disabled]", "");
}

public class SimpleBindingTreeTests : WpfUiWindowTest<SimpleBinding.MainWindow>
{
	[WpfUiFact]
	public void Tree_matches_the_snapshot() => Assert.Equal(
		"Window name=\"MainWindow\" class=Window\n"
		+ "  Text name=\"Enter a Name:\" class=Label\n"
		+ "  Edit class=TextBox\n"
		+ "  Text name=\"The name you entered:\" class=Label\n"
		+ "  Text name=\"Joe\" class=TextBlock\n", TreeSnapshot.Of(App));
}

public class MasterDetailTreeTests : WpfUiWindowTest<MasterDetail.MainWindow>
{
	[WpfUiFact]
	public void Tree_matches_the_snapshot() => Assert.Equal(
		"Window name=\"MainWindow\" class=Window\n"
		+ "  Text name=\"My Soccer Leagues\" class=Label\n"
		+ "  List class=ListBox\n"
		+ "    ListItem name=\"League A\" class=ListBoxItem\n"
		+ "    ListItem name=\"League B\" class=ListBoxItem\n"
		+ "  Text name=\"League A\" class=Label\n"
		+ "  List class=ListBox\n"
		+ "    ListItem name=\"Division A\" class=ListBoxItem\n"
		+ "    ListItem name=\"Division B\" class=ListBoxItem\n"
		+ "    ListItem name=\"Division C\" class=ListBoxItem\n"
		+ "  Text name=\"Division A\" class=Label\n"
		+ "  List class=ListBox\n"
		+ "    ListItem name=\"Team I\" class=ListBoxItem\n"
		+ "    ListItem name=\"Team II\" class=ListBoxItem\n"
		+ "    ListItem name=\"Team III\" class=ListBoxItem\n"
		+ "    ListItem name=\"Team IV\" class=ListBoxItem\n"
		+ "    ListItem name=\"Team V\" class=ListBoxItem\n", TreeSnapshot.Of(App));
}

public class DirectionalBindingTreeTests : WpfUiWindowTest<DirectionalBinding.MainWindow>
{
	[WpfUiFact]
	public void Tree_matches_the_snapshot() => Assert.Equal(
		"Window name=\"MainWindow\" class=Window\n"
		+ "  Text name=\"Total Income:\" class=Label\n"
		+ "  Text id=\"IncomeText\" name=\"5000\" class=TextBlock\n"
		+ "  Text name=\"OneTime Binding\" class=TextBlock\n"
		+ "  Text name=\"Rent\" class=Label\n"
		+ "  Text id=\"RentText\" name=\"2000\" class=TextBlock\n"
		+ "  Text name=\"OneWay Binding, with TargetUpdated event handling\" class=TextBlock\n"
		+ "  Text name=\"Food\" class=Label\n"
		+ "  Edit id=\"FoodText\" class=TextBox\n"
		+ "  Text name=\"TwoWay Binding (TextBox default), Update on PropertyChanged\" class=TextBlock\n"
		+ "  Text name=\"Miscellaneous\" class=Label\n"
		+ "  Edit id=\"MiscText\" class=TextBox\n"
		+ "  Text name=\"TwoWay Binding (TextBox default), Update on LostFocus (Te...\" class=TextBlock\n"
		+ "  Text name=\"Savings\" class=Label\n"
		+ "  Text id=\"SavingsText\" name=\"3000\" class=TextBlock\n"
		+ "  Text name=\"OneWay Binding\" class=TextBlock\n"
		+ "  Button name=\"Raise the Rent!\" class=Button\n"
		+ "  Text id=\"infoText\" class=TextBlock\n", TreeSnapshot.Of(App));
}

public class CommandSampleTreeTests : WpfUiWindowTest<HandlingEventOnCommandUsingCode.MainWindow>
{
	[WpfUiFact]
	public void Tree_matches_the_snapshot() => Assert.Equal(
		"Window name=\"MainWindow\" class=Window\n"
		+ "  Button name=\"Open (KeyBindings: Ctrl-R, Ctrl-0)\" class=Button\n", TreeSnapshot.Of(App));
}

public class MousePointerTreeTests : WpfUiWindowTest<MousePointer.MainWindow>
{
	[WpfUiFact]
	public void Tree_matches_the_snapshot() => Assert.Equal(
		"Window name=\"MainWindow\" class=Window\n"
		+ "  Text name=\"Move Cursor Over Me\" class=Label\n", TreeSnapshot.Of(App));
}
