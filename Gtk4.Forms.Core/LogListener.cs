using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}
}