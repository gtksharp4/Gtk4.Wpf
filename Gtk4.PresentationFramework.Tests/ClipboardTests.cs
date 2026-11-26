using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="Clipboard"/> and the <see cref="IClipboardBackend"/> seam.
/// All tests inject an <see cref="InMemoryClipboardBackend"/> — no display or GTK required.
/// </summary>
public sealed class ClipboardTests : IDisposable
{
	readonly IClipboardBackend _original;

	public ClipboardTests()
	{
		_original = Clipboard.Backend;
		Clipboard.Backend = new InMemoryClipboardBackend();
	}

	public void Dispose() => Clipboard.Backend = _original;

	// ── SetText / GetText / ContainsText ──────────────────────────────────────

	[Fact]
	public void SetText_GetText_roundtrip()
	{
		Clipboard.SetText("hello clipboard");
		Assert.Equal("hello clipboard", Clipboard.GetText());
	}

	[Fact]
	public void ContainsText_true_after_SetText()
	{
		Clipboard.SetText("some text");
		Assert.True(Clipboard.ContainsText());
	}

	[Fact]
	public void ContainsText_false_when_empty() =>
		Assert.False(Clipboard.ContainsText());

	[Fact]
	public void GetText_returns_empty_when_no_text() =>
		Assert.Equal(string.Empty, Clipboard.GetText());

	// ── SetDataObject / GetDataObject ─────────────────────────────────────────

	[Fact]
	public void SetDataObject_GetDataObject_roundtrip()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "my text");
		Clipboard.SetDataObject(obj);
		var retrieved = Clipboard.GetDataObject();
		Assert.NotNull(retrieved);
		Assert.Equal("my text", retrieved.GetData(DataFormats.UnicodeText));
	}

	[Fact]
	public void SetDataObject_copy_flag_does_not_throw()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "text");
		var ex = Record.Exception(() => Clipboard.SetDataObject(obj, copy: true));
		Assert.Null(ex);
	}

	[Fact]
	public void GetDataObject_returns_null_when_empty() =>
		Assert.Null(Clipboard.GetDataObject());

	// ── Clear ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Clear_removes_text()
	{
		Clipboard.SetText("to be cleared");
		Clipboard.Clear();
		Assert.False(Clipboard.ContainsText());
		Assert.Equal(string.Empty, Clipboard.GetText());
	}

	[Fact]
	public void Clear_removes_data_object()
	{
		Clipboard.SetDataObject(new DataObject(DataFormats.UnicodeText, "x"));
		Clipboard.Clear();
		Assert.Null(Clipboard.GetDataObject());
	}

	// ── ContainsData ─────────────────────────────────────────────────────────

	[Fact]
	public void ContainsData_true_for_stored_format()
	{
		var obj = new DataObject();
		obj.SetData("app/json", "{}");
		Clipboard.SetDataObject(obj);
		Assert.True(Clipboard.ContainsData("app/json"));
	}

	[Fact]
	public void ContainsData_false_for_absent_format()
	{
		Clipboard.SetText("hello");
		Assert.False(Clipboard.ContainsData("app/json"));
	}

	// ── Degrade: InMemoryClipboardBackend (simulates headless / no display) ───

	[Fact]
	public void InMemory_backend_roundtrips_without_display()
	{
		var backend = new InMemoryClipboardBackend();
		backend.SetText("headless");
		Assert.True(backend.ContainsText());
		Assert.Equal("headless", backend.GetText());
		backend.Clear();
		Assert.False(backend.ContainsText());
		Assert.Null(backend.GetText());
	}

	[Fact]
	public void InMemory_backend_GetDataObject_null_when_empty()
	{
		var backend = new InMemoryClipboardBackend();
		Assert.Null(backend.GetDataObject());
	}

	// ── Degrade: real GtkClipboardBackend headless (no display) ───────────────
	// Exercises the production default backend: its native push is best-effort and
	// must not throw when Gdk.Display.Default is null; the in-memory mirror serves
	// synchronous reads, so a round-trip still works headless.

	[Fact]
	public void GtkBackend_text_roundtrips_without_display()
	{
		var backend = new GtkClipboardBackend();
		var ex = Record.Exception(() =>
		{
			backend.SetText("gtk headless");
			Assert.True(backend.ContainsText());
			Assert.Equal("gtk headless", backend.GetText());
			backend.Clear();
			Assert.False(backend.ContainsText());
		});
		Assert.Null(ex);
	}

	[Fact]
	public void GtkBackend_data_object_roundtrips_without_display()
	{
		var backend = new GtkClipboardBackend();
		var ex = Record.Exception(() =>
		{
			backend.SetDataObject(new DataObject(DataFormats.UnicodeText, "payload"));
			var got = backend.GetDataObject();
			Assert.NotNull(got);
			Assert.Equal("payload", got!.GetData(DataFormats.UnicodeText));
		});
		Assert.Null(ex);
	}
}
