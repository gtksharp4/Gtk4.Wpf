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
	[Issue(IssueTracker.Github, 4360, "UWP: TapGestureRecognizer works on Layout only if BackgroundColor is set",
		PlatformAffected.UWP)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Issue4360 : ContentPage
	{
		const string Success = "Success";

		public Issue4360()
		{

			InitializeComponent();
		}

		void TapGestureRecognizer_OnTapped(object sender, EventArgs e)
		{
			Label.Text = Success;
		}

	}
#endif
}