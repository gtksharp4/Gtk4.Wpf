using System;
using System.Reflection;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{x:Static Member}</c>.
/// <see cref="ProvideValue"/> returns the value of the named static field or property
/// via reflection.
/// </summary>
public class StaticExtension : MarkupExtension
{
	/// <summary>
	/// Fully-qualified member in the form <c>TypeName.MemberName</c> or
	/// <c>Namespace.TypeName.MemberName</c> (assembly-qualified type names are also accepted).
	/// </summary>
	public string? Member { get; set; }

	/// <summary>Creates an empty extension (Member set separately).</summary>
	public StaticExtension() { }

	/// <summary>Creates the extension with the given member string (positional parameter).</summary>
	public StaticExtension(string member) => Member = member ?? throw new ArgumentNullException(nameof(member));

	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">When <see cref="Member"/> is null/empty.</exception>
	/// <exception cref="InvalidOperationException">When the type or member cannot be resolved.</exception>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (string.IsNullOrEmpty(Member))
			throw new InvalidOperationException("StaticExtension: Member must be set.");

		// Split at the last dot to separate TypeName from MemberName.
		int lastDot = Member!.LastIndexOf('.');
		if (lastDot < 0)
			throw new InvalidOperationException(
				$"x:Static: Member '{Member}' must be in the form 'TypeName.MemberName'.");

		string typePart = Member.Substring(0, lastDot);
		string memberPart = Member.Substring(lastDot + 1);

		// Resolve type via IXamlTypeResolver when available, then fall back to Type.GetType.
		Type? ownerType = null;
		if (serviceProvider?.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver resolver)
		{
			try { ownerType = resolver.Resolve(typePart); } catch { /* fall through */ }
		}
		ownerType ??= System.Type.GetType(typePart, throwOnError: false);

		if (ownerType == null)
			throw new InvalidOperationException(
				$"x:Static: cannot resolve type '{typePart}' from Member '{Member}'.");

		// Field?
		FieldInfo? field = ownerType.GetField(memberPart,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		if (field != null) return field.GetValue(null);

		// Property?
		PropertyInfo? prop = ownerType.GetProperty(memberPart,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		if (prop != null) return prop.GetValue(null);

		throw new InvalidOperationException(
			$"x:Static: '{ownerType.FullName}' has no public static field or property named '{memberPart}'.");
	}
}
