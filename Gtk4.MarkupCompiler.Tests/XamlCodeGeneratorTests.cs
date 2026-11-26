using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Unit tests for <see cref="XamlCodeGenerator"/>.</summary>
public class XamlCodeGeneratorTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	// ------------------------------------------------------------------ helpers

	static string Window(string body = "", string attrs = "")
	{
		return $@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	{attrs}>
	{body}
</Window>";
	}

	// ------------------------------------------------------------------ tests

	[Fact]
	public void NoXClass_ReturnsNull()
	{
		string xaml = @"<Window xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" />";
		string code = _gen.Generate(xaml, out string className);
		Assert.Null(code);
		Assert.Null(className);
	}

	[Fact]
	public void ClassName_ParsedFromXClass()
	{
		_gen.Generate(Window(), out string className);
		Assert.Equal("MyApp.MainWindow", className);
	}

	[Fact]
	public void Namespace_EmittedCorrectly()
	{
		string code = _gen.Generate(Window(), out _);
		Assert.Contains("namespace MyApp {", code);
	}

	[Fact]
	public void PartialClass_Declared()
	{
		string code = _gen.Generate(Window(), out _);
		Assert.Contains("public partial class MainWindow : global::System.Windows.Window {", code);
	}

	[Fact]
	public void IdempotentGuard_Present()
	{
		string code = _gen.Generate(Window(), out _);
		Assert.Contains("bool _contentLoaded;", code);
		Assert.Contains("if (_contentLoaded) return;", code);
		Assert.Contains("_contentLoaded = true;", code);
	}

	[Fact]
	public void PlainAttribute_EmittedAsPropertySet()
	{
		string xaml = Window(attrs: @"Title=""Hello World""");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"this.Title = ""Hello World"";", code);
	}

	[Fact]
	public void ChildElement_ConstructedAndAssigned()
	{
		string xaml = Window(@"<Grid><TextBlock Text=""hi"" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("new System.Windows.Controls.Grid()", code);
		Assert.Contains("new System.Windows.Controls.TextBlock()", code);
		Assert.Contains(@".Text = ""hi"";", code);
	}

	[Fact]
	public void NestedGridTextBlock_GridChildrenAdd()
	{
		string xaml = Window(@"<Grid><TextBlock Text=""hi"" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		// Grid uses .Children.Add(child)
		Assert.Contains(".Children.Add(", code);
		// Window uses .Content = child
		Assert.Contains(".Content = ", code);
	}

	[Fact]
	public void XName_FieldDeclaredAndAssigned()
	{
		string xaml = Window(@"<Grid><TextBlock x:Name=""greeting"" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("internal System.Windows.Controls.TextBlock greeting;", code);
		Assert.Contains("this.greeting = ", code);
	}

	[Fact]
	public void BindingAttribute_EmitsSetBinding()
	{
		string xaml = Window(@"<Grid><TextBlock x:Name=""tb"" Text=""{Binding Greeting}"" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetBinding(System.Windows.Controls.TextBlock.TextProperty,", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""Greeting"")", code);
	}

	[Fact]
	public void FullWindowExample_MatchesSpec()
	{
		// The canonical example from the task description.
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	Title=""Hi"">
  <Grid><TextBlock x:Name=""greeting"" Text=""{Binding Greeting}"" /></Grid>
</Window>";

		string code = _gen.Generate(xaml, out string className);

		Assert.Equal("MyApp.MainWindow", className);
		Assert.Contains("namespace MyApp {", code);
		Assert.Contains("public partial class MainWindow : global::System.Windows.Window {", code);
		Assert.Contains("internal System.Windows.Controls.TextBlock greeting;", code);
		Assert.Contains("bool _contentLoaded;", code);
		Assert.Contains("public void InitializeComponent() {", code);
		Assert.Contains("if (_contentLoaded) return;", code);
		Assert.Contains("_contentLoaded = true;", code);
		Assert.Contains(@"this.Title = ""Hi"";", code);
		Assert.Contains("new System.Windows.Controls.Grid()", code);
		Assert.Contains("new System.Windows.Controls.TextBlock()", code);
		Assert.Contains("this.greeting = ", code);
		Assert.Contains("SetBinding(System.Windows.Controls.TextBlock.TextProperty,", code);
		Assert.Contains(@"new System.Windows.Data.Binding(""Greeting"")", code);
		Assert.Contains(".Children.Add(", code);
		Assert.Contains("this.Content = ", code);
	}

	[Fact]
	public void ApplicationDefinition_EmitsMain()
	{
		string xaml = @"<Application x:Class=""MyApp.App""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	StartupUri=""MainWindow.xaml"">
</Application>";

		string code = _gen.Generate(xaml, out string className);

		Assert.Equal("MyApp.App", className);
		Assert.Contains("public partial class App : global::System.Windows.Application {", code);
		// StartupUri is emitted as System.Uri (not a plain string).
		Assert.Contains(
			@"this.StartupUri = new System.Uri(""MainWindow.xaml"", System.UriKind.RelativeOrAbsolute);",
			code);
		// [STAThread] Main
		Assert.Contains("[System.STAThread]", code);
		Assert.Contains("static void Main() {", code);
		Assert.Contains("var app = new App();", code);
		Assert.Contains("app.InitializeComponent();", code);
		Assert.Contains("app.Run();", code);
	}

	// ------------------------------------------------------------------ #7 typed-attribute tests
	// Round 5: DP attributes now go through XamlValueConverter.Convert for runtime type resolution.
	// Tests updated from literal-emission assertions to converter-emission assertions.

	[Fact]
	public void Attribute_enum_emits_converter_call()
	{
		// Orientation="Horizontal" on a StackPanel → SetValue + XamlValueConverter.Convert.
		string xaml = Window(@"<StackPanel Orientation=""Horizontal"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"Horizontal\"", code);
		Assert.Contains("SetValue(", code);
	}

	[Fact]
	public void Attribute_integer_emits_converter_call()
	{
		// FontSize="14" → XamlValueConverter.Convert returns double 14 at runtime.
		string xaml = Window(@"<StackPanel><TextBlock FontSize=""14"" /></StackPanel>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"14\"", code);
	}

	[Fact]
	public void Attribute_double_emits_converter_call()
	{
		// Width="1.5" → XamlValueConverter.Convert returns double 1.5 at runtime.
		string xaml = Window(@"<StackPanel><TextBlock Width=""1.5"" /></StackPanel>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"1.5\"", code);
	}

	[Fact]
	public void Attribute_bool_true_emits_converter_call()
	{
		// IsEnabled="true" → XamlValueConverter.Convert returns bool true at runtime.
		string xaml = Window(@"<StackPanel><Button IsEnabled=""true"" /></StackPanel>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"true\"", code);
	}

	[Fact]
	public void Attribute_bool_false_emits_converter_call()
	{
		// IsEnabled="False" → XamlValueConverter.Convert returns bool false at runtime.
		string xaml = Window(@"<StackPanel><Button IsEnabled=""False"" /></StackPanel>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"False\"", code);
	}

	[Fact]
	public void Attribute_string_still_emits_string_literal()
	{
		string xaml = Window(attrs: @"Title=""My Window""");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains(@"this.Title = ""My Window"";", code);
	}

	[Fact]
	public void Generate_IsStateless_RunTwiceGivesSameOutput()
	{
		string xaml = Window(@"<Grid><TextBlock x:Name=""tb"" Text=""x"" /></Grid>");
		string a = _gen.Generate(xaml, out _);
		// Reset by creating a fresh generator — counter is instance state.
		var gen2 = new XamlCodeGenerator();
		string b = gen2.Generate(xaml, out _);
		Assert.Equal(a, b);
	}

	[Fact]
	public void NoChildren_InitializeComponentStillValid()
	{
		string xaml = Window();
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("public void InitializeComponent() {", code);
		Assert.Contains("_contentLoaded = true;", code);
	}

	[Fact]
	public void InvalidXaml_ThrowsXamlCodeGeneratorException()
	{
		Assert.Throws<XamlCodeGeneratorException>(() =>
			_gen.Generate("<not valid xml", out _));
	}
}