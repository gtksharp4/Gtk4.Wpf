namespace Gtk4.Forms
{
	public interface IMultiPageController<T>
	{
		T GetPageByIndex(int index);
	}
}