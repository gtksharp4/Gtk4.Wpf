using System;
using System.Runtime.CompilerServices;
using Xunit;

namespace Gtk4.Wpf.Testing;

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

/// <summary>
/// A UI test that injects real operating-system input (<see cref="NativeInput"/>). It runs only on Windows and only
/// when <c>XF_NATIVE_INPUT=1</c> is set, because it moves the user's cursor and needs an unlocked desktop. Put
/// <c>[Trait("UI", "Native")]</c> on the class so a run can also select or exclude these tests by trait.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WpfNativeFactAttribute : FactAttribute
{
	public WpfNativeFactAttribute([CallerFilePath] string? sourceFilePath = null,
		[CallerLineNumber] int sourceLineNumber = -1) : base(sourceFilePath, sourceLineNumber)
	{
		SkipUnless = nameof(WpfNativeEnvironment.CanRun);
		SkipType = typeof(WpfNativeEnvironment);
		Skip = "Native input tests are opt-in: Windows only, with XF_NATIVE_INPUT=1 on an interactive desktop.";
	}
}

/// <summary>What <see cref="WpfNativeFactAttribute"/> asks at run time.</summary>
public static class WpfNativeEnvironment
{
	/// <summary>True only for <c>XF_NATIVE_INPUT</c> set to exactly <c>1</c>.</summary>
	public static bool OptedIn(string? value) => value == "1";

	public static bool CanRun =>
		NativeInput.IsSupported && OptedIn(Environment.GetEnvironmentVariable("XF_NATIVE_INPUT"))
		&& WpfUiEnvironment.CanRun;
}
