using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9588, "Frame inside SwipeView can't be swiped", PlatformAffected.iOS)]
	public partial class Issue9588 : TestContentPage
	{
		public Issue9588()
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