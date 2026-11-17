using System;

namespace Gtk4.Forms
{
	public abstract class RouteFactory
	{
		public abstract Element GetOrCreate();
	}
}