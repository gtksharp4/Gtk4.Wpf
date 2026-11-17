using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh11541 : ContentPage
{
	public Gh11541() => InitializeComponent();
	public Gh11541(bool useCompiledXaml)
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
		public void RectangleGeometryDoesntThrow(bool useCompiledXaml)
		{
			AssertEx.DoesNotThrow(() => new Gh11541(useCompiledXaml));
		}
	}
}