using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 13684,
		"[Bug] Label: LineBreakMode incompatible with MaxLines",
		PlatformAffected.Android)]
	public partial class Issue13684 : TestContentPage
	{
		public Issue13684()
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