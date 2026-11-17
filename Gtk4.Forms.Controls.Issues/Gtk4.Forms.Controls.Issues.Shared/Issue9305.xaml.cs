using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9305, "Swipe View BackgroundColor Issues", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue9305 : TestContentPage
	{
		public Issue9305()
		{
#if APP
			Title = "Issue 9305";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}