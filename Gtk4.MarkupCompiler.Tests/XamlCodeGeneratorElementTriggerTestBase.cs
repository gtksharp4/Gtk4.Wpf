using Gtk4.MarkupCompiler;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Shared fixture for the element-level trigger compiler tests
/// (<c>&lt;Button.Triggers&gt;</c> / <c>&lt;FrameworkElement.Triggers&gt;</c>):
/// a fresh generator and a root-<c>Window</c> XAML wrapper.
/// </summary>
public abstract class XamlCodeGeneratorElementTriggerTestBase
{
	protected readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	protected static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	{body}
</Window>";
}
