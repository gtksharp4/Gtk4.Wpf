using System.Collections.Generic;
using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Keyboard-focus routed events, <c>IsKeyboardFocused</c>/<c>IsKeyboardFocusWithin</c>, and the focus move.
/// </summary>
public abstract partial class UIElement
{
	/// <summary>Tunnel event raised before keyboard focus enters this element; handling it cancels the move.</summary>
	public static readonly RoutedEvent PreviewGotKeyboardFocusEvent =
		Keyboard.PreviewGotKeyboardFocusEvent.AddOwner(typeof(UIElement));

	/// <summary>Bubble event raised after keyboard focus entered this element.</summary>
	public static readonly RoutedEvent GotKeyboardFocusEvent =
		Keyboard.GotKeyboardFocusEvent.AddOwner(typeof(UIElement));

	/// <summary>Tunnel event raised before keyboard focus leaves this element; handling it cancels the move.</summary>
	public static readonly RoutedEvent PreviewLostKeyboardFocusEvent =
		Keyboard.PreviewLostKeyboardFocusEvent.AddOwner(typeof(UIElement));

	/// <summary>Bubble event raised after keyboard focus left this element.</summary>
	public static readonly RoutedEvent LostKeyboardFocusEvent =
		Keyboard.LostKeyboardFocusEvent.AddOwner(typeof(UIElement));

	/// <summary>Occurs before keyboard focus enters this element.</summary>
	public event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus
	{
		add => AddHandler(PreviewGotKeyboardFocusEvent, value);
		remove => RemoveHandler(PreviewGotKeyboardFocusEvent, value);
	}

	/// <summary>Occurs after keyboard focus entered this element.</summary>
	public event KeyboardFocusChangedEventHandler GotKeyboardFocus
	{
		add => AddHandler(GotKeyboardFocusEvent, value);
		remove => RemoveHandler(GotKeyboardFocusEvent, value);
	}

	/// <summary>Occurs before keyboard focus leaves this element.</summary>
	public event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus
	{
		add => AddHandler(PreviewLostKeyboardFocusEvent, value);
		remove => RemoveHandler(PreviewLostKeyboardFocusEvent, value);
	}

	/// <summary>Occurs after keyboard focus left this element.</summary>
	public event KeyboardFocusChangedEventHandler LostKeyboardFocus
	{
		add => AddHandler(LostKeyboardFocusEvent, value);
		remove => RemoveHandler(LostKeyboardFocusEvent, value);
	}

	/// <summary>Called before keyboard focus enters this element (class handler).</summary>
	protected virtual void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { }

	/// <summary>Called after keyboard focus entered this element (class handler).</summary>
	protected virtual void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { }

	/// <summary>Called before keyboard focus leaves this element (class handler).</summary>
	protected virtual void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { }

	/// <summary>Called after keyboard focus left this element (class handler).</summary>
	protected virtual void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { }

	static void RegisterKeyboardFocusClassHandlers()
	{
		EventManager.RegisterClassHandler(typeof(UIElement), PreviewGotKeyboardFocusEvent,
			new KeyboardFocusChangedEventHandler((s, e) => ((UIElement)s).OnPreviewGotKeyboardFocus(e)));
		EventManager.RegisterClassHandler(typeof(UIElement), GotKeyboardFocusEvent,
			new KeyboardFocusChangedEventHandler((s, e) => ((UIElement)s).OnGotKeyboardFocus(e)));
		EventManager.RegisterClassHandler(typeof(UIElement), PreviewLostKeyboardFocusEvent,
			new KeyboardFocusChangedEventHandler((s, e) => ((UIElement)s).OnPreviewLostKeyboardFocus(e)));
		EventManager.RegisterClassHandler(typeof(UIElement), LostKeyboardFocusEvent,
			new KeyboardFocusChangedEventHandler((s, e) => ((UIElement)s).OnLostKeyboardFocus(e)));
	}

	// ── IsKeyboardFocused / IsKeyboardFocusWithin ────────────────────────────

