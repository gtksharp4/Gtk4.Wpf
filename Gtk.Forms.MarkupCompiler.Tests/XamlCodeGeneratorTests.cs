using Gtk.Forms.MarkupCompiler;
using Xunit;

namespace Gtk.Forms.MarkupCompiler.Tests
{
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
			Assert.Contains("public partial class MainWindow {", code);
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
			Assert.Contains("public partial class MainWindow {", code);
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
			Assert.Contains("public partial class App {", code);
			// StartupUri is a root attribute
			Assert.Contains(@"this.StartupUri = ""MainWindow.xaml"";", code);
			// [STAThread] Main
			Assert.Contains("[System.STAThread]", code);
			Assert.Contains("static void Main() {", code);
			Assert.Contains("var app = new App();", code);
			Assert.Contains("app.InitializeComponent();", code);
			Assert.Contains("app.Run();", code);
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
}
