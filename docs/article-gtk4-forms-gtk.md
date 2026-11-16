# Keeping Xamarin.Forms Alive on Linux: A GTK-Only Maintenance Fork on .NET 10

## Introduction

Xamarin.Forms reached end of support on **May 1, 2024**. Microsoft's guidance is unambiguous:
move to .NET MAUI. For iOS, Android, Windows and macOS that is the right advice and there is no
argument to be had.

There is, however, one gap in that story. **.NET MAUI has no Gtk backend.** Gtk4.Forms did —
`Gtk4.Forms.Platform.Gtk` shipped as a community-maintained platform that mapped the Forms
visual tree onto native Gtk 3 widgets, and it was the only way to take a Forms XAML codebase and
run it as a real Linux desktop application. When Forms was retired, that capability had nowhere
to go.

This article walks through a maintenance fork that was built to close exactly that gap: the
`5.0.0` branch of Gtk4.Forms, **pruned down to the Gtk leg only**, retargeted at the modern
.NET SDK, converted to xUnit v3, given a real CI pipeline, and published under the `Gtk4.`
package prefix. It is an interesting codebase to study whether or not you ever intend to ship a
Gtk app, because it is a concrete example of what it takes to keep a large, abandoned UI
framework structurally healthy rather than merely compiling.

## What Was Removed, and Why That Is the Point

The first substantive change was deletion. A single commit removed the Android, iOS, UAP, WPF,
Tizen, macOS and DualScreen projects, the Cake build, the Azure Pipelines definitions and the
original `Xamarin.Forms.sln`.

That sounds destructive, and it is worth being clear about why it is the correct move for a fork
with one maintainer and one target. Upstream Gtk4.Forms is a multi-platform monolith: the
solution contained a dozen platform backends, each with its own SDK requirements, emulators,
provisioning scripts and UITest runners. Keeping them meant every build was gated on toolchains
nobody in this fork would ever install, and every refactor had to be reasoned about across
platforms that could not be compiled, let alone tested. Dead code that cannot be built is not
"kept for later" — it is a permanent source of false signal.

What remains is a clean two-layer shape:

- **`Gtk4.Forms.Core`** — everything platform-independent: `BindableObject` /
  `BindableProperty` (the property system behind bindings, styles and triggers), the
  `Element → VisualElement → View/Page/Layout` tree, layout and measurement, `Shell`,
  `ResourceDictionary` / `Style` / CSS `StyleSheets`, `MessagingCenter`, `DependencyService`
  and animation.
- **`Gtk4.Forms.Platform.Gtk`** — one `IVisualElementRenderer` per control, plus the platform
  services: `IPlatformServices`, the ticker, image source handlers, fonts and isolated storage.

Plus the XAML pair (`Gtk4.Forms.Xaml` at runtime, `Gtk4.Forms.Build.Tasks` at build time),
`Gtk4.Forms.Maps` and its Gtk implementation, and the `Gtk4.Forms.Controls` sample gallery.

Every library project targets a single TFM, `netstandard2.0`. The three test projects target
`net10.0`. `global.json` pins the .NET SDK to **10.0.100** with `rollForward: latestFeature`.

## How a Forms Control Becomes a Gtk Widget

If you have only ever consumed Gtk4.Forms, the renderer mechanism is worth understanding,
because it is the whole trick.

`Gtk4.Forms.Internals.Registrar<T>` maps a Forms type to a renderer type, keyed additionally
by *Visual* and a priority. It is populated during `Forms.Init()`, which scans assemblies for
four attributes:

```csharp
[assembly: ExportRenderer(typeof(MyControl), typeof(MyControlRenderer))]
[assembly: ExportCell(typeof(MyCell), typeof(MyCellRenderer))]
[assembly: ExportImageSourceHandler(typeof(MySource), typeof(MyHandler))]
[assembly: ExportEffect(typeof(MyEffect), "MyEffect")]
```

