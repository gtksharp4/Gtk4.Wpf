using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public class Gh6176VM
{
}

public class Gh6176Base<TVM> : ContentPage where TVM : class
{
	public TVM ViewModel => BindingContext as TVM;
	protected void ShowMenu(object sender, EventArgs e) { }
}

public partial class Gh6176
{
	public Gh6176() => InitializeComponent();
	public Gh6176(bool useCompiledXaml)
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
		public void XamlCDoesntFail(bool useCompiledXaml)
		{
			var layout = new Gh6176(useCompiledXaml);
		}
	}
}