using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12590,
		"[Bug] LineBreakMode=TailTruncation doesn't work with Label.FormattedText",
		PlatformAffected.iOS)]
	public partial class Issue12590 : TestContentPage
	{
		public Issue12590()
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