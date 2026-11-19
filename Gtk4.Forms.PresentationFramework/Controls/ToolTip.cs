using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// A WPF-compatible <c>ToolTip</c> control. Derives from <see cref="ContentControl"/>
/// so the <see cref="ContentControl.Content"/> property carries the tooltip body
/// (a string or any <see cref="UIElement"/>).
/// <para>
/// <b>Rendering:</b> When a <see cref="ToolTip"/> instance is assigned to
/// <see cref="FrameworkElement.ToolTip"/>, the GTK native tooltip is populated with
/// <c>Content.ToString()</c>. Rich-content tooltip rendering (custom widget inside
/// a GTK popover) is not implemented; the object model is complete.
/// </para>
/// </summary>
public class ToolTip : ContentControl
{
	// ── IsOpen DP ────────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="IsOpen"/> dependency property.
	/// Default: <see langword="false"/>.
	/// </summary>
	public static readonly DependencyProperty IsOpenProperty =
		DependencyProperty.Register(
			nameof(IsOpen),
			typeof(bool),
			typeof(ToolTip),
			new FrameworkPropertyMetadata(false,
				FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	/// <summary>
	/// Gets or sets whether the tooltip is currently open.
	/// Stored; not yet forwarded to GTK (GTK manages tooltip visibility natively).
	/// </summary>
	public bool IsOpen
	{
		get => (bool?)GetValue(IsOpenProperty) == true;
		set => SetValue(IsOpenProperty, value);
	}

	// ── Placement DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="Placement"/> dependency property.
	/// Default: <see cref="PlacementMode.MousePoint"/>.
	/// </summary>
	public static readonly DependencyProperty PlacementProperty =
		DependencyProperty.Register(
			nameof(Placement),
			typeof(PlacementMode),
			typeof(ToolTip),
			new PropertyMetadata(PlacementMode.MousePoint));

	/// <summary>
	/// Gets or sets the placement mode hint. Stored; GTK positions the tooltip automatically.
	/// </summary>
	public PlacementMode Placement
	{
		get => (PlacementMode)(GetValue(PlacementProperty) ?? PlacementMode.MousePoint);
		set => SetValue(PlacementProperty, value);
	}

	// ── PlacementTarget DP ───────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="PlacementTarget"/> dependency property.
	/// Default: <see langword="null"/>.
	/// </summary>
	public static readonly DependencyProperty PlacementTargetProperty =
		DependencyProperty.Register(
			nameof(PlacementTarget),
			typeof(UIElement),
			typeof(ToolTip),
			new PropertyMetadata(null));

	/// <summary>
	/// Gets or sets the element the tooltip anchors to.
	/// Stored; the GTK tooltip is anchored to the host widget automatically.
	/// </summary>
	public UIElement? PlacementTarget
	{
		get => (UIElement?)GetValue(PlacementTargetProperty);
		set => SetValue(PlacementTargetProperty, value);
	}

	// ── StaysOpen DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="StaysOpen"/> dependency property.
	/// Default: <see langword="true"/>.
	/// </summary>
	public static readonly DependencyProperty StaysOpenProperty =
		DependencyProperty.Register(
			nameof(StaysOpen),
			typeof(bool),
			typeof(ToolTip),
			new PropertyMetadata(true));

	/// <summary>
	/// Gets or sets whether the tooltip stays open after the pointer moves away.
	/// Stored; GTK controls tooltip dismiss timing natively.
	/// </summary>
	public bool StaysOpen
	{
		get => (bool?)GetValue(StaysOpenProperty) != false;
		set => SetValue(StaysOpenProperty, value);
	}
}
