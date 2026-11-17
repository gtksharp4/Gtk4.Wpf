using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Core.UnitTests;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Pr3384 : ContentPage
{
	public Pr3384()
	{
		InitializeComponent();
	}

	public Pr3384(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
		: IDisposable{
		public Tests()
		{
			Device.PlatformServices = new MockPlatformServices { RuntimePlatform = Device.iOS };
		}

		public void Dispose()
		{
			Device.PlatformServices = null;
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void RecyclingStrategyIsHandled(bool useCompiledXaml)
		{
			var p = new Pr3384(useCompiledXaml);
			Assert.Equal(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
		}
	}
}