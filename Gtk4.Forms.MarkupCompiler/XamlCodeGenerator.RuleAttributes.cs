using System;
using System.Globalization;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// C# expression for an attribute on a validation rule, typed by the rule's property. The
		/// <c>ValidationStep</c> enum and <c>ValidatesOnTargetUpdated</c> are declared on the base class
		/// and are handled without a resolver lookup (consumer rule types are often unresolvable).
		/// </summary>
		string RuleAttrExpression(string ruleType, string name, string value)
		{
			string v = value.Trim();
			if (name == "ValidationStep") return "System.Windows.Controls.ValidationStep." + v;
			if (name == "ValidatesOnTargetUpdated" && bool.TryParse(v, out bool flag))
				return flag ? "true" : "false";

			string typeName = ruleType.StartsWith("global::", StringComparison.Ordinal)
				? ruleType.Substring("global::".Length) : ruleType;
			Type t = _resolver.GetPropertyType(typeName, name);
			if (t == null) return ClrValueExpression(name, value);

			if (t.IsEnum) return "global::" + t.FullName.Replace('+', '.') + "." + v;
			if (t == typeof(string)) return "\"" + EscapeString(value) + "\"";
			if (t == typeof(bool) && bool.TryParse(v, out bool b)) return b ? "true" : "false";
			var inv = CultureInfo.InvariantCulture;
			if (t == typeof(decimal) && decimal.TryParse(v, NumberStyles.Float, inv, out decimal m))
				return m.ToString(inv) + "m";
			if (t == typeof(float) && float.TryParse(v, NumberStyles.Float, inv, out float f))
				return f.ToString("R", inv) + "f";
			if (t == typeof(double) && double.TryParse(v, NumberStyles.Float, inv, out double d))
				return d.ToString("R", inv) + "d";
			return ClrValueExpression(name, value);
		}
	}
}
