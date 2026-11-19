namespace System.Windows;

/// <summary>Pure decision logic for <see cref="ShutdownMode"/>.</summary>
static class ShutdownPolicy
{
	/// <summary>
	/// Whether closing a window should shut the application down.
	/// <paramref name="remainingWindows"/> is the count left after the closing window was removed.
	/// </summary>
	internal static bool ShouldShutdown(ShutdownMode mode, bool closingIsMain, int remainingWindows)
	{
		switch (mode)
		{
			case ShutdownMode.OnLastWindowClose: return remainingWindows == 0;
			case ShutdownMode.OnMainWindowClose: return closingIsMain;
			default: return false;
		}
	}
}