There is a subtlety that trips people up when they extend the framework rather than an app.
**Default renderers are not declared with `ExportRenderer` at all.** They come from
`[RenderWith(typeof(XRenderer))]` markers on internal placeholder classes in
`Stubs/Xamarin.Forms.Platform.cs`, compiled into a platform-flavoured
`Xamarin.Forms.Platform.dll` that Core references through a `netstandard` facade. Adding a new
control with a default renderer therefore means editing *two* places: the stub file and the Gtk
renderer itself.

The Gtk backend implements the full modern Forms surface — not just the 2015-era controls. The
`Renderers` folder covers `CollectionView`, `CarouselView`, `IndicatorView`, `RefreshView`,
`SwipeView`, `RadioButton`, `CheckBox`, `ImageButton`, `Shell`, `FlyoutPage`, `TabbedPage`,
`WebView`, `OpenGLView`, and a `Shapes` subtree (`Line`, `Path`, `Polyline`, `Polygon`,
`Rectangle`, `Ellipse`). Where Gtk has no equivalent widget, the fork supplies one: the
`Controls` folder contains hand-built `Carousel`, `ShellWidget`, `ShellTabBar`, `NotebookWrapper`,
`ScrolledTextView`, `CustomComboBox` and friends.

## Two Native Dependencies, Handled at Runtime Instead of Build Time

Two renderers need something beyond a base Gtk 3 install, and how the fork handles them is a
design decision worth stealing.

The obvious approach is MSBuild switches — `EnableGtkWebView`, `EnableGtkOpenGL` — so consumers
compile the backend to match their machine. The fork deleted both properties. Instead:

| Renderer | Needs | Without it |
| --- | --- | --- |
| `WebView` | `libwebkit2Gtk-4.1.so.0` | Every P/Invoke entry point is resolved lazily by hand, so the renderer always compiles and registers; it paints a "WebView unavailable" placeholder. |
| `OpenGLView` | Gtk 3.16+ with a usable GL/EGL driver | Runs on `Gtk.GLArea`, which is part of Gtk itself; a machine with no GL driver gets Gtk's own error placeholder inside the view bounds. |

Nothing throws, and nothing requires the consumer to rebuild the backend from source. The
`OpenGLView` change also removed the entire OpenTK 3 dependency and its per-platform GLX/WGL/AGL
window-info initializers.

The principle: **a missing optional native library should degrade a single control at runtime,
not fork your build matrix.**

## Getting Started

The application entry point is small. Gtk is initialized, Forms is initialized, a `FormsWindow`
hosts the `Application`, and the Gtk main loop runs:

```csharp
using Gtk;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;

[STAThread]
static void Main(string[] args)
{
    GtkThemes.Init();
    Application.Init();
    Forms.Init();

    var window = new FormsWindow();
    window.LoadApplication(new App());
    window.SetApplicationTitle("My App");
    window.SetApplicationIcon("icon.png");
    window.Show();

    Application.Run();
}
```

If your renderers live outside the app assembly, use the overload that takes the extra
assemblies — `Registrar` can only scan what has been loaded, and merely referencing a package is
not enough to force a load:

```csharp
Forms.Init(new[] { typeof(Xamarin.Forms.Maps.GTK.MapRenderer).Assembly });
Xamarin.Forms.Maps.GTK.FormsMaps.Init();
```

Packages are on NuGet under the `Gtk4.` prefix — `Gtk4.Forms.Core`,
`Gtk4.Forms.Xaml`, `Gtk4.Forms.Platform.Gtk`, `Gtk4.Forms.Maps.Gtk`,
`Gtk4.Forms.Build.Tasks`. The Gtk package pulls in Core and depends on `GtkSharp` 4.

**The native GTK 4 runtime ships with the package — there is nothing to install.** The `GtkSharp`
4 packages are published on nuget.org (built from the GtkSharp4 fork) and carry the native GTK 4
libraries for every supported OS as runtime assets, so `dotnet restore` alone leaves a Windows,
Linux or macOS machine ready to run. No `gvsbuild`, no `apt install libgtk-4-*`, no
`brew install gtk4`: the old "install the GTK runtime separately" step is gone on all three OSes.

**Threading, because Gtk is not thread-safe.** Everything that touches a widget must run on the
thread that called `Forms.Init()` — the thread running the Gtk main loop. From anywhere else, use
`Device.BeginInvokeOnMainThread`.

