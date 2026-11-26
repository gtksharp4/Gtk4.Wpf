using System;
using System.Windows;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for the WPF input system:
/// focus DPs, keyboard/mouse routed-event tunnel+bubble, Handled stops routing,
/// KeyBinding dispatch, Keyboard.Modifiers / FocusManager.
/// All tests use the internal raise seam — no live GTK widget needed.
/// </summary>
public class InputRoutedEventTests
{
	// ── Test-only UIElement subclass ───────────────────────────────────────────

	sealed class TestElement : UIElement
	{
		public new UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}

	// ── Focus DPs ─────────────────────────────────────────────────────────────

	[Fact]
	public void Focusable_default_is_true()
	{
		var el = new TestElement();
		Assert.True(el.Focusable);
	}

	[Fact]
	public void Focus_sets_IsFocused_and_IsKeyboardFocused()
	{
		var el = new TestElement();
		el.Focus();
		Assert.True(el.IsFocused);
		Assert.True(el.IsKeyboardFocused);
	}

	[Fact]
	public void Focus_raises_GotFocus()
	{
		var el = new TestElement();
		bool fired = false;
		el.GotFocus += (s, e) => fired = true;
		el.Focus();
		Assert.True(fired);
	}

	[Fact]
	public void SetFocusedInternal_false_raises_LostFocus()
	{
		var el = new TestElement();
		el.Focus();
		bool fired = false;
		el.LostFocus += (s, e) => fired = true;
		el.SetFocusedInternal(false);
		Assert.False(el.IsFocused);
		Assert.True(fired);
	}

	[Fact]
	public void Keyboard_FocusedElement_tracks_focused_element()
	{
		var el = new TestElement();
		el.Focus();
		Assert.Same(el, Keyboard.FocusedElement);
	}

	[Fact]
	public void FocusManager_GetFocusedElement_returns_element_after_Focus()
	{
		var el = new TestElement();
		el.Focus();
		Assert.Same(el, FocusManager.GetFocusedElement(el));
	}

	// ── KeyDown bubble ────────────────────────────────────────────────────────

	[Fact]
	public void KeyDown_bubbles_from_child_to_parent()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		Key received = Key.None;
		parent.KeyDown += (s, e) => received = e.Key;

		child.RaiseKeyDownForTest(Key.A);

		Assert.Equal(Key.A, received);
	}

	[Fact]
	public void PreviewKeyDown_tunnels_parent_to_child_before_KeyDown()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		var order  = new System.Collections.Generic.List<string>();

		parent.AddHandler(UIElement.PreviewKeyDownEvent,
			new KeyEventHandler((s, e) => order.Add("preview-parent")));
		child.AddHandler(UIElement.PreviewKeyDownEvent,
			new KeyEventHandler((s, e) => order.Add("preview-child")));
		child.KeyDown += (s, e) => order.Add("keydown-child");
		parent.KeyDown += (s, e) => order.Add("keydown-parent");

		child.RaiseKeyDownForTest(Key.S);

		// Tunnel: parent-preview → child-preview; then bubble: child-keydown → parent-keydown
		Assert.Equal(new[] { "preview-parent", "preview-child", "keydown-child", "keydown-parent" }, order);
	}

	[Fact]
	public void Handled_true_stops_KeyDown_bubble()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool parentFired = false;

		child.KeyDown += (s, e) => e.Handled = true;
		parent.KeyDown += (s, e) => parentFired = true;

		child.RaiseKeyDownForTest(Key.A);

		Assert.False(parentFired);
	}

	[Fact]
	public void PreviewKeyDown_Handled_stops_KeyDown()
	{
		var parent = new TestElement();
		var child  = new TestElement { LogicalParent = parent };
		bool keyDownFired = false;

		parent.AddHandler(UIElement.PreviewKeyDownEvent,
			new KeyEventHandler((s, e) => e.Handled = true));
		child.KeyDown += (s, e) => keyDownFired = true;

		child.RaiseKeyDownForTest(Key.A);

		Assert.False(keyDownFired);
	}

	// ── Keyboard statics ──────────────────────────────────────────────────────

	[Fact]
	public void Keyboard_Modifiers_updated_by_raise_seam()
	{
		var el = new TestElement();
		el.RaiseKeyDownForTest(Key.S, ModifierKeys.Control);
		Assert.Equal(ModifierKeys.Control, Keyboard.Modifiers);
	}

	[Fact]
	public void Keyboard_IsKeyDown_true_during_handler()
	{
		var el = new TestElement();
		bool isDown = false;
		el.KeyDown += (s, e) => isDown = Keyboard.IsKeyDown(Key.S);
		el.RaiseKeyDownForTest(Key.S);
		Assert.True(isDown);
	}

	// ── KeyBinding dispatch ───────────────────────────────────────────────────

	[Fact]
	public void KeyBinding_CtrlS_invokes_command_on_KeyDown()
	{
		var el      = new TestElement();
		int count   = 0;
		var command = new RelayCommand(_ => count++);
		el.InputBindings.Add(new KeyBinding(command, Key.S, ModifierKeys.Control));

		el.RaiseKeyDownForTest(Key.S, ModifierKeys.Control);

		Assert.Equal(1, count);
	}

	[Fact]
	public void KeyBinding_not_invoked_for_wrong_modifier()
	{
		var el      = new TestElement();
		int count   = 0;
		var command = new RelayCommand(_ => count++);
		el.InputBindings.Add(new KeyBinding(command, Key.S, ModifierKeys.Control));

		el.RaiseKeyDownForTest(Key.S, ModifierKeys.None);

		Assert.Equal(0, count);
	}

	// ── Helper ───────────────────────────────────────────────────────────────

	sealed class RelayCommand : System.Windows.Input.ICommand
	{
		readonly Action<object?> _execute;
		readonly Func<object?, bool> _canExecute;

		internal RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
		{
			_execute    = execute;
			_canExecute = canExecute ?? (_ => true);
		}

		public bool CanExecute(object? parameter) => _canExecute(parameter);
		public void Execute(object? parameter)    => _execute(parameter);
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
	}
}