using System;

namespace System.Windows.Controls;

/// <summary>
/// Layout DPs for <see cref="TextBlock"/>:
/// <see cref="TextAlignment"/>, <see cref="TextTrimming"/>, <see cref="Padding"/>,
/// and <see cref="LineHeight"/>.
/// </summary>
public partial class TextBlock
{
	// ── TextAlignment DP ─────────────────────────────────────────────────────

	/// <summary>Horizontal alignment of text within the block. Default: <see cref="TextAlignment.Left"/>.</summary>
	public static readonly DependencyProperty TextAlignmentProperty =
		DependencyProperty.Register(
			nameof(TextAlignment),
			typeof(TextAlignment),
			typeof(TextBlock),
			new FrameworkPropertyMetadata(TextAlignment.Left,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits,
				OnTextAlignmentChanged));

	static void OnTextAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> (d as TextBlock)?.ApplyTextAlignment();

	/// <summary>Gets or sets the horizontal alignment of text.</summary>
	public TextAlignment TextAlignment
	{
		get => (TextAlignment)(GetValue(TextAlignmentProperty) ?? TextAlignment.Left);
		set => SetValue(TextAlignmentProperty, value);
	}

	void ApplyTextAlignment()
	{
		if (_label == null) return;
		var (justify, xalign) = MapTextAlignment(TextAlignment);
		_label.Justify = justify;
		_label.Xalign = xalign;
	}

	// ── TextTrimming DP ──────────────────────────────────────────────────────

	/// <summary>
	/// Ellipsis trimming mode. Default: <see cref="TextTrimming.None"/>.
	/// <para>
	/// Pango distinguishes <c>EllipsizeMode.Start/Middle/End</c> but not
	/// word vs. character boundaries. Both <see cref="TextTrimming.CharacterEllipsis"/>
	/// and <see cref="TextTrimming.WordEllipsis"/> map to <c>EllipsizeMode.End</c>.
	/// </para>
	/// </summary>
	public static readonly DependencyProperty TextTrimmingProperty =
		DependencyProperty.Register(
			nameof(TextTrimming),
			typeof(TextTrimming),
			typeof(TextBlock),
			new PropertyMetadata(TextTrimming.None, OnTextTrimmingChanged));

