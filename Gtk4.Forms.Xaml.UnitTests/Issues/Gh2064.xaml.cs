using System;
using System.Collections.Generic;

using Xunit;

using Gtk4.Forms;
using Gtk4.Forms.Build.Tasks;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Gh2064 : ContentPage
{
	public Gh2064()
	{
		InitializeComponent();
	}

	public Gh2064(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(false), InlineData(true)]
		public void ReportMissingTargetTypeOnStyle(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2064)));
			else
				Assert.Throws<XamlParseException>(() => new Gh2064(useCompiledXaml));
		}
	}
}