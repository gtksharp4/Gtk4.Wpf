using System;

namespace Gtk4.Forms
{
	[Flags]
	public enum MeasureFlags
	{
		None = 0,
		IncludeMargins = 1 << 0
	}
}