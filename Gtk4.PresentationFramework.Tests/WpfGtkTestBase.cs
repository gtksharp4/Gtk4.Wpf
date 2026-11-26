using Xunit;

/// <summary>
/// Base class for WPF-on-GTK smoke tests. Skips automatically when the native GTK
/// runtime is unavailable (no GTK installed, no DISPLAY, etc.).
/// </summary>
public abstract class WpfGtkTestBase
{
	protected WpfGtkTestBase()
	{
		var reason = WpfGtkFixture.Instance.UnavailableReason;
		if (reason != null)
			SkipGtk(reason);
	}

	static void SkipGtk(string reason)
	{
		// Fail hard only when XF_GTK_REQUIRE_NATIVE is set (CI enforces it).
		var required = System.Environment.GetEnvironmentVariable("XF_GTK_REQUIRE_NATIVE");
		Assert.False(
			!string.IsNullOrEmpty(required),
			$"{reason} (XF_GTK_REQUIRE_NATIVE is set, so this is a failure rather than a skip.)");
		Assert.Skip(reason);
	}

	/// <summary>Runs <paramref name="body"/> on the GTK thread.</summary>
	protected void Run(System.Action body) => WpfGtkFixture.Instance.Invoke(body);

	/// <summary>Runs <paramref name="body"/> on the GTK thread and returns its value.</summary>
	protected T Run<T>(System.Func<T> body) => WpfGtkFixture.Instance.Invoke(body);
}
