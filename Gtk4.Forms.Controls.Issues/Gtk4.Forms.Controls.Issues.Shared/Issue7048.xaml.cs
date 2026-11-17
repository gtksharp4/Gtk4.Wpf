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
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7048, "[Bug][UWP] CheckBox Has Incosistent Paddings",
		PlatformAffected.UWP)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue7048 : ContentPage
	{
		public Issue7048()
		{
			InitializeComponent();
		}
	}
#endif
}