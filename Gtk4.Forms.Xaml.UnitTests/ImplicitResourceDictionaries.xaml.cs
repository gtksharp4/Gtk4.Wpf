using System;
using Xunit;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class ImplicitResourceDictionaries : ContentPage
{
	public ImplicitResourceDictionaries()
	{
		InitializeComponent();
	}

	public ImplicitResourceDictionaries(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
		: IDisposable{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		public void Dispose()
		{
			Device.PlatformServices = null;
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ImplicitRDonContentViews(bool useCompiledXaml)
		{
			var layout = new ImplicitResourceDictionaries(useCompiledXaml);
			Assert.Equal(Color.Purple, layout.label.TextColor);
		}
	}
}