using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="DataObject"/>, <see cref="IDataObject"/>, and <see cref="DataFormats"/>.
/// No display or GTK required.
/// </summary>
public sealed class DataObjectTests
{
	// ── DataFormats constants ─────────────────────────────────────────────────

	[Fact]
	public void DataFormats_Text_is_non_empty() =>
		Assert.False(string.IsNullOrEmpty(DataFormats.Text));

	[Fact]
	public void DataFormats_UnicodeText_is_non_empty() =>
		Assert.False(string.IsNullOrEmpty(DataFormats.UnicodeText));

	// ── Round-trip text ───────────────────────────────────────────────────────

	[Fact]
	public void Roundtrips_UnicodeText()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "hello");
		Assert.Equal("hello", obj.GetData(DataFormats.UnicodeText));
	}

	[Fact]
	public void Roundtrips_text_via_type()
	{
		var obj = new DataObject();
		obj.SetData(typeof(string), "world");
		Assert.Equal("world", obj.GetData(typeof(string)));
	}

	[Fact]
	public void Roundtrips_custom_format()
	{
		var obj = new DataObject();
		var value = new object();
		obj.SetData("MyApp.CustomFormat", value);
		Assert.Same(value, obj.GetData("MyApp.CustomFormat"));
	}

	[Fact]
	public void Absent_format_GetData_returns_null()
	{
		var obj = new DataObject();
		Assert.Null(obj.GetData("not/there"));
	}

	// ── Auto-conversion Text ↔ UnicodeText ────────────────────────────────────

	[Fact]
	public void Text_auto_converts_to_UnicodeText()
	{
		var obj = new DataObject(DataFormats.Text, "hi");
		Assert.Equal("hi", obj.GetData(DataFormats.UnicodeText));
	}

	[Fact]
	public void UnicodeText_auto_converts_to_Text()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "hi");
		Assert.Equal("hi", obj.GetData(DataFormats.Text));
	}

	[Fact]
	public void No_auto_convert_when_disabled()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "hi");
		Assert.Null(obj.GetData(DataFormats.Text, autoConvert: false));
	}

	// ── GetDataPresent ────────────────────────────────────────────────────────

	[Fact]
	public void GetDataPresent_true_for_stored_format()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "x");
		Assert.True(obj.GetDataPresent(DataFormats.UnicodeText));
	}

	[Fact]
	public void GetDataPresent_false_for_absent_format()
	{
		var obj = new DataObject();
		Assert.False(obj.GetDataPresent("not/there"));
	}

	[Fact]
	public void GetDataPresent_true_for_auto_convert_alternate()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "x");
		Assert.True(obj.GetDataPresent(DataFormats.Text));
	}

	[Fact]
	public void GetDataPresent_false_without_auto_convert_for_alternate()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "x");
		Assert.False(obj.GetDataPresent(DataFormats.Text, autoConvert: false));
	}

	[Fact]
	public void GetDataPresent_by_type_true_when_stored()
	{
		var obj = new DataObject();
		obj.SetData(typeof(int), 42);
		Assert.True(obj.GetDataPresent(typeof(int)));
		Assert.False(obj.GetDataPresent(typeof(string)));
	}

	// ── GetFormats ────────────────────────────────────────────────────────────

	[Fact]
	public void GetFormats_returns_stored_formats()
	{
		var obj = new DataObject();
		obj.SetData("fmt1", "a");
		obj.SetData("fmt2", "b");
		var formats = obj.GetFormats(autoConvert: false);
		Assert.Contains("fmt1", formats);
		Assert.Contains("fmt2", formats);
	}

	[Fact]
	public void GetFormats_with_autoConvert_exposes_text_alternate()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "hi");
		var formats = obj.GetFormats(autoConvert: true);
		Assert.Contains(DataFormats.Text, formats);
		Assert.Contains(DataFormats.UnicodeText, formats);
	}

	[Fact]
	public void GetFormats_without_autoConvert_does_not_expose_alternate()
	{
		var obj = new DataObject(DataFormats.UnicodeText, "hi");
		var formats = obj.GetFormats(autoConvert: false);
		Assert.Contains(DataFormats.UnicodeText, formats);
		Assert.DoesNotContain(DataFormats.Text, formats);
	}

	// ── SetData(object) uses type as format ───────────────────────────────────

	[Fact]
	public void SetData_object_uses_type_as_format()
	{
		var obj = new DataObject();
		obj.SetData("payload");
		Assert.True(obj.GetDataPresent(typeof(string)));
	}
}
