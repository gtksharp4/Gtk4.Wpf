using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace System.Windows.Markup;

/// <summary>String to <see cref="ICommand"/> for the built-in command libraries (WPF CommandConverter).</summary>
static partial class CommandConverters
{
	// WPF's lookup order.
	static readonly Type[] _libraries =
	{
		typeof(ApplicationCommands), typeof(ComponentCommands), typeof(NavigationCommands),
		typeof(MediaCommands), typeof(EditingCommands),
	};

	// Controls that own commands; only the qualified form ("Slider.DecreaseSmall") names them.
	static readonly Type[] _controlOwners = { typeof(Slider) };

	/// <summary>
	/// Accepts "Open" or "ApplicationCommands.Open" (case-sensitive); the empty string is null.
	/// </summary>
	/// <remarks>
	/// Prefixed owners (<c>local:MyCommands.Exit</c>) need a type resolver: see the two-argument overload.
	/// </remarks>
	internal static ICommand? ParseCommand(string raw)
	{
		if (raw.Length == 0)
			return null;

		string name = raw.Trim();
		int dot = name.IndexOf('.');
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
		string member = name.Substring(dot + 1);
		foreach (Type library in _libraries.Concat(_controlOwners))
		{
			if (string.Equals(library.Name, typeName, StringComparison.Ordinal))
				return Find(library, member) ?? throw NotSupported(raw);
		}
		throw NotSupported(raw);
	}

	static ICommand? Find(Type library, string member)
	{
		PropertyInfo? p = library.GetProperty(member, BindingFlags.Public | BindingFlags.Static);
		return p?.GetValue(null) as ICommand;
	}

	static NotSupportedException NotSupported(string raw) =>
		new NotSupportedException($"CommandConverter cannot convert from \"{raw}\".");
}
