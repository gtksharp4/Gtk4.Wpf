using System.Collections.Generic;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Xaml;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 11571, "[Bug][Brushes] LinearGradientBrush diagonal platform differences",
		PlatformAffected.iOS)]
	public partial class Issue11571 : TestContentPage
	{
		public Issue11571()
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