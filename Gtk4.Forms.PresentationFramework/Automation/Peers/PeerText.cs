using System.Text;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Plain-text name helpers shared by the content-bearing peers.</summary>
static class PeerText
{
	/// <summary>Removes access-key markers: <c>_OK</c> becomes <c>OK</c>, <c>__</c> becomes <c>_</c>.</summary>
	internal static string RemoveAccessKey(string text)
	{
		if (text.IndexOf('_') < 0) return text;
		var sb = new StringBuilder(text.Length);
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '_' && i + 1 < text.Length)
				i++;
			else if (text[i] == '_')
				break;
			sb.Append(text[i]);
		}
		return sb.ToString();
	}

	/// <summary>The name of a content object: string content without access keys, else its plain text.</summary>
	internal static string FromContent(object? content) =>
		content is string s ? RemoveAccessKey(s) : Plain(content);

	/// <summary>The text a content object shows: a <see cref="TextBlock"/>'s text, found through wrappers.</summary>
	internal static string Plain(object? content)
	{
		switch (content)
		{
			case null: return string.Empty;
			case string s: return s;
			case TextBlock block: return block.Text ?? string.Empty;
			case ContentControl control: return Plain(control.Content);
			case UIElement element:
				var sb = new StringBuilder();
				foreach (var child in element.VisualChildren)
					sb.Append(Plain(child));
				return sb.ToString();
			default: return content.ToString() ?? string.Empty;
		}
	}
}
