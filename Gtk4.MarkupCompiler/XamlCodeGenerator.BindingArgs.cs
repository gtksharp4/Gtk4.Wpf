using System;
using System.Collections.Generic;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		/// <summary>
		/// Handles the binding arguments beyond the core set (ConverterParameter, ConverterCulture,
		/// Source and the validation flags). Returns the C# initializer fragment, or null when
		/// <paramref name="name"/> is not one of them or the value cannot be emitted.
		/// </summary>
		internal string ExtraBindingInit(string name, string value)
		{
			switch (name)
			{
				case "ConverterParameter":
					return "ConverterParameter = " + BindingValueExpr(value);
				case "Source":
					return "Source = " + BindingValueExpr(value);
				case "XPath":
					return "XPath = \"" + EscapeString(Unquote(value)) + "\"";
				case "ConverterCulture":
					return "ConverterCulture = System.Globalization.CultureInfo.GetCultureInfo(\""
						+ EscapeString(Unquote(value)) + "\")";
				case "ValidatesOnDataErrors":
				case "ValidatesOnExceptions":
				case "ValidatesOnNotifyDataErrors":
				case "NotifyOnValidationError":
				case "BindsDirectlyToSource":
					return bool.TryParse(value.Trim(), out bool b)
						? name + " = " + (b ? "true" : "false")
						: null;
				default:
					return null;
			}
		}

		/// <summary>
		/// Parses a <c>Name=value</c> binding argument; returns false when it is not an extra argument.
		/// </summary>
		bool TryExtraBindingArg(string arg, List<string> inits)
		{
			int eq = arg.IndexOf('=');
			if (eq <= 0) return false;
			string init = ExtraBindingInit(arg.Substring(0, eq).Trim(), arg.Substring(eq + 1).Trim());
			if (init == null) return false;
			inits.Add(init);
			return true;
		}

		/// <summary>
		/// C# expression for a binding argument value: <c>{x:Null}</c>, <c>{x:Type}</c>, <c>{x:Static}</c>
		/// member, <c>{StaticResource}</c> lookup, or a string literal (a leading <c>{}</c> escape is stripped).
		/// </summary>
		/// <exception cref="XamlCodeGeneratorException">
		/// <c>{DynamicResource}</c> (not valid on a Binding) or any other unsupported markup extension.
		/// </exception>
		string BindingValueExpr(string raw)
		{
			string t = raw.Trim();
			bool quoted = t.Length >= 2 && t[0] == '\'' && t[t.Length - 1] == '\'';
			if (!quoted)
			{
				if (IsXNull(t)) return "null";
				if (IsXType(t, out string typeofExpr, _prefixMap)) return typeofExpr;
				if (IsXStatic(t, out string staticExpr, _prefixMap)) return staticExpr;
				if (t.StartsWith("{DynamicResource", StringComparison.Ordinal))
					throw new XamlCodeGeneratorException(
						"{DynamicResource} is not valid on a Binding argument: " + t);
				if (IsStaticOrDynamicResource(t, out string key))
					return StaticResourceLookup("this", EscapeString(key));
				if (t.StartsWith("{", StringComparison.Ordinal) && !t.StartsWith("{}", StringComparison.Ordinal))
					throw new XamlCodeGeneratorException("Unsupported markup extension on a Binding argument: " + t);
			}
			string literal = Unquote(t);
			if (literal.StartsWith("{}", StringComparison.Ordinal)) literal = literal.Substring(2);
			return "\"" + EscapeString(literal) + "\"";
		}

		/// <summary>A string-valued binding argument: XAML quotes removed, then a leading {} escape dropped.</summary>
		static string BindingStringArg(string raw)
		{
			string literal = Unquote(raw);
			return literal.StartsWith("{}", StringComparison.Ordinal) ? literal.Substring(2) : literal;
		}

		static string Unquote(string s)
		{
			string t = s.Trim();
			if (t.Length >= 2 && t[0] == '\'' && t[t.Length - 1] == '\'')
				return t.Substring(1, t.Length - 2);
			return t;
		}
	}
}
