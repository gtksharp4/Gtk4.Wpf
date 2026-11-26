using System;
using System.Reflection;

namespace Gtk4.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>
		/// True when <paramref name="ownerFullName"/> declares (or inherits) the routed event <c>{name}Event</c>.
		/// <paramref name="handlerType"/> is the delegate type of its CLR event wrapper, or the second parameter
		/// of its static <c>Add{name}Handler</c> (attached events), or <c>RoutedEventHandler</c> when neither
		/// exists. It is a <c>global::</c>-qualified C# name.
		/// </summary>
		internal bool TryGetRoutedEvent(string ownerFullName, string name, out string handlerType)
		{
			handlerType = null;
			if (!_hasReferences) return false;
			try
			{
				Type owner = FindType(ownerFullName);
				FieldInfo field = owner?.GetField(name + "Event",
					BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				if (field == null || field.FieldType.Name != "RoutedEvent") return false;
				for (Type t = owner; t != null; t = SafeBase(t))
				{
					EventInfo ev = t.GetEvent(
						name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (ev == null) continue;
					handlerType = GlobalName(ev.EventHandlerType);
					return true;
				}
				MethodInfo adder = owner.GetMethod("Add" + name + "Handler",
					BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
				ParameterInfo[] ps = adder?.GetParameters();
				if (ps != null && ps.Length == 2 && AcceptsDependencyObject(ps[0].ParameterType))
				{
					handlerType = GlobalName(ps[1].ParameterType);
					return true;
				}
				handlerType = "global::System.Windows.RoutedEventHandler";
				return true;
			}
			catch { return false; }
		}

		// By name: the layer assemblies can be loaded more than once, so IsAssignableFrom is unreliable.
		static bool AcceptsDependencyObject(Type t)
			=> t.FullName == "System.Windows.DependencyObject"
				|| t.FullName == "System.Windows.Threading.DispatcherObject"
				|| t.FullName == "System.Object";

		static string GlobalName(Type t)
		{
			string name = (t.FullName ?? t.Name).Replace('+', '.');
			if (!t.IsGenericType) return "global::" + name;
			string[] args = Array.ConvertAll(t.GetGenericArguments(), GlobalName);
			return "global::" + name.Substring(0, name.IndexOf('`')) + "<" + string.Join(", ", args) + ">";
		}
	}
}
