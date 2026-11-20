using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Pure tests for the string conversions that sample apps hit during InitializeComponent.</summary>
public class StartupValueConversionTests
{
	static object? Conv(DependencyProperty dp, string raw) => XamlValueConverter.Convert(typeof(Button), dp, raw);

	// ── Transform ────────────────────────────────────────────────────────────

	[Fact]
	public void Transform_matrix_string_gives_MatrixTransform()
	{
		var t = Assert.IsType<MatrixTransform>(Conv(UIElement.RenderTransformProperty, "1,0,0,1,3,3"));
		Assert.Equal(new Matrix(1, 0, 0, 1, 3, 3), t.Matrix);
	}

	[Fact]
	public void Transform_identity_string()
	{
		var t = Assert.IsType<MatrixTransform>(Conv(UIElement.RenderTransformProperty, "Identity"));
		Assert.True(t.Matrix.IsIdentity);
	}

	[Theory]
	[InlineData("1,0,0,1,3")]
	[InlineData("a,b,c,d,e,f")]
	public void Transform_bad_string_throws_FormatException(string raw)
	{
		Assert.ThrowsAny<FormatException>(() => Conv(UIElement.RenderTransformProperty, raw));
	}

	// ── Length units ─────────────────────────────────────────────────────────

	[Theory]
	[InlineData("30px", 30.0)]
	[InlineData("1in", 96.0)]
	[InlineData("2.54cm", 96.0)]
	[InlineData("72pt", 96.0)]
	[InlineData("10", 10.0)]
	[InlineData("Infinity", double.PositiveInfinity)]
	public void Length_units_convert_to_device_units(string raw, double expected)
	{
		Assert.Equal(expected, (double)Conv(FrameworkElement.HeightProperty, raw)!, 9);
	}

	[Theory]
	[InlineData("Auto")]
	[InlineData("auto")]
	[InlineData("NaN")]
	public void Length_auto_is_NaN(string raw)
	{
		Assert.True(double.IsNaN((double)Conv(FrameworkElement.WidthProperty, raw)!));
	}

	[Fact]
	public void Length_bad_unit_throws_FormatException()
	{
		Assert.Throws<FormatException>(() => Conv(FrameworkElement.HeightProperty, "30xx"));
	}

	// ── ImageSource ──────────────────────────────────────────────────────────

	[Theory]
	[InlineData("data\\picture1.jpg")]
	[InlineData("data/picture1.jpg")]
	[InlineData("pack://application:,,,/data/picture1.jpg")]
	[InlineData("file:///C:/x/picture1.jpg")]
	public void ImageSource_string_gives_BitmapImage_with_uri(string raw)
	{
		var img = Assert.IsType<BitmapImage>(Conv(Image.SourceProperty, raw));
		Assert.NotNull(img.UriSource);
	}

	[Fact]
	public void ImageSource_relative_path_resolves_against_app_base_directory()
	{
		var img = (BitmapImage)Conv(Image.SourceProperty, "data\\picture1.jpg")!;
		var expected = System.IO.Path.GetFullPath(
			System.IO.Path.Combine(AppContext.BaseDirectory, "data", "picture1.jpg"));
		Assert.Equal(expected, ImageSourceLocation.ResolveFilePath(img));
	}

	[Fact]
	public void ImageSource_pack_uri_maps_to_base_directory_file()
	{
		var img = (BitmapImage)Conv(Image.SourceProperty, "pack://application:,,,/data/a.png")!;
		var expected = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "data", "a.png"));
		Assert.Equal(expected, ImageSourceLocation.ResolveFilePath(img));
	}

	[Fact]
	public void ImageSource_pack_component_uri_drops_assembly_segment()
	{
		var img = (BitmapImage)Conv(Image.SourceProperty, "pack://application:,,,/Some.Asm;component/img/a.png")!;
		var expected = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "img", "a.png"));
		Assert.Equal(expected, ImageSourceLocation.ResolveFilePath(img));
	}

	[Fact]
	public void ImageSource_absolute_file_path_is_kept()
	{
		var full = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppContext.BaseDirectory, "z.png"));
		var img = (BitmapImage)Conv(Image.SourceProperty, full)!;
		Assert.Equal(full, ImageSourceLocation.ResolveFilePath(img));
	}

	// ── ICommand ─────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("ApplicationCommands.Open", "Open")]
	[InlineData("Open", "Open")]
	[InlineData("Copy", "Copy")]
	[InlineData("EditingCommands.ToggleBold", "ToggleBold")]
	[InlineData("NavigationCommands.BrowseBack", "BrowseBack")]
	[InlineData("MoveLeft", "MoveLeft")]
	[InlineData("System.Windows.Input.ApplicationCommands.Save", "Save")]
	public void Command_name_resolves(string raw, string name)
	{
		var cmd = Assert.IsAssignableFrom<RoutedCommand>(Conv(Button.CommandProperty, raw));
		Assert.Equal(name, cmd.Name);
	}

	[Fact]
	public void Command_resolves_to_the_singleton()
	{
		Assert.Same(ApplicationCommands.Open, Conv(Button.CommandProperty, "ApplicationCommands.Open"));
	}

	[Fact]
	public void Command_empty_is_null()
	{
		Assert.Null(Conv(Button.CommandProperty, ""));
	}

	[Theory]
	[InlineData("NoSuchCommand")]
	[InlineData("ApplicationCommands.NoSuch")]
	[InlineData("Foo.Open")]
	public void Command_unknown_throws_NotSupportedException(string raw)
	{
		Assert.Throws<NotSupportedException>(() => Conv(Button.CommandProperty, raw));
	}
}
