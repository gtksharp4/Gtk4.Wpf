using System;
using System.Windows.Controls;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>String-to-<see cref="Uri"/> on the dependency-property converter path (WPF UriTypeConverter).</summary>
public class XamlValueConverterUriTests
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
	public void FrameSource_ConvertedValue_IsAcceptedByTheDependencyProperty()
	{
		var frame = new Frame();
		frame.SetValue(Frame.SourceProperty,
			XamlValueConverter.Convert(typeof(Frame), Frame.SourceProperty, "Page2.xaml"));

		Assert.Equal(new Uri("Page2.xaml", UriKind.Relative), frame.Source);
	}
}
