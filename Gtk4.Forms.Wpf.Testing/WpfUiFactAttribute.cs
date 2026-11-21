using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Gtk4.Forms.Wpf.Testing;

/// <summary>
/// A UI test. It is skipped when no GTK runtime or display is available, unless <c>XF_GTK_REQUIRE_NATIVE</c> is
/// set: CI sets it so a broken install fails instead of passing by absence.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WpfUiFactAttribute : FactAttribute
{
	public WpfUiFactAttribute([CallerFilePath] string? sourceFilePath = null,
		[CallerLineNumber] int sourceLineNumber = -1) : base(sourceFilePath, sourceLineNumber)
	{
		SkipUnless = nameof(WpfUiEnvironment.CanRun);
		SkipType = typeof(WpfUiEnvironment);
		Skip = "No GTK 4 runtime or display is available (set XF_GTK_REQUIRE_NATIVE=1 to fail instead).";
	}
}

/// <summary>What the UI test attributes ask at run time.</summary>
public static class WpfUiEnvironment
{
	/// <summary>True when GTK runs, or when its absence must fail the test instead of skipping it.</summary>
	public static bool CanRun =>
		!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("XF_GTK_REQUIRE_NATIVE"))
		|| GtkThread.UnavailableReason == null;
}
