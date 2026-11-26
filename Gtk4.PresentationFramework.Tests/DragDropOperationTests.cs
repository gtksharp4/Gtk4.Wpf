using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="DragDrop.DoDragDrop"/> and the <c>AllowDrop</c> DP.
/// All tests inject <see cref="InProcessDragDropBackend"/> — no GTK required.
/// </summary>
public sealed class DragDropOperationTests : IDisposable
{
	readonly IDragDropBackend _original;
	readonly InProcessDragDropBackend _backend = new();
	readonly SpyDragDropBackend _spy = new();

	public DragDropOperationTests()
	{
		_original = DragDrop.Backend;
		DragDrop.Backend = _backend;
	}

	public void Dispose() => DragDrop.Backend = _original;

	// ── Test element ───────────────────────────────────────────────────────────

	sealed class TestElement : UIElement
	{
		public new UIElement? LogicalParent
		{
			get => base.LogicalParent;
			set => base.LogicalParent = value;
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
	}

	// ── DoDragDrop wrapping ───────────────────────────────────────────────────

	[Fact]
	public void DoDragDrop_wraps_non_IDataObject_in_DataObject()
	{
		DragDrop.Backend = _spy;
		DragDrop.DoDragDrop(new TestElement(), "plain string", DragDropEffects.Copy);

		Assert.NotNull(_spy.LastData);
		Assert.IsType<DataObject>(_spy.LastData);
	}

	[Fact]
	public void DoDragDrop_accepts_IDataObject_directly()
	{
		DragDrop.Backend = _spy;
		var obj = new DataObject("text", "hello");
		DragDrop.DoDragDrop(new TestElement(), obj, DragDropEffects.Move);

		Assert.Same(obj, _spy.LastData);
	}

	[Fact]
	public void DoDragDrop_null_source_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			DragDrop.DoDragDrop(null!, "data", DragDropEffects.Copy));
	}

	[Fact]
	public void DoDragDrop_null_data_throws()
	{
		Assert.Throws<ArgumentNullException>(() =>
			DragDrop.DoDragDrop(new TestElement(), null!, DragDropEffects.Copy));
	}

	[Fact]
	public void DoDragDrop_headless_returns_simulated_result()
	{
		_backend.SimulatedResult = DragDropEffects.Move;
		DragDrop.Backend = _backend;

		var result = DragDrop.DoDragDrop(new TestElement(), new DataObject(), DragDropEffects.All);

		Assert.Equal(DragDropEffects.Move, result);
	}

	[Fact]
	public void DoDragDrop_headless_clamps_to_allowed_effects()
	{
		_backend.SimulatedResult = DragDropEffects.Move | DragDropEffects.Copy;
		DragDrop.Backend = _backend;

		var result = DragDrop.DoDragDrop(new TestElement(), new DataObject(), DragDropEffects.Copy);

		Assert.Equal(DragDropEffects.Copy, result);
		Assert.True((result & DragDropEffects.Move) == 0);
	}

	[Fact]
	public void DoDragDrop_headless_none_when_no_overlap()
	{
		_backend.SimulatedResult = DragDropEffects.Move;
		DragDrop.Backend = _backend;

		var result = DragDrop.DoDragDrop(new TestElement(), new DataObject(), DragDropEffects.Copy);

		Assert.Equal(DragDropEffects.None, result);
	}

	// ── AllowDrop DP ──────────────────────────────────────────────────────────

	[Fact]
	public void AllowDrop_default_is_false()
	{
		Assert.False(new TestElement().AllowDrop);
	}

	[Fact]
	public void AllowDrop_roundtrip()
	{
		var el = new TestElement();
		el.AllowDrop = true;
		Assert.True(el.AllowDrop);
		el.AllowDrop = false;
		Assert.False(el.AllowDrop);
	}

	[Fact]
	public void AllowDrop_set_via_SetValue_roundtrip()
	{
		var el = new TestElement();
		el.SetValue(UIElement.AllowDropProperty, true);
		Assert.True(el.AllowDrop);
	}

	// ── Spy ───────────────────────────────────────────────────────────────────

	sealed class SpyDragDropBackend : IDragDropBackend
	{
		internal IDataObject? LastData { get; private set; }

		public DragDropEffects DoDragDrop(
			DependencyObject source,
			IDataObject data,
			DragDropEffects allowedEffects)
		{
			LastData = data;
			return DragDropEffects.None;
		}
	}
}
