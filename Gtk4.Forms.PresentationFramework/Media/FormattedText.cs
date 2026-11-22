using System.Globalization;

namespace System.Windows.Media;

/// <summary>
/// Formatted text for <see cref="DrawingContext.DrawText"/>. Mirrors the stored values of WPF's FormattedText;
/// <see cref="Width"/>/<see cref="Height"/> are measured with Cairo's toy text API.
/// </summary>
public sealed class FormattedText
{
	/// <summary>The text content.</summary>
	public string Text { get; }

	/// <summary>Culture used to format the text.</summary>
	public CultureInfo Culture { get; }

	/// <summary>Flow direction of the text.</summary>
	public FlowDirection FlowDirection { get; }

	/// <summary>Typeface of the text.</summary>
	public Typeface Typeface { get; }

	/// <summary>Font size in device-independent pixels.</summary>
	public double FontSize { get; }

	/// <summary>Foreground brush.</summary>
	public Brush? Foreground { get; set; }

	/// <summary>Pixels-per-DIP the text was created for.</summary>
	public double PixelsPerDip { get; set; } = 1.0;

	/// <summary>Creates a FormattedText instance with just the text.</summary>
	public FormattedText(string text)
		: this(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, null!, 1.0) { }

	/// <summary>Creates formatted text.</summary>
	[Obsolete("Use the PixelsPerDip override")]
	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection,
		Typeface typeface, double emSize, Brush foreground)
		: this(textToFormat, culture, flowDirection, typeface, emSize, foreground, 1.0) { }

	/// <summary>Creates formatted text.</summary>
	public FormattedText(string textToFormat, CultureInfo culture, FlowDirection flowDirection,
		Typeface typeface, double emSize, Brush foreground, double pixelsPerDip)
	{
		Text = textToFormat ?? throw new ArgumentNullException(nameof(textToFormat));
		Culture = culture;
		FlowDirection = flowDirection;
		Typeface = typeface;
		FontSize = emSize;
		Foreground = foreground;
		PixelsPerDip = pixelsPerDip;
	}

	/// <summary>Measured width of the text.</summary>
	public double Width => FormattedTextMetrics.Measure(this).Width;

	/// <summary>Measured height of the text.</summary>
	public double Height => FormattedTextMetrics.Measure(this).Height;
}
