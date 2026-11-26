using System;
using System.Windows.Input;

namespace System.Windows.Controls.Primitives;

/// <summary>
/// Base of <see cref="Button"/>, <see cref="RepeatButton"/> and <see cref="ToggleButton"/>: the
/// routed <see cref="Click"/> event, <see cref="ClickMode"/> and the command members
/// (<c>ButtonBase.Command.cs</c>). Subclasses create the GTK widget and call
/// <see cref="OnClick"/> when it is clicked.
/// </summary>
public abstract partial class ButtonBase : ContentControl, ICommandSource, IDisposable
{
	/// <summary>Identifies the <see cref="Click"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent ClickEvent =
		EventManager.RegisterRoutedEvent(
			nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ButtonBase));

	/// <summary>Occurs when the button is clicked.</summary>
	public event RoutedEventHandler Click
	{
		add => AddHandler(ClickEvent, value);
		remove => RemoveHandler(ClickEvent, value);
	}

	/// <summary>Identifies the <see cref="ClickMode"/> property.</summary>
	/// <remarks>Stored only: GTK buttons always click on release.</remarks>
	public static readonly DependencyProperty ClickModeProperty =
		DependencyProperty.Register(
			nameof(ClickMode), typeof(ClickMode), typeof(ButtonBase),
			new PropertyMetadata(ClickMode.Release),
			v => v is ClickMode m && m >= ClickMode.Release && m <= ClickMode.Hover);

	/// <summary>Gets or sets when <see cref="Click"/> is raised.</summary>
	public ClickMode ClickMode
	{
		get => (ClickMode)GetValue(ClickModeProperty)!;
		set => SetValue(ClickModeProperty, value);
	}

	/// <summary>Raises <see cref="Click"/>, then executes <see cref="Command"/>.</summary>
	protected virtual void OnClick()
	{
		RaiseEvent(new RoutedEventArgs(ClickEvent) { Source = this });
		ExecuteCommandIfAble();
	}

	/// <inheritdoc/>
	protected override Gtk.Box ContentContainer =>
		throw new InvalidOperationException(GetType().Name + " manages its own content.");

	/// <inheritdoc/>
	protected override Gtk.Label ContentLabel =>
		throw new InvalidOperationException(GetType().Name + " manages its own content.");
}
