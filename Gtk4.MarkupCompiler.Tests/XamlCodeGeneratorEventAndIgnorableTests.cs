using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Codegen for CLR events (wired with <c>+=</c>, not assigned) and markup-compatibility
/// attributes (<c>mc:Ignorable</c> and the design <c>d:</c> namespace are stripped).
/// </summary>
public class XamlCodeGeneratorEventAndIgnorableTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	const string RootOpen =
		@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
	xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
	mc:Ignorable=""d""";

	// ── Events ───────────────────────────────────────────────────────────────

	[Fact]
	public void ElementEvent_WiredWithPlusEquals()
	{
		string xaml = RootOpen + @">
	<Button x:Name=""Go"" Click=""Go_Click"" />
</Window>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains(".Click += this.Go_Click;", code);
		Assert.DoesNotContain("Click = \"", code);
	}

	[Fact]
	public void RootEvent_WiredWithPlusEquals()
	{
		string xaml = RootOpen + @" Loaded=""Window_Loaded"">
</Window>";

		string code = _gen.Generate(xaml, out _);

		Assert.Contains("this.Loaded += this.Window_Loaded;", code);
		Assert.DoesNotContain("Loaded = \"", code);
	}

	// ── Markup compatibility ───────────────────────────────────────────────────

	[Fact]
	public void McIgnorable_IsStripped()
	{
		string xaml = RootOpen + @">
</Window>";

		string code = _gen.Generate(xaml, out _);

		// mc:Ignorable="d" must not become this.Ignorable = "d".
		Assert.DoesNotContain("Ignorable", code);
	}

	[Fact]
	public void DesignNamespaceAttributes_AreStripped()
	{
		string xaml = RootOpen + @" d:DesignHeight=""300"" d:DesignWidth=""400"">
	<Button x:Name=""Go"" d:IsHidden=""true"" Click=""Go_Click"" />
</Window>";

		string code = _gen.Generate(xaml, out _);

		Assert.DoesNotContain("DesignHeight", code);
		Assert.DoesNotContain("DesignWidth", code);
		Assert.DoesNotContain("IsHidden", code);
		// The real event on the same element is still wired.
		Assert.Contains(".Click += this.Go_Click;", code);
	}
}