## Drop-in Conversion: Add One Package

Writing that `Main` by hand is optional. There is a meta-package, **`Gtk4.Forms.Base`**, whose only
job is to make an *existing* Xamarin.Forms project run on GTK with **no source changes** — no edits
to controls, XAML, `using`s or namespaces. That works because the whole fork keeps the
`Xamarin.Forms.*` assembly names and namespaces (only the directories, projects and package ids
became `Gtk4.Forms.*`), so the `Gtk4.Forms.*` DLLs are binary drop-ins and the consumer's code binds
unchanged. Only the `.csproj` needs rewiring, and the package does it:

- An auto-imported `build/Gtk4.Forms.Base.targets` runs a small tool
  (`ProjectConverter --convert-project`) at `BeforeBuild` that rewrites the consumer `.csproj`: it
  strips the official `Xamarin.Forms`/`Xamarin.Forms.Maps` package references (they would clash with
  the `Gtk4.Forms.*` drop-ins), collapses the `net*-android;net*-ios;…` / `netstandard2.0` target
  frameworks to a single `net10.0`, and sets `OutputType=Exe`. The rewrite is textual, minimal-diff,
  idempotent and BOM-preserving (modelled on the `ResxProcessor`/`Gtk.Windows.Forms.Base` pattern).
- `build/Gtk4.Forms.Base.props` injects a generated `Main` that calls a reflection-based host
  (`GtkHost.Run`): it finds the app's `Xamarin.Forms.Application` subclass, loads the GTK renderer
  assemblies, and runs the `GtkThemes.Init → Application.Init → Forms.Init → FormsWindow → Run`
  sequence above. A `[assembly: GtkMainApplication(typeof(App))]` attribute disambiguates if there is
  more than one `Application`; `SkipGtkHost` / `SkipGtkConvert` are the opt-outs.

One sharp edge, inherent to MSBuild: a build target cannot change the target framework of the build
already in progress, so **the first build after adding the package rewrites the project and the
second build is the clean one** (the rewrite is idempotent, so re-running is safe). A single-project
app pinned to `net*-android` on a box without that workload has to run the converter once by hand
first. Consumers whose UI is XAML also need the `Gtk4.Forms.Build.Tasks` MSBuild targets to reach
them transitively (through the meta-package) — the remaining packaging step for a fully hands-off
XAML conversion.

## Maps Without an API Key

`Gtk4.Forms.Maps` on Gtk is not a wrapper over a platform map SDK, because Gtk has none. The
implementation draws its own Web-Mercator slippy map with **Cairo**, over OpenStreetMap-compatible
raster tiles. That means no API key and no native dependency beyond Gtk 3 itself.

`MapType`, `MoveToRegion`, `VisibleRegion`, pan/zoom, `Pins` and `MapElements`
(`Polyline` / `Polygon` / `Circle`) are implemented, and `Geocoder` is backed by Nominatim. The
honest limitation is `IsShowingUser`: Gtk has no location service, so the application has to
supply a position provider. Anyone shipping to production should also point it at their own tile
server rather than leaning on the OSM community's.

## Testing a UI Framework That Actually Draws

This is the part of the fork I find most instructive. There are three suites, all **xUnit v3 on
`net10.0`**:

| Project | Approximate size | What it covers |
| --- | --- | --- |
| `Gtk4.Forms.Core.UnitTests` | ~4,800 tests | Platform-independent Core |
| `Gtk4.Forms.Xaml.UnitTests` | ~1,000 tests | XAML loader and the XamlC compiler |
| `Gtk4.Forms.Platform.Gtk.UnitTests` | ~170 tests | **Real Gtk 3 widgets** |

The first two were converted from NUnit 3, and the conversion surfaced a set of traps that are
worth knowing about if you ever do the same migration:

- **`Assert.Pass()` has no xUnit equivalent.** In NUnit it unwound the whole test from inside an
  event handler. A naive port to a bare `return;` only exits the *lambda*, leaving a trailing
  `Assert.Fail()` reachable. The fix is a `fired` flag asserted after the act.
