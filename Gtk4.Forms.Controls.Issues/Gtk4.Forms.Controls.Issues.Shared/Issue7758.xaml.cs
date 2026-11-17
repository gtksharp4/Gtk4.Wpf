using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7758, "[iOS] EmptyView is not rendered in screen center", PlatformAffected.iOS)]
	public partial class Issue7758 : TestContentPage
	{
		public Issue7758()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}