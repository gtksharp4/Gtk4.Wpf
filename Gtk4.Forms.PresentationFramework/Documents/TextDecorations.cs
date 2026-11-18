namespace System.Windows;

/// <summary>
/// Provides static, pre-built <see cref="TextDecorationCollection"/> values for the
/// four WPF decoration locations.
/// </summary>
public static class TextDecorations
{
	/// <summary>A single-entry collection containing an <see cref="TextDecorationLocation.Underline"/> decoration.</summary>
	public static TextDecorationCollection Underline { get; } =
		new TextDecorationCollection { new TextDecoration(TextDecorationLocation.Underline) };

	/// <summary>A single-entry collection containing a <see cref="TextDecorationLocation.Strikethrough"/> decoration.</summary>
	public static TextDecorationCollection Strikethrough { get; } =
		new TextDecorationCollection { new TextDecoration(TextDecorationLocation.Strikethrough) };

	/// <summary>A single-entry collection containing an <see cref="TextDecorationLocation.OverLine"/> decoration.
	/// <para>Pango support: emitted as <c>overline="single"</c> (requires Pango ≥ 1.46 / GTK ≥ 4.0).
	/// Silently absent on runtimes that do not recognise the attribute.</para>
	/// </summary>
	public static TextDecorationCollection OverLine { get; } =
		new TextDecorationCollection { new TextDecoration(TextDecorationLocation.OverLine) };

	/// <summary>A single-entry collection containing a <see cref="TextDecorationLocation.Baseline"/> decoration.
	/// <para>Pango has no direct baseline decoration; rendered as <c>underline="single"</c>.</para>
	/// </summary>
	public static TextDecorationCollection Baseline { get; } =
		new TextDecorationCollection { new TextDecoration(TextDecorationLocation.Baseline) };
}
