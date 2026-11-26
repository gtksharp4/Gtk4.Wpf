using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// G3: attached-property attributes ("Grid.Row", "Canvas.Left", "DockPanel.Dock")
/// must emit static setter calls, not dotted property accesses.
/// G4: <see cref="XamlCodeGenerator"/> must escape newlines in string literals.
/// Each test fails without the matching fix.
/// </summary>
public class XamlCodeGeneratorAttachedPropTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";

	// ── G3: Grid.Row ──────────────────────────────────────────────────────────

	/// <summary>
	/// G3 pinning test: <c>Grid.Row="1"</c> must emit
	/// <c>System.Windows.Controls.Grid.SetRow(__eN, (int)XamlValueConverter.Convert(...))</c>.
	/// Before the fix it emitted <c>SetValue(Button.Grid.RowProperty, ...)</c> which does not compile.
	/// </summary>
	[Fact]
	public void Grid_Row_emits_SetRow_call()
	{
		string xaml = Window("<Grid><Button Grid.Row=\"1\" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Grid.SetRow(", code);
		Assert.Contains("XamlValueConverter.Convert(", code);
		// Old broken form would use SetValue with the dotted DP on the target type.
		Assert.DoesNotContain("Button.Grid.RowProperty", code);
		Assert.DoesNotContain("Button.Grid.Row", code);
	}

	[Fact]
	public void Grid_Column_emits_SetColumn_call()
	{
		string xaml = Window("<Grid><Button Grid.Column=\"2\" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Grid.SetColumn(", code);
		Assert.DoesNotContain("Button.Grid.ColumnProperty", code);
	}

	[Fact]
	public void Grid_RowSpan_emits_SetRowSpan_call()
	{
		string xaml = Window("<Grid><Button Grid.RowSpan=\"3\" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Grid.SetRowSpan(", code);
	}

	[Fact]
	public void Grid_ColumnSpan_emits_SetColumnSpan_call()
	{
		string xaml = Window("<Grid><Button Grid.ColumnSpan=\"2\" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Grid.SetColumnSpan(", code);
	}

	// ── G3: Canvas.Left ───────────────────────────────────────────────────────

	[Fact]
	public void Canvas_Left_emits_SetLeft_call()
	{
		string xaml = Window("<Canvas><Button Canvas.Left=\"5\" /></Canvas>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Canvas.SetLeft(", code);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.DoesNotContain("Button.Canvas.LeftProperty", code);
	}

	[Fact]
	public void Canvas_Top_emits_SetTop_call()
	{
		string xaml = Window("<Canvas><Button Canvas.Top=\"10\" /></Canvas>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.Canvas.SetTop(", code);
	}

	// ── G3: DockPanel.Dock ────────────────────────────────────────────────────

	[Fact]
	public void DockPanel_Dock_emits_SetDock_call()
	{
		string xaml = Window("<DockPanel><Button DockPanel.Dock=\"Top\" /></DockPanel>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Windows.Controls.DockPanel.SetDock(", code);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.DoesNotContain("Button.DockPanel.DockProperty", code);
	}

	// ── G3: int cast for Grid row ─────────────────────────────────────────────

	[Fact]
	public void Grid_Row_emits_int_cast()
	{
		string xaml = Window("<Grid><Button Grid.Row=\"1\" /></Grid>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("(int)", code);
	}

	[Fact]
	public void Canvas_Left_emits_double_cast()
	{
		string xaml = Window("<Canvas><Button Canvas.Left=\"5.5\" /></Canvas>");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("(double)", code);
	}

	// ── G4: EscapeString newlines ──────────────────────────────────────────────

	/// <summary>
	/// G4 pinning tests: <see cref="XamlCodeGenerator.EscapeString"/> must produce
	/// valid C# string-literal escape sequences for control characters.
	/// Before the fix \n, \r, \t were left raw, producing invalid C# string literals.
	/// Tests call <c>EscapeString</c> directly (internal via InternalsVisibleTo) because
	/// XML parsers normalize whitespace in attribute values before the generator sees them.
	/// </summary>
	[Fact]
	public void EscapeString_escapes_newline()
	{
		string result = XamlCodeGenerator.EscapeString("line1\nline2");
		Assert.Equal(@"line1\nline2", result);
	}

	[Fact]
	public void EscapeString_escapes_carriage_return()
	{
		string result = XamlCodeGenerator.EscapeString("val\rue");
		Assert.Equal(@"val\rue", result);
	}

	[Fact]
	public void EscapeString_escapes_tab()
	{
		string result = XamlCodeGenerator.EscapeString("col1\tcol2");
		Assert.Equal(@"col1\tcol2", result);
	}

	[Fact]
	public void EscapeString_escapes_backslash_and_quote()
	{
		// Pre-existing escapes still work.
		Assert.Equal(@"a\\b", XamlCodeGenerator.EscapeString(@"a\b"));
		Assert.Equal(@"say \""hi\""", XamlCodeGenerator.EscapeString("say \"hi\""));
	}
}