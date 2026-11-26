#pragma warning disable WPF0001 // layer-internal use of the experimental ThemeMode
using System.ComponentModel;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace System.Windows;

public partial class Window
{
	ThemeMode _themeMode = ThemeMode.None;

	/// <summary>
	/// The window theme. Store-only: GTK 4 has one colour scheme per display, so a per-window theme
	/// cannot be applied; set <see cref="Application.ThemeMode"/> to change the scheme.
	/// </summary>
	/// <exception cref="ArgumentException">The value is not None, System, Light or Dark.</exception>
#if NET8_0_OR_GREATER
	[Experimental("WPF0001")]
#endif
	[TypeConverter(typeof(ThemeModeConverter))]
	public ThemeMode ThemeMode
	{
		get
		{
			VerifyAccess();
			return _themeMode;
		}
		set
		{
			VerifyAccess();
			if (!ThemeModeGtk.IsValid(value))
				throw new ArgumentException($"ThemeMode value {value} is invalid. Use None, System, Light or Dark");
			_themeMode = value;
		}
	}
}
