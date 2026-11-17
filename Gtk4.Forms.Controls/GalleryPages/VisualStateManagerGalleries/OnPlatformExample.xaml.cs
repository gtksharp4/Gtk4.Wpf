using System;
using Gtk4.Forms.Xaml;

namespace Gtk4.Forms.Controls.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OnPlatformExample : ContentPage
	{
		const string DefaultState = "Normal";
		string _currentState = DefaultState;

		public OnPlatformExample()
		{
			InitializeComponent();
		}

		void Button_OnClicked(object sender, EventArgs e)
		{
			if (_currentState == DefaultState)
			{
				_currentState = "CustomState";
				VisualStateManager.GoToState(DemoLabel, _currentState);
				ToggleButton.Text = "Change Label to Normal state";

			}
			else
			{
				_currentState = DefaultState;
				VisualStateManager.GoToState(DemoLabel, _currentState);
				ToggleButton.Text = "Change Label to Custom state";
			}
		}
	}
}