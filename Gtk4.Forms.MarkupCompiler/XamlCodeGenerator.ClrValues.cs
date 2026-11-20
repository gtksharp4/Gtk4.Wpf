using System;

namespace Gtk4.Forms.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// C# expression for a plain attribute value on a CLR (non-DP) property. Enum and struct property
		/// types (<c>Rect</c>, <c>TileMode</c>, ...) are converted to their type; everything else keeps the
		/// literal rules of <see cref="ClrValueExpression"/>.
		/// </summary>
		string ClrPropertyExpression(string ownerFqType, string propName, string val)
		{
			Type t = string.IsNullOrEmpty(ownerFqType) ? null : _resolver.GetPropertyType(ownerFqType, propName);
			if (t == null || t.IsGenericType) return ClrValueExpression(propName, val);
			if (t.IsEnum) return EnumExpression(t, val.Trim());
			if (!t.IsValueType || t.IsPrimitive || t == typeof(decimal)) return ClrValueExpression(propName, val);
			string name = "global::" + t.FullName.Replace('+', '.');
			return $"({name})System.Windows.Markup.XamlValueConverter.ConvertByType(" +
				$"typeof({name}), \"{EscapeString(val)}\")";
		}
	}
}
