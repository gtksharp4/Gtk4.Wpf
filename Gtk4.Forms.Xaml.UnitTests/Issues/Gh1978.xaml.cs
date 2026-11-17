using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Gh1978 : ContentPage
{
	public Gh1978()
	{
		InitializeComponent();
	}

	public Gh1978(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(true)]
		public void ReportError(bool useCompiledXaml)
		{
			if (!useCompiledXaml)
				return;
			Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh1978)));
		}
	}
}