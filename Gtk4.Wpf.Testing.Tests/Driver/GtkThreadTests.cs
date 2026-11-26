using System;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Driver;

/// <summary>The no-runtime path: GTK is never iterated after a failed start, which would kill the test host.</summary>
public class GtkThreadTests
{
	[Fact]
	public void A_missing_native_library_reports_the_reason_and_never_iterates()
	{
		int iterations = 0;
		string signalled = null;
		string reason = GtkThread.RunLoop(() => throw new DllNotFoundException("libgtk-4-1.so.0"),
			() => iterations++, r => signalled = r);
		Assert.Contains("libgtk-4-1.so.0", reason);
		Assert.Equal(reason, signalled);
		Assert.Equal(0, iterations);
	}

	[Fact]
	public void A_type_initializer_failure_caused_by_a_missing_library_is_the_same_case()
	{
		int iterations = 0;
		string reason = GtkThread.RunLoop(
			() => throw new TypeInitializationException("Gtk.Application", new DllNotFoundException("gtk-4-1.dll")),
			() => iterations++, _ => { });
		Assert.Contains("gtk-4-1.dll", reason);
		Assert.Equal(0, iterations);
	}

	[Fact]
	public void Another_init_failure_is_not_swallowed()
	{
		Assert.Throws<InvalidOperationException>(() =>
			GtkThread.RunLoop(() => throw new InvalidOperationException(), () => { }, _ => { }));
	}
}
