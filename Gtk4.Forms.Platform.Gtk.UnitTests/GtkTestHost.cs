using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Gtk4.Forms;
using Gtk4.Forms.Platform.GTK;
using Xunit;

/// <summary>
/// Assembly-wide GTK + Forms bootstrap and the layout pump. Deliberately in the global namespace
/// so every fixture can reach it without a using.
///
/// xUnit has no <c>[SetUpFixture]</c>/<c>[OneTimeSetUp]</c>, so the bootstrap is a latched static
/// initializer instead. <see cref="GtkTestBase"/> calls it from its constructor, which xUnit runs
/// before every test; the latch makes all but the first call free. That is on purpose rather than
/// an assembly fixture: it keeps the verdict on an unusable environment ("no DISPLAY", "no native
/// GTK") attached to each test rather than to fixture construction, where xUnit reports it once
/// and swallows the rest.
/// </summary>
public static class GtkTestHost
{
	/// <summary>
	/// The number of Pump rounds a layout assertion needs. Geometry is pushed to GTK from a
	/// GLib.Idle callback (plan M3 root cause 2 - "never mutate geometry inside a size-allocate"),
	/// so a single drain of the event queue is one round short of the answer.
	/// </summary>
	public const int DefaultPumpRounds = 6;

	/// <summary>
	/// Set this in an environment that is SUPPOSED to be able to run GTK - CI does, in
	/// linux-gtk.yml - and an unusable environment becomes a hard failure instead of a skip.
	/// Without it a broken xvfb, or a GTK runtime that silently stopped being installed, would
	/// turn the whole suite green-by-absence, which is exactly the outcome the comments in that
	/// workflow forbid.
	/// </summary>
	const string RequiredVariable = "XF_GTK_REQUIRE_NATIVE";

	/// <summary>
	/// Brings up the single GTK thread (via <see cref="Gtk4.Forms.Platform.GTK.UnitTests.GtkFixture"/>)
	/// and either returns - GTK is running on that thread - or skips, so an environment that cannot host
	/// GTK reports as such rather than as a wall of identical failures. Called from
	/// <see cref="GtkTestBase"/>'s constructor, which xUnit runs before every test.
	///
	/// <para>The actual <c>Gtk.Application.Init(); Forms.Init();</c> no longer runs here: under GTK4 it
	/// MUST run on a dedicated STA thread on Windows (OleInitialize), so the fixture owns it. This
	/// method only surfaces the fixture's verdict per-test.</para>
	/// </summary>
	public static void EnsureInitialized()
	{
		var reason = Gtk4.Forms.Platform.GTK.UnitTests.GtkFixture.Instance.UnavailableReason;

		if (reason != null)
			Unavailable(reason);
	}

	/// <summary>
	/// Why GTK cannot be brought up here, or null if it can.
	/// </summary>
	/// <remarks>
	/// DISPLAY is an X11 concept. GTK3 on Windows draws through the Win32 GDK backend and has no
	/// DISPLAY at all, so gating on it there rejected an environment that was never asked for one;
	/// the honest test on Windows is whether the native runtime loads, which is left to
	/// <see cref="Gtk.Application.Init"/> to answer.
	/// </remarks>
	internal static string DisplayUnavailableReason()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			return null;

