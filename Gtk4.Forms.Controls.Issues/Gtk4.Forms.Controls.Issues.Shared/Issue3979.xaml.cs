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
	[Issue(IssueTracker.Github, 3979, "Removing LineBreakMode support from WindowsResourcesProvider", PlatformAffected.UWP)]
	public partial class Issue3979 : TestContentPage
	{
		public Issue3979()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
#if APP
		private void Button_OnClicked(object sender, EventArgs e)
		{
			TargetSpan.Style = Device.Styles.BodyStyle;
		}
#endif
	}
}