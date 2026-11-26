using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class PrintDialog
{
	const double PointsPerDip = 72.0 / 96.0;

	/// <summary>What the GTK dialog returned.</summary>
	internal readonly struct Setup
	{
		internal Setup(Gtk.PageSetup pageSetup, Gtk.PrintSettings settings)
		{
			PageSetup = pageSetup;
			Settings = settings;
		}

		internal Gtk.PageSetup PageSetup { get; }

		internal Gtk.PrintSettings Settings { get; }
	}

	/// <summary>Shows the setup dialog; replaced in tests, which cannot open one.</summary>
	internal static Func<Gtk.Window?, Setup?> Presenter { get; set; } = ShowGtkSetup;

	static Gtk.Window? OwnerWindow() => Application.Current?.MainWindow?.Widget as Gtk.Window;

	static Setup? ShowGtkSetup(Gtk.Window? parent)
	{
		var dialog = new Gtk.PrintDialog { Modal = true };
		Gtk.PrintSetup? result = null;
		var loop = new GLib.MainLoop();
		dialog.Setup(parent, null!, (source, async, data) =>
		{
			try { result = dialog.SetupFinish(async); }
			catch (GLib.GException) { result = null; } // dismissed by the user
			finally { loop.Quit(); }
		});
		loop.Run();
		return result == null ? (Setup?)null : new Setup(result.PageSetup, result.PrintSettings);
	}

	void ReadPrintableArea(Gtk.PageSetup page)
	{
		const double InchDips = 96.0;
		_areaWidth = page.GetPageWidth(Gtk.Unit.Inch) * InchDips;
		_areaHeight = page.GetPageHeight(Gtk.Unit.Inch) * InchDips;
	}

	void Print(DocumentPaginator paginator, string description)
	{
		var pages = SelectPages(paginator.PageCount);
		if (pages.Length == 0) return;
		var op = new Gtk.PrintOperation
		{
			JobName = description ?? string.Empty,
			NPages = pages.Length,
			Unit = Gtk.Unit.Points,
			UseFullPage = true
		};
		if (_settings != null) op.PrintSettings = _settings;
		if (_pageSetup != null) op.DefaultPageSetup = _pageSetup;
		if (ExportFilename != null) op.ExportFilename = ExportFilename;
		Exception? failure = null;
		op.DrawPage += (o, args) =>
		{
			try { DrawPage(paginator, pages[args.PageNr], args.Context); }
			catch (Exception ex) { failure = ex; }
		};
		var action = ExportFilename != null ? Gtk.PrintOperationAction.Export : Gtk.PrintOperationAction.Print;
		var result = op.Run(action, null!);
		if (failure != null) throw failure;
		if (result == Gtk.PrintOperationResult.Error) throw new InvalidOperationException("The print job failed.");
	}

	static void DrawPage(DocumentPaginator paginator, int pageNumber, Gtk.PrintContext context)
	{
		var page = paginator.GetPage(pageNumber - 1);
		try
		{
			var cr = context.CairoContext;
			cr.Save();
			cr.Scale(PointsPerDip, PointsPerDip);
			DocumentPageRenderer.Render(page, cr);
			cr.Restore();
		}
		finally
		{
			page.Dispose();
		}
	}

	sealed class SinglePagePaginator : DocumentPaginator, IDocumentPaginatorSource
	{
		readonly DocumentPage _page;

		internal SinglePagePaginator(DocumentPage page) => _page = page;

		public override bool IsPageCountValid => true;

		public override int PageCount => 1;

		public override Size PageSize { get; set; }

		public override IDocumentPaginatorSource Source => this;

		DocumentPaginator IDocumentPaginatorSource.DocumentPaginator => this;

		// The page is handed out once: PrintDialog disposes what it gets, and a visual printed twice is redrawn.
		public override DocumentPage GetPage(int pageNumber) =>
			pageNumber == 0 ? new DocumentPage(_page.Visual!, _page.Size, _page.BleedBox, _page.ContentBox)
				: DocumentPage.Missing;
	}
}
