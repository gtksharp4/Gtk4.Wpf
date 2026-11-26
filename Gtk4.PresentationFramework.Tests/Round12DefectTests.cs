using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Themes;
using Xunit;
using WpfButton   = System.Windows.Controls.Button;
using WpfControl  = System.Windows.Controls.Control;
using WpfTextBox  = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for Round-12 defects (X1, X3, X4, X6).
/// All are pure (no GTK required).
/// </summary>
public class Round12DefectTests
{
	// ── X1: default-style applied automatically in Control() ─────────────────

	[Fact]
	public void X1_Button_no_manual_call_receives_default_style_setters()
	{
		Generic.Reset();
		// No manual ApplyDefaultStyle — the Control() ctor must do it.
		var btn = new WpfButton();
		var padding = (Thickness?)btn.GetValue(WpfControl.PaddingProperty);
		// Generic registers Padding(6,2,6,2) for Button.
		Assert.NotNull(padding);
		Assert.Equal(new Thickness(6, 2, 6, 2), padding!.Value);
	}

	[Fact]
	public void X1_local_value_wins_over_default_style()
	{
		Generic.Reset();
		var btn = new WpfButton();
		btn.SetValue(WpfControl.PaddingProperty, new Thickness(99));
		Assert.Equal(new Thickness(99), (Thickness)btn.GetValue(WpfControl.PaddingProperty)!);
	}

	[Fact]
	public void X1_OverridesDefaultStyle_suppresses_ApplyDefaultStyle()
	{
		// Tests the guard inside ApplyDefaultStyle directly.
		Generic.Reset();

		// New instance: auto-apply ran from Control() ctor.
		var btn = new WpfButton();
		var paddingWithStyle = (Thickness?)btn.GetValue(WpfControl.PaddingProperty);
		Assert.Equal(new Thickness(6, 2, 6, 2), paddingWithStyle!.Value);

		// Clear the style layer, then re-apply with OverridesDefaultStyle = true.
		var snapshot = btn.SnapshotStyleValues();
		btn.ClearStyleValues(snapshot);
		btn.OverridesDefaultStyle = true;
		btn.ApplyDefaultStyle();
		// Padding must stay at the DP metadata default (zero) — style skipped.
		var paddingAfter = btn.GetValue(WpfControl.PaddingProperty);
		Assert.NotEqual((object)new Thickness(6, 2, 6, 2), paddingAfter!);
	}

	// ── X3: focusing el2 unfocuses el1 ───────────────────────────────────────

	sealed class PureFocusElement : UIElement
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void X3_focus_el2_clears_el1_IsFocused_and_raises_LostFocus()
	{
		var el1 = new PureFocusElement();
		var el2 = new PureFocusElement();

		el1.Focus();
		Assert.True(el1.IsFocused);
		Assert.Same(el1, Keyboard.FocusedElement);

		bool lostFocusFired = false;
		el1.LostFocus += (s, e) => lostFocusFired = true;

		el2.Focus();

		Assert.False(el1.IsFocused, "el1 must lose focus when el2 is focused.");
		Assert.True(lostFocusFired,  "el1.LostFocus must fire when el2 gains focus.");
		Assert.True(el2.IsFocused,  "el2 must be focused.");
		Assert.Same(el2, Keyboard.FocusedElement);
	}

	[Fact]
	public void X3_Keyboard_FocusedElement_is_single_owner()
	{
		var el1 = new PureFocusElement();
		var el2 = new PureFocusElement();
		el1.Focus();
		el2.Focus();
		Assert.Same(el2, Keyboard.FocusedElement);
	}

	// ── X4: Button default style has no ControlTemplate setter ──────────────

	[Fact]
	public void X4_Button_default_style_has_no_ControlTemplate_setter()
	{
		Generic.Reset();
		var style = ThemeManager.FindDefaultStyle(typeof(WpfButton));
		Assert.NotNull(style);
		foreach (var setter in style!.Setters)
		{
			if (setter is Setter s && s.Property == WpfControl.TemplateProperty)
				Assert.Fail("Button default style must not contain a ControlTemplate setter.");
		}
	}

	[Fact]
	public void X4_Button_default_style_still_has_Padding_setter()
	{
		Generic.Reset();
		var style = ThemeManager.FindDefaultStyle(typeof(WpfButton));
		Assert.NotNull(style);
		bool found = false;
		foreach (var setter in style!.Setters)
		{
			if (setter is Setter s && s.Property == WpfControl.PaddingProperty)
				found = true;
		}
		Assert.True(found, "Button default style must still have a Padding setter.");
	}

	// ── X6: MaxLength truncation, SelectionStart re-clamp, SelectedText ──────

	[Fact]
	public void X6a_MaxLength_truncates_Text_on_Text_set()
	{
		var tb = new WpfTextBox { MaxLength = 5 };
		tb.Text = "hello world";
		Assert.Equal("hello", tb.Text);
	}

	[Fact]
	public void X6a_MaxLength_shrink_truncates_existing_Text()
	{
		var tb = new WpfTextBox { Text = "hello world" };
		tb.MaxLength = 5;
		Assert.Equal("hello", tb.Text);
	}

	[Fact]
	public void X6b_SelectionStart_move_reclamps_SelectionLength()
	{
		var tb = new WpfTextBox { Text = "hello" };
		tb.SelectionStart  = 2;
		tb.SelectionLength = 3; // selects "llo" (positions 2,3,4)
		Assert.Equal(3, tb.SelectionLength);

		tb.SelectionStart = 4; // only 1 char left after position 4
		Assert.Equal(1, tb.SelectionLength);
	}

	[Fact]
	public void X6c_SelectedText_setter_positions_caret_at_end_and_collapses_selection()
	{
		var tb = new WpfTextBox { Text = "hello world" };
		tb.SelectionStart  = 6;
		tb.SelectionLength = 5;
		tb.SelectedText    = "there";
		Assert.Equal("hello there", tb.Text);
		Assert.Equal(11, tb.SelectionStart);  // end of "there"
		Assert.Equal(0,  tb.SelectionLength); // selection collapsed
	}
}