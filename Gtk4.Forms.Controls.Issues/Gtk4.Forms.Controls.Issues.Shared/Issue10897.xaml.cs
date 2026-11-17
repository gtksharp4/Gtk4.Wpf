using System.Collections.Generic;
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
	[Issue(IssueTracker.Github, 10897, "[Bug] UWP: CollectionView in CarouselView crashes when resizing the window",
		PlatformAffected.UWP)]
	public partial class Issue10897 : TestContentPage
	{
		public Issue10897()
		{
#if APP
			InitializeComponent();

			Issue10897ViewModel vm = new Issue10897ViewModel();
			vm.CarouselItems = new List<Issue10897Items>();
			vm.CarouselItems.Add(new Issue10897Items() { Items = new List<Issue10897Item>() { new Issue10897Item() { Text = "Item1" }, new Issue10897Item() { Text = "Item2" } } });
			BindingContext = vm;
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class Issue10897Item
	{
		public string Text { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue10897Items
	{
		public List<Issue10897Item> Items { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class Issue10897ViewModel
	{
		public List<Issue10897Items> CarouselItems { get; set; }
	}
}