	static readonly DependencyPropertyKey _isKeyboardFocusedKey =
		DependencyProperty.RegisterReadOnly(nameof(IsKeyboardFocused), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsKeyboardFocused"/> property.</summary>
	public static readonly DependencyProperty IsKeyboardFocusedProperty = _isKeyboardFocusedKey.DependencyProperty;

	static readonly DependencyPropertyKey _isKeyboardFocusWithinKey =
		DependencyProperty.RegisterReadOnly(nameof(IsKeyboardFocusWithin), typeof(bool), typeof(UIElement),
			new PropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsKeyboardFocusWithin"/> property.</summary>
	public static readonly DependencyProperty IsKeyboardFocusWithinProperty =
		_isKeyboardFocusWithinKey.DependencyProperty;

	/// <summary>Whether this element has keyboard focus.</summary>
	public bool IsKeyboardFocused => (bool)(GetValue(IsKeyboardFocusedProperty) ?? false);

	/// <summary>Whether keyboard focus is on this element or on an element in its subtree.</summary>
	public bool IsKeyboardFocusWithin => (bool)(GetValue(IsKeyboardFocusWithinProperty) ?? false);

	/// <summary>Occurs when <see cref="IsKeyboardFocused"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsKeyboardFocusedChanged;

	/// <summary>Occurs when <see cref="IsKeyboardFocusWithin"/> changes.</summary>
	public event DependencyPropertyChangedEventHandler? IsKeyboardFocusWithinChanged;

	void SetKeyboardFocused(bool value)
	{
		bool old = IsKeyboardFocused;
		if (old == value) return;
		SetValue(_isKeyboardFocusedKey, value);
		IsKeyboardFocusedChanged?.Invoke(this,
			new DependencyPropertyChangedEventArgs(IsKeyboardFocusedProperty, old, value));
	}

	void SetKeyboardFocusWithin(bool value)
	{
		bool old = IsKeyboardFocusWithin;
		if (old == value) return;
		SetValue(_isKeyboardFocusWithinKey, value);
		IsKeyboardFocusWithinChanged?.Invoke(this,
			new DependencyPropertyChangedEventArgs(IsKeyboardFocusWithinProperty, old, value));
	}

	static void UpdateKeyboardFocusWithin(UIElement? oldOwner, UIElement? newOwner)
	{
		var newChain = new HashSet<UIElement>(ReferenceEqualityComparer.Instance);
		for (var e = newOwner; e != null; e = e.LogicalParent) newChain.Add(e);
		for (var e = oldOwner; e != null; e = e.LogicalParent)
		{
			if (!newChain.Contains(e)) e.SetKeyboardFocusWithin(false);
		}
		for (var e = newOwner; e != null; e = e.LogicalParent) e.SetKeyboardFocusWithin(true);
	}

	// ── The focus move ───────────────────────────────────────────────────────

	/// <summary>
	/// Gives this element managed keyboard focus, or clears it. Raises the Preview*KeyboardFocus events
	/// (either can cancel the move, returning <c>false</c>), updates the focus state, then raises
	/// *KeyboardFocus and Got/LostFocus.
	/// </summary>
	internal bool SetFocusedInternal(bool focused)
	{
		if (focused == IsFocused) return true;
		var oldOwner = focused ? Keyboard.FocusedElement as UIElement : this;
		var newOwner = focused ? this : null;
		if (RaiseFocusEvent(oldOwner, newOwner, PreviewLostKeyboardFocusEvent, oldOwner)) return false;
		if (RaiseFocusEvent(oldOwner, newOwner, PreviewGotKeyboardFocusEvent, newOwner)) return false;

		if (oldOwner != null)
		{
			oldOwner.SetValue(_isFocusedKey, false);
			oldOwner.SetKeyboardFocused(false);
			Keyboard.ClearFocused(oldOwner);
		}
		if (newOwner != null)
		{
			newOwner.SetValue(_isFocusedKey, true);
			newOwner.SetKeyboardFocused(true);
			Keyboard.SetFocused(newOwner);
			FocusManager.SetFocusedElement(newOwner, newOwner);
		}
		UpdateKeyboardFocusWithin(oldOwner, newOwner);

		RaiseFocusEvent(oldOwner, newOwner, LostKeyboardFocusEvent, oldOwner);
		RaiseFocusEvent(oldOwner, newOwner, GotKeyboardFocusEvent, newOwner);
		oldOwner?.RaiseEvent(new RoutedEventArgs(LostFocusEvent));
		newOwner?.RaiseEvent(new RoutedEventArgs(GotFocusEvent));
		return true;
	}

	static bool RaiseFocusEvent(UIElement? oldOwner, UIElement? newOwner, RoutedEvent routedEvent, UIElement? target)
	{
		if (target == null) return false;
		var args = new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, Environment.TickCount, oldOwner, newOwner)
		{
			RoutedEvent = routedEvent,
		};
		target.RaiseEvent(args);
		return args.Handled;
	}
}
