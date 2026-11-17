using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh2007 : ContentPage
{
	public Gh2007()
	{
		InitializeComponent();
	}

	public Gh2007(bool useCompiledXaml)
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
		public void UsefullxResourceErrorMessages(bool useCompiledXaml)
		{
			Assert.Throws<XamlParseException>(() => new Gh2007(useCompiledXaml));
		}
	}
}