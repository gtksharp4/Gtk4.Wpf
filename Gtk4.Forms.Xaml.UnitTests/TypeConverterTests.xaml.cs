using System;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class TypeConverterTests : ContentPage
{
	public TypeConverterTests()
	{
		InitializeComponent();
	}

	public TypeConverterTests(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void UriAreConverted(bool useCompiledXaml)
		{
			var layout = new TypeConverterTests(useCompiledXaml);
			Assert.IsType<Uri>(layout.imageSource.Uri);
			Assert.Equal("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
		}
	}
}