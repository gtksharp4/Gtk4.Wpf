namespace System.Windows.Controls;

/// <summary>
/// Base class for controls that have a <see cref="Header"/> and a <see cref="ContentControl.Content"/>.
/// Used by <see cref="Expander"/>, <see cref="GroupBox"/>, and <see cref="TabItem"/>.
/// </summary>
public partial class HeaderedContentControl : ContentControl
{
	// ── Header DP ────────────────────────────────────────────────────────────

	/// <summary>The header of this control (a <see cref="UIElement"/> or a string).</summary>
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(
			nameof(Header),
			typeof(object),
			typeof(HeaderedContentControl),
			new PropertyMetadata(null, OnHeaderChanged));

	static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((HeaderedContentControl)d).SetValue(_hasHeaderKey, e.NewValue != null);
		((HeaderedContentControl)d).OnHeaderChanged(e.OldValue, e.NewValue);
	}

	/// <summary>Gets or sets the header.</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	/// <summary>
	/// Called when <see cref="Header"/> changes. Override to push the new header
	/// value into the backing GTK widget (e.g. <c>Gtk.Expander.LabelWidget</c>).
	/// </summary>
	protected virtual void OnHeaderChanged(object? oldHeader, object? newHeader) { }
}