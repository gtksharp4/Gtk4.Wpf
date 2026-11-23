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
			if (fqType != GridType || (!rows && propName != "ColumnDefinitions")
				|| val.TrimStart().StartsWith("{", System.StringComparison.Ordinal)) return false;
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
	}
}
