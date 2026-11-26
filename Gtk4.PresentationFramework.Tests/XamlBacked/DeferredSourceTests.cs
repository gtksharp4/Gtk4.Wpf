using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

/// <summary>Deferred dictionary values stay deferred through Source and merged dictionaries.</summary>
public class DeferredSourceTests : WpfGtkTestBase
{
	const string Ns = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"";

	[Fact]
	public void A_merged_Source_window_constructs_and_the_unused_key_throws_when_read() => Run(() =>
	{
		var window = new Round58cMergedWindow();

		Assert.IsType<Style>(window.FindResource("Forward"));
		Assert.Throws<InvalidOperationException>(() => window.FindResource("Unused"));
	});

	[Fact]
	public void An_unkeyed_nested_Source_dictionary_keeps_its_values_deferred() => Run(() =>
	{
		var border = (Border)XamlReader.Parse("<Border " + Ns + "><Border.Resources>" +
			"<ResourceDictionary Source=\"XamlBacked/DeferredDictionary.xaml\"/></Border.Resources></Border>");

		Assert.Throws<InvalidOperationException>(() => border.FindResource("Unused"));
		Assert.Throws<InvalidOperationException>(() => border.Resources["Unused"]);
		Assert.NotSame(border.Resources["Unshared"], border.Resources["Unshared"]);
		Assert.Same(border.Resources["Later"], border.Resources["Later"]);
	});
}
