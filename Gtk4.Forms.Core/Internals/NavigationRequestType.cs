using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum NavigationRequestType
	{
		Unknown = 0,
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
	}
}