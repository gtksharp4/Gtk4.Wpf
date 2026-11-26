using System;
using System.Linq;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>True for a concrete class with a public parameterless constructor.</summary>
		internal bool IsInstantiable(string typeFullName)
		{
			if (!_hasReferences) return false;
			try
			{
				Type t = FindType(typeFullName);
				return t != null && t.IsClass && !t.IsAbstract
					&& t.GetConstructors().Any(c => c.GetParameters().All(p => p.IsOptional));
			}
			catch { return false; }
		}

		/// <summary>True when the named property has a public setter.</summary>
		internal bool IsWritable(string typeFullName, string propName)
		{
			try
			{
				Type t = FindType(typeFullName);
				PropertyInfo pi = t == null ? null : FindProperty(t, propName);
				return pi != null && pi.GetSetMethod() != null;
			}
			catch { return false; }
		}

		/// <summary>
		/// The parameter type of the single-argument public <c>Add</c> of a collection property; an empty
		/// <paramref name="propName"/> means the type itself is the collection.
		/// </summary>
		internal Type CollectionItemType(string typeFullName, string propName)
		{
			Type pt = propName.Length == 0 ? FindType(typeFullName) : FindPropertyType(typeFullName, propName);
			if (pt == null || !IsAddable(pt)) return null;
			try
			{
				var types = new System.Collections.Generic.List<Type> { pt };
				types.AddRange(pt.GetInterfaces());
				foreach (Type x in types)
				foreach (MethodInfo m in x.GetMethods(BindingFlags.Public | BindingFlags.Instance))
					if (m.Name == "Add" && m.GetParameters().Length == 1) return m.GetParameters()[0].ParameterType;
			}
			catch { }
			return null;
		}

		/// <summary>
		/// True when an instance of <paramref name="typeFullName"/> is assignable to <paramref name="target"/>.
		/// </summary>
		internal bool IsAssignable(string typeFullName, Type target)
		{
			try
			{
				Type t = FindType(typeFullName);
				return t != null && target != null && target.IsAssignableFrom(t);
			}
			catch { return false; }
		}

		/// <summary>True when <paramref name="typeFullName"/> has <paramref name="baseFullName"/> in its base chain.</summary>
		internal bool DerivesFromByName(string typeFullName, string baseFullName)
		{
			try
			{
				for (Type t = FindType(typeFullName); t != null; t = t.BaseType)
					if (t.FullName == baseFullName) return true;
			}
			catch { }
			return false;
		}
	}
}