		// Wayland sessions have no DISPLAY either unless Xwayland is up.
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) ||
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY")))
			return null;

		return "No DISPLAY. These are real GTK widget tests and need an X server: " +
			"run them as `xvfb-run -a dotnet test Gtk4.Forms.Platform.GTK.UnitTests`.";
	}

	internal static string NoNativeRuntime(string detail) =>
		"The native GTK 4 runtime is not available, so there are no widgets to test: " + detail +
		". On Linux install libgtk-4-1; on Windows these tests need a GTK 4 runtime on PATH " +
		"(the GtkSharp package ships managed bindings only).";

	/// <summary>
	/// Skips, so that an environment which cannot host GTK reports as such rather than as 169
	/// identical assertion failures - unless <see cref="RequiredVariable"/> says the environment
	/// was meant to be able to, in which case it fails.
	/// </summary>
	static void Unavailable(string reason)
	{
		Assert.False(
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(RequiredVariable)),
			$"{reason} ({RequiredVariable} is set, so this is a failure rather than a skip.)");

		Assert.Skip(reason);
	}

	/// <summary>
	/// Skips a test on the macOS GTK 4 Quartz backend, which crashes the process natively on
	/// GtkTextView operations (the multi-line widget behind the Editor renderer). The renderer is
	/// exercised on the Win32 and X11 backends, where these tests pass, so this is a Quartz-backend
	/// limitation of the headless CI host, not a product defect. Remove when GtkSharp4's Quartz
	/// GtkTextView support stabilises. Call before <c>Run(...)</c> so the skip lands on the test thread.
	/// </summary>
	internal static void SkipOnQuartz(string what)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			Assert.Skip($"{what} crashes the GTK 4 macOS Quartz backend natively; covered on Windows and Linux.");
	}

	/// <summary>
	/// Pumps GTK so that queued relayout actually happens.
	///
	/// MEASURED (plan V3c/§10.2), and the single most important detail in this harness: draining
	/// EventsPending() is NOT sufficient headlessly. GTK3 relayout is driven by the frame clock,
	/// which does not tick under Xvfb without a redraw cycle, so Fixed.Move() and
	/// SetSizeRequest() register on the widget (the Fixed child properties do update) while
	/// Widget.Allocation keeps returning the STALE rectangle. Forcing an explicit SizeAllocate on
	/// the toplevel is what commits it.
	///
	/// Without this, layout assertions read stale allocations and quietly compare stale to
	/// stale - passing when they should fail.
	/// </summary>
	public static void Pump(Gtk.Window toplevel = null, int rounds = DefaultPumpRounds)
	{
		for (int i = 0; i < rounds; i++)
		{
			Drain();

			if (toplevel != null)
			{
				// GTK4: AllocatedWidth/AllocatedHeight are obsolete; use Width/Height.
				ForceAllocate(toplevel, toplevel.Width, toplevel.Height);
			}

			while (GLib.MainContext.Iteration(false))
			{
			}
		}
	}

	/// <summary>
	/// Commits an allocation on <paramref name="toplevel"/> of at least the size the caller asked
	/// for, never smaller than GTK's measured minimum for the window.
	/// </summary>
	/// <remarks>
	/// MEASURED (GTK4): <c>gtk_widget_size_allocate</c> REFUSES an allocation smaller than the
	/// widget's measured minimum and leaves the subtree unallocated, logging
	/// <c>Gtk-CRITICAL: Allocation height too small. Tried to allocate 500x400, but ... needs at
	/// least 500x439</c>. GTK3 tolerated the under-allocation and clipped; GTK4 does not, so the
	/// forced SizeAllocate the headless harness depends on (the frame clock does not tick under a
	/// forced size, so an explicit allocation is the only thing that commits geometry) is dropped
	/// on the floor and every descendant renderer is never built - which is why GetRenderer then
	/// returns null and the control "resolves to nothing".
	///
	/// So clamp the requested size up to the measured minimum. This honours the test's requested
	/// geometry whenever the content fits and only grows the window when the content genuinely does
	/// not - it does not mask a renderer that reports a wrong minimum, it just stops GTK4 from
	/// discarding the whole layout pass when the window is a few pixels short of its chrome.
	/// </remarks>
	public static void ForceAllocate(Gtk.Window toplevel, int width, int height)
	{
		if (toplevel == null)
			return;

		toplevel.Measure(Gtk.Orientation.Horizontal, -1, out int minWidth, out _, out _, out _);
		toplevel.Measure(Gtk.Orientation.Vertical, -1, out int minHeight, out _, out _, out _);

		int w = Math.Max(width, minWidth);
		int h = Math.Max(height, minHeight);

		toplevel.SizeAllocate(new Gdk.Rectangle(0, 0, w, h));
	}

	static void Drain()
	{
		while (Gtk.Application.EventsPending())
			Gtk.Application.RunIteration();
	}

	/// <summary>
	/// Drains pending main-loop work without blocking, after each test body, so a failure in layout,
	/// a draw function or an idle callback a test queued is attributed to that test rather than to
	/// whichever runs next. Bounded, so a test that queues work forever fails by timing out instead of
	/// wedging the GTK thread. Called by <see cref="Gtk4.Forms.Platform.GTK.UnitTests.GtkFixture"/>.
	/// </summary>
	internal static void PumpPending()
	{
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration(false);
	}

	/// <summary>
	/// Pumps until <paramref name="condition"/> holds, or the deadline expires. Returns whether
	/// it held.
	///
	/// MEASURED, and the reason a fixed <see cref="Pump"/> round count is not always enough:
	/// anything that goes through the window manager - <c>Gtk.Window.Resize</c> above all - is a
	/// REQUEST, not a state change. Under Xvfb there is no real window manager and the request
	/// is serviced within a round or two, so a fixed count happened to be sufficient on Linux.
	/// The Win32 GDK backend round-trips through the actual Windows window manager, where the
	/// latency is neither zero nor bounded by a round count, and a fixed pump silently reads the
	/// pre-resize geometry.
	///
	/// Callers should still assert on the result: this waits for the condition, it does not
	/// excuse it never happening.
	/// </summary>
	public static bool PumpUntil(Func<bool> condition, Gtk.Window toplevel = null, int timeoutMs = 5000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

		while (true)
		{
			if (condition())
				return true;

			if (DateTime.UtcNow >= deadline)
				return false;

			Pump(toplevel, 1);
		}
	}

	/// <summary>
	/// Waits for a task by pumping GTK, instead of blocking the thread.
	///
	/// MEASURED: an <c>async</c> test method deadlocks this suite outright. <see cref="FormsWindow"/>'s
	/// constructor installs a <see cref="GtkSynchronizationContext"/> on whatever thread creates it
	/// (FormsWindow.cs:22), which is the test thread; every subsequent <c>await</c> continuation is
	/// then posted to the GTK main loop, and nothing runs that loop during a test. The run hangs
	/// with no failure and no output - the first version of this suite stopped dead after
	/// PlatformServiceTests.IdiomIsDesktop and had to be killed.
	///
	/// So: no <c>async</c> test methods in this assembly. Await through here.
	/// </summary>
	public static T Await<T>(Task<T> task, int timeoutMs = 10000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

		while (!task.IsCompleted && DateTime.UtcNow < deadline)
			Pump(null, 1);

		Assert.True(task.IsCompleted, $"task did not complete within {timeoutMs}ms");

		return task.GetAwaiter().GetResult();
	}

	public static void Await(Task task, int timeoutMs = 10000)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

		while (!task.IsCompleted && DateTime.UtcNow < deadline)
			Pump(null, 1);

		Assert.True(task.IsCompleted, $"task did not complete within {timeoutMs}ms");

		task.GetAwaiter().GetResult();
	}

	/// <summary>
	/// Retires a test window: hide it, pump so the hide is processed, and keep the managed
	/// wrapper alive for the rest of the run.
	///
	/// MEASURED, and the reason this is not simply <c>Window.Destroy()</c>: destroying widgets
	/// between tests aborts the whole test host part-way through the suite with
	///
	///   GLib-GObject-CRITICAL: g_object_remove_toggle_ref: assertion 'G_IS_OBJECT (object)' failed
	///   GLib-GObject:ERROR:gobject.c:3379:toggle_refs_notify: assertion failed: (tstack.n_toggle_refs == 1)
	///
	/// GtkSharp's wrappers drop their toggle reference from a <c>GLib.Object</c> finalizer, which
	/// the CLR runs on the finalizer thread - not the GTK main thread - and by then
	/// <c>gtk_widget_destroy</c> has already freed the GObject. That is plan risk R8
	/// ("GtkSharp Dispose interacting with Gtk.Widget.Destroy()") reproduced deterministically.
	/// Holding a reference keeps the finalizer from ever running, which is what makes a
	/// hundred-test run survive. The windows are small and the process is short-lived.
	/// </summary>
	static void Retire(Gtk.Window window)
	{
		if (window == null)
			return;

		// GTK4: Hide() is obsolete; use Visible = false.
		window.Visible = false;
		Pump(null, 2);

		s_retired.Add(window);
	}

	static readonly List<Gtk.Window> s_retired = new List<Gtk.Window>();

	/// <summary>
	/// Creates a renderer for a standalone view and parents it in a real toplevel, which is what
	/// makes the widget realizable and its allocation meaningful. Dispose the handle to retire
	/// the window.
	/// </summary>
	public static ViewHost<TView> HostView<TView>(TView view, int width = 400, int height = 300)
		where TView : View
	{
		return new ViewHost<TView>(view, width, height);
	}

	/// <summary>
	/// Loads a real <see cref="Page"/> through <see cref="FormsWindow"/>, i.e. the same path the
	/// gallery takes, so page renderers and the Forms layout pass are exercised rather than
	/// bypassed.
	/// </summary>
	public static PageHost HostPage(Page page, int width = 800, int height = 600)
	{
		return new PageHost(page, width, height);
	}

	/// <summary>Depth-first search of a native widget tree.</summary>
	/// <remarks>
	/// GTK4: walk the real widget tree with <c>FirstChild</c>/<c>NextSibling</c>, not the compat
	/// <c>Gtk.Container.Children</c> list. The compat layer only tracks children added through its
	/// own <c>Add()</c>/<c>Put()</c> in a managed <c>_placements</c> list, and only on the handful
	/// of types that subclass the compat <c>Gtk.Container</c>. Renderers that parent through native
	/// GTK4 APIs (a <c>ScrolledWindow</c> over a <c>Viewport</c> over a <c>Box</c>, as
	/// CollectionView/CarouselView do) put their subtree entirely outside that list, and a
	/// <c>ScrolledWindow</c> is not even a <c>Gtk.Container</c> in GTK4 - so the old walk returned
	/// nothing for them ("built no Gtk.ScrolledWindow", "0 of 6 items were realized"). The native
	/// child links are the single source of truth for every widget type.
	/// </remarks>
	public static List<T> Find<T>(Gtk.Widget root) where T : Gtk.Widget
	{
		var found = new List<T>();

		void Walk(Gtk.Widget w)
		{
			if (w is T t)
				found.Add(t);

			for (var child = w.FirstChild; child != null; child = child.NextSibling)
				Walk(child);
		}

		if (root != null)
			Walk(root);

		return found;
	}

	/// <summary>
	/// Delivers a synthetic button press to <paramref name="widget"/>.
	/// </summary>
	/// <remarks>
	/// GTK4 removed native event injection entirely: there is no gtk_widget_event, no GdkWindow
	/// per widget, and no public GdkEvent constructor. The GTK4 compat layer delivers
	/// button-press-event through a GtkGestureClick controller whose pressed signal populates a
	/// managed <c>ButtonPressEventArgs</c> and fires <c>Widget.ButtonPressEvent</c>. That managed
	/// event is what renderer code hooks, so raising it directly produces the same call path a real
	/// click would take through the compat shim — with the button number intact.
	///
	/// <para>The guard is now <c>IsRealized</c> rather than "has a GdkWindow": in GTK4 all widgets
	/// are windowless; realization is still the signal that the widget is ready to receive
	/// input.</para>
	///
	/// <para><c>Widget.ButtonPressEvent</c> uses a custom event accessor whose backing delegate is
	/// private. Reflection is the only way to invoke it from outside the class — the alternative
	/// of wiring up the native GtkGestureClick::pressed signal to a fake event sequence is
	/// disproportionately complex and equally internal. This is a test-only harness path, not
	/// production code.</para>
	/// </remarks>
	public static void PressButton(Gtk.Widget widget, uint button = 1)
	{
		if (widget == null || !widget.IsRealized)
			throw new InvalidOperationException(
				$"the widget is not realized, so it cannot be pressed: {Describe(widget)}");

		var args = new Gtk.ButtonPressEventArgs
		{
			Event = new Gdk.EventButton
			{
				Type = Gdk.EventType.ButtonPress,
				Button = button,
				// A real GtkGestureClick::pressed carries the click count (OnCompatPressed copies it
				// from PressedArgs.NPress); for a single click GTK reports 1. VisualElementTracker's
				// handler drops anything with NPress < 1, so a synthetic press that leaves it at the
				// default 0 is silently discarded and raises no tap. Match the single-click count.
				NPress = 1,
			}
		};

		// ButtonPressEvent has custom add/remove accessors; the backing delegate is private.
		// Reflection is a last resort (see CLAUDE.md) but is the least-invasive option here:
		// it invokes the same path a real GestureClick::pressed would take through the compat shim.
		var field = typeof(Gtk.Widget).GetField(
			"_buttonPressEvent",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

		var handler = field?.GetValue(widget) as Gtk.ButtonPressEventHandler;
		handler?.Invoke(widget, args);
	}

	/// <summary>
	/// True when GTK never gave the widget a real allocation. GTK3's sentinel for
	/// "not allocated yet" is (-1, -1, 1, 1), and every M3 layout bug in this backend showed up
	/// as exactly this while the widget still reported <c>Visible == true</c>.
	///
	/// GTK4: <c>Widget.Allocation</c> is obsolete. Use <c>Widget.Width</c>/<c>Widget.Height</c>
	/// instead; they report the last allocated size (0 before allocation, so the unallocated
	/// sentinel is a size of 0 or 1 in GTK4 rather than the GTK3 (-1,-1,1,1)).
	/// </summary>
	public static bool IsUnallocated(Gtk.Widget widget) =>
		widget == null || (widget.Width <= 1 && widget.Height <= 1);

	/// <summary>
	/// The top-left of <paramref name="widget"/> in <paramref name="window"/>'s coordinates, which is
	/// the space a full-window pixel capture is in.
	/// </summary>
	/// <remarks>
	/// GTK4: <c>Widget.Allocation.X/Y</c> is the position inside the widget's PARENT, not inside the
	/// toplevel. When the toplevel centres or offsets its child - a single child in a larger window
	/// gets centred - the two differ, and probing at the parent-relative point lands in empty space
	/// (a red BoxView at window-y 39 read as black at y 10). <c>gtk_widget_compute_bounds</c> (exposed
	/// as <see cref="Gtk.Widget.ComputeBounds"/>) translates the widget's bounds into any ancestor's
	/// coordinate space; against the toplevel that is exactly where the capture painted it.
	/// </remarks>
	public static (int X, int Y) WindowRelativeTopLeft(Gtk.Widget widget, Gtk.Window window)
	{
		if (widget.ComputeBounds(window, out var bounds))
			return ((int)bounds.X, (int)bounds.Y);

		// ComputeBounds fails only when the widgets share no ancestor; fall back to the
		// parent-relative allocation so the caller still gets a usable point.
		return (widget.Allocation.X, widget.Allocation.Y);
	}

	public static string Describe(Gtk.Widget widget) =>
		widget == null
			? "<null>"
			: $"{widget.GetType().Name}[{widget.Width}x{widget.Height} visible={widget.Visible}]";

	public sealed class ViewHost<TView> : IDisposable where TView : View
	{
		public ViewHost(TView view, int width, int height)
		{
			View = view;
			// GTK4: WindowType enum is gone; Window() creates a toplevel by default.
			Window = new Gtk.Window();
			Window.SetDefaultSize(width, height);

			Renderer = Platform.CreateRenderer(view);
			Container = new Gtk.Fixed();
			Container.Put((Gtk.Widget)Renderer, 0, 0);
			Window.Add(Container);
			Window.ShowAll();

			// A standalone view is not inside a Forms layout pass, so nothing else will ever
			// give it bounds; do it explicitly, as CarouselViewRenderer does for its items.
			view.Layout(new Rectangle(0, 0, width, height));
			Renderer.SetElementSize(new Size(width, height));

			GtkTestHost.Pump(Window);
		}

		public TView View { get; }
		public Gtk.Window Window { get; }
		public Gtk.Fixed Container { get; }
		public IVisualElementRenderer Renderer { get; }

		/// <summary>The native control, for renderers that expose one.</summary>
		public TNative Control<TNative>() where TNative : Gtk.Widget
		{
			var native = Renderer as IVisualNativeElementRenderer;

			Assert.True(native != null,
				$"{Renderer.GetType().Name} does not expose a native control");

			return Assert.IsAssignableFrom<TNative>(native.Control);
		}

		public void Pump(int rounds = DefaultPumpRounds) => GtkTestHost.Pump(Window, rounds);

		public void Dispose() => Retire(Window);
	}

	public sealed class PageHost : IDisposable
	{
		readonly TestApplication _app;

		public PageHost(Page page, int width, int height)
		{
			Window = new FormsWindow();
			Window.SetDefaultSize(width, height);

			_app = new TestApplication { MainPage = page };
			Window.LoadApplication(_app);
			Window.ShowAll();

			// GTK4: Window.Resize() is gone (it was a WM request anyway, and unreliable under
			// Xvfb). Force the allocation directly instead, which is what the harness comment
			// always recommended for headless runs. SetDefaultSize sets the preferred size so the
			// window manager (if any) knows what we want; ForceAllocate commits it now, clamped up
			// to GTK4's measured minimum so the allocation is not refused (see ForceAllocate).
			Window.SetDefaultSize(width, height);
			GtkTestHost.ForceAllocate(Window, width, height);
			Platform.GetRenderer(page)?.SetElementSize(new Size(width, height));

			GtkTestHost.Pump(Window);
		}

		public FormsWindow Window { get; }
		public Page Page => _app.MainPage;

		public void Pump(int rounds = DefaultPumpRounds) => GtkTestHost.Pump(Window, rounds);

		public void Dispose() => Retire(Window);

		sealed class TestApplication : Application
		{
		}
	}
}

