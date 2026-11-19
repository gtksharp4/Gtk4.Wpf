using System;

namespace System.Windows.Controls;

public partial class ComboBox
{
	/// <summary>Default <see cref="MaxDropDownHeight"/>: WPF uses primary screen height / 3; 1080 / 3 here.</summary>
	internal const double DefaultMaxDropDownHeight = 360.0;

	/// <summary>Identifies <see cref="IsDropDownOpen"/>.</summary>
	public static readonly DependencyProperty IsDropDownOpenProperty =
		DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(ComboBox),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
				OnIsDropDownOpenChanged));

	/// <summary>
	/// Identifies <see cref="MaxDropDownHeight"/>. Store-only: GtkDropDown sizes its popover itself.
	/// </summary>
	public static readonly DependencyProperty MaxDropDownHeightProperty =
		DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(ComboBox),
			new FrameworkPropertyMetadata(DefaultMaxDropDownHeight));

	/// <summary>Raised when the drop-down opens.</summary>
	public event EventHandler? DropDownOpened;

	/// <summary>Raised when the drop-down closes.</summary>
	public event EventHandler? DropDownClosed;

	/// <summary>
	/// Gets or sets whether the drop-down is open. Store-only: GtkSharp 4 has no safe way to open or
	/// close <see cref="Gtk.DropDown"/> from code (its popover is internal), so only the property and
	/// <see cref="DropDownOpened"/>/<see cref="DropDownClosed"/> are driven.
	/// </summary>
	public bool IsDropDownOpen
	{
		get => (bool)GetValue(IsDropDownOpenProperty)!;
		set => SetValue(IsDropDownOpenProperty, value);
	}

	/// <summary>Gets or sets the maximum drop-down height (store-only on GTK).</summary>
	public double MaxDropDownHeight
	{
		get => (double)GetValue(MaxDropDownHeightProperty)!;
		set => SetValue(MaxDropDownHeightProperty, value);
	}

	/// <summary>Raises <see cref="DropDownOpened"/>.</summary>
	protected virtual void OnDropDownOpened(EventArgs e) => DropDownOpened?.Invoke(this, e);

	/// <summary>Raises <see cref="DropDownClosed"/>.</summary>
	protected virtual void OnDropDownClosed(EventArgs e) => DropDownClosed?.Invoke(this, e);

	static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cb = (ComboBox)d;
		var open = (bool)e.NewValue!;
		if (open) cb.OnDropDownOpened(EventArgs.Empty);
		else cb.OnDropDownClosed(EventArgs.Empty);
	}
}
