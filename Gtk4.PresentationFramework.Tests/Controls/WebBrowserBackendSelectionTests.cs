using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Default host backend selection for <see cref="WebBrowser"/>: CefSharp on Windows (the
/// net10.0-windows package variant), Photino on every other platform/variant.
/// </summary>
public class WebBrowserBackendSelectionTests
{
	[Theory]
	[InlineData(true, true, true)]    // Windows + CefSharp compiled in -> CefSharp
	[InlineData(true, false, false)]  // Windows but CefSharp not compiled (net10.0 variant) -> Photino
	[InlineData(false, true, false)]  // non-Windows -> Photino even when compiled in
	[InlineData(false, false, false)]
	public void ShouldUseCefSharp_truth_table(bool isWindows, bool cefCompiledIn, bool expected)
	{
		Assert.Equal(expected, WebBrowser.ShouldUseCefSharp(isWindows, cefCompiledIn));
	}

	[Fact]
	public void Cross_platform_net10_variant_does_not_compile_in_CefSharp()
	{
		// This test assembly references the cross-platform net10.0 variant of the library, which
		// must never carry the Windows-only CefSharp backend.
		Assert.False(WebBrowser.CefSharpCompiledIn);
	}
}
