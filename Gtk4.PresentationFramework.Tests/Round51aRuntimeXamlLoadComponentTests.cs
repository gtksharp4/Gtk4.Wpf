using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Application.LoadComponent and ResourceDictionary.Source end to end, as a converted app uses them.</summary>
public class Round51aRuntimeXamlLoadComponentTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static Color ColorOf(Brush? brush) => ((SolidColorBrush)brush!).Color;

	static ResourceDictionary Skin(Color color) => new ResourceDictionary { ["Fill"] = new SolidColorBrush(color) };

	static void RegisterSkins()
	{
		GtkFormsXamlRegistry.Register("Round51a/Blue.xaml", () => Skin(Colors.Blue));
		GtkFormsXamlRegistry.Register("Round51a/Yellow.xaml", () => Skin(Colors.Yellow));
	}

	[Fact]
	public void LoadComponent_returns_a_new_generated_instance_per_call() => Run(() =>
	{
		RegisterSkins();
		var uri = new Uri("Round51a/Blue.xaml", UriKind.Relative);

		var first = Assert.IsType<ResourceDictionary>(Application.LoadComponent(uri));
		var second = Assert.IsType<ResourceDictionary>(Application.LoadComponent(uri));

		Assert.NotSame(first, second);
		Assert.Equal(Colors.Blue, ColorOf((Brush)first["Fill"]!));
	});

	[Fact]
	public void LoadComponent_resolves_a_pack_uri_to_the_same_registration() => Run(() =>
	{
		RegisterSkins();

		var loaded = Application.LoadComponent(UnitConverters.ParseUri("pack://application:,,,/Round51a/Yellow.xaml"));

		Assert.Equal(Colors.Yellow, ColorOf((Brush)((ResourceDictionary)loaded)["Fill"]!));
	});

	[Fact]
	public void LoadComponent_unknown_uri_throws_IOException_naming_the_uri() => Run(() =>
	{
		var ex = Assert.Throws<IOException>(
			() => Application.LoadComponent(new Uri("Round51a/Nope.xaml", UriKind.Relative)));

		Assert.Contains("Round51a/Nope.xaml", ex.Message);
	});

	[Fact]
	public void Swapping_the_Application_Resources_with_a_loaded_skin_refreshes_dependants() => Run(() =>
	{
		RegisterSkins();
		var app = new Application();
		app.Resources = (ResourceDictionary)Application.LoadComponent(new Uri("Round51a/Blue.xaml", UriKind.Relative));
		var border = new Border();
		border.SetResourceReference(Border.BackgroundProperty, "Fill");
		Assert.Equal(Colors.Blue, ColorOf(border.Background));

		app.Resources = (ResourceDictionary)Application.LoadComponent(
			new Uri("Round51a/Yellow.xaml", UriKind.Relative));

		Assert.Equal(Colors.Yellow, ColorOf(border.Background));
	});

	[Fact]
	public void Setting_Source_on_a_merged_dictionary_refreshes_dependants_on_each_swap() => Run(() =>
	{
		RegisterSkins();
		var app = new Application();
		var merged = new ResourceDictionary();
		app.Resources.MergedDictionaries.Add(merged);
		var border = new Border();
		border.SetResourceReference(Border.BackgroundProperty, "Fill");

		merged.Source = new Uri("Round51a/Blue.xaml", UriKind.Relative);
		Assert.Equal(Colors.Blue, ColorOf(border.Background));

		merged.Source = new Uri("Round51a/Yellow.xaml", UriKind.Relative);
		Assert.Equal(Colors.Yellow, ColorOf(border.Background));
	});

	[Fact]
	public void Source_swap_drops_keys_the_new_dictionary_does_not_define() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Round51a/WithOld.xaml",
			() => new ResourceDictionary { ["Old"] = 1, ["Fill"] = Brushes.Red });
		GtkFormsXamlRegistry.Register("Round51a/WithoutOld.xaml", () => Skin(Colors.Green));
		var merged = new ResourceDictionary();
		merged.Source = new Uri("Round51a/WithOld.xaml", UriKind.Relative);

		merged.Source = new Uri("Round51a/WithoutOld.xaml", UriKind.Relative);

		Assert.False(merged.Contains("Old"));
		Assert.Equal(new Uri("Round51a/WithoutOld.xaml", UriKind.Relative), merged.Source);
	});

	[Fact]
	public void Source_with_an_unknown_uri_throws_IOException_and_keeps_the_entries() => Run(() =>
	{
		RegisterSkins();
		var merged = new ResourceDictionary { Source = new Uri("Round51a/Blue.xaml", UriKind.Relative) };

		Assert.Throws<IOException>(() => merged.Source = new Uri("Round51a/Nope.xaml", UriKind.Relative));

		Assert.True(merged.Contains("Fill"));
	});

	[Fact]
	public void Element_in_a_tree_follows_a_merged_dictionary_source_swap() => Run(() =>
	{
		RegisterSkins();
		var app = new Application();
		var merged = new ResourceDictionary { Source = new Uri("Round51a/Blue.xaml", UriKind.Relative) };
		app.Resources.MergedDictionaries.Add(merged);
		var panel = new StackPanel();
		var button = new Button();
		panel.Children.Add(button);
		button.SetResourceReference(Control.BackgroundProperty, "Fill");

		merged.Source = new Uri("Round51a/Yellow.xaml", UriKind.Relative);

		Assert.Equal(Colors.Yellow, ColorOf(button.Background));
	});
}
