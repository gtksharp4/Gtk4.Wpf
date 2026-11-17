using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh11551 : ContentPage
{
	public Gh11551() => InitializeComponent();
	public Gh11551(bool useCompiledXaml)
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
		public void RectBoundsDoesntThrow(bool useCompiledXaml)
		{
			var layout = new Gh11551(useCompiledXaml);
			var bounds = AbsoluteLayout.GetLayoutBounds(layout.label);
			Assert.Equal(new Rect(1, .5, -1, 22), (Rect)bounds);
		}
	}
}