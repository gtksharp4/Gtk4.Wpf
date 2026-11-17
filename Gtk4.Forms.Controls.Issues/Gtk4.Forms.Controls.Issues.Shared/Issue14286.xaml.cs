using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Gtk4.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 14286,
		"[Bug] ShapesAPI-based Circle shape has a border by default in iOS while it doesn't have a border in Android",
		PlatformAffected.Android)]
	public partial class Issue14286 : TestContentPage
	{
		public Issue14286()
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