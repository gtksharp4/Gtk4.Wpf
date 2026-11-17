using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class StringLiterals : ContentPage
{
	public StringLiterals()
	{
		InitializeComponent();
	}

	public StringLiterals(bool useCompiledXaml)
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
		[InlineData(false)]
		[InlineData(true)]
		public void EscapedStringsAreTreatedAsLiterals(bool useCompiledXaml)
		{
			var layout = new StringLiterals(useCompiledXaml);
			Assert.Equal("Foo", layout.label0.Text);
			Assert.Equal("{Foo}", layout.label1.Text);
			Assert.Equal("Foo", layout.label2.Text);
			Assert.Equal("Foo", layout.label3.Text);
		}
	}
}