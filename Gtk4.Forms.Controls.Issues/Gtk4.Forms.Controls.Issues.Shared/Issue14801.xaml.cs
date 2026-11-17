using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14801, "[Bug] RadialGradientBrush behavior is wrong on UWP", PlatformAffected.UWP)]
	public partial class Issue14801 : ContentPage
	{
		public Issue14801()
		{
#if APP
			InitializeComponent();
#endif
		}
	}

}