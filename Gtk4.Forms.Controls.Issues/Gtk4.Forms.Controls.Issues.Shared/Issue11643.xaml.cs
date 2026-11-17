using System;
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
	[Category(UITestCategories.Brush)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11643, "[Bug] SwipeBehaviorOnInvoked=RemainOpen triggers event twice",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue11643 : TestContentPage
	{
		public Issue11643()
		{
#if APP
			Title = "Issue 11643";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if APP
		void OnSwipeItemInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeView", "SwipeItem Invoked", "Ok");
		}
#endif
	}
}