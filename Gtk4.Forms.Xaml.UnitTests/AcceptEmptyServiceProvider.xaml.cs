using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

[AcceptEmptyServiceProvider]
public class FooExtension : IMarkupExtension<IServiceProvider>
{
	public IServiceProvider ProvideValue(IServiceProvider serviceProvider)
	{
		return serviceProvider;
	}

	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
	{
		return (this as IMarkupExtension<IServiceProvider>).ProvideValue(serviceProvider);
	}
}

public partial class AcceptEmptyServiceProvider : ContentPage
{
	public AcceptEmptyServiceProvider()
	{
		InitializeComponent();
	}

	public AcceptEmptyServiceProvider(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public IServiceProvider ServiceProvider { get; set; }

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
		[InlineData(true)]
		[InlineData(false)]
		public void ServiceProviderIsNullOnAttributedExtensions(bool useCompiledXaml)
		{
			var p = new AcceptEmptyServiceProvider(useCompiledXaml);
			Assert.Null(p.ServiceProvider);
		}
	}
}