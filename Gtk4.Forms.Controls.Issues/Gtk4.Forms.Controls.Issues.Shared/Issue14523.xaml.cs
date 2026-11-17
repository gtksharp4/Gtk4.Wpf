using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Gtk4.Forms.Core.UITests;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CarouselView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 14523,
		"[Bug] NullReferenceException in EntryRenderer on iOS versions <14.X with ClearButtonVisibility.WhileEditing and non-default TextColor",
		PlatformAffected.iOS)]
	public partial class Issue14523 : TestContentPage
	{
		public Issue14523()
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