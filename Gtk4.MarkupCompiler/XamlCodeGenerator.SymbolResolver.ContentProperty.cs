using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>
		/// Reads <c>[ContentProperty]</c> from <paramref name="typeFullName"/> or its nearest base type
		/// and classifies the property as a collection (it has a public <c>Add</c>) or a single value.
		/// Returns <c>false</c> when there are no references, the type is unknown, no attribute is
		/// declared, or the named property does not exist.
		/// </summary>
		internal bool TryGetContentProperty(string typeFullName, out string name, out bool isCollection)
		{
			name = null;
			isCollection = false;
			if (!_hasReferences) return false;
			try
			{
				Type type = FindType(typeFullName);
				for (Type t = type; t != null; t = SafeBase(t))
				{
					string declared = ContentPropertyName(t);
					if (declared == null) continue;
					PropertyInfo pi = FindProperty(type, declared);
					if (pi == null) return false;
					name = declared;
					isCollection = IsAddable(pi.PropertyType);
					return true;
				}
			}
			catch { }
			return false;
		}

		/// <summary>True when the type exists and <paramref name="baseFullName"/> is in its base chain.</summary>
		internal bool DerivesFrom(string typeFullName, string baseFullName)
		{
			if (!_hasReferences) return false;
			try
			{
				for (Type t = FindType(typeFullName); t != null; t = SafeBase(t))
					if (t.FullName == baseFullName) return true;
			}
			catch { }
			return false;
		}

		/// <summary>
		/// True for <c>LineBreak</c> and any type carrying <c>[TrimSurroundingWhitespace]</c>.
		/// </summary>
		internal bool TrimsSurroundingWhitespace(string typeFullName)
		{
			if (typeFullName == "System.Windows.Documents.LineBreak") return true;
			if (!_hasReferences) return false;
			try
			{
				for (Type t = FindType(typeFullName); t != null; t = SafeBase(t))
					foreach (CustomAttributeData a in t.GetCustomAttributesData())
						if (a.AttributeType.Name == "TrimSurroundingWhitespaceAttribute") return true;
			}
			catch { }
			return false;
		}

		/// <summary>The type of the property, or <c>null</c> when unresolved.</summary>
		internal Type FindPropertyType(string typeFullName, string propName)
		{
			try
			{
				Type type = FindType(typeFullName);
				return type == null ? null : FindProperty(type, propName)?.PropertyType;
			}
			catch { return null; }
		}

		static Type SafeBase(Type t)
		{
			try { return t.BaseType; }
			catch { return null; }
		}

		static string ContentPropertyName(Type t)
		{
			foreach (CustomAttributeData a in t.GetCustomAttributesData())
			{
				if (a.AttributeType.Name != "ContentPropertyAttribute") continue;
				if (a.ConstructorArguments.Count > 0 && a.ConstructorArguments[0].Value is string s)
					return s;
				foreach (CustomAttributeNamedArgument n in a.NamedArguments)
					if (n.MemberName == "Name" && n.TypedValue.Value is string named) return named;
			}
			return null;
		}

		static PropertyInfo FindProperty(Type type, string propName)
		{
			for (Type t = type; t != null; t = SafeBase(t))
			{
				PropertyInfo pi;
				try
				{
					pi = t.GetProperty(propName,
						BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				}
				catch { return null; }
				if (pi != null) return pi;
			}
			return null;
		}

		static bool IsAddable(Type t)
		{
			if (t == typeof(string) || t == typeof(object)) return false;
			var types = new List<Type> { t };
			types.AddRange(t.GetInterfaces());
			foreach (Type x in types)
				foreach (MethodInfo m in x.GetMethods(BindingFlags.Public | BindingFlags.Instance))
					if (m.Name == "Add" && m.GetParameters().Length == 1) return true;
			return false;
		}
	}
}
