using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class AB946693 : ContentPage
{
	public AB946693() => InitializeComponent();
	public AB946693(bool useCompiledXaml)
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
		public void KeylessResourceThrowsMeaningfulException(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(AB946693)));
			else
				Assert.Throws<XamlParseException>(() => new AB946693(useCompiledXaml));
		}
	}
}