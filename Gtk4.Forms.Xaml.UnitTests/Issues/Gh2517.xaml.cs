using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Gh2517 : ContentPage
{
	public Gh2517()
	{
		InitializeComponent();
	}

	public Gh2517(bool useCompiledXaml)
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
		public void ErrorOnMissingBindingTarget(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2517)));
		}
	}
}