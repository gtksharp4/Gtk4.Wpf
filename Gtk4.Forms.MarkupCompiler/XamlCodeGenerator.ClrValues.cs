using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		// Struct types with a built-in XamlValueConverter registration (XamlValueConverter.RegisterBuiltIns).
		// ConvertByType returns the raw string for any other type, so a cast to it would throw at run time.
		static readonly HashSet<string> ConverterBackedStructs = new HashSet<string>
		{
			"System.Windows.Thickness", "System.Windows.CornerRadius", "System.Windows.GridLength",
			"System.Windows.Point", "System.Windows.Size", "System.Windows.Rect",
			"System.Windows.Media.Color", "System.Windows.Media.FontWeight", "System.Windows.Media.FontStyle",
			"System.Windows.Media.FontStretch", "System.Windows.Media.Animation.Duration",
			"System.Windows.Media.Animation.RepeatBehavior",
		};

		/// <summary>
		/// C# expression for a plain attribute value on a CLR (non-DP) property. Enums and structs with a
		/// registered converter are converted to their type; everything else keeps the literal rules of
		/// <see cref="ClrValueExpression"/>.
		/// </summary>
		string ClrPropertyExpression(string ownerFqType, string propName, string val)
		{
			Type t = string.IsNullOrEmpty(ownerFqType) ? null : _resolver.GetPropertyType(ownerFqType, propName);
			if (t == null || t.IsGenericType) return ClrValueExpression(propName, val);
			if (t.IsEnum) return ClrEnumExpression(t, propName, val);
			if (!t.IsValueType || !ConverterBackedStructs.Contains(t.FullName))
				return ClrValueExpression(propName, val);
			string name = "global::" + t.FullName.Replace('+', '.');
			return $"({name})System.Windows.Markup.XamlValueConverter.ConvertByType(" +
				$"typeof({name}), \"{EscapeString(val)}\")";
		}

		// Mirrors EnumConverter: numbers, and case-insensitive names, comma-separated for flags.
		string ClrEnumExpression(Type t, string propName, string val)
		{
			string prefix = "global::" + t.FullName.Replace('+', '.');
			var members = t.GetFields(BindingFlags.Public | BindingFlags.Static).Where(f => f.IsLiteral).ToList();
			var parts = new List<string>();
			foreach (string raw in val.Split(','))
			{
				string p = raw.Trim();
				FieldInfo hit = members.FirstOrDefault(
					f => string.Equals(f.Name, p, StringComparison.OrdinalIgnoreCase));
				if (long.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
					parts.Add($"({prefix})({p})");
				else if (hit != null)
					parts.Add(prefix + "." + hit.Name);
				else
					return ClrValueExpression(propName, val);
			}
			return string.Join(" | ", parts);
		}
	}
}
