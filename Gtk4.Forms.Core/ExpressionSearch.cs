using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public abstract class ExpressionSearch
	{
		public static IExpressionSearch Default { get; set; }
	}
}