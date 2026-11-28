using System.ComponentModel;
using System.Globalization;
using System.Windows.Documents;

namespace System.Windows.Controls;

internal static partial class ContentResolution
{
	static UIElement FromInline(Inline inline)
	{
		var block = new TextBlock();
		block.Inlines.Add(inline);
		HyperlinkActivation.Attach(block);
		return block;
	}

	static UIElement FromText(object content, string? stringFormat, bool recognizesAccessKey)
	{
		var text = ToText(content, stringFormat);
		if (recognizesAccessKey && content is string && text.IndexOf('_') >= 0)
			return new AccessText { Text = text };
		return new TextBlock { Text = text };
	}

	/// <summary>The text WPF shows for <paramref name="content"/>: format, then converter, then ToString.</summary>
	internal static string ToText(object? content, string? stringFormat)
	{
		if (content == null) return string.Empty;
		// WPF shows an XmlNode as its text (default template XPath=".").
		if (content is System.Xml.XmlNode xml) content = xml.InnerText;
		if (!string.IsNullOrEmpty(stringFormat))
		{
			var fmt = stringFormat!.IndexOf('{') >= 0 ? stringFormat : "{0:" + stringFormat + "}";
			return string.Format(CultureInfo.CurrentCulture, fmt, content);
		}
		if (content is string s) return s;
		var converter = TypeDescriptor.GetConverter(content.GetType());
		if (converter.GetType() != typeof(TypeConverter) && converter.CanConvertTo(typeof(string)))
			return converter.ConvertToString(null, CultureInfo.CurrentCulture, content) ?? string.Empty;
		return content.ToString() ?? string.Empty;
	}
}
