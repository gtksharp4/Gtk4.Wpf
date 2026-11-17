using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk4.Forms;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6130, "[Bug] [Forms 4.0] [Android] Mismatch in WebView.EnableZoomControls platform-specific", PlatformAffected.Android)]
	public partial class Issue6130 : ContentPage
	{
		public Issue6130()
		{
#if APP
			InitializeComponent();
#endif
		}
	}
}