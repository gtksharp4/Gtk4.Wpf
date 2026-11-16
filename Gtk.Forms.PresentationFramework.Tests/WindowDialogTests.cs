using System;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk.Forms.PresentationFramework.Tests
{
	/// <summary>
	/// Tests for <see cref="WpfWindow.ShowDialog"/> and <see cref="WpfWindow.Close(bool?)"/>.
	/// </summary>
	/// <remarks>
	/// ShowDialog pumps the GTK main loop on the GTK thread until Close(...) flags it closed.
	/// GTK is single-threaded, so Close must run on the GTK thread: these tests schedule it via
	/// a same-thread <c>GLib.Timeout</c>, which dispatches inside ShowDialog's pump. (Calling
	/// Close from a foreign threadpool thread would crash the native GTK layer.)
	/// </remarks>
	public class WindowDialogTests : WpfGtkTestBase
	{
		/// <summary>Schedules <paramref name="close"/> on the GTK thread after a short delay.</summary>
		static void ScheduleClose(Action close) =>
			GLib.Timeout.Add(10, () => { close(); return false; });

		// ── Pure: DialogResult starts null ────────────────────────────────────────

		[Fact]
		public void DialogResult_is_null_before_ShowDialog() => Run(() =>
		{
			var win = new WpfWindow();
			Assert.Null(win.DialogResult);
		});

		// ── GTK: ShowDialog + Close(true) returns true ────────────────────────────

		[Fact]
		public void ShowDialog_returns_true_when_Close_true() => Run(() =>
		{
			var win = new WpfWindow { Title = "ModalTest" };

			ScheduleClose(() => win.Close(true));

			var result = win.ShowDialog();
			Assert.True(result);
		});

		[Fact]
		public void ShowDialog_returns_false_when_Close_false() => Run(() =>
		{
			var win = new WpfWindow { Title = "ModalFalse" };

			ScheduleClose(() => win.Close(false));

			var result = win.ShowDialog();
			Assert.False(result);
		});

		[Fact]
		public void ShowDialog_returns_null_when_Close_null() => Run(() =>
		{
			var win = new WpfWindow { Title = "ModalNull" };

			ScheduleClose(() => win.Close(null));

			var result = win.ShowDialog();
			Assert.Null(result);
		});

		// ── Pure: Owner property ──────────────────────────────────────────────────

		[Fact]
		public void Owner_can_be_set() => Run(() =>
		{
			var owner = new WpfWindow { Title = "Owner" };
			var dialog = new WpfWindow { Title = "Dialog", Owner = owner };
			Assert.Same(owner, dialog.Owner);
		});

		// ── Pure: DialogResult reflects last Close call ───────────────────────────

		[Fact]
		public void DialogResult_set_by_Close_bool() => Run(() =>
		{
			var win = new WpfWindow();

			ScheduleClose(() => win.Close(false));

			win.ShowDialog();
			Assert.False(win.DialogResult);
		});
	}
}
