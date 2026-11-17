using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISystemResourcesProvider
	{
		IResourceDictionary GetSystemResources();
	}
}