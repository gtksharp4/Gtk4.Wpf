using System;
using System.Reflection;

namespace Gtk4.Forms.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>True when the type has a public static property or field of that name.</summary>
		internal bool HasStaticMember(string typeFullName, string member)
		{
			if (!_hasReferences) return false;
			try
			{
				Type t = FindType(typeFullName);
				const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
				return t != null && (t.GetProperty(member, flags) != null || t.GetField(member, flags) != null);
			}
			catch { return false; }
		}

		/// <summary>True when the type or a base declares <c>[RuntimeNameProperty("Name")]</c>.</summary>
		internal bool HasRuntimeNameProperty(string typeFullName)
		{
			if (!_hasReferences) return false;
			try
			{
				for (Type t = FindType(typeFullName); t != null; t = SafeBase(t))
					foreach (CustomAttributeData a in t.GetCustomAttributesData())
						if (a.AttributeType.Name == "RuntimeNamePropertyAttribute"
							&& a.ConstructorArguments.Count > 0 && (a.ConstructorArguments[0].Value as string) == "Name")
							return true;
			}
			catch { }
			return false;
		}
	}
}
