using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;
using WpfButton  = System.Windows.Controls.Button;

namespace Gtk.Forms.PresentationFramework.Tests
{
	/// <summary>
	/// GTK smoke tests for the input system.
	/// Each test body runs on the GTK thread via <c>Run(...)</c>.
	/// </summary>
	public class InputGtkSmokeTests : WpfGtkTestBase
	{
		// ── Realised widget receives GTK key event → WPF KeyDown fires ────────────

		[Fact]
		public void KeyDown_fires_on_realised_element() => Run(() =>
		{
			var tb = new WpfTextBox();
			_ = tb.Widget; // realise widget

			Key received = Key.None;
			tb.KeyDown += (s, e) => received = e.Key;

			// Use the test seam — no real GTK event needed
			tb.RaiseKeyDownForTest(Key.A);

			Assert.Equal(Key.A, received);
		});

		// ── Focus() grabs GTK focus ───────────────────────────────────────────────

		[Fact]
		public void Focus_sets_IsFocused_on_realised_widget() => Run(() =>
		{
			var tb = new WpfTextBox();
			_ = tb.Widget; // realise widget
			tb.Focus();
			// IsFocused is set synchronously by the managed Focus() call
			Assert.True(tb.IsFocused);
		});

		// ── TextBox single-line backing widget is Gtk.Entry ──────────────────────

		[Fact]
		public void TextBox_singleline_widget_is_GtkEntry() => Run(() =>
		{
			var tb = new WpfTextBox { AcceptsReturn = false };
			Assert.IsType<Gtk.Entry>(tb.Widget);
		});

		// ── TextBox multiline backing widget is Gtk.ScrolledWindow ───────────────

		[Fact]
		public void TextBox_multiline_widget_is_GtkScrolledWindow() => Run(() =>
		{
			var tb = new WpfTextBox { AcceptsReturn = true };
			Assert.IsType<Gtk.ScrolledWindow>(tb.Widget);
		});

		// ── TextBox multiline child is Gtk.TextView ───────────────────────────────

		[Fact]
		public void TextBox_multiline_child_is_GtkTextView() => Run(() =>
		{
			var tb = new WpfTextBox { AcceptsReturn = true };
			var sw = (Gtk.ScrolledWindow)tb.Widget;
			// GtkSharp4 ScrolledWindow.Child wraps in a Viewport automatically
			var viewport = sw.Child as Gtk.Viewport;
			var inner    = viewport?.Child ?? sw.Child;
			Assert.IsType<Gtk.TextView>(inner);
		});
	}
}
