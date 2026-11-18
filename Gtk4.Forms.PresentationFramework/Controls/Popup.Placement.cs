namespace System.Windows.Controls.Primitives;

public partial class Popup
{
	// ── Placement DP ──────────────────────────────────────────────────────────

	/// <summary>
	/// Gets or sets where the popup appears relative to its <see cref="PlacementTarget"/>.
	/// Default is <see cref="PlacementMode.Bottom"/>.
	/// <para>
	/// GTK 4 natively supports Bottom/Top/Left/Right. All other modes fall back to Bottom;
	/// see <see cref="PopupPlacementMap.ToGtkPosition"/> for the full mapping.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty PlacementProperty =
		DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(Popup),
			new PropertyMetadata(PlacementMode.Bottom, OnPlacementChanged));

	static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var p = (Popup)d;
		if (p._popover != null)
			p._popover.Position = PopupPlacementMap.ToGtkPosition(p.Placement);
	}

	/// <summary>Gets or sets the popup placement mode.</summary>
	public PlacementMode Placement
	{
		get => (PlacementMode)(GetValue(PlacementProperty) ?? PlacementMode.Bottom);
		set => SetValue(PlacementProperty, value);
	}

	// ── HorizontalOffset DP ───────────────────────────────────────────────────

	/// <summary>
	/// Horizontal pixel offset applied to the popover when it opens.
	/// Mapped to <c>Gtk.Popover.SetOffset</c> (integer precision; sub-pixel values are truncated).
	/// </summary>
	public static readonly DependencyProperty HorizontalOffsetProperty =
		DependencyProperty.Register(nameof(HorizontalOffset), typeof(double), typeof(Popup),
			new PropertyMetadata(0.0, OnOffsetChanged));

	/// <summary>Gets or sets the horizontal offset in device-independent pixels.</summary>
	public double HorizontalOffset
	{
		get => (double)(GetValue(HorizontalOffsetProperty) ?? 0.0);
		set => SetValue(HorizontalOffsetProperty, value);
	}

	// ── VerticalOffset DP ─────────────────────────────────────────────────────

	/// <summary>
	/// Vertical pixel offset applied to the popover when it opens.
	/// Mapped to <c>Gtk.Popover.SetOffset</c> (integer precision; sub-pixel values are truncated).
	/// </summary>
	public static readonly DependencyProperty VerticalOffsetProperty =
		DependencyProperty.Register(nameof(VerticalOffset), typeof(double), typeof(Popup),
			new PropertyMetadata(0.0, OnOffsetChanged));

	/// <summary>Gets or sets the vertical offset in device-independent pixels.</summary>
	public double VerticalOffset
	{
		get => (double)(GetValue(VerticalOffsetProperty) ?? 0.0);
		set => SetValue(VerticalOffsetProperty, value);
	}

	// ── Shared callback ───────────────────────────────────────────────────────

	static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var p = (Popup)d;
		if (p._popover != null)
			p.ApplyOffset(p._popover);
	}

	// ── Helpers ───────────────────────────────────────────────────────────────

	/// <summary>Applies current <see cref="Placement"/> and offsets to <paramref name="popover"/>.</summary>
	internal void ApplyPlacement(Gtk.Popover popover)
	{
		popover.Position = PopupPlacementMap.ToGtkPosition(Placement);
		ApplyOffset(popover);
	}

	void ApplyOffset(Gtk.Popover popover)
	{
		popover.SetOffset((int)HorizontalOffset, (int)VerticalOffset);
	}
}
