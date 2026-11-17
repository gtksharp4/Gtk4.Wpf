using System;
using Xunit;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class InlineCSS : ContentPage
{
	public InlineCSS()
	{
		InitializeComponent();
	}

	public InlineCSS(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
		: IDisposable{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices();
			Gtk4.Forms.Internals.Registrar.RegisterAll(new Type[0]);
		}

		public void Dispose()
		{
			Device.PlatformServices = null;
		}

		[Theory]
		[InlineData(false), InlineData(true)]
		public void InlineCSSParsed(bool useCompiledXaml)
		{
			var layout = new InlineCSS(useCompiledXaml);
			Assert.Equal(Color.Pink, layout.label.TextColor);
		}

		[Theory]
		[InlineData(false), InlineData(true)]
		public void InitialValue(bool useCompiledXaml)
		{
			var layout = new InlineCSS(useCompiledXaml);
			Assert.Equal(Color.Green, layout.BackgroundColor);
			Assert.Equal(Color.Green, layout.stack.BackgroundColor);
			Assert.Equal(Color.Green, layout.button.BackgroundColor);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
			Assert.Equal(TextTransform.Uppercase, layout.label.TextTransform);
		}
	}
}