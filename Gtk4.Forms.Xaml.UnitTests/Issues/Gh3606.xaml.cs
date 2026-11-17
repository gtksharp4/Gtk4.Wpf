using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh3606 : ContentPage
{
	public Gh3606()
	{
		InitializeComponent();
	}

	public Gh3606(bool useCompiledXaml)
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
		public void BindingsWithSourceArentCompiled(bool useCompiledXaml)
		{
			new Gh3606(useCompiledXaml);
		}
	}
}