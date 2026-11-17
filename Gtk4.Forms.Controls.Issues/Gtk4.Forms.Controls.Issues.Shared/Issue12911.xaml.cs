using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 12911, "[Bug] Shapes in SwipeView of a CollectionView/ListView has several issues",
		PlatformAffected.Android | PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Brush)]
#endif
	public partial class Issue12911 : TestContentPage
	{
		public Issue12911()
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