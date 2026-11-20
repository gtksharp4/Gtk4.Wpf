using System;
using System.Reflection;
using System.Windows.Input;

namespace System.Windows.Markup;

/// <summary>String to <see cref="ICommand"/> for the built-in command libraries (WPF CommandConverter).</summary>
static class CommandConverters
{
	static readonly Type[] _libraries =
	{
		typeof(ApplicationCommands), typeof(ComponentCommands), typeof(NavigationCommands),
		typeof(EditingCommands), typeof(MediaCommands),
	};

	/// <summary>Accepts "Open", "ApplicationCommands.Open" or a namespace-qualified form.</summary>
	internal static ICommand? ParseCommand(string raw)
	{
		string name = raw.Trim();
		if (name.Length == 0)
			return null;

		int dot = name.LastIndexOf('.');
		if (dot < 0)
		{
			foreach (Type library in _libraries)
			{
				if (Find(library, name) is ICommand found)
					return found;
			}
			throw NotSupported(raw);
		}

		string typeName = name.Substring(0, dot);
		int typeDot = typeName.LastIndexOf('.');
		if (typeDot >= 0)
			typeName = typeName.Substring(typeDot + 1);
		string member = name.Substring(dot + 1);
		foreach (Type library in _libraries)
		{
			if (string.Equals(library.Name, typeName, StringComparison.Ordinal))
				return Find(library, member) ?? throw NotSupported(raw);
		}
		throw NotSupported(raw);
	}

	static ICommand? Find(Type library, string member)
	{
		PropertyInfo? p = library.GetProperty(member,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
		return p?.GetValue(null) as ICommand;
	}

	static NotSupportedException NotSupported(string raw) =>
		new NotSupportedException($"CommandConverter cannot convert from \"{raw}\".");
}
