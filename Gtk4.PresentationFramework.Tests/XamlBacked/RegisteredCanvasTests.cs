using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

public class RegisteredCanvasTests : WpfGtkTestBase
{
	static readonly Uri _uri = new Uri("XamlBacked/RegisteredCanvas.xaml", UriKind.Relative);

	[Fact]
	public void Compiled_root_is_resolved_by_its_xaml_path_and_populated() => Run(() =>
	{
		var canvas = Assert.IsType<RegisteredCanvas>(
			GtkFormsXamlRegistry.TryCreate(_uri, typeof(RegisteredCanvas).Assembly));

		Assert.Single(canvas.Children);
		Assert.IsType<Button>(canvas.Children[0]);
	});

	[Fact]
	public void LoadComponent_by_uri_returns_a_populated_root() => Run(() =>
	{
		var canvas = Assert.IsType<RegisteredCanvas>(Application.LoadComponent(_uri));

		Assert.Single(canvas.Children);
	});

	[Fact]
	public void Initialize_on_an_already_created_root_keeps_a_single_child() => Run(() =>
	{
		var canvas = Assert.IsType<RegisteredCanvas>(
			GtkFormsXamlRegistry.TryCreate(_uri, typeof(RegisteredCanvas).Assembly));

		GtkFormsXamlRegistry.Initialize(canvas);
		GtkFormsXamlRegistry.Initialize(canvas);

		Assert.Single(canvas.Children);
	});
}
