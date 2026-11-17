using System;

namespace Gtk4.Forms
{
	public class AppThemeChangedEventArgs : EventArgs
	{
		public AppThemeChangedEventArgs(OSAppTheme appTheme) =>
			RequestedTheme = appTheme;

		public OSAppTheme RequestedTheme { get; }
	}
}