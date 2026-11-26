using System;
using System.ComponentModel;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace System.Windows;

/// <summary>The theme applied to an <see cref="Application"/> or <see cref="Window"/> (None, System, Light, Dark).</summary>
/// <remarks>Shape per dotnet/wpf release/9.0: <c>default(ThemeMode)</c> is <see cref="None"/>.</remarks>
#if NET8_0_OR_GREATER
[Experimental("WPF0001")]
#endif
[TypeConverter(typeof(ThemeModeConverter))]
public readonly struct ThemeMode : IEquatable<ThemeMode>
{
	readonly string? _value;

	/// <summary>No theme is applied.</summary>
	public static ThemeMode None => new ThemeMode();

	/// <summary>The light theme.</summary>
	public static ThemeMode Light => new ThemeMode("Light");

	/// <summary>The dark theme.</summary>
	public static ThemeMode Dark => new ThemeMode("Dark");

	/// <summary>The theme follows the system setting.</summary>
	public static ThemeMode System => new ThemeMode("System");

	/// <summary>Creates a theme mode with the given name; the name is not validated here.</summary>
	public ThemeMode(string value) => _value = value;

	/// <summary>The theme name; "None" for <c>default(ThemeMode)</c>.</summary>
	public string Value => _value ?? "None";

	/// <inheritdoc />
	public bool Equals(ThemeMode other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is ThemeMode other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => _value != null ? StringComparer.Ordinal.GetHashCode(_value) : 0;

	/// <summary>Value equality.</summary>
	public static bool operator ==(ThemeMode left, ThemeMode right) => left.Equals(right);

	/// <summary>Value inequality.</summary>
	public static bool operator !=(ThemeMode left, ThemeMode right) => !left.Equals(right);

	/// <inheritdoc />
	public override string ToString() => Value;
}
