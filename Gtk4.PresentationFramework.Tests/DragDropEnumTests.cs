using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="DragDropEffects"/> and <see cref="DragDropKeyStates"/> enumerations.
/// </summary>
public sealed class DragDropEnumTests
{
	// ── DragDropEffects ───────────────────────────────────────────────────────

	[Fact]
	public void DragDropEffects_None_is_zero()
	{
		Assert.Equal(0, (int)DragDropEffects.None);
	}

	[Fact]
	public void DragDropEffects_flags_combine()
	{
		var combined = DragDropEffects.Copy | DragDropEffects.Move;
		Assert.True((combined & DragDropEffects.Copy) != 0);
		Assert.True((combined & DragDropEffects.Move) != 0);
		Assert.True((combined & DragDropEffects.Link) == 0);
	}

	[Fact]
	public void DragDropEffects_All_contains_Copy_Move_Link()
	{
		Assert.True((DragDropEffects.All & DragDropEffects.Copy) != 0);
		Assert.True((DragDropEffects.All & DragDropEffects.Move) != 0);
		Assert.True((DragDropEffects.All & DragDropEffects.Link) != 0);
	}

	[Fact]
	public void DragDropEffects_All_contains_Scroll()
	{
		Assert.True((DragDropEffects.All & DragDropEffects.Scroll) != 0);
	}

	[Fact]
	public void DragDropEffects_distinct_bit_values()
	{
		Assert.NotEqual(DragDropEffects.Copy, DragDropEffects.Move);
		Assert.NotEqual(DragDropEffects.Move, DragDropEffects.Link);
		Assert.NotEqual(DragDropEffects.Copy, DragDropEffects.Link);
	}

	// ── DragDropKeyStates ─────────────────────────────────────────────────────

	[Fact]
	public void DragDropKeyStates_None_is_zero()
	{
		Assert.Equal(0, (int)DragDropKeyStates.None);
	}

	[Fact]
	public void DragDropKeyStates_flags_combine()
	{
		var ks = DragDropKeyStates.LeftMouseButton | DragDropKeyStates.ControlKey;
		Assert.True((ks & DragDropKeyStates.LeftMouseButton) != 0);
		Assert.True((ks & DragDropKeyStates.ControlKey) != 0);
		Assert.True((ks & DragDropKeyStates.ShiftKey) == 0);
	}

	[Fact]
	public void DragDropKeyStates_distinct_bit_values()
	{
		Assert.NotEqual(DragDropKeyStates.LeftMouseButton, DragDropKeyStates.RightMouseButton);
		Assert.NotEqual(DragDropKeyStates.ShiftKey, DragDropKeyStates.ControlKey);
		Assert.NotEqual(DragDropKeyStates.ControlKey, DragDropKeyStates.AltKey);
	}
}
