using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public class Gh4102VM
{
	public Gh4102VM SomeNullValue { get; set; }
	public string SomeProperty { get; set; } = "Foo";
}

public partial class Gh4102 : ContentPage
{
	public Gh4102() => InitializeComponent();

	public Gh4102(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests: IDisposable
	{
		public Tests() => Device.PlatformServices = new MockPlatformServices();
		public void Dispose() => Device.PlatformServices = null;

		[Theory]
		[InlineData(true), InlineData(false)]
		public void CompiledBindingsNullInPath(bool useCompiledXaml)
		{
			var layout = new Gh4102(useCompiledXaml) { BindingContext = new Gh4102VM() };
			Assert.Null(layout.label.Text);
		}
	}
}