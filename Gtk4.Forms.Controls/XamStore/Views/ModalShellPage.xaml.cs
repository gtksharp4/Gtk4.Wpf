using System;

namespace Gtk4.Forms.Controls.XamStore.Views
{
	public partial class ModalShellPage : ContentPage
	{
		public ModalShellPage()
		{
			InitializeComponent();
		}

		void Button_Clicked(object sender, EventArgs e)
		{
			Shell.Current.Navigation.PopModalAsync(true);
		}
	}
}
