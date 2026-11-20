using System.Windows.Markup;

namespace System.Windows;

/// <summary>
/// <see cref="Language"/>. GTK gap: stored and inherited only; a GTK widget has no per-widget language, so only
/// <c>Span.Language</c>/<c>Run.Language</c> reach Pango (as <c>lang</c>).
/// </summary>
public abstract partial class FrameworkElement
{
	/// <summary>Identifies the Language property (default en-US; inherits).</summary>
	public static readonly DependencyProperty LanguageProperty = DependencyProperty.Register(
		nameof(Language), typeof(XmlLanguage), typeof(FrameworkElement),
		new FrameworkPropertyMetadata(
			XmlLanguage.GetLanguage("en-US"),
			FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Gets or sets the localization/globalization language.</summary>
	public XmlLanguage Language
	{
		get => (XmlLanguage)GetValue(LanguageProperty)!;
		set => SetValue(LanguageProperty, value);
	}
}
