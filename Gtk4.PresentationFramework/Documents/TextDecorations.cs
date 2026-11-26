namespace System.Windows;

/// <summary>
/// Provides static, pre-built <see cref="TextDecorationCollection"/> values for the
/// four WPF decoration locations. The instances are frozen, as in WPF.
/// </summary>
public static class TextDecorations
{
	static TextDecorationCollection Frozen(TextDecorationLocation location)
	{
		var collection = new TextDecorationCollection { new TextDecoration(location) };
		collection.Freeze();
		return collection;
	}

	/// <summary>
	/// A single-entry collection containing an <see cref="TextDecorationLocation.Underline"/> decoration.
	/// </summary>
	public static TextDecorationCollection Underline { get; } =
		Frozen(TextDecorationLocation.Underline);

	/// <summary>
	/// A single-entry collection containing a <see cref="TextDecorationLocation.Strikethrough"/> decoration.
	/// </summary>
	public static TextDecorationCollection Strikethrough { get; } =
		Frozen(TextDecorationLocation.Strikethrough);

	/// <summary>A single-entry collection containing an <see cref="TextDecorationLocation.OverLine"/> decoration.
	/// <para>Pango support: emitted as <c>overline="single"</c> (requires Pango ≥ 1.46 / GTK ≥ 4.0).
	/// Silently absent on runtimes that do not recognise the attribute.</para>
	/// </summary>
	public static TextDecorationCollection OverLine { get; } =
		Frozen(TextDecorationLocation.OverLine);

	/// <summary>A single-entry collection containing a <see cref="TextDecorationLocation.Baseline"/> decoration.
	/// <para>Pango has no direct baseline decoration; rendered as <c>underline="single"</c>.</para>
	/// </summary>
	public static TextDecorationCollection Baseline { get; } =
		Frozen(TextDecorationLocation.Baseline);
}
