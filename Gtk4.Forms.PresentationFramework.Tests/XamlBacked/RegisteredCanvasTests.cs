using System;
using System.Reflection;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.XamlBacked;

public class RegisteredCanvasTests : WpfGtkTestBase
{
	[Fact]
	public void Compiled_root_is_resolved_by_its_xaml_path_and_initializes_once() => Run(() =>
	{
		var uri = new Uri("XamlBacked/RegisteredCanvas.xaml", UriKind.Relative);

		var canvas = Assert.IsType<RegisteredCanvas>(GtkFormsXamlRegistry.TryCreate(uri, typeof(RegisteredCanvas).Assembly));

		var init = typeof(RegisteredCanvas).GetMethod("InitializeComponent", BindingFlags.Public | BindingFlags.Instance)!;
		init.Invoke(canvas, null);
		init.Invoke(canvas, null);
		Assert.Single(canvas.Children);
	});
}
