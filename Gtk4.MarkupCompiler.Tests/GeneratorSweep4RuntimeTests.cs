using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Sweep IV generator output that is compiled and run, not only matched as text.</summary>
public class GeneratorSweep4RuntimeTests : Sweep3Base
{
	static readonly string Bin = Path.GetDirectoryName(typeof(GeneratorSweep4RuntimeTests).Assembly.Location)!;

	static string Root(string body) => "<StackPanel " + Head + " x:Class=\"App.C\">" + body + "</StackPanel>";

	static object Prop(object o, string name) => o.GetType().GetProperty(name)!.GetValue(o)!;

	// Compiles the generated code with the stub, loads it and runs InitializeComponent on a new App.C.
	static object Run(string xaml)
	{
		string code = Gen(xaml, sources: new[] { Stub });
		var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase))
			.Concat(Directory.GetFiles(Bin, "*.dll").Where(f => Path.GetFileName(f).StartsWith("Gtk4.P")
				|| Path.GetFileName(f).StartsWith("Gtk4.WindowsBase")
				|| Path.GetFileName(f).EndsWith("Sharp.dll")))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f));
		var ct = TestContext.Current.CancellationToken;
		using var ms = new MemoryStream();
		var result = CSharpCompilation.Create("g" + Guid.NewGuid().ToString("N"),
				new[] { CSharpSyntaxTree.ParseText(code, cancellationToken: ct),
					CSharpSyntaxTree.ParseText(Stub, cancellationToken: ct) }, refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)).Emit(ms, cancellationToken: ct);
		Assert.True(result.Success, string.Join("; ", result.Diagnostics) + "\n" + code);
		ms.Position = 0;
		foreach (string layer in new[] { "Gtk4.WindowsBase.dll", "Gtk4.PresentationFramework.dll" })
			AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Bin, layer));
		Assembly asm = AssemblyLoadContext.Default.LoadFromStream(ms);
		Type type = asm.GetType("App.C")!;
		object root = Activator.CreateInstance(type)!;
		type.GetMethod("InitializeComponent")!.Invoke(root, null);
		return root;
	}

	[Fact]
	public void Object_typed_literal_is_emitted_and_runs_as_a_string()
	{
		string xaml = Root("<ComboBoxItem Tag=\"1\"/>");
		Has(Gen(xaml, sources: new[] { Stub }), ".Tag = \"1\";");

		object panel = Run(xaml);
		var children = (System.Collections.IEnumerable)Prop(panel, "Children");
		object item = Assert.Single(children.Cast<object>());

		Assert.Equal("ComboBoxItem", item.GetType().Name);
		object tag = Prop(item, "Tag");
		Assert.True(tag is string s && s == "1");
	}

	// Text-only plus AssertCompiles: the layer has no object key frames, so the generator drops the animation.
	[Fact]
	public void Object_key_frames_are_not_emitted_so_the_literal_rule_cannot_reach_them()
	{
		string with = Gen(Page("<Button Name=\"b\"/>", "<Storyboard x:Key=\"sb\"><ObjectAnimationUsingKeyFrames "
			+ "Storyboard.TargetName=\"b\" Storyboard.TargetProperty=\"Tag\"><DiscreteObjectKeyFrame KeyTime=\"0\" "
			+ "Value=\"5\"/></ObjectAnimationUsingKeyFrames></Storyboard>"), sources: new[] { Stub });
		string without = Gen(Page("<Button Name=\"b\"/>", "<Storyboard x:Key=\"sb\"><ObjectAnimationUsingKeyFrames "
			+ "Storyboard.TargetName=\"b\" Storyboard.TargetProperty=\"Tag\"><DiscreteObjectKeyFrame KeyTime=\"0\"/>"
			+ "</ObjectAnimationUsingKeyFrames></Storyboard>"), sources: new[] { Stub });

		Lacks(with, "ObjectKeyFrame");
		Assert.Equal(without, with);
		AssertCompiles(with, Stub);
	}

	[Fact]
	public void A_Run_with_TextEffects_runs_InitializeComponent_and_keeps_the_effect()
	{
		object panel = Run(Root("<TextBlock><Run><Run.TextEffects><TextEffect PositionCount=\"1\"/>"
			+ "</Run.TextEffects></Run></TextBlock>"));
		object block = ((System.Collections.IEnumerable)Prop(panel, "Children")).Cast<object>().Single();
		object run = ((System.Collections.IEnumerable)Prop(block, "Inlines")).Cast<object>().Single();

		Assert.Single(((System.Collections.IEnumerable)Prop(run, "TextEffects")).Cast<object>());
	}

	[Fact]
	public void A_Paragraph_with_TextEffects_runs_InitializeComponent_and_keeps_the_effect()
	{
		object doc = Run("<FlowDocument " + Head + " x:Class=\"App.C\"><Paragraph><Paragraph.TextEffects>"
			+ "<TextEffect PositionCount=\"1\"/></Paragraph.TextEffects></Paragraph></FlowDocument>");
		object paragraph = ((System.Collections.IEnumerable)Prop(doc, "Blocks")).Cast<object>().Single();

		Assert.Single(((System.Collections.IEnumerable)Prop(paragraph, "TextEffects")).Cast<object>());
	}

	[Fact]
	public void TextEffect_transform_name_resolves_to_the_RotateTransform()
	{
		object panel = Run(Root("<TextBlock><TextBlock.TextEffects>"
			+ "<TextEffect PositionCount=\"1\"><TextEffect.Transform>"
			+ "<RotateTransform x:Name=\"TextEffectRotateTransform\" Angle=\"0\"/></TextEffect.Transform>"
			+ "</TextEffect></TextBlock.TextEffects></TextBlock>"));
		object found = panel.GetType().GetMethod("FindName", new[] { typeof(string) })!
			.Invoke(panel, new object[] { "TextEffectRotateTransform" })!;

		Assert.Equal("RotateTransform", found.GetType().Name);
	}
}
