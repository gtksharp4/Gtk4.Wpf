using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class IsCompiledDefault : ContentPage
{
	public IsCompiledDefault()
	{
		InitializeComponent();
	}

	public IsCompiledDefault(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void IsCompiled(bool useCompiledXaml)
		{
			var layout = new IsCompiledDefault(useCompiledXaml);
			Assert.True(typeof(IsCompiledDefault).IsCompiled());
		}
	}
}