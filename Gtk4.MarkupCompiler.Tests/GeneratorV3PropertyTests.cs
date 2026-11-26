using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV3PropertyTests : GeneratorV2TestBase
{
	const string Stub = "namespace A { public partial class W : System.Windows.Window { } }";
	const string PageStub = "namespace A { public partial class P { } }";

	static string Window(string content) => $@"<Window x:Class=""A.W"" {Head}>{content}</Window>";

	[Fact]
	public void G5_an_owner_qualified_property_on_its_own_type_is_set_directly()
	{
		string code = Gen(Window(@"<TextBlock TextBlock.TextTrimming=""WordEllipsis"" TextBlock.FontSize=""12""/>"));
		Assert.DoesNotContain("SetTextTrimming", code);
		Assert.DoesNotContain("SetFontSize", code);
		Assert.Contains("TextBlock.TextTrimmingProperty", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G5_an_owner_qualified_property_of_a_base_class_is_set_directly()
	{
		string code = Gen(Window(@"<Button Control.Padding=""4""/>"));
		Assert.DoesNotContain("SetPadding", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G5_an_attached_property_of_another_type_stays_attached()
	{
		string code = Gen(Window(@"<Grid><TextBlock Grid.Column=""1"" TextElement.FontSize=""9""/></Grid>"));
		Assert.Contains("Grid.SetColumn(", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G5_a_flow_content_FontWeight_goes_through_its_dependency_property()
	{
		string code = Gen(Window(@"<TextBlock><Run FontWeight=""Bold"" Text=""x""/></TextBlock>"));
		Assert.Contains("SetValue(System.Windows.Documents.Run.FontWeightProperty, System.Windows.Markup.XamlValueConverter.Convert(", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G5_a_code_behind_partial_without_a_base_gets_the_root_type_as_base()
	{
		string code = Gen(Window("<Button/>"), sources: new[] { "namespace A { public partial class W { } }" });
		Assert.Contains("public partial class W : global::System.Windows.Window", code);
		Assert.DoesNotContain("public W()", code);
		AssertCompiles(code, "namespace A { public partial class W { public W() { InitializeComponent(); } } }");
	}

	[Fact]
	public void G5_a_code_behind_partial_that_declares_the_same_base_repeats_it()
	{
		const string cb = "namespace A { public partial class W : System.Windows.Window { } }";
		string code = Gen(Window("<Button/>"), sources: new[] { cb });
		Assert.Contains("partial class W : global::System.Windows.Window", code);
		AssertCompiles(code, cb);
	}

	[Fact]
	public void G5_a_code_behind_that_only_lists_interfaces_still_gets_the_root_base()
	{
		const string cb = "namespace A { public partial class P : System.ComponentModel.INotifyPropertyChanged { "
			+ "public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged; "
			+ "public P() { InitializeComponent(); Content = null; Loaded += (s, e) => { }; } } }";
		string code = Gen($@"<Page x:Class=""A.P"" {Head}/>", sources: new[] { cb });
		Assert.Contains(": global::System.Windows.Controls.Page", code);
		AssertCompiles(code, cb);
	}

	[Fact]
	public void G5_a_code_behind_partial_with_no_base_on_an_Application_root()
	{
		string code = Gen($@"<Application x:Class=""A.App"" {Head}/>",
			sources: new[] { "namespace A { public partial class App { } }" });
		Assert.Contains("partial class App : global::System.Windows.Application", code);
		AssertCompiles(code, "namespace A { public partial class App { } }");
	}

	[Fact]
	public void G5_Localization_attributes_are_localisation_metadata_and_ignored()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} Localization.Comments=""$Content(Hi)"">
<Border Localization.Attributes=""$Content (Readable Modifiable)"" Localization.Comments=""$Content(x)""/></Window>");
		Assert.DoesNotContain("Localization", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void G5_no_code_behind_still_emits_base_and_constructor()
	{
		string code = Gen(Window("<Button/>"), hasCodeBehind: false);
		Assert.Contains("public partial class W : global::System.Windows.Window", code);
		Assert.Contains("public W() { InitializeComponent(); }", code);
	}
}
