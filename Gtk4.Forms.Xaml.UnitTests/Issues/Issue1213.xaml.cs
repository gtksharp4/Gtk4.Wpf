using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Issue1213 : TabbedPage
{
	public Issue1213()
	{
		InitializeComponent();
	}

	public Issue1213(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void MultiPageAsContentPropertyAttribute(bool useCompiledXaml)
		{
			var page = new Issue1213(useCompiledXaml);
			Assert.Equal(2, page.Children.Count);
		}
	}
}