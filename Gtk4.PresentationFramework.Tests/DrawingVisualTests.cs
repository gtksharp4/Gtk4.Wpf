using System;
using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for <see cref="DrawingVisual"/>, <see cref="VisualCollection"/>,
/// and <see cref="DrawingContext"/>.
/// </summary>
public class DrawingVisualTests
{
	// ── DrawingVisual / RenderOpen ────────────────────────────────────────────────

	[Fact]
	public void RenderOpen_returns_scope_with_non_null_context()
	{
		var dv = new DrawingVisual();
		using var scope = dv.RenderOpen();
		Assert.NotNull(scope.Context);
	}

	[Fact]
	public void Drawing_is_null_before_RenderOpen()
	{
		var dv = new DrawingVisual();
		Assert.Null(dv.Drawing);
	}

	[Fact]
	public void Drawing_is_set_after_scope_disposed()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawRectangle(null, null, new Rect(0, 0, 10, 10));
		}
		Assert.NotNull(dv.Drawing);
	}

	[Fact]
	public void RenderOpen_records_draw_commands()
	{
		var dv = new DrawingVisual();
		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawRectangle(null, null, new Rect(1, 2, 3, 4));
			scope.Context.DrawLine(new Pen(), new Point(0, 0), new Point(10, 10));
		}

		Assert.NotNull(dv.Drawing);
		Assert.Equal(2, dv.Drawing!.Commands.Count);
		Assert.Equal(DrawingContext.DrawCommandKind.DrawRectangle, dv.Drawing.Commands[0].Kind);
		Assert.Equal(DrawingContext.DrawCommandKind.DrawLine, dv.Drawing.Commands[1].Kind);
	}

	[Fact]
	public void Second_RenderOpen_replaces_previous_drawing()
	{
		var dv = new DrawingVisual();
		DrawingContext? first;
		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawRectangle(null, null, new Rect(0, 0, 1, 1));
			first = dv.Drawing; // null until disposed
		}
		first = dv.Drawing; // now set

		using (var scope = dv.RenderOpen())
		{
			scope.Context.DrawEllipse(null, null, new Point(5, 5), 3, 3);
		}

		Assert.Single(dv.Drawing!.Commands);
		Assert.Equal(DrawingContext.DrawCommandKind.DrawEllipse, dv.Drawing.Commands[0].Kind);
	}

	// ── VisualCollection ──────────────────────────────────────────────────────────

	[Fact]
	public void VisualCollection_add_increases_count()
	{
		var dv = new DrawingVisual();
		var coll = dv.Children;
		coll.Add(new DrawingVisual());
		Assert.Equal(1, coll.Count);
	}

	[Fact]
	public void VisualCollection_index_returns_correct_item()
	{
		var dv = new DrawingVisual();
		var coll = dv.Children;
		var child = new DrawingVisual();
		coll.Add(child);
		Assert.Same(child, coll[0]);
	}

	[Fact]
	public void VisualCollection_remove_decreases_count()
	{
		var dv = new DrawingVisual();
		var coll = dv.Children;
		var child = new DrawingVisual();
		coll.Add(child);
		bool removed = coll.Remove(child);
		Assert.True(removed);
		Assert.Equal(0, coll.Count);
	}

	[Fact]
	public void VisualCollection_sets_VisualParent_on_add()
	{
		var parent = new DrawingVisual();
		var child = new DrawingVisual();
		parent.Children.Add(child);
		Assert.Same(parent, child.VisualParent);
	}

	[Fact]
	public void VisualCollection_clears_VisualParent_on_remove()
	{
		var parent = new DrawingVisual();
		var child = new DrawingVisual();
		parent.Children.Add(child);
		parent.Children.Remove(child);
		Assert.Null(child.VisualParent);
	}

	[Fact]
	public void VisualCollection_clear_removes_all_and_clears_parents()
	{
		var parent = new DrawingVisual();
		var c1 = new DrawingVisual();
		var c2 = new DrawingVisual();
		parent.Children.Add(c1);
		parent.Children.Add(c2);
		parent.Children.Clear();
		Assert.Equal(0, parent.Children.Count);
		Assert.Null(c1.VisualParent);
		Assert.Null(c2.VisualParent);
	}

	[Fact]
	public void DrawingVisual_GetChildrenCount_matches_collection()
	{
		var parent = new DrawingVisual();
		parent.Children.Add(new DrawingVisual());
		parent.Children.Add(new DrawingVisual());
		Assert.Equal(2, VisualTreeHelper.GetChildrenCount(parent));
	}

	[Fact]
	public void DrawingVisual_GetChild_returns_correct_child()
	{
		var parent = new DrawingVisual();
		var c0 = new DrawingVisual();
		var c1 = new DrawingVisual();
		parent.Children.Add(c0);
		parent.Children.Add(c1);
		Assert.Same(c0, VisualTreeHelper.GetChild(parent, 0));
		Assert.Same(c1, VisualTreeHelper.GetChild(parent, 1));
	}
}
