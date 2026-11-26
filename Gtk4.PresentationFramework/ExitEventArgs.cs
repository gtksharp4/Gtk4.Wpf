using System;

namespace System.Windows;

/// <summary>Arguments for <see cref="Application.Exit"/>.</summary>
public class ExitEventArgs : EventArgs
{
	internal ExitEventArgs(int exitCode)
	{
		ApplicationExitCode = exitCode;
	}

	/// <summary>The exit code <see cref="Application.Run()"/> returns; a handler may change it.</summary>
	public int ApplicationExitCode { get; set; }
}

/// <summary>Handles <see cref="Application.Exit"/>.</summary>
public delegate void ExitEventHandler(object sender, ExitEventArgs e);
