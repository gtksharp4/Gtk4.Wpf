using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class TestXmlnsUsing : ContentPage
{
	public TestXmlnsUsing()
	{
		InitializeComponent();
	}

	public TestXmlnsUsing(bool useCompiledXaml)
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
			Application.Current = null;
			Device.PlatformServices = null;
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void SupportUsingXmlns(bool useCompiledXaml)
		{
			var page = new TestXmlnsUsing(useCompiledXaml);
			Assert.NotNull(page.Content);
			Assert.IsType<CustomXamlView>(page.Content);
		}
	}
}