using System.Collections.Generic;
using System.Text;

namespace System.Windows.Media.Animation;

/// <summary>One step of a parsed <see cref="PropertyPath"/>.</summary>
internal sealed class PathStep
{
	/// <summary>Owner type name of a <c>(Owner.Prop)</c> step, or null.</summary>
	internal string? Owner;

	/// <summary>Property name, or null for an indexer or a parameter step.</summary>
	internal string? Name;

	/// <summary>The dependency property a placeholder step stands for.</summary>
	internal DependencyProperty? Parameter;

	/// <summary>Index of an indexer step, or -1.</summary>
	internal int Index = -1;
}

/// <summary>Parses the path string of a <see cref="PropertyPath"/>.</summary>
internal static class PropertyPathParser
{
	internal static List<PathStep> Parse(PropertyPath path)
	{
		var text = path.Path;
		var steps = new List<PathStep>();
		var i = 0;
		while (i < text.Length)
		{
			var c = text[i];
			if (c == '.' || char.IsWhiteSpace(c)) { i++; continue; }
			if (c == '(' || c == '[' || c == '{')
			{
				var close = c == '(' ? ')' : c == '[' ? ']' : '}';
				var end = text.IndexOf(close, i + 1);
				if (end < 0) throw Bad(path, "unbalanced '" + c + "'");
				steps.Add(Bracketed(path, c, text.Substring(i + 1, end - i - 1)));
				i = end + 1;
				continue;
			}
			var sb = new StringBuilder();
			while (i < text.Length && text[i] != '.' && text[i] != '[' && text[i] != '(') sb.Append(text[i++]);
			steps.Add(new PathStep { Name = sb.ToString().Trim() });
		}
		if (steps.Count == 0) throw Bad(path, "the path is empty");
		return steps;
	}

	static PathStep Bracketed(PropertyPath path, char open, string content)
	{
		content = content.Trim();
		if (open == '[')
		{
			if (TryParameter(path, content, out var dp)) return new PathStep { Parameter = dp };
			if (!int.TryParse(content, out var index) || index < 0) throw Bad(path, $"'[{content}]' is not an index");
			return new PathStep { Index = index };
		}
		if (int.TryParse(content, out var n))
		{
			if (n >= path.PathParameters.Count) throw Bad(path, $"parameter {n} is missing");
			return new PathStep { Parameter = path.PathParameters[n] as DependencyProperty ?? throw Bad(path, $"parameter {n} is not a DependencyProperty") };
		}
		var colon = content.LastIndexOf(':');
		if (colon >= 0) content = content.Substring(colon + 1);
		var dot = content.LastIndexOf('.');
		return dot < 0
			? new PathStep { Name = content }
			: new PathStep { Owner = content.Substring(0, dot).Trim(), Name = content.Substring(dot + 1).Trim() };
	}

	static bool TryParameter(PropertyPath path, string content, out DependencyProperty? dp)
	{
		dp = null;
		if (content.Length < 3 || (content[0] != '(' && content[0] != '{')) return false;
		return int.TryParse(content.Substring(1, content.Length - 2), out var n)
			&& n < path.PathParameters.Count
			&& (dp = path.PathParameters[n] as DependencyProperty) != null;
	}

	internal static InvalidOperationException Bad(PropertyPath path, string why) =>
		new InvalidOperationException($"Cannot resolve TargetProperty '{path.Path}': {why}.");
}
