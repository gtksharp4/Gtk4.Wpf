using System;
using System.Linq;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>
		/// The C# type name of the value parameter of <c>Set{propName}(DependencyObject, value)</c> on
		/// <paramref name="ownerFqType"/>, or <c>null</c> when it is not found or no references are loaded.
		/// </summary>
		internal string GetAttachedSetterValueType(string ownerFqType, string propName)
		{
			if (!_hasReferences) return null;
			try
			{
				Type owner = FindType(ownerFqType);
				if (owner == null) return null;
				MethodInfo setter = owner
					.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
					.FirstOrDefault(m => m.Name == "Set" + propName && m.GetParameters().Length == 2);
				return setter == null ? null : CSharpName(setter.GetParameters()[1].ParameterType);
			}
			catch { return null; }
		}

		// Full name usable in generated code: nested types use '.', generics use <...>.
		static string CSharpName(Type t)
		{
			switch (t.FullName)
			{
				case "System.Double": return "double";
				case "System.Single": return "float";
				case "System.Int32": return "int";
				case "System.Int64": return "long";
				case "System.Boolean": return "bool";
				case "System.String": return "string";
				case "System.Object": return "object";
			}
			if (t.IsArray) return CSharpName(t.GetElementType()) + "[]";
			Type nullable = Nullable.GetUnderlyingType(t);
			if (nullable != null) return CSharpName(nullable) + "?";

			string name = t.Name;
			int tick = name.IndexOf('`');
			if (tick >= 0) name = name.Substring(0, tick);
			if (t.IsGenericType)
				name += "<" + string.Join(", ", t.GetGenericArguments().Select(CSharpName)) + ">";
			if (t.IsNested) return CSharpName(t.DeclaringType) + "." + name;
			return string.IsNullOrEmpty(t.Namespace) ? name : t.Namespace + "." + name;
		}
	}
}
