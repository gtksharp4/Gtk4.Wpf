using System;

namespace Gtk4.Forms
{
	public enum ShellNavigationSource
	{
		Unknown = 0,
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
		ShellItemChanged,
		ShellSectionChanged,
		ShellContentChanged,
	}
}