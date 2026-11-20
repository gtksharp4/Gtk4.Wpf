using System;
using System.Reflection;

namespace Gtk4.Forms.MarkupCompiler
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
				return t != null && t.IsClass && !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null;
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

		/// <summary>The parameter type of the single-argument public <c>Add</c> of a collection property.</summary>
		internal Type CollectionItemType(string typeFullName, string propName)
		{
			Type pt = FindPropertyType(typeFullName, propName);
			if (pt == null || !IsAddable(pt)) return null;
			try
			{
				foreach (MethodInfo m in pt.GetMethods(BindingFlags.Public | BindingFlags.Instance))
					if (m.Name == "Add" && m.GetParameters().Length == 1) return m.GetParameters()[0].ParameterType;
			}
			catch { }
			return null;
		}

		/// <summary>True when an instance of <paramref name="typeFullName"/> can be assigned to <paramref name="target"/>.</summary>
		internal bool IsAssignable(string typeFullName, Type target)
		{
			try
			{
				Type t = FindType(typeFullName);
				return t != null && target != null && target.IsAssignableFrom(t);
			}
			catch { return false; }
		}
	}
}
