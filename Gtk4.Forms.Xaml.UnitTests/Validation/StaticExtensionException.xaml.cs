using System;
using System.Collections.Generic;
using Xunit;
using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class StaticExtensionException : ContentPage
{
	public StaticExtensionException()
	{
		InitializeComponent();
	}

	public StaticExtensionException(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Issue2115
	{
		[Theory]
		[InlineData(false)]
		public void xStaticThrowsMeaningfullException(bool useCompiledXaml)
		{
			new XamlParseExceptionConstraint(6, 34).Verify(() => new StaticExtensionException(useCompiledXaml));
		}
	}
}