using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 11413,
		"[Bug] Rectangle shape - Incorrect rendering/crash (depending on platform)",
		PlatformAffected.Android)]
	public partial class Issue11413 : TestContentPage
	{
		public Issue11413()
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