using System.Text;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// Flow direction and language for flow content. <see cref="TextElement"/> is not a DependencyObject here, so
/// these are plain properties that are rendered only when set explicitly: direction as a Unicode bidi isolate
/// (<see cref="BidiMarks"/>) and language as a Pango <c>lang</c> span. Unlike WPF they do not inherit from the
/// parent element and are not dependency properties.
/// </summary>
public abstract partial class TextElement
{
	FlowDirection? _flowDirection;
	XmlLanguage? _language;

	/// <summary>The localization language (default en-US).</summary>
	public XmlLanguage Language
	{
		get => _language ?? XmlLanguage.GetLanguage("en-US");
		set => _language = value ?? throw new ArgumentNullException(nameof(value));
	}

	internal bool HasFlowOrLanguage => _flowDirection != null || _language != null;

	internal FlowDirection GetFlowDirection() => _flowDirection ?? FlowDirection.LeftToRight;

	/// <summary>The explicitly set direction, or <c>null</c>.</summary>
	internal FlowDirection? ExplicitFlowDirection => _flowDirection;

	internal void SetFlowDirection(FlowDirection value)
	{
		if (!FrameworkElement.IsValidFlowDirection(value))
			throw new ArgumentException($"'{value}' is not a valid value for property 'FlowDirection'.");
		_flowDirection = value;
	}

	internal void AppendWrappedMarkup(StringBuilder sb, System.Action<StringBuilder> body)
	{
		if (_flowDirection is FlowDirection d) sb.Append(BidiMarks.Isolate(d));
		AppendLanguage(sb, body);
		if (_flowDirection != null) sb.Append(BidiMarks.Pdi);
	}

	/// <summary>
	/// Appends one paragraph: a bidi isolate of <paramref name="effective"/> that opens and closes inside the
	/// paragraph, preceded by a directional mark so the paragraph's base direction is right too (the isolate's
	/// content is skipped when Pango picks it). Isolates cannot span a paragraph separator.
	/// </summary>
	internal void AppendParagraph(StringBuilder sb, FlowDirection? effective, System.Action<StringBuilder> body)
	{
		if (effective is FlowDirection d) sb.Append(BidiMarks.Mark(d)).Append(BidiMarks.Isolate(d));
		AppendLanguage(sb, body);
		if (effective != null) sb.Append(BidiMarks.Pdi);
	}

	internal void AppendLanguage(StringBuilder sb, System.Action<StringBuilder> body)
	{
		if (_language != null)
			sb.Append("<span lang=\"").Append(PangoMarkup.Escape(_language.IetfLanguageTag)).Append("\">");
		body(sb);
		if (_language != null) sb.Append("</span>");
	}
}
