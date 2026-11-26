using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Media;

namespace System.Windows.Documents;

static class TextRangeRtfReader
{
	sealed class State
	{
		internal bool Bold, Italic, Underline;
		internal double? Size;
		internal int UnicodeSkip = 1;
		internal State Copy() => (State)MemberwiseClone();
	}

	// After \uN the next \ucN units are the ANSI fallback: a \'hh escape or one plain character each.
	static int SkipFallback(string rtf, int i, int units)
	{
		for (int n = 0; n < units && i < rtf.Length; n++)
		{
			if (rtf[i] == '\\' && i + 1 < rtf.Length && rtf[i + 1] == '\'') i += 4;
			else if (rtf[i] == '\\' || rtf[i] == '{' || rtf[i] == '}') break;
			else i++;
		}
		return Math.Min(i, rtf.Length);
	}

	internal static List<Paragraph> Read(string rtf)
	{
		if (!rtf.TrimStart().StartsWith("{\\rtf", StringComparison.Ordinal))
			throw new ArgumentException("The data is not RTF.");
		var result = new List<Paragraph>();
		var stack = new Stack<State>();
		var st = new State();
		var text = new StringBuilder();
		var para = new Paragraph();
		int skipDepth = -1;
		int i = 0;

		void Flush()
		{
			if (text.Length == 0) return;
			var run = new Run { Text = text.ToString() };
			if (st.Bold) run.FontWeight = FontWeights.Bold;
			if (st.Italic) run.FontStyle = FontStyles.Italic;
			if (st.Underline) run.TextDecorations = TextDecorations.Underline;
			if (st.Size != null) run.FontSize = st.Size.Value;
			para.Inlines.Add(run);
			text.Clear();
		}

		while (i < rtf.Length)
		{
			char c = rtf[i++];
			if (c == '{')
			{
				Flush();
				stack.Push(st);
				st = st.Copy();
			}
			else if (c == '}')
			{
				Flush();
				if (stack.Count > 0) st = stack.Pop();
				if (skipDepth > stack.Count) skipDepth = -1;
			}
			else if (c == '\r' || c == '\n')
			{
			}
			else if (c != '\\')
			{
				if (skipDepth < 0) text.Append(c);
			}
			else if (i < rtf.Length)
			{
				char n = rtf[i];
				if (n == '\\' || n == '{' || n == '}')
				{
					i++;
					if (skipDepth < 0) text.Append(n);
					continue;
				}
				if (n == '*')
				{
					i++;
					skipDepth = stack.Count;
					continue;
				}
				if (n == '\'' && i + 2 < rtf.Length)
				{
					int.TryParse(rtf.Substring(i + 1, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int hex);
					i += 3;
					if (skipDepth < 0) text.Append((char)hex);
					continue;
				}
				int start = i;
				while (i < rtf.Length && char.IsLetter(rtf[i])) i++;
				string word = rtf.Substring(start, i - start);
				int ns = i;
				if (i < rtf.Length && rtf[i] == '-') i++;
				while (i < rtf.Length && char.IsDigit(rtf[i])) i++;
				bool hasNum = int.TryParse(rtf.Substring(ns, i - ns), NumberStyles.AllowLeadingSign,
					CultureInfo.InvariantCulture, out int num);
				if (i < rtf.Length && rtf[i] == ' ') i++;
				if (skipDepth >= 0) continue;
				switch (word)
				{
					case "par":
						Flush();
						result.Add(para);
						para = new Paragraph();
						break;
					case "line":
						Flush();
						para.Inlines.Add(new LineBreak());
						break;
					case "b": st.Bold = !hasNum || num != 0; break;
					case "i": st.Italic = !hasNum || num != 0; break;
					case "ul": st.Underline = !hasNum || num != 0; break;
					case "ulnone": st.Underline = false; break;
					case "fs":
						if (hasNum) st.Size = num / 1.5;
						break;
					case "uc":
						if (hasNum) st.UnicodeSkip = Math.Max(0, num);
						break;
					case "u":
						text.Append((char)(num < 0 ? num + 65536 : num));
						i = SkipFallback(rtf, i, st.UnicodeSkip);
						break;
					case "fonttbl":
					case "colortbl":
					case "stylesheet":
					case "info":
						skipDepth = stack.Count;
						break;
				}
			}
		}
		Flush();
		if (para.Inlines.Count > 0) result.Add(para);
		return result;
	}
}