	static void OnTextTrimmingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBlock)d).ApplyTextTrimming();

	/// <summary>Gets or sets how overflowing text is trimmed.</summary>
	public TextTrimming TextTrimming
	{
		get => (TextTrimming)(GetValue(TextTrimmingProperty) ?? TextTrimming.None);
		set => SetValue(TextTrimmingProperty, value);
	}

	void ApplyTextTrimming()
	{
		if (_label == null) return;
		_label.Ellipsize = MapTextTrimming(TextTrimming);
	}

	// ── Padding DP ───────────────────────────────────────────────────────────

	/// <summary>
	/// Inner padding between the border and the text content. Default: <c>(0,0,0,0)</c>.
	/// Applied as GTK widget margins on the backing <see cref="Gtk.Label"/>.
	/// </summary>
	public static readonly DependencyProperty PaddingProperty =
		DependencyProperty.Register(
			nameof(Padding),
			typeof(Thickness),
			typeof(TextBlock),
			new PropertyMetadata(default(Thickness), OnPaddingChanged));

	static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((TextBlock)d).ApplyPadding();

	/// <summary>Gets or sets the padding inside the text block.</summary>
	public Thickness Padding
	{
		get => (Thickness)(GetValue(PaddingProperty) ?? default(Thickness));
		set => SetValue(PaddingProperty, value);
	}

	void ApplyPadding()
	{
		if (_label == null) return;
		var p = Padding;
		_label.MarginStart  = (int)Math.Round(p.Left);
		_label.MarginTop    = (int)Math.Round(p.Top);
		_label.MarginEnd    = (int)Math.Round(p.Right);
		_label.MarginBottom = (int)Math.Round(p.Bottom);
	}

	// ── LineHeight DP ────────────────────────────────────────────────────────

	/// <summary>
	/// Distance between lines of text. <see cref="double.NaN"/> (default) means auto.
	/// <para>
	/// Stored and read-back faithfully, but <b>store-only</b>: GTK 4's <see cref="Gtk.Label"/>
	/// exposes line spacing only via Pango attribute-list factory methods that GtkSharp4 does
	/// not surface at this version, so no GTK-side change is applied (see <see cref="ApplyLineHeight"/>).
	/// </para>
	/// </summary>
	public static readonly DependencyProperty LineHeightProperty =
		DependencyProperty.Register(
			nameof(LineHeight),
			typeof(double),
			typeof(TextBlock),
			new PropertyMetadata(double.NaN, OnLineHeightChanged));

	static void OnLineHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> (d as TextBlock)?.ApplyLineHeight();

	/// <summary>
	/// Gets or sets the line height in device-independent pixels.
	/// <see cref="double.NaN"/> means auto (GTK default spacing).
	/// </summary>
	public double LineHeight
	{
		get => (double)(GetValue(LineHeightProperty) ?? double.NaN);
		set => SetValue(LineHeightProperty, value);
	}

	void ApplyLineHeight()
	{
		// LineHeight on Gtk.Label is not cleanly settable: Pango attribute constructors
		// (pango_attr_line_spacing_new / pango_attr_leading_new) are not surfaced in
		// GtkSharp4's managed binding at this version.  The value is stored in the DP
		// and readable, but no GTK-side change is applied.
		// A future implementation can apply it via Pango.AttrList when the binding
		// exposes a suitable factory method.
		_ = _label; // suppress "field unused" warning
	}

	// ── Layout application ───────────────────────────────────────────────────

	/// <summary>
	/// Applies all layout properties to <see cref="_label"/> at once.
	/// Called from <see cref="CreateWidget"/> after the label is constructed.
	/// </summary>
	void ApplyLayoutToLabel()
	{
		ApplyTextAlignment();
		ApplyTextTrimming();
		ApplyPadding();
		ApplyLineHeight();
		ApplyTextWrapping();
	}
}

public partial class TextBlock
{
	/// <inheritdoc/>
	internal override void QueueSlotResize() => _label?.QueueResize();

	// A label's minimum is its text, so a long unwrapped text would widen a fixed Grid track and squeeze the
	// siblings; WPF clips it to the slot instead. The measure is capped at the slot (see CapMeasure) and the text
	// that does not fit is clipped, not painted over the neighbours. The CSS name keeps the theme's and the font
	// rules' "label" selector matching the subclass.
	[Gtk.CssName("label")]
	sealed class SlotCappedLabel : Gtk.Label
	{
		readonly TextBlock _owner;

		internal SlotCappedLabel(TextBlock owner) : base(string.Empty)
		{
			_owner = owner;
			Name = "GtkSlotCappedLabel"; // ElementCssStyle.IsUnnamed treats a Gtk* name as unset
		}

		bool _cappedH, _cappedV;

		protected override void OnMeasure(Gtk.Orientation orientation, int forSize, out int minimum, out int natural,
			out int minimumBaseline, out int naturalBaseline)
		{
			base.OnMeasure(orientation, forSize, out minimum, out natural, out minimumBaseline, out naturalBaseline);
			int margins = orientation == Gtk.Orientation.Horizontal ? MarginStart + MarginEnd : MarginTop + MarginBottom;
			bool capped = _owner.CapMeasure(orientation, ref minimum, ref natural, margins);
			if (orientation == Gtk.Orientation.Horizontal) _cappedH = capped;
			else _cappedV = capped;
		}

		// GTK clips a widget's own paint to its box when Overflow is Hidden, which would cut italic overhang and
		// tall glyphs of every label; WPF clips only a text that does not fit its slot.
		protected override void OnSizeAllocate(int width, int height, int baseline)
		{
			base.OnSizeAllocate(width, height, baseline);
			var overflow = _cappedH || _cappedV || _owner.ClipToBounds ? Gtk.Overflow.Hidden : Gtk.Overflow.Visible;
			if (Overflow != overflow) Overflow = overflow;
		}
	}
}
