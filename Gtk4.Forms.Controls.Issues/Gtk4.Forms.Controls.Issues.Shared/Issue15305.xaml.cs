using System.Collections.ObjectModel;
using Gtk4.Forms.CustomAttributes;
using Gtk4.Forms.Internals;
using Gtk4.Forms.Xaml;

#if UITEST
using Gtk4.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Gtk4.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 15305, "[Android] TalkBack always considers ListView's header and footer for indexing/counting", PlatformAffected.Android)]
	public partial class Issue15305 : TestContentPage
	{
		public Issue15305()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new ViewModelIssue15305();
		}
	}

	[Preserve(AllMembers = true)]
	public class ViewModelIssue15305
	{
		public ViewModelIssue15305()
		{
			Items = new ObservableCollection<string>() { "first item", "second item", };
		}

		public ObservableCollection<string> Items { get; }
	}
}