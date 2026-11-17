using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10333, "[iOS] Opacity on Frame behavior change in 4.5", PlatformAffected.iOS)]
	public partial class Issue10333 : ContentPage
	{
		public Issue10333()
		{
#if APP
			InitializeComponent();
#endif

		}
	}

}