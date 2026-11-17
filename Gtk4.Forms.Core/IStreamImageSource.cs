using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Gtk4.Forms
{
	public interface IStreamImageSource
	{
		Task<Stream> GetStreamAsync(CancellationToken userToken = default(CancellationToken));
	}
}