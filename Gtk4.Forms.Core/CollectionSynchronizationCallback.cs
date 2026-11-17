using System;
using System.Collections;

namespace Gtk4.Forms
{
	public delegate void CollectionSynchronizationCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess);
}