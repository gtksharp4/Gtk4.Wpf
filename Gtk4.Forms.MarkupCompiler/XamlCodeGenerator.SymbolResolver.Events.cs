using System;
using System.Reflection;

namespace Gtk4.Forms.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		/// <summary>
		/// True when <paramref name="ownerFullName"/> declares (or inherits) the routed event <c>{name}Event</c>.
		/// <paramref name="handlerType"/> is the delegate type of its CLR event wrapper, or <c>RoutedEventHandler</c>
		/// when there is none. A generic delegate type is not supported and answers false.
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
					EventInfo ev = t.GetEvent(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
					if (ev == null) continue;
					if (ev.EventHandlerType.IsGenericType) return false;
					handlerType = ev.EventHandlerType.FullName.Replace('+', '.');
					return true;
				}
				handlerType = "System.Windows.RoutedEventHandler";
				return true;
			}
			catch { return false; }
		}
	}
}
