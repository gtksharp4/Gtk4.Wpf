using System;
using System.Collections.Generic;

using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Gh7097Base : ContentPage
{
	public Gh7097Base() => InitializeComponent();
	public Gh7097Base(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
}