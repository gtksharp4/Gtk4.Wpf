using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="DragEventArgs"/>, <see cref="QueryContinueDragEventArgs"/>,
/// and <see cref="GiveFeedbackEventArgs"/>.
/// </summary>
public sealed class DragEventArgsTests
{
	// ── DragEventArgs ─────────────────────────────────────────────────────────

	[Fact]
	public void DragEventArgs_carries_data_effects_keystates()
	{
		var data = new DataObject("hello");
		var e = new DragEventArgs(
			UIElement.DragEnterEvent, data,
			DragDropEffects.Copy, DragDropKeyStates.ControlKey,
			new Point(10, 20));

		Assert.Same(data, e.Data);
		Assert.Equal(DragDropEffects.Copy, e.AllowedEffects);
		Assert.Equal(DragDropEffects.Copy, e.Effects);
		Assert.Equal(DragDropKeyStates.ControlKey, e.KeyStates);
	}

	[Fact]
	public void DragEventArgs_GetPosition_returns_position()
	{
		var e = new DragEventArgs(
			UIElement.DragEnterEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None,
			new Point(5, 7));

		var pos = e.GetPosition(null);
		Assert.Equal(5.0, pos.X);
		Assert.Equal(7.0, pos.Y);
	}

	[Fact]
	public void DragEventArgs_Effects_is_mutable()
	{
		var e = new DragEventArgs(
			UIElement.DragEnterEvent, new DataObject(),
			DragDropEffects.All, DragDropKeyStates.None,
			new Point(0, 0));

		e.Effects = DragDropEffects.Move;
		Assert.Equal(DragDropEffects.Move, e.Effects);
	}

	[Fact]
	public void DragEventArgs_null_data_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			new DragEventArgs(UIElement.DragEnterEvent, null!, DragDropEffects.None,
				DragDropKeyStates.None, default));
	}

	[Fact]
	public void DragEventArgs_Handled_starts_false()
	{
		var e = new DragEventArgs(UIElement.DragEnterEvent, new DataObject(),
			DragDropEffects.Copy, DragDropKeyStates.None, default);
		Assert.False(e.Handled);
	}

	[Fact]
	public void DragEventArgs_Effects_defaults_to_AllowedEffects()
	{
		var e = new DragEventArgs(UIElement.DropEvent, new DataObject(),
			DragDropEffects.Move, DragDropKeyStates.None, default);
		Assert.Equal(DragDropEffects.Move, e.AllowedEffects);
		Assert.Equal(DragDropEffects.Move, e.Effects);
	}

	// ── QueryContinueDragEventArgs ────────────────────────────────────────────

	[Fact]
	public void QueryContinueDragEventArgs_carries_escape_and_keystates()
	{
		var e = new QueryContinueDragEventArgs(
			UIElement.DragEnterEvent,
			escapePressed: true,
			DragDropKeyStates.ShiftKey);

		Assert.True(e.EscapePressed);
		Assert.Equal(DragDropKeyStates.ShiftKey, e.KeyStates);
	}

	[Fact]
	public void QueryContinueDragEventArgs_default_action_is_Continue()
	{
		var e = new QueryContinueDragEventArgs(
			UIElement.DragEnterEvent, false, DragDropKeyStates.None);
		Assert.Equal(DragAction.Continue, e.Action);
	}

	[Fact]
	public void QueryContinueDragEventArgs_action_is_mutable()
	{
		var e = new QueryContinueDragEventArgs(
			UIElement.DragEnterEvent, false, DragDropKeyStates.None);
		e.Action = DragAction.Cancel;
		Assert.Equal(DragAction.Cancel, e.Action);
	}

	// ── GiveFeedbackEventArgs ─────────────────────────────────────────────────

	[Fact]
	public void GiveFeedbackEventArgs_carries_effects()
	{
		var e = new GiveFeedbackEventArgs(UIElement.DragEnterEvent, DragDropEffects.Copy);
		Assert.Equal(DragDropEffects.Copy, e.Effects);
	}

	[Fact]
	public void GiveFeedbackEventArgs_UseDefaultCursors_is_true_by_default()
	{
		var e = new GiveFeedbackEventArgs(UIElement.DragEnterEvent, DragDropEffects.Move);
		Assert.True(e.UseDefaultCursors);
	}

	[Fact]
	public void GiveFeedbackEventArgs_UseDefaultCursors_is_mutable()
	{
		var e = new GiveFeedbackEventArgs(UIElement.DragEnterEvent, DragDropEffects.None);
		e.UseDefaultCursors = false;
		Assert.False(e.UseDefaultCursors);
	}
}
