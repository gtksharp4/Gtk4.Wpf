using System.Text;
using System.Windows.Markup;

namespace System.Windows.Documents;

/// <summary>
/// Flow direction and language for flow content. <see cref="TextElement"/> is not a DependencyObject here, so
/// these are plain properties that are rendered only when set explicitly: direction as a Unicode bidi isolate
/// (U+2066/U+2067 ... U+2069) and language as a Pango <c>lang</c> span. Unlike WPF they do not inherit from the
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

	internal void SetFlowDirection(FlowDirection value)
	{
		if (!FrameworkElement.IsValidFlowDirection(value))
			throw new ArgumentException($"'{value}' is not a valid value for property 'FlowDirection'.");
		_flowDirection = value;
	}

	internal void AppendWrappedMarkup(StringBuilder sb, System.Action<StringBuilder> body)
	{
		if (_flowDirection is FlowDirection d)
			sb.Append(d == FlowDirection.RightToLeft ? '⁧' : '⁦');
		if (_language != null)
			sb.Append("<span lang=\"").Append(PangoMarkup.Escape(_language.IetfLanguageTag)).Append("\">");
		body(sb);
		if (_language != null) sb.Append("</span>");
		if (_flowDirection != null) sb.Append('⁩');
	}
}
