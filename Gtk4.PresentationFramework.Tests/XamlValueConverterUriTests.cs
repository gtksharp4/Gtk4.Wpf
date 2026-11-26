using System;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>String-to-<see cref="Uri"/> on the dependency-property converter path (WPF UriTypeConverter).</summary>
public class XamlValueConverterUriTests : WpfGtkTestBase
{
	[Fact]
	public void FrameSource_RelativeString_ConvertsToRelativeUri()
	{
		var value = XamlValueConverter.Convert(typeof(Frame), Frame.SourceProperty, "GradientBrushExample.xaml");

		var uri = Assert.IsType<Uri>(value);
		Assert.False(uri.IsAbsoluteUri);
		Assert.Equal("GradientBrushExample.xaml", uri.OriginalString);
	}

	[Fact]
	public void FrameSource_AbsoluteString_ConvertsToAbsoluteUri()
	{
		var value = XamlValueConverter.Convert(typeof(Frame), Frame.SourceProperty, "http://example.com/a.xaml");

		Assert.True(Assert.IsType<Uri>(value).IsAbsoluteUri);
	}

	[Fact]
	public void FrameSource_ConvertedValue_IsAcceptedByTheDependencyProperty() => Run(() =>
	{
		var frame = new Frame();
		frame.SetValue(Frame.SourceProperty,
			XamlValueConverter.Convert(typeof(Frame), Frame.SourceProperty, "Page2.xaml"));

		Assert.Equal(new Uri("Page2.xaml", UriKind.Relative), frame.Source);
	});


	[Theory]
	[InlineData("/a.xaml")]
	[InlineData("/Pages/P.xaml")]
	public void Uri_with_a_leading_slash_stays_relative(string text)
	{
		var uri = Assert.IsType<Uri>(XamlValueConverter.Convert(typeof(Frame), Frame.SourceProperty, text));

		Assert.False(uri.IsAbsoluteUri);
		Assert.Equal(text, uri.OriginalString);
	}
	// Unix only: a real absolute path (existing parent below the root) stays absolute; WPF's app-root form stays relative.
	[Theory]
	[InlineData("/home/u/app/z.png", true, true)]
	[InlineData("/home/u/app/missing-dir/z.png", true, false)]
	[InlineData("/a.xaml", true, false)]
	[InlineData("/Pages/P.xaml", true, false)]
	[InlineData("/home/u/app/z.png", false, false)]
	public void Leading_slash_is_a_file_path_only_for_an_existing_Unix_directory(string text, bool unix, bool expected)
	{
		static bool Exists(string dir) => dir == "/home/u/app" || dir == "/home/u" || dir == "/home";
		Assert.Equal(expected, UnitConverters.IsUnixFilePath(text, unix, Exists));
	}

	[Fact]
	public void Uri_keeps_the_original_string_with_backslashes()
		=> Assert.Equal(@"sub\a.xaml", UnitConverters.ParseUri(@"sub\a.xaml").OriginalString);

	// .NET cannot parse the ",,," authority, so ParseUri falls back to the normalised pack form.
	[Fact]
	public void Uri_of_a_pack_application_string_is_normalised_because_dotnet_cannot_parse_it()
		=> Assert.Equal("pack://application/a.xaml",
			UnitConverters.ParseUri(" pack://application:,,,/a.xaml ").OriginalString);
}
