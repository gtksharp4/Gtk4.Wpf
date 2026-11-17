using System;

namespace Gtk4.Forms
{
	[Flags]
	public enum EffectiveFlowDirection
	{
		RightToLeft = 1 << 0,
		Explicit = 1 << 1
	}
}