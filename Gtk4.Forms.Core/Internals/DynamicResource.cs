using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class DynamicResource
	{
		public string Key { get; private set; }
		public DynamicResource(string key) => Key = key;
	}
}