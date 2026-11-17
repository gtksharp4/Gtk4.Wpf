using System;

namespace Gtk4.Forms
{
	public interface IScrollViewController : ILayoutController
	{
		Point GetScrollPositionForElement(VisualElement item, ScrollToPosition position);

		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		void SendScrollFinished();

		void SetScrolledPosition(double x, double y);

		Rectangle LayoutAreaOverride { get; set; }
	}
}