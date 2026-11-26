using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Pure unit tests for <see cref="CursorMap.ToGdkName"/>. No GTK runtime required.
/// </summary>
public class CursorMapTests
{
	[Theory]
	[InlineData(CursorType.Arrow,       "default")]
	[InlineData(CursorType.AppStarting, "progress")]
	[InlineData(CursorType.Cross,       "crosshair")]
	[InlineData(CursorType.Hand,        "pointer")]
	[InlineData(CursorType.Help,        "help")]
	[InlineData(CursorType.IBeam,       "text")]
	[InlineData(CursorType.No,          "not-allowed")]
	[InlineData(CursorType.SizeAll,     "move")]
	[InlineData(CursorType.SizeNESW,    "nesw-resize")]
	[InlineData(CursorType.SizeNS,      "ns-resize")]
	[InlineData(CursorType.SizeNWSE,    "nwse-resize")]
	[InlineData(CursorType.SizeWE,      "ew-resize")]
	[InlineData(CursorType.Wait,        "wait")]
	[InlineData(CursorType.None,        "none")]
	[InlineData(CursorType.UpArrow,     "n-resize")]
	[InlineData(CursorType.Pen,         "cell")]
	[InlineData(CursorType.ScrollAll,   "all-scroll")]
	[InlineData(CursorType.ScrollN,     "n-resize")]
	[InlineData(CursorType.ScrollS,     "s-resize")]
	[InlineData(CursorType.ScrollE,     "e-resize")]
	[InlineData(CursorType.ScrollW,     "w-resize")]
	[InlineData(CursorType.ScrollNS,    "ns-resize")]
	[InlineData(CursorType.ScrollEW,    "ew-resize")]
	[InlineData(CursorType.ScrollNE,    "ne-resize")]
	[InlineData(CursorType.ScrollNW,    "nw-resize")]
	[InlineData(CursorType.ScrollSE,    "se-resize")]
	[InlineData(CursorType.ScrollSW,    "sw-resize")]
	[InlineData(CursorType.ArrowCD,     "no-drop")]
	public void ToGdkName_returns_expected_css_name(CursorType type, string expected)
	{
		Assert.Equal(expected, CursorMap.ToGdkName(type));
	}

	[Fact]
	public void ToGdkName_unknown_value_returns_default()
	{
		// An unrecognised future enum value must not throw.
		string name = CursorMap.ToGdkName((CursorType)999);
		Assert.Equal("default", name);
	}
}
