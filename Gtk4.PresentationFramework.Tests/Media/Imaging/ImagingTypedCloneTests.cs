using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Imaging;

/// <summary>WPF declares <c>public new X Clone()</c> and <c>CloneCurrentValue()</c> on each imaging type.</summary>
public class ImagingTypedCloneTests
{
	[Theory]
	[InlineData(typeof(ImageSource))]
	[InlineData(typeof(BitmapSource))]
	[InlineData(typeof(BitmapImage))]
	[InlineData(typeof(CroppedBitmap))]
	[InlineData(typeof(FormatConvertedBitmap))]
	[InlineData(typeof(TransformedBitmap))]
	[InlineData(typeof(CachedBitmap))]
	[InlineData(typeof(DrawingImage))]
	public void Clone_and_CloneCurrentValue_return_the_declaring_type(System.Type type)
	{
		foreach (string name in new[] { "Clone", "CloneCurrentValue" })
		{
			var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
				null, System.Type.EmptyTypes, null);
			Assert.NotNull(method);
			Assert.Equal(type, method!.ReturnType);
		}
	}

	[Fact]
	public void The_typed_overloads_return_instances_of_the_concrete_type()
	{
		var source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4);
		CroppedBitmap crop = new CroppedBitmap(source, System.Windows.Int32Rect.Empty).Clone();
		FormatConvertedBitmap gray = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0).Clone();
		TransformedBitmap turned = new TransformedBitmap(source, new RotateTransform(90)).CloneCurrentValue();
		CachedBitmap cached = new CachedBitmap(source, BitmapCreateOptions.None, BitmapCacheOption.Default).Clone();
		DrawingImage drawing = new DrawingImage(new GeometryDrawing()).Clone();
		ImageSource image = ImageSource.FromFile("a.png").Clone();
		Assert.NotNull(crop);
		Assert.NotNull(gray);
		Assert.NotNull(turned);
		Assert.NotNull(cached);
		Assert.NotNull(drawing);
		Assert.NotNull(image);
	}
}
