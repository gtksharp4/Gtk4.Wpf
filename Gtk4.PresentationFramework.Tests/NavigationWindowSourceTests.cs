using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class SourcePage : Page { }

public class NavigationWindowSourceTests : WpfGtkTestBase
{
	[Fact]
	public void SourceProperty_SharesFrameOwnerChain()
	{
		Assert.Same(Frame.SourceProperty, NavigationWindow.SourceProperty);
		Assert.IsAssignableFrom<FrameworkPropertyMetadata>(
			NavigationWindow.SourceProperty.GetMetadata(typeof(NavigationWindow)));
		Assert.Equal(typeof(Uri), NavigationWindow.SourceProperty.PropertyType);
		Assert.Null(NavigationWindow.SourceProperty.GetMetadata(typeof(NavigationWindow)).DefaultValue);
	}

	[Fact]
	public void Source_NavigatesTheService_AndReflectsTheCurrentUri() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("SourcePageA.xaml", () => new SourcePage());
		var window = new NavigationWindow();

		window.Source = new Uri("SourcePageA.xaml", UriKind.Relative);

		Assert.IsType<SourcePage>(window.Content);
		Assert.Equal(new Uri("SourcePageA.xaml", UriKind.Relative), window.NavigationService.CurrentSource);
		Assert.Equal(new Uri("SourcePageA.xaml", UriKind.Relative), window.Source);
	});

	[Fact]
	public void NavigatingByUri_UpdatesSource_WithoutNavigatingTwice() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("SourcePageB.xaml", () => new SourcePage());
		var window = new NavigationWindow();
		int count = 0;
		window.Navigated += (s, e) => count++;

		window.Navigate(new Uri("SourcePageB.xaml", UriKind.Relative));

		Assert.Equal(new Uri("SourcePageB.xaml", UriKind.Relative), window.Source);
		Assert.Equal(1, count);
	});

	[Fact]
	public void NavigatingToAnObject_ClearsSource() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("SourcePageC.xaml", () => new SourcePage());
		var window = new NavigationWindow { Source = new Uri("SourcePageC.xaml", UriKind.Relative) };

		window.Navigate(new SourcePage());

		Assert.Null(window.Source);
	});

	[Fact]
	public void StringConversion_YieldsARelativeUri()
	{
		var value = XamlValueConverter.Convert(typeof(NavigationWindow), NavigationWindow.SourceProperty, "Page.xaml");

		Assert.Equal(new Uri("Page.xaml", UriKind.Relative), value);
	}
}
