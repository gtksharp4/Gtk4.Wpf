using System.Text;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// Flow direction and language for flow content. Flow direction is a plain property here, so
/// it is rendered only when set explicitly: direction as a Unicode bidi isolate
/// (<see cref="BidiMarks"/>) and language as a Pango <c>lang</c> span. Unlike WPF it does not inherit from the
/// parent element and is not a dependency property.
/// </summary>
public abstract partial class TextElement
{
	FlowDirection? _flowDirection;

	internal bool HasFlowOrLanguage => _flowDirection != null || HasLocalLanguage;

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
		if (_flowDirection is FlowDirection d) BidiMarks.AppendIsolated(sb, d, false, s => AppendLanguage(s, body));
		else AppendLanguage(sb, body);
	}

	/// <summary>
	/// Appends one paragraph: a bidi isolate of <paramref name="effective"/> that opens and closes inside the
	/// paragraph, preceded by a directional mark so the paragraph's base direction is right too (the isolate's
	/// content is skipped when Pango picks it). Isolates cannot span a paragraph separator, so each line inside closes and reopens it.
	/// </summary>
	internal void AppendParagraph(StringBuilder sb, FlowDirection? effective, System.Action<StringBuilder> body)
	{
		if (effective is FlowDirection d) BidiMarks.AppendIsolated(sb, d, true, s => AppendLanguage(s, body));
		else AppendLanguage(sb, body);
	}

	/// <summary>Wraps <paramref name="body"/> in the local font size and language spans (nothing when unset).</summary>
	internal void AppendLanguage(StringBuilder sb, System.Action<StringBuilder> body)
	{
		string? size = PangoSizeAttribute();
		if (size != null) sb.Append("<span ").Append(size).Append('>');
		if (HasLocalLanguage)
			sb.Append("<span lang=\"").Append(PangoMarkup.Escape(Language.IetfLanguageTag)).Append("\">");
		body(sb);
		if (HasLocalLanguage) sb.Append("</span>");
		if (size != null) sb.Append("</span>");
	}
}