- **`Assert.Equal<object>` does not coerce numerics**, where NUnit's `Assert.AreEqual` did. A
  boxed `int` compared against a boxed `double` produces the memorably unhelpful
  `Expected: 123 / Actual: 123`.
- **`Assert.ThrowsAsync` returns a `Task`** and must be awaited; NUnit's ran synchronously.
- **`Assert.Throws<T>` matches the type exactly**, where NUnit's accepted derived types.
- NUnit's `Assume.That` maps to `Assert.SkipWhen` — it means "inconclusive", not "assert".

All three suites **disable test parallelisation**, and this is load-bearing rather than tidiness.
These tests mutate process-global state: `Device.PlatformServices`, `Application.Current`, the
`Registrar`. Without the guard, the Core suite does not merely fail — it hangs indefinitely.

### The Gtk suite drives real widgets

`Gtk4.Forms.Platform.Gtk.UnitTests` instantiates actual Gtk 3 windows and widgets. It runs on
Windows directly against the GDK Win32 backend, and on Linux under Xvfb:

```bash
xvfb-run -a --server-args="-screen 0 1280x1024x24" dotnet test \
  Gtk4.Forms.Platform.Gtk.UnitTests
```

(The default 640x480 Xvfb screen is too small — it clamps the 800x600 test windows.)

The harness, `GtkTestHost`, encodes several pieces of measured behaviour that no amount of
reasoning would have produced:

- **Pump, don't assume.** Geometry is pushed to Gtk from a `GLib.Idle` callback, because you must
  never mutate geometry inside a size-allocate. A single drain of the event queue is therefore one
  round short of the answer; assertions pump six rounds.
- **Never write an `async` test method.** A `GtkSynchronizationContext` is installed on the test
  thread, and every continuation deadlocks. The harness supplies `GtkTestHost.Await` instead.
- **Never `Destroy()` a test window** — GtkSharp's finalizer aborts the run. The harness hides and
  holds the window instead.
- **`Gtk.Window.Resize()` is only a request to the window manager.** Under Xvfb it is serviced
  immediately; on Win32 in a non-interactive session it is *never* serviced at all. Tests drive
  `Window.SizeAllocate(...)` directly.

There is also a nice guard against green-by-absence. An unusable environment normally *skips* the
Gtk tests, which is right on a headless box with no display — but in CI, where the suite is
supposed to run for real, a skip would silently turn it green. So an environment variable,
`XF_Gtk_REQUIRE_NATIVE`, converts "no display" into a hard failure. CI sets it. (The native GTK
runtime is no longer a skip reason at all: it restores with the GtkSharp 4 package, so it is always
present.)

## Build and CI

The build is plain `dotnet` / MSBuild — no Cake, no `build.cmd`:

```powershell
dotnet build Gtk4.Forms.sln
dotnet test  Gtk4.Forms.sln
```

One ordering constraint: **`Gtk4.Forms.Build.Tasks` must be built before anything that consumes
XAML**, since it supplies the XAML MSBuild tasks. Building the solution handles that for you; a
project-at-a-time build after a `bin`/`obj` wipe does not.

Two build-wide settings shape day-to-day work:

- **`TreatWarningsAsErrors=true` for every project.** A new warning breaks the build. This is
  aggressive for an inherited codebase and it is the reason the tree stays clean.
- **`UseOSSpecificOutputPaths=true`** redirects output to `bin-wsl/` / `obj-wsl/`, so a Windows
  build and a WSL build of the same working tree do not fight over `obj/`.

CI is a single GitHub Actions workflow that builds the solution and runs all three suites on
**Windows, Linux and macOS**. The native GTK 4 runtime is no longer a CI setup step — it restores
with the GtkSharp 4 package on every OS — so the only OS-level provisioning left is a virtual
display (Xvfb) on headless Linux, plus the optional WebKitGtk and Mesa that the WebView and OpenGL
renderers degrade gracefully without. It builds **Debug, and that is forced rather
than preferred**: upstream wraps `[assembly: InternalsVisibleTo("Xamarin.Forms.Xaml.UnitTests")]`
in `#if DEBUG`, so a Release build of the XAML test project fails with `error CS0122:
'XamlCAssemblyResolver' is inaccessible due to its protection level`. The fork's fix was to change
the guard to `#if !SIGNED_ASSEMBLY`, so the test assembly keeps its access in every unsigned
configuration.

