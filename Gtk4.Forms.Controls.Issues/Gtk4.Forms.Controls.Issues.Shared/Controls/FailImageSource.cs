using System.Threading.Tasks;

namespace Gtk4.Forms.Controls
{
	public sealed class FailImageSource : ImageSource
	{
		public override Task<bool> Cancel()
		{
			return Task.FromResult(false);
		}
	}
}
