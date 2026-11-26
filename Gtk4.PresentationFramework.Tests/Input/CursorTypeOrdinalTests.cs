using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

public class CursorTypeOrdinalTests
{
	[Fact]
	public void Ordinals_match_wpf()
	{
		var expected = new[]
		{
			(CursorType.None, 0), (CursorType.No, 1), (CursorType.Arrow, 2), (CursorType.AppStarting, 3),
			(CursorType.Cross, 4), (CursorType.Help, 5), (CursorType.IBeam, 6), (CursorType.SizeAll, 7),
			(CursorType.SizeNESW, 8), (CursorType.SizeNS, 9), (CursorType.SizeNWSE, 10), (CursorType.SizeWE, 11),
			(CursorType.UpArrow, 12), (CursorType.Wait, 13), (CursorType.Hand, 14), (CursorType.Pen, 15),
			(CursorType.ScrollNS, 16), (CursorType.ScrollWE, 17), (CursorType.ScrollAll, 18),
			(CursorType.ScrollN, 19), (CursorType.ScrollS, 20), (CursorType.ScrollW, 21), (CursorType.ScrollE, 22),
			(CursorType.ScrollNW, 23), (CursorType.ScrollNE, 24), (CursorType.ScrollSW, 25),
			(CursorType.ScrollSE, 26), (CursorType.ArrowCD, 27),
		};
		Assert.Equal(28, expected.Length);
		foreach (var (type, ordinal) in expected) Assert.Equal(ordinal, (int)type);
	}

	[Fact]
	public void ScrollEW_is_a_hidden_alias_of_ScrollWE()
	{
		Assert.Equal(CursorType.ScrollWE, CursorType.ScrollEW);
	}
}
