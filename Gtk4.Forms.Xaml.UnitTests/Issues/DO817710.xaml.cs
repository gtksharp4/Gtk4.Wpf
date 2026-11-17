using System;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class DO817710 : ContentPage
{
	public DO817710() => InitializeComponent();
	public DO817710(bool useCompiledXaml)
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
		public void EmptyResourcesElement(bool useCompiledXaml)
		{
			AssertEx.DoesNotThrow(() => new DO817710(useCompiledXaml: useCompiledXaml));
		}
	}
}