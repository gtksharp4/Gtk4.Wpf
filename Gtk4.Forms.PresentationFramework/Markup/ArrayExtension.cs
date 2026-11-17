using System;
using System.Collections.Generic;

namespace System.Windows.Markup;

/// <summary>
/// Runtime representation of <c>{x:Array Type=...}</c>.
/// <see cref="ProvideValue"/> returns a typed array built from <see cref="Items"/>.
/// </summary>
public class ArrayExtension : MarkupExtension
{
	/// <summary>The element type of the array to create.</summary>
	public Type? Type { get; set; }

	/// <summary>Items to populate the array with.</summary>
	public List<object?> Items { get; } = new List<object?>();

	/// <summary>Creates an empty extension.</summary>
	public ArrayExtension() { }

	/// <summary>Creates the extension with the given element type.</summary>
	public ArrayExtension(Type type) => Type = type ?? throw new ArgumentNullException(nameof(type));

	/// <inheritdoc/>
	/// <exception cref="InvalidOperationException">When <see cref="Type"/> is not set.</exception>
	public override object? ProvideValue(IServiceProvider? serviceProvider)
	{
		if (Type == null)
			throw new InvalidOperationException("ArrayExtension: Type must be set.");

		Array array = Array.CreateInstance(Type, Items.Count);
		for (int i = 0; i < Items.Count; i++)
			array.SetValue(Items[i], i);
		return array;
	}
}
