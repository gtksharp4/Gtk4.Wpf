using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>
/// A menu-item control. Backed by a <see cref="Gtk.Button"/> for leaf items and by a
/// <see cref="Gtk.MenuButton"/> + <see cref="Gtk.Popover"/> for items with sub-items
/// (<c>Gtk.PopoverMenu</c> integration is a future phase).
/// Implements <see cref="ICommandSource"/>; activating fires <see cref="Click"/> and
/// invokes <see cref="Command"/>. Command members live in <c>MenuItem.Commanding.cs</c>.
/// </summary>
/// <remarks>
/// GTK-widget choice: leaf items use <c>Gtk.Button</c>; parent items use
/// <c>Gtk.MenuButton</c> that opens a <c>Gtk.Popover</c> containing child buttons.
/// A full <c>Gtk.PopoverMenu</c> menu-model integration is deferred.
/// </remarks>
public partial class MenuItem : Control, ICommandSource, IDisposable
{
	// ── Header DP ─────────────────────────────────────────────────────────────

	/// <summary>Display text shown as the menu item label.</summary>
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(
			nameof(Header),
			typeof(object),
			typeof(MenuItem),
			new PropertyMetadata(null, OnHeaderChanged));

	static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var mi = (MenuItem)d;
		var text = mi.HeaderMnemonic();
		if (mi._gtkButton != null) mi._gtkButton.Label = text;
		if (mi._gtkMenuButton != null) mi._gtkMenuButton.Label = text;
	}

	/// <summary>Gets or sets the item header (typically a string).</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	// ── IsCheckable / IsChecked DPs ───────────────────────────────────────────

	/// <summary><c>true</c> when this item acts as a toggle.</summary>
	public static readonly DependencyProperty IsCheckableProperty =
		DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(MenuItem),
			new PropertyMetadata(false));

	/// <summary>Gets or sets whether this item can be checked.</summary>
	public bool IsCheckable
	{
		get => (bool?)GetValue(IsCheckableProperty) == true;
		set => SetValue(IsCheckableProperty, value);
	}

	/// <summary>Checked state; relevant when <see cref="IsCheckable"/> is <c>true</c>.</summary>
	public static readonly DependencyProperty IsCheckedProperty =
		DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(MenuItem),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsCheckedChanged));

	static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var mi = (MenuItem)d;
		bool nowChecked = e.NewValue is true;
		if (nowChecked)
			mi.OnChecked(new RoutedEventArgs(CheckedEvent) { Source = mi });
		else
			mi.OnUnchecked(new RoutedEventArgs(UncheckedEvent) { Source = mi });
	}

	/// <summary>Gets or sets the checked state.</summary>
	public bool IsChecked
	{
		get => (bool?)GetValue(IsCheckedProperty) == true;
		set => SetValue(IsCheckedProperty, value);
	}

	// ── InputGestureText DP ───────────────────────────────────────────────────

	/// <summary>Display text for the keyboard shortcut (e.g. "Ctrl+S").</summary>
	public static readonly DependencyProperty InputGestureTextProperty =
		DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(MenuItem),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the input gesture text shown at the right of the item.</summary>
	public string? InputGestureText
	{
		get => (string?)GetValue(InputGestureTextProperty);
		set => SetValue(InputGestureTextProperty, value);
	}

	// ── Items collection (sub-items) ──────────────────────────────────────────

	/// <summary>
	/// Sub-menu entries. A <see cref="MenuItem"/> is rendered as an item and a <see cref="Separator"/> as a
	/// section break; other objects are stored only.
	/// </summary>
	public IList<object> Items { get; } = new List<object>();

	// ── GTK backing ───────────────────────────────────────────────────────────

	Gtk.Button? _gtkButton;
	Gtk.MenuButton? _gtkMenuButton;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		var label = HeaderMnemonic();
		if (Items.OfType<MenuItem>().Any())
		{
			_gtkMenuButton = new Gtk.MenuButton { UseUnderline = true, Label = label };
			var popover = new Gtk.Popover();
			var box = new Gtk.Box(Gtk.Orientation.Vertical, 2);
			foreach (var child in Items)
			{
				if (child is MenuItem item)
				{
					item.LogicalParent = this;
					box.Add(item.Widget);
				}
				else if (child is Separator separator)
				{
					separator.LogicalParent = this;
					box.Add(separator.Widget);
				}
			}
			popover.Child = box;
			_gtkMenuButton.Popover = popover;
			return _gtkMenuButton;
		}

		_gtkButton = new Gtk.Button { UseUnderline = true, Label = label };
		_gtkButton.Clicked += OnGtkClicked;
		return _gtkButton;
	}

	void OnGtkClicked(object? sender, EventArgs e) => Activate();

	/// <summary>Activates the item: fires <see cref="Click"/> and toggles check if applicable.</summary>
	internal void InvokeClick_ForTest() => Activate();

	void Activate()
	{
		if (IsCheckable)
		{
			// Toggle; OnIsCheckedChanged raises Checked or Unchecked.
			SetValue(IsCheckedProperty, !IsChecked);
		}
		OnClick();
	}
}