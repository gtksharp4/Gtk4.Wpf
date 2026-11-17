using System.ComponentModel;

namespace Gtk4.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	interface ILineHeightElement
	{
		double LineHeight { get; }

		void OnLineHeightChanged(double oldValue, double newValue);
	}
}