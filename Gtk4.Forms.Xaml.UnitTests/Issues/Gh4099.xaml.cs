using System;
using System.Collections.Generic;

using Xunit;

using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Gh4099 : ContentPage
{
	public Gh4099()
	{
		InitializeComponent();
	}

	public Gh4099(bool useCompiledXaml)
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
		public void BetterExceptionReport(bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				try
				{
					MockCompiler.Compile(typeof(Gh4099));
				}
				catch (BuildException xpe)
				{
					Assert.Equal(5, xpe.XmlInfo.LineNumber);
					return;
				}
				Assert.Fail();
			}
		}
	}
}