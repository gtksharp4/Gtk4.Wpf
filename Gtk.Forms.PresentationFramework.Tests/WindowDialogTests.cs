using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk.Forms.PresentationFramework.Tests
{
	/// <summary>
	/// Tests for <see cref="WpfWindow.ShowDialog"/> and <see cref="WpfWindow.Close(bool?)"/>.
	/// </summary>
	/// <remarks>
	/// ShowDialog runs a nested GLib.MainLoop. In a GTK test environment we can call
	/// <see cref="WpfWindow.Close(bool?)"/> from a background thread to unblock it.
	/// Tests that do NOT need a real GLib loop are marked as pure and don't use GTK.
	/// GTK-loop tests run via Run(…) on the GTK thread and schedule Close on a threadpool thread.
	/// </remarks>
	public class WindowDialogTests : WpfGtkTestBase
	{
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
			bool? result = null;

			// Schedule Close from a background thread after a short delay.
			Task.Run(() =>
			{
				Thread.Sleep(50);
				win.Close(true);
			});

			result = win.ShowDialog();
			Assert.True(result);
		});

		[Fact]
		public void ShowDialog_returns_false_when_Close_false() => Run(() =>
		{
			var win = new WpfWindow { Title = "ModalFalse" };

			Task.Run(() =>
			{
				Thread.Sleep(50);
				win.Close(false);
			});

			var result = win.ShowDialog();
			Assert.False(result);
		});

		[Fact]
		public void ShowDialog_returns_null_when_Close_null() => Run(() =>
		{
			var win = new WpfWindow { Title = "ModalNull" };

			Task.Run(() =>
			{
				Thread.Sleep(50);
				win.Close(null);
			});

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

			Task.Run(() =>
			{
				Thread.Sleep(50);
				win.Close(false);
			});

			win.ShowDialog();
			Assert.False(win.DialogResult);
		});
	}
}
