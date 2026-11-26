using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>
/// Pure DP tests for <see cref="UIElement.IsTabStop"/>, <see cref="UIElement.TabIndex"/>,
/// and the <see cref="KeyboardNavigation"/> attached properties.
/// No GTK runtime required.
/// </summary>
public class TabNavigationDpTests
{
	// ── IsTabStop DP ─────────────────────────────────────────────────────────

	[Fact]
	public void IsTabStop_default_is_true()
	{
		var el = new TextBlock();
		Assert.True(el.IsTabStop);
	}

	[Fact]
	public void IsTabStop_set_false_round_trip()
	{
		var el = new TextBlock();
		el.IsTabStop = false;
		Assert.False(el.IsTabStop);
	}

	[Fact]
	public void IsTabStop_set_true_round_trip()
	{
		var el = new TextBlock();
		el.IsTabStop = false;
		el.IsTabStop = true;
		Assert.True(el.IsTabStop);
	}

	[Fact]
	public void IsTabStop_set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		var ex = Record.Exception(() => el.IsTabStop = false);
		Assert.Null(ex);
		Assert.False(el.IsTabStop);
	}

	// ── TabIndex DP ───────────────────────────────────────────────────────────

	[Fact]
	public void TabIndex_default_is_int_MaxValue()
	{
		var el = new TextBlock();
		Assert.Equal(int.MaxValue, el.TabIndex);
	}

	[Fact]
	public void TabIndex_set_get_round_trip()
	{
		var el = new TextBlock();
		el.TabIndex = 42;
		Assert.Equal(42, el.TabIndex);
	}

	[Fact]
	public void TabIndex_set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		var ex = Record.Exception(() => el.TabIndex = 10);
		Assert.Null(ex);
		Assert.Equal(10, el.TabIndex);
	}

	[Fact]
	public void TabIndex_persists_after_set()
	{
		var el = new TextBlock();
		el.TabIndex = 7;
		Assert.Equal(7, el.TabIndex);
	}

	// ── KeyboardNavigation.GetTabNavigation / SetTabNavigation ─────────────────

	[Fact]
	public void TabNavigation_default_is_Continue()
	{
		var el = new StackPanel();
		Assert.Equal(KeyboardNavigationMode.Continue, KeyboardNavigation.GetTabNavigation(el));
	}

	[Fact]
	public void TabNavigation_set_get_round_trip()
	{
		var el = new StackPanel();
		KeyboardNavigation.SetTabNavigation(el, KeyboardNavigationMode.Cycle);
		Assert.Equal(KeyboardNavigationMode.Cycle, KeyboardNavigation.GetTabNavigation(el));
	}

	[Fact]
	public void TabNavigation_set_all_modes()
	{
		var el = new StackPanel();
		foreach (KeyboardNavigationMode mode in Enum.GetValues(typeof(KeyboardNavigationMode)))
		{
			KeyboardNavigation.SetTabNavigation(el, mode);
			Assert.Equal(mode, KeyboardNavigation.GetTabNavigation(el));
		}
	}

	// ── KeyboardNavigation.DirectionalNavigation ──────────────────────────────

	[Fact]
	public void DirectionalNavigation_default_is_Continue()
	{
		var el = new StackPanel();
		Assert.Equal(KeyboardNavigationMode.Continue,
			KeyboardNavigation.GetDirectionalNavigation(el));
	}

	[Fact]
	public void DirectionalNavigation_set_None_round_trip()
	{
		var el = new StackPanel();
		KeyboardNavigation.SetDirectionalNavigation(el, KeyboardNavigationMode.None);
		Assert.Equal(KeyboardNavigationMode.None,
			KeyboardNavigation.GetDirectionalNavigation(el));
	}

	// ── KeyboardNavigation.IsTabStop attached mirror ──────────────────────────

	[Fact]
	public void Attached_IsTabStop_default_is_true()
	{
		var el = new TextBlock();
		Assert.True(KeyboardNavigation.GetIsTabStop(el));
	}

	[Fact]
	public void Attached_IsTabStop_set_false_round_trip()
	{
		var el = new TextBlock();
		KeyboardNavigation.SetIsTabStop(el, false);
		Assert.False(KeyboardNavigation.GetIsTabStop(el));
	}

	// ── KeyboardNavigation.TabIndex attached mirror ───────────────────────────

	[Fact]
	public void Attached_TabIndex_default_is_int_MaxValue()
	{
		var el = new TextBlock();
		Assert.Equal(int.MaxValue, KeyboardNavigation.GetTabIndex(el));
	}

	[Fact]
	public void Attached_TabIndex_set_get_round_trip()
	{
		var el = new TextBlock();
		KeyboardNavigation.SetTabIndex(el, 3);
		Assert.Equal(3, KeyboardNavigation.GetTabIndex(el));
	}

	// ── Null argument guards ──────────────────────────────────────────────────

	[Fact]
	public void GetTabNavigation_null_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			KeyboardNavigation.GetTabNavigation(null!));
	}

	[Fact]
	public void SetTabNavigation_null_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			KeyboardNavigation.SetTabNavigation(null!, KeyboardNavigationMode.Cycle));
	}
}
