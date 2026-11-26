using Gtk4.MarkupCompiler;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Shared fixture for the Round-15A <c>VisualStateManager.VisualStateGroups</c>
/// compiler tests: a fresh generator and a root-<c>Grid</c> XAML wrapper.
/// </summary>
public abstract class XamlCodeGeneratorVsmTestBase
{
	protected readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	protected static string Grid(string body) =>
		$@"<Grid x:Class=""MyApp.MyView""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Grid>";
}
