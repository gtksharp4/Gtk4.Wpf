using System;
using System.Collections.Generic;

using Gtk4.Forms;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls
{
	public partial class XamlNativeViews : ContentPage
	{
		public XamlNativeViews()
		{
			InitializeComponent();
			BindingContext = new VM { NativeText = "Text set to Native view using native binding" };
		}
	}

	[Preserve(AllMembers = true)]
	public class VM
	{
		public string NativeText { get; set; }
	}
}