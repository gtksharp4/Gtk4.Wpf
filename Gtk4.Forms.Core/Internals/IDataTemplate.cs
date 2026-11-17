using System;
using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IDataTemplate
	{
		Func<object> LoadTemplate { get; set; }
	}
}