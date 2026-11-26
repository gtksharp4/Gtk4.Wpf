using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Pure tests for the <c>FrameworkElement.Cursor</c> DP and
/// <see cref="Mouse.OverrideCursor"/>. No GTK runtime required.
/// </summary>
public class CursorElementTests
{
	// ── FrameworkElement.Cursor DP ────────────────────────────────────────────

	[Fact]
	public void FrameworkElement_Cursor_default_is_null()
	{
		var el = new TextBlock();
		Assert.Null(el.Cursor);
	}

	[Fact]
	public void FrameworkElement_Cursor_set_get_round_trip()
	{
		var el = new TextBlock();
		el.Cursor = Cursors.Hand;
		Assert.Same(Cursors.Hand, el.Cursor);
	}

	[Fact]
	public void FrameworkElement_Cursor_set_null_clears()
	{
		var el = new TextBlock();
		el.Cursor = Cursors.Wait;
		el.Cursor = null;
		Assert.Null(el.Cursor);
	}

	[Fact]
	public void FrameworkElement_Cursor_set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		// No widget materialised yet.
		var ex = Record.Exception(() => el.Cursor = Cursors.IBeam);
		Assert.Null(ex);
		Assert.Equal("text", el.Cursor!.GdkName);
	}

	[Fact]
	public void FrameworkElement_Cursor_persists_across_widget_creation()
	{
		var el = new TextBlock();
		el.Cursor = Cursors.Cross;
		// DP value must be intact regardless of widget state.
		Assert.Equal("crosshair", el.Cursor!.GdkName);
	}

	// ── Mouse.OverrideCursor ──────────────────────────────────────────────────

	[Fact]
	public void Mouse_OverrideCursor_default_is_null()
	{
		Mouse.OverrideCursor = null; // reset first to isolate from other tests
		Assert.Null(Mouse.OverrideCursor);
	}

	[Fact]
	public void Mouse_OverrideCursor_set_get_round_trip()
	{
		var prev = Mouse.OverrideCursor;
		try
		{
			Mouse.OverrideCursor = Cursors.Wait;
			Assert.Same(Cursors.Wait, Mouse.OverrideCursor);
		}
		finally
		{
			Mouse.OverrideCursor = prev;
		}
	}

	[Fact]
	public void Mouse_OverrideCursor_set_null_clears()
	{
		Mouse.OverrideCursor = Cursors.Hand;
		Mouse.OverrideCursor = null;
		Assert.Null(Mouse.OverrideCursor);
	}
}
