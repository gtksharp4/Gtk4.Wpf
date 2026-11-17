using System;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{x:Type TypeName}</c>.
/// <see cref="ProvideValue"/> returns the resolved <see cref="Type"/>.
/// </summary>
public class TypeExtension : MarkupExtension
{
	/// <summary>The CLR type to return. Takes precedence over <see cref="TypeName"/>.</summary>
	public Type? Type { get; set; }

	/// <summary>
	/// Fully-qualified or assembly-qualified type name. Used when <see cref="Type"/> is null.
	/// Resolution is attempted via <see cref="System.Type.GetType(string)"/>.
	/// </summary>
	public string? TypeName { get; set; }

	/// <summary>Creates an empty extension.</summary>
	public TypeExtension() { }

	/// <summary>Creates the extension for the given type (positional parameter).</summary>
	public TypeExtension(Type type) => Type = type ?? throw new ArgumentNullException(nameof(type));

	/// <summary>Creates the extension for a type name string.</summary>
	public TypeExtension(string typeName) => TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));

	/// <inheritdoc/>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (Type != null) return Type;

		if (TypeName != null)
		{
			// Try IXamlTypeResolver first (honours xmlns mappings in a live XAML context).
			if (serviceProvider?.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver resolver)
				return resolver.Resolve(TypeName);

			var t = System.Type.GetType(TypeName, throwOnError: false);
			if (t != null) return t;
			throw new InvalidOperationException(
				$"x:Type: cannot resolve type '{TypeName}'.");
		}

		throw new InvalidOperationException(
			"TypeExtension: either Type or TypeName must be set.");
	}
}
