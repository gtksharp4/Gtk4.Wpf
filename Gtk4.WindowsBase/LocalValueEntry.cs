using System;

namespace System.Windows;

/// <summary>A dependency property and its local value. Mirrors <c>System.Windows.LocalValueEntry</c>.</summary>
public readonly struct LocalValueEntry
{
	internal LocalValueEntry(DependencyProperty property, object? value)
	{
		Property = property;
		Value = value;
	}

	/// <summary>The property that has a local value.</summary>
	public DependencyProperty Property { get; }

	/// <summary>The local value.</summary>
	public object? Value { get; }

	/// <summary>Compares two entries for equality.</summary>
	public static bool operator ==(LocalValueEntry obj1, LocalValueEntry obj2) => obj1.Equals(obj2);

	/// <summary>Compares two entries for inequality.</summary>
	public static bool operator !=(LocalValueEntry obj1, LocalValueEntry obj2) => !obj1.Equals(obj2);

	/// <inheritdoc/>
	public override bool Equals(object? obj) =>
		obj is LocalValueEntry e && ReferenceEquals(Property, e.Property) && Equals(Value, e.Value);

	/// <inheritdoc/>
	public override int GetHashCode() => (Property?.GetHashCode() ?? 0) ^ (Value?.GetHashCode() ?? 0);
}
