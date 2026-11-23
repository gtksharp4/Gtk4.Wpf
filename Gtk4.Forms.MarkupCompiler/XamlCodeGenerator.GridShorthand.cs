using System.Text.RegularExpressions;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// .NET 9 Grid RowDefinitions="Auto,*,Auto" / ColumnDefinitions="100,2*,*": one definition per GridLength
		// token (comma and/or whitespace separated), added in order.
		bool TryEmitDefinitionShorthand(IndentedWriter w, string fqType, string varName, string propName, string val)
		{
			bool rows = propName == "RowDefinitions";
			if ((!rows && propName != "ColumnDefinitions")
				|| val.TrimStart().StartsWith("{", System.StringComparison.Ordinal) || !IsGrid(fqType)) return false;
			string def = rows ? "RowDefinition { Height" : "ColumnDefinition { Width";
			foreach (string token in Regex.Split(val.Trim(), @"[\s,]+"))
			{
				if (token.Length == 0) continue;
				w.Line($"{varName}.{propName}.Add(new global::System.Windows.Controls.{def} = (global::System.Windows."
					+ $"GridLength){Converter}.ConvertByType(typeof(global::System.Windows.GridLength), "
					+ $"\"{EscapeString(token)}\") }});");
			}
			return true;
		}

		// Grid or a subclass: a layer/referenced type through the resolver, a project type through its declared base.
		bool IsGrid(string fqType)
		{
			string user = fqType.StartsWith("global::", System.StringComparison.Ordinal) ? fqType.Substring(8) : fqType;
			if (user == GridType || _resolver.DerivesFromByName(user, GridType)) return true;
			for (int hop = 0; hop < 8 && CodeBehindSources != null; hop++)
			{
				string baseName = DeclaredBaseOf(user, out string[] usings);
				if (baseName == null) return false;
				string resolved = ResolveBaseType(baseName, usings);
				if (resolved != null) return resolved == GridType || _resolver.DerivesFromByName(resolved, GridType);
				user = baseName.Contains(".") ? baseName : NamespaceOf(user) + "." + baseName;
			}
			return false;
		}
	}
}
