using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDynamicResourceHandler
	{
		void SetDynamicResource(BindableProperty property, string key);
	}
}