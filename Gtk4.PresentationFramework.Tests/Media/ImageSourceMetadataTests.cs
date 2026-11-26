using System.Linq;
using System.Reflection;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

public class ImageSourceMetadataTests
{
	[Fact]
	public void Metadata_is_declared_on_ImageSource_and_overridden_by_BitmapSource()
	{
		var onSource = typeof(ImageSource).GetProperty("Metadata")!;
		Assert.Equal(typeof(ImageMetadata), onSource.PropertyType);
		var onBitmap = typeof(BitmapSource).GetProperty("Metadata", BindingFlags.Public | BindingFlags.Instance
			| BindingFlags.DeclaredOnly)!;
		Assert.Equal(typeof(BitmapSource), onBitmap.DeclaringType);

		ImageSource source = BitmapSource.Create(1, 1, 96, 96, PixelFormats.Gray8, null, new byte[1], 1);
		Assert.Null(source.Metadata);
		var frame = BitmapFrame.Create((BitmapSource)source);
		Assert.Null(((BitmapSource)frame).Metadata);
	}

	[Fact]
	public void Typeface_exposes_only_the_object_Equals_overload()
	{
		var equals = typeof(Typeface).GetMethods(BindingFlags.Public | BindingFlags.Instance)
			.Where(m => m.Name == "Equals").ToList();
		Assert.Single(equals);
		Assert.Equal(typeof(object), equals[0].GetParameters()[0].ParameterType);
		var a = new Typeface("Arial");
		Assert.True(a.Equals((object)new Typeface("Arial")));
	}
}
