namespace Gtk4.Forms
{
	public interface IShellContentInsetObserver
	{
		void OnInsetChanged(Thickness inset, double tabThickness);
	}
}