using System.Threading.Tasks;

namespace Gtk4.Forms.Controls.Tests
{
	public interface ITestingPlatformService
	{
		Task CreateRenderer(VisualElement visualElement);
	}
}
