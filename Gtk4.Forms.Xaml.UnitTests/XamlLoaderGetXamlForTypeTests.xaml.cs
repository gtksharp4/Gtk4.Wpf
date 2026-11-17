using System;
using System.Collections.Generic;

using Xunit;

using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class XamlLoaderGetXamlForTypeTests : ContentPage
{
	public XamlLoaderGetXamlForTypeTests()
	{
		InitializeComponent();
	}

	public XamlLoaderGetXamlForTypeTests(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
		: IDisposable{
		public Tests()
		{
#pragma warning disable 0618
			Gtk4.Forms.Xaml.Internals.XamlLoader.XamlFileProvider = null;
#pragma warning restore 0618
		}

		public void Dispose()
		{
			Device.PlatformServices = null;
			XamlLoader.FallbackTypeResolver = null;
			XamlLoader.ValueCreatedCallback = null;
			XamlLoader.InstantiationFailedCallback = null;
#pragma warning disable 0618
			Gtk4.Forms.Internals.ResourceLoader.ExceptionHandler = null;
			Gtk4.Forms.Xaml.Internals.XamlLoader.DoNotThrowOnExceptions = false;
#pragma warning restore 0618

		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void XamlContentIsReplaced(bool useCompiledXaml)
		{
			var layout = new XamlLoaderGetXamlForTypeTests(useCompiledXaml);
			Assert.IsType<Button>(layout.Content);

#pragma warning disable 0618
			Gtk4.Forms.Xaml.Internals.XamlLoader.XamlFileProvider = (t) =>
			{
#pragma warning restore 0618
				if (t == typeof(XamlLoaderGetXamlForTypeTests))
					return @"
	<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
		xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
		x:Class=""Gtk4.Forms.Xaml.UnitTests.XamlLoaderGetXamlForTypeTests"">
		<Label x:Name=""Label""/>
	</ContentPage>";
				return null;
			};

			layout = new XamlLoaderGetXamlForTypeTests(useCompiledXaml);
			Assert.IsType<Label>(layout.Content);
		}
	}
}