using System;
using System.Text;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

public class ZzDiagTests : WpfGtkTestBase
{
	static string Report(string name, Gtk.Window w)
	{
		w.Present();
		GtkPump.For(400);
		var s = w.Surface;
		var tb = w.Titlebar;
		return $"{name}: widget={w.Width}x{w.Height} surface={s?.Width}x{s?.Height} scale={s?.ScaleFactor} " +
			$"titlebar={(tb == null ? "none" : tb.Width + "x" + tb.Height)} " +
			$"css=[{string.Join(",", w.CssClasses ?? Array.Empty<string>())}] decorated={w.Decorated}\n";
	}

	[Fact]
	public void Diag() => Run(() =>
	{
		var sb = new StringBuilder();
		var a = new Gtk.Window();
		a.SetDefaultSize(420, 310);
		sb.Append(Report("defaultOnly", a)); a.Close();
		var b = new Gtk.Window();
		b.SetDefaultSize(420, 310); b.SetSizeRequest(420, 310);
		sb.Append(Report("default+request", b)); b.Close();
		var c = new Gtk.Window { Titlebar = new Gtk.HeaderBar() };
		c.SetDefaultSize(420, 310); c.SetSizeRequest(420, 310);
		sb.Append(Report("headerbar+both", c)); c.Close();
		var d = new Gtk.Window { Decorated = false };
		d.SetDefaultSize(420, 310); d.SetSizeRequest(420, 310);
		sb.Append(Report("undecorated+both", d)); d.Close();
		var e = new Gtk.Window();
		e.SetDefaultSize(420, 310); e.SetSizeRequest(420, 310);
		sb.Append(Report("plain+both", e)); e.Close();
		throw new Exception("DIAG\n" + sb);
	});
}
