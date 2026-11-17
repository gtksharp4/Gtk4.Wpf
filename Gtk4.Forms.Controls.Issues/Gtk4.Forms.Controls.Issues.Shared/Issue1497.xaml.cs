using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1497, "Grid sizing issue", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1497 : ContentPage
	{
		public Issue1497()
		{
			InitializeComponent();
		}
	}
#endif
}

