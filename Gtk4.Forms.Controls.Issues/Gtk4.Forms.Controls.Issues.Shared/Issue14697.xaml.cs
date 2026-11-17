using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14697, "[Bug][Android] Shape no longer redrawing itself when the container changes size (sr4 and sr5)", PlatformAffected.iOS)]
	public partial class Issue14697 : ContentPage
	{
		public Issue14697()
		{
#if APP
			InitializeComponent();
#endif
		}
	}

}