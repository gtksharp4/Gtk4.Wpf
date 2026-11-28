using System;
using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Displays a read-only text string or a sequence of <see cref="Inline"/> elements.
/// Backed by a <see cref="Gtk.Label"/>; Pango markup is used when inlines are present.
/// <para>Setting <see cref="Text"/> creates a single <see cref="Run"/> and clears any
/// existing inlines. Adding inlines clears <see cref="Text"/>.</para>
/// </summary>
[ContentProperty("Inlines")]
public partial class TextBlock : FrameworkElement
{
	// ── Text DP ──────────────────────────────────────────────────────────────────

	/// <summary>The plain text to display.</summary>
	public static readonly DependencyProperty TextProperty =
		DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(TextBlock),
			new PropertyMetadata(string.Empty, OnTextChanged));

	static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var tb = (TextBlock)d;
		var text = (string?)e.NewValue ?? string.Empty;
		if (tb._settingFromInlines) return;
		// Setting Text: replace inlines with a single Run, suppress the inlines→Text loop.
		tb._settingFromText = true;
		try
		{
			tb._inlines.Clear();
			if (text.Length > 0)
				tb._inlines.Add(new Run { Text = text });
		}
		finally { tb._settingFromText = false; }
		tb.ApplyToLabel();
	}

	/// <summary>Gets or sets the displayed text.</summary>
	public string Text
	{
		get => (string?)GetValue(TextProperty) ?? string.Empty;
		set => SetValue(TextProperty, value);
	}

	// ── TextDecorations DP ───────────────────────────────────────────────────────

	/// <summary>Text decorations applied to the entire <see cref="TextBlock"/>.</summary>
	public static readonly DependencyProperty TextDecorationsProperty =
		DependencyProperty.Register(
			nameof(TextDecorations),
			typeof(TextDecorationCollection),
			typeof(TextBlock),
			new PropertyMetadata(null, OnTextDecorationsChanged));

	static void OnTextDecorationsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBlock)d).ApplyToLabel();
	}

	/// <summary>Gets or sets the text decorations (underline, strikethrough, etc.) for this block.</summary>
	public TextDecorationCollection? TextDecorations
	{
		get => (TextDecorationCollection?)GetValue(TextDecorationsProperty);
		set => SetValue(TextDecorationsProperty, value);
	}

	// ── Typography DP change ─────────────────────────────────────────────────────

	// Typography is registered on typeof(Typography) as attached DPs; we override the
	// metadata on TextBlock so that changes trigger a markup refresh.
	static TextBlock()
	{
		Documents.Typography.CapitalsProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(Documents.FontCapitals.Normal, OnTypographyChanged));
		Documents.Typography.NumeralStyleProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(Documents.FontNumeralStyle.Normal, OnTypographyChanged));
		Documents.Typography.FractionProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(Documents.FontFraction.Normal, OnTypographyChanged));
		Documents.Typography.StandardLigaturesProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(true, OnTypographyChanged));
		Documents.Typography.KerningProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(true, OnTypographyChanged));
		Documents.Typography.VariantsProperty.OverrideMetadata(
			typeof(TextBlock),
			new PropertyMetadata(Documents.FontVariants.Normal, OnTypographyChanged));
	}

	static void OnTypographyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((TextBlock)d).ApplyToLabel();
	}

	// ── GTK ──────────────────────────────────────────────────────────────────────

	Gtk.Label _label = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_label = new SlotCappedLabel(this);
		var host = CreateInlineHost();
		ApplyLayoutToLabel();
		ApplyToLabel();
		return host ?? _label;
	}

	/// <summary>
	/// Applies the current content (plain text or inlines) to the backing GTK label.
	/// When inlines are present, Pango markup is used (<see cref="BuildMarkup"/>).
	/// </summary>
	void ApplyToLabel()
	{
		SyncPointerHost();
		SyncInlineChildren();
		if (_label == null) return;
		ApplyLabelContent();
		ApplyInlineObjects();
	}

	void ApplyLabelContent()
	{
		if (_inlines.Count == 0)
		{
			_label.UseMarkup = false;
			_label.Text = string.Empty;
			return;
		}
		// Check if the single inline is a plain Run with no formatting (including no decorations
		// and no Typography features).
		var blockDec = TextDecorations;
		string typographyFeatures = Documents.TypographyPango.BuildFontFeatures(this);
		if (_inlines.Count == 1 && _inlines[0] is Run r &&
		    !r.HasFlowOrLanguage &&
		    !r.HasLocalFormat && LocalFamily(r) == null &&
		    (r.TextDecorations == null || r.TextDecorations.Count == 0) &&
		    (blockDec == null || blockDec.Count == 0) &&
		    typographyFeatures.Length == 0)
		{
			_label.UseMarkup = false;
			_label.Text = WithDirection(r.Text);
			return;
		}
		_label.UseMarkup = true;
		_label.Markup = WithDirection(BuildMarkup());
	}

	/// <summary>Re-renders the label so its base direction follows the new FlowDirection.</summary>
	internal override void OnFlowDirectionApplied() => ApplyToLabel();

	// GtkLabel keeps Pango's auto direction (the first strong character decides), so a directional mark in front
	// of the pushed text gives the paragraph the FlowDirection. Only the label text changes, never Text itself.
	string WithDirection(string text)
	{
		string mark = DirectionMark();
		return mark.Length == 0 ? text : mark + Documents.BidiMarks.MarkLines(text, mark[0]);
	}

	string DirectionMark()
	{
		if (FlowDirection == FlowDirection.RightToLeft) return Documents.BidiMarks.Rlm.ToString();
		return ReadLocalValue(FlowDirectionProperty) != DependencyProperty.UnsetValue
			? Documents.BidiMarks.Lrm.ToString() : string.Empty;
	}

	/// <summary>
	/// Builds the Pango markup string from the current <see cref="Inlines"/> collection,
	/// wrapping the whole output in a decoration span when <see cref="TextDecorations"/> is set,
	/// and in a <c>font_features</c> span when any <see cref="Documents.Typography"/> property
	/// differs from its default.
	/// Exposed internally for unit testing without requiring a live GTK label.
	/// </summary>
	internal string BuildMarkup()
	{
		var sb = new System.Text.StringBuilder();
		foreach (var inline in _inlines)
			AppendInline(sb, inline);
		string inner = sb.ToString();
		string withDecorations = Documents.TextDecorationMarkup.WrapWithDecorations(TextDecorations, inner);
		return Documents.TypographyPango.WrapWithFontFeatures(this, withDecorations);
	}
}