using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Attached properties on the root element resolve their owner through the xmlns mapping.</summary>
public class GeneratorRootAttachedTests : GeneratorV2TestBase
{
	[Fact]
	public void Window_RootAttachedBinding_UsesOwnerType()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W"" Title=""t""
			FocusManager.FocusedElement=""{{Binding ElementName=tb}}""><TextBox x:Name=""tb""/></Window>");

		Assert.True(code.Contains("global::System.Windows.Input.FocusManager.FocusedElementProperty"), code);
		Assert.DoesNotContain("this.FocusManager", code);
		AssertCompiles(code,
			"namespace A { public partial class W : System.Windows.Window { public W() { InitializeComponent(); } } }");
	}

	[Fact]
	public void Page_RootAttachedBinding_UsesOwnerType()
	{
		string code = Gen($@"<Page {Head} x:Class=""A.P""
			FocusManager.FocusedElement=""{{Binding ElementName=tb}}""><TextBox x:Name=""tb""/></Page>");

		Assert.True(code.Contains("global::System.Windows.Input.FocusManager.FocusedElementProperty"), code);
		AssertCompiles(code,
			"namespace A { public partial class P : System.Windows.Controls.Page "
			+ "{ public P() { InitializeComponent(); } } }");
	}

	[Fact]
	public void Window_RootPlainAttachedValue_UsesSetter()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W"" Title=""t"" Grid.Row=""2""/>");

		Assert.Contains("Grid.SetRow(this", code);
		Assert.DoesNotContain("this.FocusManager", code);
		AssertCompiles(code,
			"namespace A { public partial class W : System.Windows.Window { public W() { InitializeComponent(); } } }");
	}

	[Fact]
	public void Window_RootAttachedPropertyElement_UsesOwnerSetter()
	{
		string code = Gen($@"<Window {Head} x:Class=""A.W"" Title=""t"">
			<WindowChrome.WindowChrome><WindowChrome CaptionHeight=""30""/></WindowChrome.WindowChrome>
			<Grid/></Window>");

		Assert.Contains("global::System.Windows.Shell.WindowChrome.SetWindowChrome(this, ", code);
		AssertCompiles(code,
			"namespace A { public partial class W : System.Windows.Window { public W() { InitializeComponent(); } } }");
	}
}