/// <summary>
/// Base class for every fixture here. Its constructor brings up the single GTK thread (skipping the
/// test if the environment cannot host GTK), and <see cref="Run(System.Action)"/> marshals a test body
/// onto that thread.
/// </summary>
/// <remarks>
/// GTK4 forces this: <c>gtk_init</c> must run on a dedicated STA thread on Windows (OleInitialize) and
/// every subsequent GTK call must stay on that same thread. So a test body may no longer run inline on
/// the xUnit runner thread - it is wrapped in <c>Run(() =&gt; { ... })</c>, which hands it to the GTK
/// thread and rethrows whatever it throws (asserts included) on the runner thread.
///
/// <para>Pattern for every test in this assembly:</para>
/// <code>
/// [Fact]
/// public void Foo() => Run(() =&gt; { /* body, Asserts, GtkTestHost.* calls */ });
///
/// [Theory]
/// [InlineData(1)]
/// public void Bar(int x) => Run(() =&gt; { /* body using x */ });
///
/// // when the body produces a value the assertion needs:
/// var widget = Run(() =&gt; (Gtk.Widget)host.Renderer);
/// </code>
/// </remarks>
public abstract class GtkTestBase
{
	protected GtkTestBase() => GtkTestHost.EnsureInitialized();

	/// <summary>Runs a test body on the GTK thread, rethrowing anything it throws.</summary>
	protected void Run(System.Action body) =>
		Gtk4.Forms.Platform.GTK.UnitTests.GtkFixture.Instance.Invoke(body);

	/// <summary>Runs a test body on the GTK thread and returns its value.</summary>
	protected T Run<T>(System.Func<T> body) =>
		Gtk4.Forms.Platform.GTK.UnitTests.GtkFixture.Instance.Invoke(body);
}
