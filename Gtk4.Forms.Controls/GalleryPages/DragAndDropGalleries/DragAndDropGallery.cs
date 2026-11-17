using System.Collections.Generic;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls.GalleryPages.DragAndDropGalleries
{
	[Preserve(AllMembers = true)]
	public class DragAndDropGallery : Shell
	{
		public DragAndDropGallery()
		{
			Items.Add(new EnablingAndDisablingGestureTests());
			Items.Add(new VariousDragAndDropPermutations());
			Items.Add(new DragAndDropBetweenLayouts());
			Items.Add(new DragAndDropEvents());
			Items.Add(new DragPaths());
		}
	}
}
