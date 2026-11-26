using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pins <see cref="System.Windows.Application.StartupUriToTypeName"/> — the decision that maps a WPF
/// StartupUri to the compiled Window type name that Application.Run instantiates.
/// </summary>
public class StartupUriResolutionTests
{
	[Theory]
	[InlineData("MainWindow.xaml", "MainWindow")]
	[InlineData("Views/MainWindow.xaml", "MainWindow")]
	[InlineData("Views\\Shell.xaml", "Shell")]
	[InlineData("sub/folder/Login.xaml", "Login")]
	[InlineData("NoExtensionWindow", "NoExtensionWindow")]
	public void Relative_uri_reduces_to_leaf_type_name(string uri, string expected)
	{
		var result = System.Windows.Application.StartupUriToTypeName(
			new Uri(uri, UriKind.RelativeOrAbsolute));

		Assert.Equal(expected, result);
	}

	[Fact]
	public void Absolute_file_uri_reduces_to_leaf_type_name()
	{
		var uri = new Uri("file:///C:/app/Views/MainWindow.xaml", UriKind.Absolute);

		Assert.Equal("MainWindow", System.Windows.Application.StartupUriToTypeName(uri));
	}

	[Fact]
	public void Extension_match_is_case_insensitive()
	{
		Assert.Equal("MainWindow", System.Windows.Application.StartupUriToTypeName(
			new Uri("MainWindow.XAML", UriKind.RelativeOrAbsolute)));
	}
}