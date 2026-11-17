using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 12887, "[Bug] Label TextTransform settings ignored if LineHeight is set",
		PlatformAffected.iOS)]
	public partial class Issue12887 : TestContentPage
	{
		public Issue12887()
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