using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Xaml.UnitTests;

public class Bz53318ListView : ListView
{
}

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Bz53318 : ContentPage
{
	public Bz53318()
	{
		InitializeComponent();
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

		[Fact]
		public void DoesCompilesArgsInsideDataTemplate()
		{
			AssertEx.DoesNotThrow(() => MockCompiler.Compile(typeof(Bz53318)));
		}
	}
}