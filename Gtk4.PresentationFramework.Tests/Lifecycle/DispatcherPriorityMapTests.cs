using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>Pure tests of the DispatcherPriority to GLib priority mapping.</summary>
public class DispatcherPriorityMapTests
{
	static readonly DispatcherPriority[] _wpfOrder =
	{
		DispatcherPriority.Send, DispatcherPriority.Normal, DispatcherPriority.DataBind,
		DispatcherPriority.Render, DispatcherPriority.Loaded, DispatcherPriority.Input,
		DispatcherPriority.Background, DispatcherPriority.ContextIdle,
		DispatcherPriority.ApplicationIdle, DispatcherPriority.SystemIdle,
	};

	[Fact]
	public void Mapping_preserves_WPF_order_with_distinct_priorities()
	{
		var glib = _wpfOrder.Select(Application.ToGLibPriority).ToArray();
		for (int i = 1; i < glib.Length; i++)
			Assert.True(glib[i - 1] < glib[i], $"{_wpfOrder[i - 1]} must run before {_wpfOrder[i]}");
	}

	[Theory]
	[InlineData(DispatcherPriority.Loaded)]
	[InlineData(DispatcherPriority.Input)]
	[InlineData(DispatcherPriority.Render)]
	[InlineData(DispatcherPriority.DataBind)]
	[InlineData(DispatcherPriority.Normal)]
	[InlineData(DispatcherPriority.Send)]
	public void Input_and_above_is_never_idle(DispatcherPriority priority) =>
		Assert.True(Application.ToGLibPriority(priority) <= (int)GLib.Priority.Default);

	[Fact]
	public void Background_and_below_are_idle_priorities()
	{
		foreach (var p in new[] { DispatcherPriority.Background, DispatcherPriority.ContextIdle,
			DispatcherPriority.ApplicationIdle, DispatcherPriority.SystemIdle })
			Assert.True(Application.ToGLibPriority(p) > (int)GLib.Priority.Default);
	}

	[Fact]
	public void Background_runs_after_GTK_redraw_and_before_ContextIdle()
	{
		var background = Application.ToGLibPriority(DispatcherPriority.Background);
		Assert.True(background > (int)GLib.Priority.HighIdle + 20);
		Assert.True(background < Application.ToGLibPriority(DispatcherPriority.ContextIdle));
	}

	[Theory]
	[InlineData(DispatcherPriority.Invalid)]
	[InlineData(DispatcherPriority.Inactive)]
	[InlineData((DispatcherPriority)11)]
	public void Unprocessable_priorities_are_rejected(DispatcherPriority priority) =>
		Assert.Throws<ArgumentOutOfRangeException>(() => Application.ToGLibPriority(priority));
}
