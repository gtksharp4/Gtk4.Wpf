using System;
using System.Windows.Threading;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests: <see cref="DispatcherTimer"/> fires on the GTK thread,
/// and a <see cref="WpfTextBlock"/> with inlines uses Pango markup.
/// </summary>
public class DispatcherTimerGtkSmokeTests : WpfGtkTestBase
{
	[Fact]
	public void DispatcherTimer_fires_on_gtk_thread_via_test_seam() => Run(() =>
	{
		int count = 0;
		Action? fire = null;
		var saved = DispatcherTimer.FireActionOverride;
		try
		{
			DispatcherTimer.FireActionOverride = cb => { fire = cb; };
			var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
			timer.Tick += (s, e) => count++;
			timer.Start();

			Assert.NotNull(fire);
			fire!();
			Assert.Equal(1, count);
		}
		finally
		{
			DispatcherTimer.FireActionOverride = saved;
		}
	});

	[Fact]
	public void Dispatcher_CheckAccess_true_on_gtk_thread() => Run(() =>
	{
		// On the GTK thread the default IsOnDispatcherThread returns true.
		Assert.True(System.Windows.Threading.Dispatcher.CurrentDispatcher.CheckAccess());
	});

	[Fact]
	public void TextBlock_with_inlines_uses_markup_on_label() => Run(() =>
	{
		var tb = new WpfTextBlock();
		var bold = new System.Windows.Documents.Bold();
		bold.Inlines.Add(new System.Windows.Documents.Run { Text = "hello" });
		tb.Inlines.Add(bold);

		var label = Assert.IsType<Gtk.Label>(tb.Widget);
		Assert.True(label.UseMarkup);
	});

	[Fact]
	public void TextBlock_plain_text_does_not_use_markup() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "plain" };
		var label = Assert.IsType<Gtk.Label>(tb.Widget);
		// Plain text: UseMarkup is false, Text matches.
		Assert.False(label.UseMarkup);
		Assert.Equal("plain", label.Text);
	});
}