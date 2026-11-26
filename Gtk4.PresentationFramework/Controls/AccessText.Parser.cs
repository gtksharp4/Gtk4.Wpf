using System.Globalization;
using System.Text;

namespace System.Windows.Controls;

/// <summary>
/// WPF's access-key text rules (<c>AccessText.FindAccessKeyMarker</c>/<c>RemoveAccessKeyMarker</c>): the first
/// single '_' not followed by another '_' marks the next text element as the access key; '__' is one literal '_';
/// a trailing '_' is literal; only the first marker counts.
/// </summary>
static class AccessKeyParser
{
	const char Marker = '_';

	/// <summary>The parse of one string: what is displayed, the key and where it sits in the display text.</summary>
	internal readonly struct Result
	{
		internal Result(string displayText, string? accessKey, int keyIndex)
		{
			DisplayText = displayText;
			AccessKey = accessKey;
			KeyIndex = keyIndex;
		}

		internal string DisplayText { get; }
		internal string? AccessKey { get; }
		internal int KeyIndex { get; }
	}

	internal static Result Parse(string? text)
	{
		if (string.IsNullOrEmpty(text)) return new Result(string.Empty, null, -1);
		if (text!.IndexOf(Marker) < 0) return new Result(text, null, -1);
		int marker = FindAccessKeyMarker(text);
		string display = RemoveAccessKeyMarker(text);
		if (marker < 0) return new Result(display, null, -1);
		string key = StringInfo.GetNextTextElement(text, marker + 1);
		int index = text.Substring(0, marker).Replace("__", "_").Length;
		return new Result(display, key, index);
	}

	/// <summary>Index of the access-key marker, or -1 when there is none.</summary>
	internal static int FindAccessKeyMarker(string text)
	{
		int start = 0;
		while (start < text.Length)
		{
			int index = text.IndexOf(Marker, start);
			if (index < 0 || index == text.Length - 1) return -1;
			if (text[index + 1] != Marker) return index;
			start = index + 2;
		}
		return -1;
	}

	/// <summary>The text with its one marker removed and every '__' collapsed to '_'.</summary>
	internal static string RemoveAccessKeyMarker(string text)
	{
		if (text.IndexOf(Marker) < 0) return text;
		int marker = FindAccessKeyMarker(text);
		if (marker >= 0) text = text.Remove(marker, 1);
		return text.Replace("__", "_");
	}

	/// <summary>
	/// GTK <c>UseUnderline</c> text with WPF's display: every displayed '_' doubled, one '_' before the key, so
	/// GTK's own '_' rules never apply.
	/// </summary>
	internal static string ToGtkMnemonic(string? text)
	{
		var r = Parse(text);
		if (r.DisplayText.IndexOf(Marker) < 0 && r.AccessKey == null) return r.DisplayText;
		var sb = new StringBuilder(r.DisplayText.Length + 2);
		for (int i = 0; i < r.DisplayText.Length; i++)
		{
			if (i == r.KeyIndex) sb.Append(Marker);
			char c = r.DisplayText[i];
			sb.Append(c);
			if (c == Marker) sb.Append(Marker);
		}
		return sb.ToString();
	}
}
