using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ApplicationLoadComponentTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void LoadComponent_returns_the_object_registered_for_a_relative_uri() => Run(() =>
	{
		var skin = new ResourceDictionary { ["Brand"] = "blue" };
		GtkFormsXamlRegistry.Register("Round49/BlueSkin.xaml", () => skin);

		var loaded = Application.LoadComponent(new Uri("Round49/BlueSkin.xaml", UriKind.Relative));

		Assert.Same(skin, loaded);
	});

	[Theory]
	[InlineData("pack://application:,,,/Round49/PackSkin.xaml")]
	[InlineData("/Round49/PackSkin.xaml")]
	[InlineData("/SomeAssembly;component/Round49/PackSkin.xaml")]
	public void LoadComponent_accepts_pack_and_component_forms(string uri) => Run(() =>
	{
		var skin = new ResourceDictionary();
		GtkFormsXamlRegistry.Register("Round49/PackSkin.xaml", () => skin);

		Assert.Same(skin, Application.LoadComponent(UnitConverters.ParseUri(uri)));
	});

	[Fact]
	public void LoadComponent_throws_ArgumentException_for_an_absolute_non_pack_uri() => Run(() =>
		Assert.Throws<ArgumentException>(() => Application.LoadComponent(new Uri("http://example.com/a.xaml"))));

	[Fact]
	public void LoadComponent_throws_ArgumentNullException_for_null() => Run(() =>
		Assert.Throws<ArgumentNullException>(() => Application.LoadComponent((Uri)null!)));

	[Fact]
	public void LoadComponent_throws_IOException_when_nothing_is_registered() => Run(() =>
		Assert.Throws<IOException>(() => Application.LoadComponent(new Uri("Round49/Missing.xaml", UriKind.Relative))));

	[Fact]
	public void LoadComponent_into_an_instance_runs_the_registered_loader() => Run(() =>
	{
		var target = new Button();
		object? seen = null;
		GtkFormsXamlRegistry.RegisterLoader("Round49/Page.xaml", c => seen = c);

		Application.LoadComponent(target, new Uri("Round49/Page.xaml", UriKind.Relative));

		Assert.Same(target, seen);
	});

	[Fact]
	public void LoadComponent_into_an_instance_throws_when_nothing_is_registered() => Run(() =>
		Assert.Throws<IOException>(() =>
			Application.LoadComponent(new Button(), new Uri("Round49/NoLoader.xaml", UriKind.Relative))));

	[Fact]
	public void Resources_is_settable_and_replaces_the_dictionary() => Run(() =>
	{
		var app = new Application();
		var replacement = new ResourceDictionary { ["K"] = 1 };

		app.Resources = replacement;

		Assert.Same(replacement, app.Resources);
		Assert.Equal(1, app.FindResource("K"));
	});

	[Fact]
	public void Resources_set_null_throws() => Run(() =>
		Assert.Throws<ArgumentNullException>(() => new Application().Resources = null!));

	[Fact]
	public void Replacing_Resources_refreshes_live_DynamicResource_references() => Run(() =>
	{
		var app = new Application();
		app.Resources["Fill"] = Brushes.Red;
		var block = new Border();
		block.SetResourceReference(Border.BackgroundProperty, "Fill");
		Assert.Equal(Colors.Red, ((SolidColorBrush)block.Background!).Color);

		app.Resources = new ResourceDictionary { ["Fill"] = Brushes.Blue };
		Assert.Equal(Colors.Blue, ((SolidColorBrush)block.Background!).Color);

		app.Resources["Fill"] = Brushes.Green;
		Assert.Equal(Colors.Green, ((SolidColorBrush)block.Background!).Color);
	});
}
