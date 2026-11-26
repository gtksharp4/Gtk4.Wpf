#pragma warning disable WPF0001 // layer-internal use of the experimental ThemeMode
using System.ComponentModel;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace System.Windows;

public partial class Application
{
	ThemeMode _themeMode = ThemeMode.None;

	/// <summary>
	/// The application theme. Light/Dark/System/None map to the GTK 4 <c>gtk-interface-color-scheme</c>
	/// (None and System use the platform default). The Fluent theme dictionaries WPF merges into
	/// <see cref="Resources"/> do not exist here, so only the GTK colour scheme changes.
	/// </summary>
	/// <exception cref="ArgumentException">The value is not None, System, Light or Dark.</exception>
#if NET8_0_OR_GREATER
	[Experimental("WPF0001")]
#endif
	[TypeConverter(typeof(ThemeModeConverter))]
	public ThemeMode ThemeMode
	{
		get => _themeMode;
		set
		{
			VerifyAccess();
			if (!ThemeModeGtk.IsValid(value))
				throw new ArgumentException($"ThemeMode value {value} is invalid. Use None, System, Light or Dark");
			_themeMode = value;
			ThemeModeGtk.Apply(value);
		}
	}
}
