using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Gtk4.Forms.Platform;

namespace Gtk4.Forms
{
	[RenderWith(typeof(_CollectionViewRenderer))]
	public class CollectionView : GroupableItemsView
	{
	}
}
