using System;
using Xunit;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Bz44213 : ContentPage
{
	public Bz44213()
	{
		InitializeComponent();
	}

	public Bz44213(bool useCompiledXaml)
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
		[InlineData(true)]
		[InlineData(false)]
		public void BindingInOnPlatform(bool useCompiledXaml)
		{
			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
			var p = new Bz44213(useCompiledXaml);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.Equal("Foo", p.label.Text);
			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
			p = new Bz44213(useCompiledXaml);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.Equal("Bar", p.label.Text);
		}
	}
}