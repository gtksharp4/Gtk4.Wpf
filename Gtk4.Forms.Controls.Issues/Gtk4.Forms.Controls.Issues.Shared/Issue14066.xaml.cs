using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
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
	[Issue(IssueTracker.Github, 14066, "[Bug] Deleting items with SwipeView in StackLayout",
		PlatformAffected.Android)]
	public partial class Issue14066 : TestContentPage
	{
		public Issue14066()
		{
#if APP
			InitializeComponent();
			BindingContext = new Issue14066ViewModel();
#endif
		}

		protected override void Init()
		{

		}
	}

	public class Issue14066Model
	{
		public string Name { get; set; }
	}

	public class Issue14066ViewModel : BindableObject
	{
		ObservableCollection<Issue14066Model> _collectionsList;

		public ObservableCollection<Issue14066Model> CollectionsList
		{
			get { return _collectionsList; }
			set
			{
				_collectionsList = value;
				OnPropertyChanged();
			}
		}

		public ICommand DeleteCommand { get; }

		public ICommand AddCommand { get; }


		public Issue14066ViewModel()
		{
			CollectionsList = new ObservableCollection<Issue14066Model>()
			{
				new Issue14066Model { Name= "Item 1" },
				new Issue14066Model { Name= "Item 2" },
				new Issue14066Model { Name= "Item 3" },
				new Issue14066Model { Name= "Item 4" },
			};

			DeleteCommand = new Command(OnDeleteTapped);

			AddCommand = new Command(AddItmes);
		}

		void AddItmes(object obj)
		{
			Issue14066Model offersModel = new Issue14066Model
			{
				Name = "New Item Added"
			};

			CollectionsList.Add(offersModel);
		}

		void OnDeleteTapped(object obj)
		{
			var content = obj as Issue14066Model;
			CollectionsList.Remove(content);
		}
	}
}