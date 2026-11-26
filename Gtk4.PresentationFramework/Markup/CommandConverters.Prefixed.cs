using System;
using System.Reflection;
using System.Windows.Input;

namespace System.Windows.Markup;

static partial class CommandConverters
{
	/// <summary>
	/// WPF CommandConverter with an <c>IXamlTypeResolver</c>: <c>prefix:Type.Member</c> resolves the type through
	/// <paramref name="resolveType"/> and reads its public static <c>Member</c> or <c>MemberCommand</c>
	/// (property or field). Anything else behaves as <see cref="ParseCommand(string)"/>.
	/// </summary>
	internal static ICommand? ParseCommand(string raw, Func<string, Type?> resolveType)
	{
		string name = raw.Trim();
		int colon = name.IndexOf(':');
		int dot = name.IndexOf('.');
		if (colon <= 0 || dot < colon)
			return ParseCommand(raw);

		Type? owner = resolveType(name.Substring(0, dot));
		string member = name.Substring(dot + 1);
		if (owner != null)
		{
			foreach (string candidate in new[] { member, member + "Command" })
			{
				if (FindStatic(owner, candidate) is ICommand found)
					return found;
			}
		}
		throw NotSupported(raw);
	}

	static object? FindStatic(Type type, string member)
	{
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
		return type.GetProperty(member, flags)?.GetValue(null) ?? type.GetField(member, flags)?.GetValue(null);
	}
}
