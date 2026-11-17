using Gtk4.Forms.Internals;

namespace Gtk4.Forms
{
	public interface IGridController
	{
		void InvalidateMeasureInernalNonVirtual(InvalidationTrigger trigger);
	}
}