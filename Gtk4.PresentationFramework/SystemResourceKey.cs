using System.Reflection;

namespace System.Windows;

/// <summary>A resource key naming a <see cref="SystemParameters"/> value. Equal when the names are equal.</summary>
sealed class SystemResourceKey : ResourceKey
{
	readonly string _name;

	internal SystemResourceKey(string name) => _name = name;

	/// <inheritdoc/>
	public override Assembly Assembly => typeof(SystemParameters).Assembly;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is SystemResourceKey other && other._name == _name;

	/// <inheritdoc/>
	public override int GetHashCode() => _name.GetHashCode();

	/// <inheritdoc/>
	public override string ToString() => "SystemParameters." + _name;
}
