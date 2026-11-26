using System;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for <see cref="PasswordBox"/>.
/// Each body runs on the GTK thread via <c>Run(…)</c>; skipped when GTK is unavailable.
/// </summary>
/// <remarks>
/// GTK 4 limitation: <see cref="Gtk.PasswordEntry"/> has no native MaxLength API;
/// the limit is enforced in managed code. The GTK entry may temporarily show characters
/// that exceed the limit before the managed clamp sync runs.
/// </remarks>
public class PasswordBoxGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void Widget_is_GtkPasswordEntry() => Run(() =>
	{
		var pb = new PasswordBox();
		Assert.IsType<Gtk.PasswordEntry>(pb.Widget);
	});

	[Fact]
	public void Password_set_before_realize_syncs_to_entry() => Run(() =>
	{
		var pb = new PasswordBox { Password = "hello" };
		var entry = (Gtk.PasswordEntry)pb.Widget;
		Assert.Equal("hello", entry.Text);
	});

	[Fact]
	public void Password_set_after_realize_syncs_to_entry() => Run(() =>
	{
		var pb = new PasswordBox();
		_ = pb.Widget; // realize
		pb.Password = "world";
		var entry = (Gtk.PasswordEntry)pb.Widget;
		Assert.Equal("world", entry.Text);
	});

	[Fact]
	public void GTK_entry_text_change_updates_Password_and_raises_PasswordChanged() => Run(() =>
	{
		var pb = new PasswordBox();
		var entry = (Gtk.PasswordEntry)pb.Widget; // realize
		bool fired = false;
		pb.PasswordChanged += (s, e) => fired = true;

		entry.Text = "typed";

		Assert.Equal("typed", pb.Password);
		Assert.True(fired);
	});

	[Fact]
	public void GTK_entry_text_change_raises_PasswordChanged_once() => Run(() =>
	{
		var pb = new PasswordBox();
		var entry = (Gtk.PasswordEntry)pb.Widget;
		int count = 0;
		pb.PasswordChanged += (s, e) => count++;

		entry.Text = "abc";

		Assert.Equal(1, count);
	});

	[Fact]
	public void MaxLength_clamps_GTK_entry_input() => Run(() =>
	{
		var pb = new PasswordBox { MaxLength = 4 };
		var entry = (Gtk.PasswordEntry)pb.Widget;

		// Simulate typing more than MaxLength characters.
		entry.Text = "abcdefgh";

		Assert.Equal("abcd", pb.Password);
	});

	[Fact]
	public void MaxLength_set_before_realize_clamps_on_GTK_input() => Run(() =>
	{
		var pb = new PasswordBox { MaxLength = 3 };
		var entry = (Gtk.PasswordEntry)pb.Widget;

		entry.Text = "abcdef";

		Assert.Equal("abc", pb.Password);
	});

	[Fact]
	public void Clear_on_realized_passwordbox_clears_entry() => Run(() =>
	{
		var pb = new PasswordBox { Password = "secret" };
		_ = pb.Widget;
		pb.Clear();
		var entry = (Gtk.PasswordEntry)pb.Widget;
		Assert.Equal(string.Empty, entry.Text);
	});
}
