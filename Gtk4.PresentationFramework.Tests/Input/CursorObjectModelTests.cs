using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Pure object-model tests for <see cref="Cursor"/> and the <see cref="Cursors"/>
/// standard-cursor accessors. No GTK runtime required.
/// (Element + <see cref="Mouse.OverrideCursor"/> tests live in <c>CursorElementTests</c>.)
/// </summary>
public class CursorObjectModelTests
{
	// ── Cursor ────────────────────────────────────────────────────────────────

	[Fact]
	public void Cursor_internal_ctor_stores_GdkName()
	{
		var c = new Cursor(CursorType.Hand);
		Assert.Equal("pointer", c.GdkName);
	}

	[Fact]
	public void Cursor_file_ctor_does_not_throw()
	{
		var ex = Record.Exception(() => _ = new Cursor("some.cur"));
		Assert.Null(ex);
	}

	[Fact]
	public void Cursor_stream_ctor_does_not_throw()
	{
		using var ms = new System.IO.MemoryStream();
		var ex = Record.Exception(() => _ = new Cursor(ms));
		Assert.Null(ex);
	}

	[Fact]
	public void Cursor_file_ctor_falls_back_to_default()
	{
		var c = new Cursor("some.cur");
		Assert.Equal("default", c.GdkName);
	}

	[Fact]
	public void Cursor_ToString_returns_GdkName()
	{
		var c = new Cursor(CursorType.IBeam);
		Assert.Equal("text", c.ToString());
	}

	// ── Cursors static properties ──────────────────────────────────────────

	[Fact]
	public void Cursors_Arrow_is_nonnull_with_default_name()
	{
		Assert.NotNull(Cursors.Arrow);
		Assert.Equal("default", Cursors.Arrow.GdkName);
	}

	[Fact]
	public void Cursors_Hand_is_nonnull_with_pointer_name()
	{
		Assert.NotNull(Cursors.Hand);
		Assert.Equal("pointer", Cursors.Hand.GdkName);
	}

	[Fact]
	public void Cursors_IBeam_is_nonnull_with_text_name()
	{
		Assert.NotNull(Cursors.IBeam);
		Assert.Equal("text", Cursors.IBeam.GdkName);
	}

	[Fact]
	public void Cursors_Wait_is_nonnull_with_wait_name()
	{
		Assert.NotNull(Cursors.Wait);
		Assert.Equal("wait", Cursors.Wait.GdkName);
	}

	[Fact]
	public void Cursors_Cross_is_nonnull_with_crosshair_name()
	{
		Assert.NotNull(Cursors.Cross);
		Assert.Equal("crosshair", Cursors.Cross.GdkName);
	}

	[Fact]
	public void Cursors_No_is_nonnull_with_not_allowed_name()
	{
		Assert.NotNull(Cursors.No);
		Assert.Equal("not-allowed", Cursors.No.GdkName);
	}

	[Fact]
	public void Cursors_None_is_nonnull_with_none_name()
	{
		Assert.NotNull(Cursors.None);
		Assert.Equal("none", Cursors.None.GdkName);
	}

	[Fact]
	public void Cursors_Help_is_nonnull_with_help_name()
	{
		Assert.NotNull(Cursors.Help);
		Assert.Equal("help", Cursors.Help.GdkName);
	}

	[Fact]
	public void Cursors_AppStarting_is_nonnull_with_progress_name()
	{
		Assert.NotNull(Cursors.AppStarting);
		Assert.Equal("progress", Cursors.AppStarting.GdkName);
	}

	[Fact]
	public void Cursors_SizeAll_is_nonnull_with_move_name()
	{
		Assert.NotNull(Cursors.SizeAll);
		Assert.Equal("move", Cursors.SizeAll.GdkName);
	}

	[Fact]
	public void Cursors_SizeNS_is_nonnull_with_ns_resize_name()
	{
		Assert.NotNull(Cursors.SizeNS);
		Assert.Equal("ns-resize", Cursors.SizeNS.GdkName);
	}

	[Fact]
	public void Cursors_SizeWE_is_nonnull_with_ew_resize_name()
	{
		Assert.NotNull(Cursors.SizeWE);
		Assert.Equal("ew-resize", Cursors.SizeWE.GdkName);
	}

	[Fact]
	public void Cursors_SizeNWSE_is_nonnull_with_nwse_resize_name()
	{
		Assert.NotNull(Cursors.SizeNWSE);
		Assert.Equal("nwse-resize", Cursors.SizeNWSE.GdkName);
	}

	[Fact]
	public void Cursors_SizeNESW_is_nonnull_with_nesw_resize_name()
	{
		Assert.NotNull(Cursors.SizeNESW);
		Assert.Equal("nesw-resize", Cursors.SizeNESW.GdkName);
	}
}
