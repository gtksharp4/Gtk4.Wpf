using System.Collections.Generic;
using Xunit;
using WpfContextMenu    = System.Windows.Controls.ContextMenu;
using WpfMenu           = System.Windows.Controls.Menu;
using WpfMenuItem       = System.Windows.Controls.MenuItem;
using WpfToolBar        = System.Windows.Controls.ToolBar;
using WpfToolTipService = System.Windows.Controls.ToolTipService;
using WpfTreeView       = System.Windows.Controls.TreeView;
using WpfTreeViewItem   = System.Windows.Controls.TreeViewItem;
using WpfButton         = System.Windows.Controls.Button;
using WpfTextBlock      = System.Windows.Controls.TextBlock;
using WpfPopup          = System.Windows.Controls.Primitives.Popup;

namespace Gtk.Forms.PresentationFramework.Tests.Controls
{
	/// <summary>
	/// Pure DP tests for Round-6 hierarchical and popup controls.
	/// No GTK required; each test fails without the feature.
	/// </summary>
	public class HierarchicalDpTests
	{
		// ── MenuItem ─────────────────────────────────────────────────────────────

		[Fact]
		public void MenuItem_Header_default_is_null()
		{
			var mi = new WpfMenuItem();
			Assert.Null(mi.Header);
		}

		[Fact]
		public void MenuItem_Header_roundtrip()
		{
			var mi = new WpfMenuItem { Header = "File" };
			Assert.Equal("File", mi.Header);
		}

		[Fact]
		public void MenuItem_IsCheckable_default_false()
		{
			var mi = new WpfMenuItem();
			Assert.False(mi.IsCheckable);
		}

		[Fact]
		public void MenuItem_IsChecked_default_false()
		{
			var mi = new WpfMenuItem();
			Assert.False(mi.IsChecked);
		}

		[Fact]
		public void MenuItem_IsChecked_roundtrip()
		{
			var mi = new WpfMenuItem { IsCheckable = true, IsChecked = true };
			Assert.True(mi.IsChecked);
		}

		[Fact]
		public void MenuItem_Click_fires_on_InvokeClick()
		{
			var mi = new WpfMenuItem { Header = "Open" };
			bool fired = false;
			mi.Click += (s, e) => { fired = true; };
			mi.InvokeClick_ForTest();
			Assert.True(fired);
		}

		[Fact]
		public void MenuItem_IsCheckable_toggle_on_InvokeClick()
		{
			var mi = new WpfMenuItem { IsCheckable = true };
			Assert.False(mi.IsChecked);
			mi.InvokeClick_ForTest();
			Assert.True(mi.IsChecked);
			mi.InvokeClick_ForTest();
			Assert.False(mi.IsChecked);
		}

		[Fact]
		public void MenuItem_Checked_event_fires_on_toggle()
		{
			var mi = new WpfMenuItem { IsCheckable = true };
			bool fired = false;
			mi.Checked += (s, e) => { fired = true; };
			mi.InvokeClick_ForTest();
			Assert.True(fired);
		}

		[Fact]
		public void MenuItem_InputGestureText_roundtrip()
		{
			var mi = new WpfMenuItem { InputGestureText = "Ctrl+S" };
			Assert.Equal("Ctrl+S", mi.InputGestureText);
		}

		[Fact]
		public void MenuItem_Items_starts_empty()
		{
			var mi = new WpfMenuItem();
			Assert.Empty(mi.Items);
		}

		[Fact]
		public void MenuItem_Items_add_subitem()
		{
			var parent = new WpfMenuItem { Header = "File" };
			var child  = new WpfMenuItem { Header = "Open" };
			parent.Items.Add(child);
			Assert.Single(parent.Items);
			Assert.Same(child, parent.Items[0]);
		}

		// ── TreeViewItem ─────────────────────────────────────────────────────────

		[Fact]
		public void TreeViewItem_IsExpanded_default_false()
		{
			var tvi = new WpfTreeViewItem();
			Assert.False(tvi.IsExpanded);
		}

		[Fact]
		public void TreeViewItem_IsExpanded_roundtrip()
		{
			var tvi = new WpfTreeViewItem { IsExpanded = true };
			Assert.True(tvi.IsExpanded);
		}

		[Fact]
		public void TreeViewItem_IsSelected_default_false()
		{
			var tvi = new WpfTreeViewItem();
			Assert.False(tvi.IsSelected);
		}

		[Fact]
		public void TreeViewItem_IsSelected_roundtrip()
		{
			var tvi = new WpfTreeViewItem { IsSelected = true };
			Assert.True(tvi.IsSelected);
		}

		[Fact]
		public void TreeViewItem_Header_roundtrip()
		{
			var tvi = new WpfTreeViewItem { Header = "Root" };
			Assert.Equal("Root", tvi.Header);
		}

		[Fact]
		public void TreeViewItem_Items_add_child()
		{
			var root  = new WpfTreeViewItem { Header = "Root" };
			var child = new WpfTreeViewItem { Header = "Child" };
			root.Items.Add(child);
			Assert.Single(root.Items);
		}

		[Fact]
		public void TreeViewItem_Selected_event_fires_on_IsSelected_true()
		{
			var tvi = new WpfTreeViewItem();
			bool fired = false;
			tvi.Selected += (s, e) => { fired = true; };
			tvi.IsSelected = true;
			Assert.True(fired);
		}

		[Fact]
		public void TreeViewItem_Unselected_event_fires_on_IsSelected_false()
		{
			var tvi = new WpfTreeViewItem { IsSelected = true };
			bool fired = false;
			tvi.Unselected += (s, e) => { fired = true; };
			tvi.IsSelected = false;
			Assert.True(fired);
		}
	}
}
