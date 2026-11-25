#pragma warning disable WPF0001
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class ThemeModeTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void Default_is_None_and_equals_the_None_value()
	{
		Assert.Equal("None", default(ThemeMode).Value);
		Assert.True(default(ThemeMode) == ThemeMode.None);
		Assert.Equal(ThemeMode.None.GetHashCode(), default(ThemeMode).GetHashCode());
		Assert.Equal("None", ThemeMode.None.ToString());
	}

	[Fact]
	public void Named_values_compare_by_ordinal_value()
	{
		Assert.Equal("Light", ThemeMode.Light.Value);
		Assert.Equal("Dark", ThemeMode.Dark.Value);
		Assert.Equal("System", ThemeMode.System.Value);
		Assert.True(new ThemeMode("Dark") == ThemeMode.Dark);
		Assert.True(new ThemeMode("dark") != ThemeMode.Dark);
		Assert.True(ThemeMode.Light.Equals((object)new ThemeMode("Light")));
		Assert.False(ThemeMode.Light.Equals("Light"));
		Assert.True(new ThemeMode("None") == ThemeMode.None);
	}

	[Fact]
	public void Converter_round_trips_strings()
	{
		var converter = TypeDescriptor.GetConverter(typeof(ThemeMode));
		Assert.IsType<ThemeModeConverter>(converter);
		Assert.True(converter.CanConvertFrom(typeof(string)));
		Assert.False(converter.CanConvertFrom(typeof(int)));
		Assert.Equal(ThemeMode.Dark, converter.ConvertFromInvariantString("Dark"));
		Assert.Equal("Light", converter.ConvertToString(null, CultureInfo.InvariantCulture, ThemeMode.Light));
		Assert.Throws<NotSupportedException>(() => converter.ConvertFrom(null!));
		Assert.Throws<NotSupportedException>(() => converter.ConvertTo(ThemeMode.Dark, typeof(int)));
	}

	[Fact]
	public void Application_ThemeMode_defaults_to_None_and_stores_valid_values() => Run(() =>
	{
		var app = new Application();
		Assert.Equal(ThemeMode.None, app.ThemeMode);
		app.ThemeMode = ThemeMode.System;
		Assert.Equal(ThemeMode.System, app.ThemeMode);
		app.ThemeMode = ThemeMode.None;
	});

	[Fact]
	public void Application_ThemeMode_rejects_an_unknown_value() => Run(() =>
	{
		var app = new Application();
		var ex = Assert.Throws<ArgumentException>(() => app.ThemeMode = new ThemeMode("Sepia"));
		Assert.Contains("Sepia", ex.Message);
		Assert.Equal(ThemeMode.None, app.ThemeMode);
	});

	[Fact]
	public void ThemeMode_setters_verify_thread_affinity() => Run(() =>
	{
		var app = new Application();
		var window = new Window();
		var original = System.Windows.Threading.Dispatcher.IsOnDispatcherThread;
		System.Windows.Threading.Dispatcher.IsOnDispatcherThread = () => false;
		try
		{
			Assert.Throws<InvalidOperationException>(() => app.ThemeMode = ThemeMode.Dark);
			Assert.Throws<InvalidOperationException>(() => window.ThemeMode = ThemeMode.Dark);
			Assert.Throws<InvalidOperationException>(() => window.ThemeMode);
		}
		finally { System.Windows.Threading.Dispatcher.IsOnDispatcherThread = original; }
	});

	[Fact]
	public void Application_ThemeMode_sets_the_gtk_colour_scheme() => Run(() =>
	{
		var app = new Application();
		var settings = Gtk.Settings.Default;
		if (!ThemeModeGtk.HasColorScheme(settings))
		{
			// GTK before 4.20 has no interface colour scheme, only the prefer-dark flag.
			var darkBefore = settings.GtkApplicationPreferDarkTheme;
			try
			{
				app.ThemeMode = ThemeMode.Dark;
				Assert.True(settings.GtkApplicationPreferDarkTheme);
				app.ThemeMode = ThemeMode.Light;
				Assert.False(settings.GtkApplicationPreferDarkTheme);
				app.ThemeMode = ThemeMode.Dark;
				app.ThemeMode = ThemeMode.System;
				Assert.False(settings.GtkApplicationPreferDarkTheme);
			}
			finally { settings.GtkApplicationPreferDarkTheme = darkBefore; }
			return;
		}

		var before = settings.GtkInterfaceColorScheme;
		try
		{
			app.ThemeMode = ThemeMode.Dark;
			Assert.Equal(Gtk.InterfaceColorScheme.Dark, settings.GtkInterfaceColorScheme);
			app.ThemeMode = ThemeMode.Light;
			Assert.Equal(Gtk.InterfaceColorScheme.Light, settings.GtkInterfaceColorScheme);
			app.ThemeMode = ThemeMode.System;
			Assert.Equal(Gtk.InterfaceColorScheme.Default, settings.GtkInterfaceColorScheme);
		}
		finally { settings.GtkInterfaceColorScheme = before; }
	});

	[Fact]
	public void Window_ThemeMode_defaults_to_None_validates_and_is_store_only() => Run(() =>
	{
		var window = new Window();
		Assert.Equal(ThemeMode.None, window.ThemeMode);
		window.ThemeMode = ThemeMode.Dark;
		Assert.Equal(ThemeMode.Dark, window.ThemeMode);
		Assert.Throws<ArgumentException>(() => window.ThemeMode = new ThemeMode("x"));
		Assert.Equal(ThemeMode.Dark, window.ThemeMode);
	});

	[Fact]
	public void ThemeMode_properties_carry_the_converter()
	{
		foreach (var owner in new[] { typeof(Application), typeof(Window) })
		{
			var prop = owner.GetProperty("ThemeMode")!;
			var attr = (TypeConverterAttribute)Attribute.GetCustomAttribute(prop, typeof(TypeConverterAttribute))!;
			Assert.Contains("ThemeModeConverter", attr.ConverterTypeName);
		}
	}
}
