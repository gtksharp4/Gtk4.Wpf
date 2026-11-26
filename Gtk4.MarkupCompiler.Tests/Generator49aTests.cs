using System.Text.RegularExpressions;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Round 49a: markup-extension trimming, deferred DynamicResource, user element types, generics.</summary>
public class Generator49aTests : GeneratorV2TestBase
{
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } }";

	static string Window(string content, string attrs = "")
		=> $@"<Window x:Class=""A.W"" {Head} xmlns:local=""clr-namespace:A"" {attrs}>{content}</Window>";

	[Theory]
	[InlineData("{ StaticResource   Key }")]
	[InlineData("{StaticResource\n   Key}")]
	[InlineData("{\tStaticResource Key\t}")]
	public void Markup_extension_whitespace_does_not_reach_the_key(string markup)
	{
		string code = Gen(Window($"<Window.Resources><SolidColorBrush x:Key=\"Key\" Color=\"Red\"/></Window.Resources>"
			+ $"<Button Content=\"x\" Background=\"{markup}\"/>"));
		Assert.Contains("\"Key\"", code);
		Assert.DoesNotContain("\"Key \"", code);
		Assert.DoesNotContain("\" Key", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void Whitespace_inside_quoted_markup_arguments_is_kept()
	{
		Assert.Equal("{Binding Path=A, StringFormat='  x  '}",
			XamlCodeGenerator.NormalizeMarkupValue("{ Binding   Path=A,\n StringFormat='  x  ' }"));
		Assert.Equal("{}{0}  x", XamlCodeGenerator.NormalizeMarkupValue("{}{0}  x"));
	}

	[Fact]
	public void A_dynamic_resource_on_a_DP_is_a_deferred_reference_not_an_eager_lookup()
	{
		string code = Gen(Window("<Button FocusVisualStyle=\"{DynamicResource MyFocusVisual}\"/>"));
		Assert.Contains("SetLocalResourceReference(__e0, System.Windows.Controls.Button.FocusVisualStyleProperty",
			code);
		Assert.DoesNotContain("FindResource(\"MyFocusVisual\")", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void A_dynamic_resource_on_a_root_DP_is_deferred()
	{
		string code = Gen(Window("", "Background=\"{DynamicResource bg}\""));
		Assert.Contains("SetLocalResourceReference(this, ", code);
		Assert.DoesNotContain("FindResource(\"bg\")", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void A_nested_x_Static_dynamic_resource_key_is_the_static_member_not_its_text()
	{
		string code = Gen(Window("<Button Foreground=\"{DynamicResource "
			+ "{x:Static SystemColors.ControlTextBrushKey}}\"/>"));
		Assert.Contains(".SystemColors.ControlTextBrushKey)", code);
		Assert.DoesNotContain("x:Static", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void A_string_typed_property_gets_a_string_literal_for_a_numeric_looking_value()
	{
		string code = Gen(Window("<TextBox Text=\"0\"/>"));
		Assert.DoesNotMatch(@"\.Text = 0;", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void A_prefixed_EventTrigger_owner_references_the_static_RoutedEvent_field()
	{
		const string stub = "namespace A { public class Tog : System.Windows.Controls.Primitives.ToggleButton "
			+ "{ public static readonly System.Windows.RoutedEvent ExpandedEvent = null; } "
			+ "public partial class W : System.Windows.Window { } }";
		string code = Gen(Window("<Window.Triggers><EventTrigger RoutedEvent=\"local:Tog.Expanded\">"
			+ "<BeginStoryboard><Storyboard/></BeginStoryboard></EventTrigger></Window.Triggers>"),
			sources: new[] { stub });
		Assert.Contains("RoutedEvent = global::A.Tog.ExpandedEvent", code);
		Assert.DoesNotContain("typeof(local:", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void x_TypeArguments_on_the_root_resolves_a_prefixed_generic_base()
	{
		string code = Gen($@"<PageFunction x:Class=""A.P1"" {Head} xmlns:local=""clr-namespace:A"" "
			+ @"x:TypeArguments=""local:Res""/>");
		Assert.Contains("partial class P1 : global::System.Windows.Navigation.PageFunction<global::A.Res>", code);
	}

	[Fact]
	public void x_TypeArguments_resolves_comma_separated_and_keyword_arguments()
	{
		string code = Gen($@"<PageFunction x:Class=""A.P2"" {Head} xmlns:local=""clr-namespace:A"" "
			+ @"x:TypeArguments=""local:Res, x:String""/>");
		Assert.Contains("PageFunction<global::A.Res, string>", code);
	}

	[Fact]
	public void A_named_element_of_a_local_type_gets_a_typed_field_and_a_name()
	{
		const string stub = "namespace A { public class Box : System.Windows.Controls.TextBox { } "
			+ "public partial class W : System.Windows.Window { } }";
		string code = Gen(Window("<local:Box x:Name=\"DisplayBox\" Height=\"30\" Grid.Row=\"1\"/>"),
			sources: new[] { stub });
		Assert.Contains("internal global::A.Box DisplayBox;", code);
		Assert.Contains("this.DisplayBox = ", code);
		Assert.Contains("_fe.Name = \"DisplayBox\"", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void A_routed_event_attribute_on_a_local_type_is_wired_not_set_as_a_property()
	{
		const string stub = "namespace A { public class Btn : System.Windows.Controls.Button "
			+ "{ public event System.Windows.RoutedEventHandler Tap; } "
			+ "public partial class W : System.Windows.Window { void OnTap(object s, "
			+ "System.Windows.RoutedEventArgs e) { } } }";
		string code = Gen(Window("<local:Btn Tap=\"OnTap\"/>"), sources: new[] { stub });
		Assert.Matches(new Regex(@"\.Tap \+= this\.OnTap;"), code);
		Assert.DoesNotContain("SetClrProperty", code);
	}

	[Fact]
	public void LayoutUpdated_is_wired_as_a_clr_event()
	{
		const string stub = "namespace A { public partial class W : System.Windows.Window "
			+ "{ void OnLu(object s, System.EventArgs e) { } } }";
		string code = Gen(Window("<Canvas LayoutUpdated=\"OnLu\"/>"), sources: new[] { stub });
		Assert.Matches(new Regex(@"\.LayoutUpdated \+= this\.OnLu;"), code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void A_dynamic_resource_on_a_local_FrameworkElement_type_is_a_local_reference_not_FindResource()
	{
		const string stub = "namespace A { public class Pane : System.Windows.Controls.Border { "
			+ "public static readonly System.Windows.DependencyProperty AccentProperty = "
			+ "System.Windows.DependencyProperty.Register(\"Accent\", typeof(object), typeof(Pane)); } "
			+ "public partial class W : System.Windows.Window { } }";
		string code = Gen(Window("<local:Pane Accent=\"{DynamicResource k}\"/>"), sources: new[] { stub });
		Assert.Contains("(global::System.Windows.FrameworkElement)(object)__e0, global::A.Pane.AccentProperty, \"k\")",
			code);
		Assert.DoesNotContain("FindResource(\"k\")", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void Two_dynamic_resources_on_one_local_FrameworkElement_compile()
	{
		const string stub = "namespace A { public class Pane : System.Windows.Controls.Border { "
			+ "public static readonly System.Windows.DependencyProperty AccentProperty = "
			+ "System.Windows.DependencyProperty.Register(\"Accent\", typeof(object), typeof(Pane)); "
			+ "public static readonly System.Windows.DependencyProperty TintProperty = "
			+ "System.Windows.DependencyProperty.Register(\"Tint\", typeof(object), typeof(Pane)); } "
			+ "public partial class W : System.Windows.Window { } }";
		string code = Gen(Window("<local:Pane Accent=\"{DynamicResource k}\" Tint=\"{DynamicResource t}\"/>"),
			sources: new[] { stub });
		Assert.Contains("AccentProperty, \"k\")", code);
		Assert.Contains("TintProperty, \"t\")", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void An_inherited_event_on_a_local_type_is_subscribed_not_set_as_a_property()
	{
		const string stub = "namespace A { public class Btn : System.Windows.Controls.Button { } "
			+ "public partial class W : System.Windows.Window { "
			+ "void OnClick(object s, System.Windows.RoutedEventArgs e) { } } }";
		string code = Gen(Window("<local:Btn Click=\"OnClick\"/>"), sources: new[] { stub });
		Assert.Contains(".Click += this.OnClick;", code);
		Assert.DoesNotContain("SetClrProperty", code);
		AssertCompiles(code, stub);
	}

	[Fact]
	public void Dictionary_paths_that_differ_only_by_underscore_or_slash_get_distinct_classes()
	{
		const string xaml = "<ResourceDictionary " + Head + "/>";
		Gen(xaml, out string a, false, xamlPath: "A_B/C.xaml", rootNamespace: "N");
		Gen(xaml, out string b, false, xamlPath: "A/B_C.xaml", rootNamespace: "N");
		Assert.NotEqual(a, b);
	}
}
