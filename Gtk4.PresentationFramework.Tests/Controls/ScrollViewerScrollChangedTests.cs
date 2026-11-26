using Xunit;
using System.Windows.Controls;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for <see cref="ScrollViewer.ScrollChanged"/>.
/// All bodies run on the GTK thread via Run(…); skipped when GTK is unavailable.
/// </summary>
public class ScrollViewerScrollChangedTests : WpfGtkTestBase
{
	[Fact]
	public void ScrollChanged_fires_on_ScrollToVerticalOffset() => Run(() =>
	{
		var (sv, _, _) = ScrollViewerGtkSmokeTests.MakeScrollViewer();
		ScrollChangedEventArgs? fired = null;
		sv.ScrollChanged += (_, e) => fired = e;
		sv.ScrollToVerticalOffset(60.0);
		Assert.NotNull(fired);
	});

	[Fact]
	public void ScrollToVerticalOffset_raises_ScrollChanged_exactly_once() => Run(() =>
	{
		var (sv, _, _) = ScrollViewerGtkSmokeTests.MakeScrollViewer();
		int count = 0;
		sv.ScrollChanged += (_, _) => count++;
		// WPF raises ScrollChanged once per programmatic scroll; the reentrancy guard must
		// suppress GTK's synchronous value-changed so only the explicit raise fires.
		sv.ScrollToVerticalOffset(60.0);
		Assert.Equal(1, count);
	});

	[Fact]
	public void ScrollChanged_args_carry_new_offset() => Run(() =>
	{
		var (sv, _, _) = ScrollViewerGtkSmokeTests.MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		ScrollChangedEventArgs? fired = null;
		sv.ScrollChanged += (_, e) => fired = e;
		sv.ScrollToVerticalOffset(80.0);
		Assert.NotNull(fired);
		Assert.Equal(80.0, fired!.VerticalOffset, 6);
	});

	[Fact]
	public void ScrollChanged_args_carry_extent_and_viewport() => Run(() =>
	{
		var (sv, _, _) = ScrollViewerGtkSmokeTests.MakeScrollViewer(upper: 500.0, pageSize: 100.0);
		ScrollChangedEventArgs? fired = null;
		sv.ScrollChanged += (_, e) => fired = e;
		sv.ScrollToVerticalOffset(50.0);
		Assert.Equal(500.0, fired!.ExtentHeight, 6);
		Assert.Equal(100.0, fired!.ViewportHeight, 6);
	});

	[Fact]
	public void ScrollChanged_fires_when_adjustment_changed_externally() => Run(() =>
	{
		var (sv, vadj, _) = ScrollViewerGtkSmokeTests.MakeScrollViewer();
		ScrollChangedEventArgs? fired = null;
		sv.ScrollChanged += (_, e) => fired = e;
		vadj.Value = 55.0;
		Assert.NotNull(fired);
		Assert.Equal(55.0, fired!.VerticalOffset, 6);
	});
}
