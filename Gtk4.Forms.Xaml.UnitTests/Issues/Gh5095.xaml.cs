using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Gh5095 : ContentPage
{
	public Gh5095() => InitializeComponent();
	public Gh5095(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests: IDisposable
	{
		public Tests() => Device.PlatformServices = new MockPlatformServices();
		public void Dispose() => Device.PlatformServices = null;

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void ThrowsOnInvalidXaml(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh5095)));
			else
				Assert.Throws<XamlParseException>(() => new Gh5095(false));
		}
	}
}