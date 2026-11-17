using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.CollectionView)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 13588, "[Bug] Button inside collectionview not appearing until UWP window is resized #13552", PlatformAffected.UWP)]
	public partial class Issue13588 : TestContentPage
	{
		public Issue13588()
		{
#if APP
			Title = "Issue 13588";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}