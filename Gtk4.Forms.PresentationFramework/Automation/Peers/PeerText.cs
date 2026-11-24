using System.Text;
using System.Windows.Controls;

namespace System.Windows.Automation.Peers;

/// <summary>Plain-text name helpers shared by the content-bearing peers.</summary>
static class PeerText
{
	/// <summary>
	/// The name of a content object where WPF strips access keys (button, label, menu item, tab item and group
	/// box peers): string content has its marker removed (<c>_OK</c> becomes <c>OK</c>, <c>__</c> becomes
	/// <c>_</c>), else its plain text.
	/// </summary>
	internal static string FromContent(object? content) =>
		content is string s ? AccessKeyParser.RemoveAccessKeyMarker(s) : Plain(content);

	/// <summary>The name of a content object where WPF keeps underscores (list, combo, tree, expander).</summary>
	internal static string FromContentLiteral(object? content) => Plain(content);

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
