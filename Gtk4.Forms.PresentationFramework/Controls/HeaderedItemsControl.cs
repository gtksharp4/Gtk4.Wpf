namespace System.Windows.Controls;

/// <summary>
/// Base class for items controls that also carry a <see cref="Header"/>
/// (e.g. <see cref="ToolBar"/>, <see cref="TreeViewItem"/>).
/// Extends <see cref="ItemsControl"/> with a single <see cref="Header"/> DP.
/// </summary>
public abstract partial class HeaderedItemsControl : ItemsControl
{
	// ── Header DP ────────────────────────────────────────────────────────────

	/// <summary>The header of this control (a <see cref="UIElement"/> or a string).</summary>
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(
			nameof(Header),
			typeof(object),
			typeof(HeaderedItemsControl),
			new PropertyMetadata(null, OnHeaderChanged));

	static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedItemsControl)d).SetValue(_hasHeaderKey, e.NewValue != null);
		((HeaderedItemsControl)d).OnHeaderChanged(e.OldValue, e.NewValue);
	}

	/// <summary>Gets or sets the header.</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	/// <summary>
	/// Called when <see cref="Header"/> changes. Override to push to the GTK widget.
	/// </summary>
	protected virtual void OnHeaderChanged(object? oldHeader, object? newHeader) { }

	/// <summary>Returns the header as a plain string (used for GTK labels).</summary>
	protected string HeaderText => ContentResolution.ToText(Header, HeaderStringFormat);
}