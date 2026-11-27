#pragma warning disable WPF0001 // ThemeMode is experimental
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Themes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The layer's PresentationFramework.Fluent theme dictionary (the .NET 9 Fluent resource keys).</summary>
public class FluentThemeTests : WpfGtkTestBase
{
	const string FluentUri = "pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml";

	static ResourceDictionary Load(string uri = FluentUri) =>
		new ResourceDictionary { Source = UnitConverters.ParseUri(uri) };

	[Fact]
	public void Fluent_dictionary_supplies_the_corner_radius_variables() => Run(() =>
	{
		var dictionary = Load();
		Assert.Equal(new CornerRadius(4), dictionary["ControlCornerRadius"]);
		Assert.Equal(new CornerRadius(8), dictionary["OverlayCornerRadius"]);
		Assert.Equal(new CornerRadius(8), dictionary["PopupCornerRadius"]);
	});

	[Fact]
	public void Fluent_dictionary_supplies_the_light_colours_and_brushes() => Run(() =>
	{
		var dictionary = Load();
		Assert.Equal(Color.FromArgb(0xE4, 0, 0, 0), ((SolidColorBrush)dictionary["TextFillColorPrimaryBrush"]!).Color);
		Assert.Equal(Color.FromArgb(0xFF, 0xFA, 0xFA, 0xFA), (Color)dictionary["ApplicationBackgroundColor"]!);
		Assert.IsType<Style>(dictionary["DefaultButtonStyle"]);
		Assert.IsType<Style>(dictionary["BodyTextBlockStyle"]);
		Assert.Equal("Segoe Fluent Icons, Segoe MDL2 Assets", ((FontFamily)dictionary["SymbolThemeFontFamily"]!).Source);
	});

	[Fact]
	public void Accent_brushes_resolve_through_the_system_accent_colours() => Run(() =>
	{
		var dictionary = Load();
		Assert.Equal(SystemColors.AccentColorDark1, ((SolidColorBrush)dictionary["AccentFillColorDefaultBrush"]!).Color);
		Assert.Equal(SystemColors.AccentColor, (Color)dictionary["SystemAccentColor"]!);
	});

	[Fact]
	public void The_Dark_variant_has_the_dark_colours() => Run(() =>
	{
		var dictionary = Load(FluentUri.Replace("Fluent.xaml", "Fluent.Dark.xaml"));
		Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), ((SolidColorBrush)dictionary["TextFillColorPrimaryBrush"]!).Color);
		Assert.Equal(new CornerRadius(4), dictionary["ControlCornerRadius"]);
	});

	[Fact]
	public void The_unqualified_dictionary_follows_the_application_ThemeMode() => Run(() =>
	{
		var app = Application.Current ?? new Application();
		app.ThemeMode = ThemeMode.Dark;
		try
		{
			Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF),
				((SolidColorBrush)Load()["TextFillColorPrimaryBrush"]!).Color);
		}
		finally { app.ThemeMode = ThemeMode.None; }
	});

	[Fact]
	public void Changing_ThemeMode_reloads_a_merged_Fluent_dictionary() => Run(() =>
	{
		var app = Application.Current ?? new Application();
		var merged = Load();
		app.Resources.MergedDictionaries.Add(merged);
		try
		{
			Assert.Equal(Color.FromArgb(0xE4, 0, 0, 0), ((SolidColorBrush)merged["TextFillColorPrimaryBrush"]!).Color);
			app.ThemeMode = ThemeMode.Dark;
			Assert.Equal(Color.FromRgb(0xFF, 0xFF, 0xFF), ((SolidColorBrush)merged["TextFillColorPrimaryBrush"]!).Color);
			app.ThemeMode = ThemeMode.Light;
			Assert.Equal(Color.FromArgb(0xE4, 0, 0, 0), ((SolidColorBrush)merged["TextFillColorPrimaryBrush"]!).Color);
		}
		finally
		{
			app.Resources.MergedDictionaries.Remove(merged);
			app.ThemeMode = ThemeMode.None;
		}
	});

	[Theory]
	[InlineData("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml", true, null)]
	[InlineData("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.Light.xaml", true, false)]
	[InlineData("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.Dark.xaml", true, true)]
	[InlineData("pack://application:,,,/MyApp;component/Themes/Fluent.xaml", false, null)]
	[InlineData("pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.HC.xaml", false, null)]
	[InlineData("Themes/Fluent.xaml", false, null)]
	public void Only_the_Fluent_assembly_uris_are_claimed(string uri, bool claimed, bool? dark)
	{
		Assert.Equal(claimed, FluentTheme.IsFluentUri(UnitConverters.ParseUri(uri), out bool? actualDark));
		if (claimed) Assert.Equal(dark, actualDark);
	}
}
