using System;
using System.Text;
using System.Windows.Automation;
using Condition = System.Windows.Automation.Condition;

namespace Gtk4.Wpf.Testing;

/// <summary>Renders an automation subtree as indented text, for failure messages and evidence files.</summary>
public static class TreeDump
{
	const int MaxLines = 200;

	/// <summary>Dumps <paramref name="root"/> and its descendants, one element per line.</summary>
	public static string Of(AutomationElement root, int maxDepth = 12) =>
		GtkThread.Invoke(() =>
		{
			var text = new StringBuilder();
			int lines = 0;
			bool truncated = false;
			Walk(root, 0, maxDepth, text, ref lines, ref truncated);
			if (truncated)
				text.AppendLine("... (truncated)");
			return text.ToString();
		});

	static void Walk(AutomationElement element, int depth, int maxDepth, StringBuilder text, ref int lines,
		ref bool truncated)
	{
		if (lines >= MaxLines)
		{
			truncated = true;
			return;
		}
		lines++;
		text.Append(' ', depth * 2).AppendLine(Describe(element));
		if (depth >= maxDepth) return;
		foreach (AutomationElement child in element.FindAll(TreeScope.Children, Condition.TrueCondition))
			Walk(child, depth + 1, maxDepth, text, ref lines, ref truncated);
	}

	static string Describe(AutomationElement element)
	{
		var info = element.Current;
		var line = new StringBuilder(info.ControlType.ProgrammaticName.Replace("ControlType.", ""));
		if (info.AutomationId.Length > 0) line.Append(" id=\"").Append(info.AutomationId).Append('"');
		if (info.Name.Length > 0) line.Append(" name=\"").Append(Shorten(info.Name)).Append('"');
		line.Append(" class=").Append(info.ClassName);
		if (!info.IsEnabled) line.Append(" [disabled]");
		if (info.IsOffscreen) line.Append(" [offscreen]");
		return line.ToString();
	}

	static string Shorten(string value)
	{
		value = value.Replace("\r", "\\r").Replace("\n", "\\n");
		return value.Length <= 60 ? value : value.Substring(0, 57) + "...";
	}
}
