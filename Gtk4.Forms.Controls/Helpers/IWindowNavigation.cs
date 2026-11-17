using System.Threading.Tasks;

namespace Gtk4.Forms.Controls
{
	public interface IWindowNavigation
	{
		Task OpenNewWindowAsync();
		void NavigateToAnotherPage(Page page);
	}
}