The two OSes do not agree on absolutely everything — one pixel-readback test currently passes on
Windows and fails under Xvfb — which is itself a useful reminder that "the suite is green" is a
question you have to ask per platform.

## Packaging: Where the Real Bugs Hid

A theme worth calling out, because it generalises far beyond this repository: several of the
nastiest defects found in this fork were not in rendering code at all. They were in the build.

- A NuGet utility package auto-imported MSBuild targets that **overwrote `PackageVersion` at pack
  time**, so every package produced on Windows was stamped `5.0.0` regardless of commit, while the
  same commit packed on CI carried a git-derived version. Two different commits, one package
  identity.
- The same package ran a third-party obfuscator over shipping assemblies in Release builds on
  Windows only, gated on an environment variable that existed on exactly one machine.
- `GenerateAssemblyInfo` was off with a hand-written `AssemblyInfo.cs`, so two shipping assemblies
  compiled with `AssemblyVersion 0.0.0.0` while Core and Xaml carried `2.0.0.0` — meaning binding
  redirects and strong-name identity checks saw a version the rest of the framework never used.
- A `Content` item copied a runtime-loaded PNG to the output directory for in-tree
  `ProjectReference` builds, but the SDK packs `Content` into `contentFiles` with
  `copyToOutput` defaulting to `false` — so the file never reached the output of anyone consuming
  the *package*, and a relative-path load threw at runtime for exactly the consumers the package
  exists for. The fix was `PackageCopyToOutput`.

The response was not just to fix them but to make regressions loud. The Gtk project carries two
MSBuild assertion targets that **fail the build** if the removed tooling is reintroduced, or if
`PackageVersion` still equals its static fallback at `GenerateNuspec` time. A bug that was silent
twice deserves a tripwire, not a comment.

## Should You Use It?

Be honest about the tradeoffs.

**Do not start a new cross-platform mobile app on this.** Use .NET MAUI. Upstream Gtk4.Forms is
out of support and this fork does not change that for iOS or Android — those backends do not exist
here.

**It is genuinely useful if** you have an existing Gtk4.Forms XAML codebase and need a Linux
(or Gtk-on-Windows) desktop target that MAUI cannot give you, or you need to keep a Forms-based
line-of-business application maintainable on a modern .NET SDK while a MAUI migration proceeds at
its own pace. Running on .NET 10 with a working CI pipeline and ~6,000 tests is a materially
different position from running on an unsupported SDK with no signal at all.

**And it is worth reading regardless** if you maintain something inherited and large. The
techniques here — delete the legs you cannot test, make optional native dependencies degrade at
runtime, put measured platform behaviour in harness comments rather than in tribal memory, treat
warnings as errors, and turn silent build-system bugs into hard failures — are not Gtk-specific.
They are what the difference between "still compiles" and "still maintained" actually looks like.

## Summary

.NET MAUI succeeded Gtk4.Forms everywhere except Gtk. This fork keeps that one leg alive: the
Gtk4.Forms `5.0.0` Core and XAML stack, mapped onto native Gtk 3 widgets through GtkSharp,
building on the .NET 10 SDK, tested by three xUnit v3 suites — one of which drives real Gtk
windows on both Windows and headless Linux — and shipped as `Gtk4.`-prefixed NuGet packages.

Whether you need a Linux desktop target for an existing Forms application or you are simply
curious what disciplined maintenance of an end-of-life framework looks like in practice, the
codebase answers both questions.

---

*Repository: [github.com/pieroviano/Xamarin.Forms](https://github.com/pieroviano/Xamarin.Forms),
branch `5.0.0`. Original project: [xamarin/Xamarin.Forms](https://github.com/xamarin/Xamarin.Forms)
(MIT). For new cross-platform work, see [.NET MAUI](https://github.com/dotnet/maui) and the
[official upgrade guidance](https://learn.microsoft.com/dotnet/maui/migration).*
