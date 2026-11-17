using System;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10166, "[iOS] IsClippedToBounds Property is ignored by Frames", PlatformAffected.iOS)]
	public partial class Issue10166 : TestContentPage
	{
		public Issue10166()
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