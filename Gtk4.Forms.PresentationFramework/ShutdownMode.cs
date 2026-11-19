namespace System.Windows;

/// <summary>When an <see cref="Application"/> shuts down automatically.</summary>
public enum ShutdownMode
{
	/// <summary>When the last window closes (a created-but-never-shown window counts).</summary>
	OnLastWindowClose = 0,

	/// <summary>When <see cref="Application.MainWindow"/> closes.</summary>
	OnMainWindowClose = 1,

	/// <summary>Only on an explicit <see cref="Application.Shutdown()"/> call.</summary>
	OnExplicitShutdown = 2,
}
