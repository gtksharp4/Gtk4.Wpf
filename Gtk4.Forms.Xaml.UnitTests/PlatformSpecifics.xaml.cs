using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.PlatformConfiguration;
using Gtk4.Forms.PlatformConfiguration.WindowsSpecific;
using WindowsOS = Gtk4.Forms.PlatformConfiguration.Windows;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class PlatformSpecific : FlyoutPage
{
	public PlatformSpecific()
	{
		InitializeComponent();
	}

	public PlatformSpecific(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void PlatformSpecificPropertyIsSet(bool useCompiledXaml)
		{
			var layout = new PlatformSpecific(useCompiledXaml);
			Assert.Equal(layout.On<WindowsOS>().GetCollapseStyle(), CollapseStyle.Partial);
			Assert.Equal(layout.On<WindowsOS>().CollapsedPaneWidth(), 96d);
		}
	}
}