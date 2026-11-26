using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for <see cref="LogicalTreeHelper"/>.
/// </summary>
public class LogicalTreeHelperTests
{
	// ── Panel tests ───────────────────────────────────────────────────────────────

	[Fact]
	public void Panel_GetChildren_returns_children()
	{
		var panel = new TestPanel();
		var c0 = new TextBlock();
		var c1 = new TextBlock();
		panel.Children.Add(c0);
		panel.Children.Add(c1);

		var children = LogicalTreeHelper.GetChildren(panel).Cast<object>().ToList();

		Assert.Equal(2, children.Count);
		Assert.Contains(c0, children);
		Assert.Contains(c1, children);
	}

	[Fact]
	public void Panel_GetChildren_empty_panel_returns_empty()
	{
		var panel = new TestPanel();
		var children = LogicalTreeHelper.GetChildren(panel).Cast<object>().ToList();
		Assert.Empty(children);
	}

	[Fact]
	public void Panel_GetParent_returns_panel()
	{
		var panel = new TestPanel();
		var child = new TextBlock();
		panel.Children.Add(child);

		Assert.Same(panel, LogicalTreeHelper.GetParent(child));
	}

	[Fact]
	public void Panel_GetParent_root_returns_null()
	{
		var panel = new TestPanel();
		Assert.Null(LogicalTreeHelper.GetParent(panel));
	}

	// ── ContentControl tests ──────────────────────────────────────────────────────

	[Fact]
	public void ContentControl_GetChildren_returns_UIElement_content()
	{
		var cc = new TestContentControl();
		var child = new TextBlock();
		cc.Content = child;

		var children = LogicalTreeHelper.GetChildren(cc).Cast<object>().ToList();

		Assert.Single(children);
		Assert.Contains(child, children);
	}

	[Fact]
	public void ContentControl_GetChildren_no_content_returns_empty()
	{
		var cc = new TestContentControl();
		var children = LogicalTreeHelper.GetChildren(cc).Cast<object>().ToList();
		Assert.Empty(children);
	}

	[Fact]
	public void ContentControl_GetChildren_string_content_returns_empty()
	{
		var cc = new TestContentControl();
		cc.Content = "text";
		var children = LogicalTreeHelper.GetChildren(cc).Cast<object>().ToList();
		Assert.Empty(children);
	}

	[Fact]
	public void ContentControl_GetParent_returns_panel()
	{
		var panel = new TestPanel();
		var cc = new TestContentControl();
		panel.Children.Add(cc);

		Assert.Same(panel, LogicalTreeHelper.GetParent(cc));
	}

	// ── FindLogicalNode tests ─────────────────────────────────────────────────────

	[Fact]
	public void FindLogicalNode_finds_named_child()
	{
		var panel = new TestPanel();
		var child = new TextBlock { Name = "targetChild" };
		panel.Children.Add(child);

		var found = LogicalTreeHelper.FindLogicalNode(panel, "targetChild");

		Assert.Same(child, found);
	}

	[Fact]
	public void FindLogicalNode_returns_null_when_not_found()
	{
		var panel = new TestPanel();
		panel.Children.Add(new TextBlock { Name = "other" });

		var found = LogicalTreeHelper.FindLogicalNode(panel, "missing");

		Assert.Null(found);
	}

	[Fact]
	public void FindLogicalNode_finds_root_itself()
	{
		var panel = new TestPanel { Name = "root" };

		var found = LogicalTreeHelper.FindLogicalNode(panel, "root");

		Assert.Same(panel, found);
	}

	[Fact]
	public void FindLogicalNode_searches_nested_tree()
	{
		var outer = new TestPanel();
		var inner = new TestPanel();
		var deepChild = new TextBlock { Name = "deep" };
		inner.Children.Add(deepChild);
		outer.Children.Add(inner);

		var found = LogicalTreeHelper.FindLogicalNode(outer, "deep");

		Assert.Same(deepChild, found);
	}

	// ── GTK-free test doubles ─────────────────────────────────────────────────────

	sealed class TestPanel : Panel
	{
		internal override bool IsWidgetCreated => false;
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
		protected override void AddChildWidget(UIElement child) { }
		protected override void RemoveChildWidget(UIElement child) { }
	}

	sealed class TestContentControl : ContentControl
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}
}
