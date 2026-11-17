using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh6361 : ContentPage
{
	public Gh6361() => InitializeComponent();
	public Gh6361(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests: IDisposable
	{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices();
			Gtk4.Forms.Internals.Registrar.RegisterAll(new Type[0]);
		}

		public void Dispose() => Device.PlatformServices = null;

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void CSSBorderRadiusDoesNotFail(bool useCompiledXaml)
		{
			var layout = new Gh6361(useCompiledXaml);
		}
	}
}