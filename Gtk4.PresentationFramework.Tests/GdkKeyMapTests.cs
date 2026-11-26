using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure unit tests for <see cref="GdkKeyMap"/> (X2).
/// No GTK runtime required — tests use the raw GDK keyval constants.
/// </summary>
public class GdkKeyMapTests
{
	// ── FromKeyval: representative letters ───────────────────────────────────

	[Theory]
	[InlineData(0x0061u, Key.A)]  // 'a'
	[InlineData(0x0041u, Key.A)]  // 'A' (uppercase)
	[InlineData(0x007au, Key.Z)]  // 'z'
	[InlineData(0x005au, Key.Z)]  // 'Z' (uppercase)
	public void FromKeyval_letters(uint keyval, Key expected)
		=> Assert.Equal(expected, GdkKeyMap.FromKeyval(keyval));

	// ── FromKeyval: digits ───────────────────────────────────────────────────

	[Theory]
	[InlineData(0x0030u, Key.D0)]
	[InlineData(0x0039u, Key.D9)]
	public void FromKeyval_digits(uint keyval, Key expected)
		=> Assert.Equal(expected, GdkKeyMap.FromKeyval(keyval));

	// ── FromKeyval: function keys ────────────────────────────────────────────

	[Theory]
	[InlineData(0xffbeu, Key.F1)]
	[InlineData(0xffc9u, Key.F12)]
	public void FromKeyval_function_keys(uint keyval, Key expected)
		=> Assert.Equal(expected, GdkKeyMap.FromKeyval(keyval));

	// ── FromKeyval: navigation / special ────────────────────────────────────

	[Theory]
	[InlineData(0xff0du, Key.Return)]   // Enter
	[InlineData(0xff1bu, Key.Escape)]   // Escape
	[InlineData(0x0020u, Key.Space)]    // Space
	[InlineData(0xff09u, Key.Tab)]      // Tab
	[InlineData(0xff08u, Key.Back)]     // Backspace
	[InlineData(0xff51u, Key.Left)]     // Left arrow
	[InlineData(0xff52u, Key.Up)]       // Up arrow
	[InlineData(0xff53u, Key.Right)]    // Right arrow
	[InlineData(0xff54u, Key.Down)]     // Down arrow
	public void FromKeyval_navigation_and_special(uint keyval, Key expected)
		=> Assert.Equal(expected, GdkKeyMap.FromKeyval(keyval));

	// ── FromKeyval: unknown returns None ────────────────────────────────────

	[Fact]
	public void FromKeyval_unknown_returns_None()
		=> Assert.Equal(Key.None, GdkKeyMap.FromKeyval(0xDEADBEEF));

	// ── FromGdkModifier: Ctrl / Shift / Alt ─────────────────────────────────

	[Fact]
	public void FromGdkModifier_Ctrl()
	{
		var mod = GdkKeyMap.FromGdkModifier(Gdk.ModifierType.ControlMask);
		Assert.Equal(ModifierKeys.Control, mod);
	}

	[Fact]
	public void FromGdkModifier_Shift()
	{
		var mod = GdkKeyMap.FromGdkModifier(Gdk.ModifierType.ShiftMask);
		Assert.Equal(ModifierKeys.Shift, mod);
	}

	[Fact]
	public void FromGdkModifier_Alt()
	{
		var mod = GdkKeyMap.FromGdkModifier(Gdk.ModifierType.AltMask);
		Assert.Equal(ModifierKeys.Alt, mod);
	}

	[Fact]
	public void FromGdkModifier_CtrlShift()
	{
		var mod = GdkKeyMap.FromGdkModifier(
			Gdk.ModifierType.ControlMask | Gdk.ModifierType.ShiftMask);
		Assert.Equal(ModifierKeys.Control | ModifierKeys.Shift, mod);
	}

	[Fact]
	public void FromGdkModifier_None_returns_None()
	{
		var mod = GdkKeyMap.FromGdkModifier(Gdk.ModifierType.None);
		Assert.Equal(ModifierKeys.None, mod);
	}
}