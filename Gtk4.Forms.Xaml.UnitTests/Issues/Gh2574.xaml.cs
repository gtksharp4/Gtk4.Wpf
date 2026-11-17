using System;
using Xunit;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh2574 : ContentPage
{
	public Gh2574()
	{
		InitializeComponent();
	}

	public Gh2574(bool useCompiledXaml)
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
		[InlineData(false), InlineData(true)]
		public void xNameOnRoot(bool useCompiledXaml)
		{
			var layout = new Gh2574(useCompiledXaml);
			Assert.Equal(layout, layout.page);
		}
	}
}