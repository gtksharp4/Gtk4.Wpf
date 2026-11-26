using System.Collections.Specialized;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The native clipboard advertises media formats it was given.</summary>
public class ClipboardMediaGtkTests : WpfGtkTestBase
{
	[Fact]
	public void Native_formats_map_to_the_wpf_media_formats() => Run(() =>
	{
		var formats = new Gdk.ContentFormats(new[] { "image/png", "text/uri-list", "audio/wav" });
		Assert.True(GtkClipboardBackend.FormatsContain(formats, DataFormats.Bitmap));
		Assert.True(GtkClipboardBackend.FormatsContain(formats, DataFormats.FileDrop));
		Assert.True(GtkClipboardBackend.FormatsContain(formats, DataFormats.WaveAudio));
		Assert.False(GtkClipboardBackend.FormatsContain(formats, DataFormats.Html));
		var textOnly = new Gdk.ContentFormats(new[] { "text/plain" });
		Assert.False(GtkClipboardBackend.FormatsContain(textOnly, DataFormats.Bitmap));
		Assert.False(GtkClipboardBackend.FormatsContain(textOnly, DataFormats.WaveAudio));
		Assert.False(GtkClipboardBackend.FormatsContain(textOnly, DataFormats.FileDrop));
	});

	[Fact]
	public void SetFileDropList_on_the_gtk_backend_round_trips() => Run(() =>
	{
		var saved = Clipboard.Backend;
		Clipboard.Backend = new GtkClipboardBackend();
		try
		{
			Clipboard.SetFileDropList(new StringCollection { "/tmp/one.txt" });
			Assert.True(Clipboard.ContainsFileDropList());
			Assert.Equal("/tmp/one.txt", Clipboard.GetFileDropList()[0]);
			Clipboard.Clear();
			Assert.False(Clipboard.ContainsFileDropList());
		}
		finally { Clipboard.Backend = saved; }
	});

	[Fact]
	public void Uri_lists_parse_to_local_paths_and_skip_comments() => Run(() =>
	{
		var paths = GtkClipboardBackend.ParseUriList("# c\r\nfile:///tmp/a%20b.txt\r\nhttp://x/y\r\n\r\n");
		Assert.Single(paths);
		Assert.EndsWith("a b.txt", paths[0]);
	});
}
