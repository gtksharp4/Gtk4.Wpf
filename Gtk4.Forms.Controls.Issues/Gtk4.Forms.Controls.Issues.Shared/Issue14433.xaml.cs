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
	[Issue(IssueTracker.Github, 14433,
		"[Bug] LinearGradientBrush Background with alpha channel color don't works",
		PlatformAffected.iOS)]
	public partial class Issue14433 : TestContentPage
	{
		public Issue14433()
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