using Xunit;
using WpfTabControl = System.Windows.Controls.TabControl;
using WpfTabItem    = System.Windows.Controls.TabItem;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pinning tests for G2: <see cref="WpfTabControl.SelectionChanged"/> must fire
/// exactly once for each logical selection change, regardless of entry point.
/// </summary>
public class TabControlSelectionTests : WpfGtkTestBase
{
	static (WpfTabControl tc, WpfTabItem ti1, WpfTabItem ti2) BuildTwo()
	{
		var ti1 = new WpfTabItem { Header = "Tab1" };
		var ti2 = new WpfTabItem { Header = "Tab2" };
		var tc = new WpfTabControl();
		tc.ItemsSource = new[] { ti1, ti2 };
		return (tc, ti1, ti2);
	}

	// ── Pure DP (no GTK) ─────────────────────────────────────────────────────

	[Fact]
	public void SelectionChanged_fires_exactly_once_on_SelectedItem_set()
	{
		var (tc, _, ti2) = BuildTwo();
		int count = 0;
		tc.SelectionChanged += (_, _) => count++;
		tc.SelectedItem = ti2;
		Assert.Equal(1, count);
	}

	[Fact]
	public void SelectionChanged_fires_exactly_once_on_SelectedIndex_set()
	{
		var (tc, _, _) = BuildTwo();
		int count = 0;
		tc.SelectionChanged += (_, _) => count++;
		tc.SelectedIndex = 1;
		Assert.Equal(1, count);
	}

	[Fact]
	public void SelectedIndex_and_SelectedItem_consistent_after_SelectedItem_set()
	{
		var (tc, _, ti2) = BuildTwo();
		tc.SelectedItem = ti2;
		Assert.Equal(1, tc.SelectedIndex);
		Assert.Same(ti2, tc.SelectedItem);
	}

	[Fact]
	public void SelectedIndex_and_SelectedItem_consistent_after_SelectedIndex_set()
	{
		var (tc, ti1, _) = BuildTwo();
		tc.SelectedIndex = 0;
		Assert.Equal(0, tc.SelectedIndex);
		Assert.Same(ti1, tc.SelectedItem);
	}

	// ── GTK-origin (Notebook realized) ────────────────────────────────────────

	/// <summary>
	/// G2 pinning test: with the Notebook realized, setting <see cref="WpfTabControl.SelectedItem"/>
	/// programmatically must fire <see cref="WpfTabControl.SelectionChanged"/> exactly once.
	/// Before the fix it fired twice (once from <c>OnSelectedItemChanged</c>, once from
	/// <c>OnGtkSwitchPage</c>).
	/// </summary>
	[Fact]
	public void TabControl_SelectionChanged_fires_once_programmatic_with_notebook_realized()
	{
		Run(() =>
		{
			var (tc, _, ti2) = BuildTwo();
			_ = tc.Widget; // realize the Notebook

			int count = 0;
			tc.SelectionChanged += (_, _) => count++;
			tc.SelectedItem = ti2;
			Assert.Equal(1, count);
		});
	}

	[Fact]
	public void TabControl_SelectedIndex_consistent_after_programmatic_set_with_notebook()
	{
		Run(() =>
		{
			var (tc, _, ti2) = BuildTwo();
			_ = tc.Widget;
			tc.SelectedItem = ti2;
			Assert.Equal(1, tc.SelectedIndex);
			Assert.Same(ti2, tc.SelectedItem);
		});
	}

	[Fact]
	public void TabControl_SelectionChanged_fires_once_on_GtkPageSwitch()
	{
		Run(() =>
		{
			var (tc, _, _) = BuildTwo();
			var nb = (Gtk.Notebook)tc.Widget;

			int count = 0;
			tc.SelectionChanged += (_, _) => count++;
			nb.CurrentPage = 1;
			Assert.Equal(1, count);
		});
	}

	[Fact]
	public void A_tab_marked_IsSelected_before_it_has_an_owner_becomes_the_selection()
	{
		Run(() =>
		{
			var a = new WpfTabItem();
			var b = new WpfTabItem { IsSelected = true };
			var tc = new WpfTabControl();
			tc.Items.Add(a);
			tc.Items.Add(b);
			_ = tc.Widget;

			Assert.Same(b, tc.SelectedItem);
			Assert.False(a.IsSelected);
			Assert.True(b.IsSelected);
			Assert.Equal(1, tc.SelectedIndex);
		});
	}
}
