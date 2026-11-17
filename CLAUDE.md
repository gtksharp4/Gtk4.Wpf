# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository

Fork of `xamarin/Xamarin.Forms` (origin: `pieroviano/Xamarin.Forms`), working on the **`1.0.0`**
branch, which is also the default/PR-target branch here. Upstream Xamarin.Forms reached end of
support on May 1, 2024 and was succeeded by .NET MAUI; this repo is a maintenance fork of the
1.0.0 line.

**This fork has been pruned to the GTK leg only.** The prune removed the Android, iOS, UAP, WPF,
Tizen, MacOS and DualScreen projects, the Cake build, the Azure pipelines and the original
`Xamarin.Forms.sln`. No commit is cited here because none survives: those files were later purged
from history entirely, which emptied the prune commit and dropped it — the record is
`docs/plans/history-purge-deleted-files.md` on the `gtk4` branch. What remains is a shared portable
UI abstraction (`Gtk4.Forms.Core`) mapped onto native **GTK 4** widgets by the renderers in
`Gtk4.Forms.Platform.Gtk`.

**Naming after the rename.** The directories, project files, the solution (`Gtk4.Forms.sln`) and the
NuGet package ids were renamed `Xamarin.Forms.*` → `Gtk4.Forms.*`, but the **assembly names and code
namespaces are deliberately kept as `Xamarin.Forms.*`** (binding compat; the `InternalsVisibleTo`
wiring and string-based renderer resolution key off them). So a project lives in
`Gtk4.Forms.Platform.Gtk/` yet still compiles to `Xamarin.Forms.Platform.Gtk4.dll` with
`namespace Xamarin.Forms.Platform.GTK`. Paths, commands and project names below use the `Gtk4.Forms.*`
names; assembly names, namespaces and `InternalsVisibleTo` strings stay `Xamarin.Forms.*`.

**The backend binds GtkSharp 4 (GTK 4.22), not GtkSharp 3.** `Platform.Gtk` and `Maps.Gtk`
reference the `GtkSharp4` package (`Directory.Build.props` `$(GtkSharp4Version)`, floated `4.22.4.*`)
restored from the `Local` feed (`nuget.config` → `Packages`, a symlink to a shared feed), and the
renderer carries the **Gtk4** name — assembly `Xamarin.Forms.Platform.Gtk4`, NuGet package
`Gtk4.Forms.Platform.Gtk`. The GtkSharp4 fork keeps assembly name `GtkSharp.dll` and
`namespace Gtk` but exposes the GTK 4 API, with a GTK3→GTK4 compat layer under its
`Source/Libs/GtkSharp/Compat/`. Many GTK3 APIs survive there as **`[Obsolete]`** shims
(`Widget.Allocation`, `StyleContext`, `CssProvider.LoadFromData`, `Gtk.ListStore`, `MessageDialog`,
`ComboBox`…); because `TreatWarningsAsErrors` is on, `Platform.Gtk`/`Maps.Gtk`/the gallery demote
`CS0612;CS0618` via `WarningsNotAsErrors` while the backend is still migrating off those shims — only
genuine GTK4 *removals* are hard errors. Removing that line is the signal the idiomatic rewrite is
complete.

Empty directories such as `Xamarin.Forms.Platform.Android/` may still exist in a working tree as
local `bin`/`obj` residue from before the prune. They contain no tracked files — ignore them, or
delete them locally.

## Build

Plain `dotnet` / MSBuild. There is no Cake build and no `build.cmd` — those were deleted.
`global.json` pins .NET SDK **10.0.100** (`rollForward: latestFeature`) and
`MSBuild.Sdk.Extras` 3.0.44.

```powershell
dotnet build Gtk4.Forms.sln                 # whole solution
dotnet build Gtk4.Forms.Platform.Gtk/Gtk4.Forms.Platform.Gtk.csproj
dotnet build Gtk4.Forms.sln -c Release      # note the Release caveat below
```

`Gtk4.Forms.sln` is the only solution in the tree.

**`Gtk4.Forms.Build.Tasks` must be built before anything else that consumes XAML.** It supplies
the XAML MSBuild tasks; `Directory.Build.props` points `_XFBuildTasksLocation` at
`artifacts/build-tasks/netstandard2.0/` (or `artifacts/build-tasks-unix/netstandard2.0/` when
`UseOSSpecificOutputPaths` is set). After a clean or a `bin`/`obj` wipe, build that project first.
Building the solution handles the ordering for you.

**Release builds.** It used to not: `Gtk4.Forms.Xaml.UnitTests` failed with
`error CS0122: 'XamlCAssemblyResolver' is inaccessible due to its protection level`, because
upstream wrapped `[assembly: InternalsVisibleTo("Xamarin.Forms.Xaml.UnitTests")]` in `#if DEBUG`.
`Gtk4.Forms.Build.Tasks/Properties/AssemblyInfo.cs` now guards it with `#if !SIGNED_ASSEMBLY`
instead, so the test assembly keeps its access in every unsigned configuration. CI still builds
Debug, but Release is no longer expected to fail.

Build-wide settings live in `Directory.Build.props` / `Environment.Build.props`:
- **`TreatWarningsAsErrors=true` for every project** — a new warning breaks the build.
- `LangVersion` is **8.0** (test projects override this to `latest`).
- `UseOSSpecificOutputPaths=true` redirects output to `bin-wsl/` / `obj-wsl/`, so a Windows build
  and a WSL build of the same tree do not fight over `obj/`.
- Versions come from GitInfo + `Version.targets`; `AssemblyVersion` is deliberately frozen at
  `2.0.0.0` (binding-compat) while `PackageVersion` is computed from the git tag/branch. Base
  version is in `GitInfo.txt`.
- `ANDROID_RENDERERS` and `AndroidTargetFrameworks` are still declared in
  `Environment.Build.props` but are vestigial — no Android project remains.

All library projects (`Core`, `Xaml`, `Platform.Gtk`, `Build.Tasks`, `Maps`, `Maps.Gtk`) target a
single TFM, `netstandard2.0`. The three test projects target `net10.0`.

## Tests

Three suites, all **xUnit v3 on `net10.0`**, all run with `dotnet test`:

| Project | Tests | Notes |
|---|---|---|
| `Gtk4.Forms.Core.UnitTests` | ~4856 | platform-independent Core |
| `Gtk4.Forms.Xaml.UnitTests` | ~1047 | XAML loader + XamlC compiler, incl. the `MSBuildTests` integration tests (all green: the fixture's `_Directory.Build.{props,targets}` import `Gtk4.Forms.Build.Tasks\build\Xamarin.Forms.{props,targets}` after the rename, and the copied `NuGet.config`'s local feeds are absolutised so the per-test temp build restores) |
| `Gtk4.Forms.Platform.Gtk.UnitTests` | ~191 (+3 skip) | **real GTK 4 widgets** |

```powershell
dotnet test Gtk4.Forms.sln                          # everything
dotnet test Gtk4.Forms.Core.UnitTests --no-build
dotnet test Gtk4.Forms.Core.UnitTests --filter FullyQualifiedName~BindingUnitTests
```

These were converted from NUnit 3. When touching a converted test, be aware of the traps the
conversion hit (all fixed, but the patterns recur):

- **`Assert.Pass()` has no xUnit equivalent.** It unwound the whole test from inside an event
  handler. A bare `return;` only exits the *lambda*, leaving a trailing `Assert.Fail()` reachable.
  Use a `fired` flag and assert it after the act.
- **`Assert.Equal<object>` does not coerce numerics.** NUnit's `Assert.AreEqual` did. Boxed `int`
  vs boxed `double` produces the baffling `Expected: 123 / Actual: 123`.
- **`Assert.ThrowsAsync` returns a `Task`** and must be awaited; NUnit's ran synchronously.
- **`Assert.Throws<T>` matches the type exactly**, where NUnit's accepted derived types.
- **NUnit `Assume.That` → `Assert.SkipWhen`** (both mean "inconclusive"), not an assertion.

**All three suites disable test parallelisation** via an `AssemblyTestConfig.cs`, and this is
load-bearing, not tidiness. These tests mutate process-global state (`Device.PlatformServices`,
`Application.Current`, the `Registrar`). Without the guard `Core.UnitTests` does not merely fail —
it hangs indefinitely. Do not remove it. `Xaml.UnitTests` additionally applies an assembly-level
`[EnsurePlatformServices]` hook, because many of its fixtures never set `Device.PlatformServices`
themselves and previously passed only by accident of ordering.

`Gtk4.Forms.Core.UnitTests` and `Gtk4.Forms.Xaml.UnitTests` are **SDK-style with default
globbing** (`EnableDefaultCompileItems=true`) — a new test file is picked up automatically. (The
legacy explicit-`<Compile Include>` form, where new files were silently ignored, is gone.) Core
unit tests derive from `BaseTestFixture` and rely on `MockPlatformServices`/`MockDispatcher`,
shared into the XAML tests via linked `<Compile Include="..\Gtk4.Forms.Core.UnitTests\...">`.
XAML compiler tests invoke `XamlCTask` in-process through
`Gtk4.Forms.Xaml.UnitTests/MockCompiler.cs`.

### GTK renderer tests

`Gtk4.Forms.Platform.Gtk.UnitTests` drives **real GTK 4 widgets** and runs on both OSes:

- **Windows** — works directly against the GDK Win32 backend; no `DISPLAY`, no Xvfb.
- **Linux** — needs an X server: `xvfb-run -a --server-args="-screen 0 1280x1024x24" dotnet test …`
  with `GDK_BACKEND=x11`. The default 640x480 would clamp the 800x600 test windows.

**GTK 4 must be initialised on an STA thread on Windows** (its Win32 GDK backend calls
`OleInitialize`, which fails on the MTA pool thread xUnit runs tests on — `Gdk-ERROR: OleInitialize
failed`, and the whole run hangs). `GtkFixture` owns one STA (on Windows) background thread that runs
`Gtk.Application.Init(); Forms.Init();` and a work queue; **every test body must run on it** by
deriving from `GtkTestBase` and wrapping the body in `Run(() => { … })` / `Run<T>(() => …)`. Test
parallelisation stays disabled.

`GtkTestHost` is the harness. Read its comments before adding a test — they record measured
behaviour, not theory. In particular: pump with `GtkTestHost.Pump`/`PumpUntil` rather than
assuming a relayout happened (`Pump`'s `ForceAllocate` allocates at least the widget's measured
minimum — GTK 4 *refuses* an allocation smaller than that and drops the layout pass); never write an
`async` test method (a `GtkSynchronizationContext` is installed and every continuation deadlocks —
use `GtkTestHost.Await`); and never `Destroy()` a test window (GtkSharp's finalizer aborts the run —
`Retire` hides and holds). Renderers must likewise only tear native widgets down on a deterministic
`Dispose` (`disposing == true`), never from the GC finalizer — `Gtk.Widget.Destroy()` on the
finalizer thread throws `MissingIntPtrCtorException`.

**`Gtk.Window.Resize()` is only a request to the window manager.** Under Xvfb it is serviced
immediately; on Win32 in a non-interactive session it is never serviced at all. Drive
`Window.SizeAllocate(...)` directly instead of resizing and measuring.

The two OSes do not agree on everything. Known open difference:
`CoreControlMappingTests.BoxViewPaintsItsColourAndRepaintsOnChange` passes on Windows and fails
under Xvfb ("a red BoxView painted rgb(0,0,0)"). Verify both sides before calling the suite green.

CI is `.github/workflows/build-and-test.yml` (Windows, Linux and macOS). All three suites gate it on
every OS; per-OS logic lives in `scripts/ci/test-{windows,linux,macos}`.

## Architecture

**`Gtk4.Forms.Core`** (netstandard2.0, assembly `Xamarin.Forms.Core`) holds everything platform-independent:
`BindableObject`/`BindableProperty` (the property system backing bindings, styles, triggers), the
`Element` → `VisualElement` → `View`/`Page`/`Layout` tree, layout and measurement, `Shell`,
`ResourceDictionary`/`Style`/CSS `StyleSheets`, `MessagingCenter`, `DependencyService`, animation,
and `PlatformConfiguration/<Platform>Specific`.

**Renderer resolution.** `Xamarin.Forms.Internals.Registrar<T>` (`Gtk4.Forms.Core/Registrar.cs`)
maps a Forms type → renderer type, keyed additionally by *Visual* and a priority. It is populated
during `Forms.Init()` (`Gtk4.Forms.Platform.Gtk/Forms.cs`), which scans assemblies for
`[assembly: ExportRenderer(typeof(Control), typeof(ControlRenderer))]`, `ExportCell`,
`ExportImageSourceHandler` and `ExportEffect`.

**Default renderers** are *not* declared with `ExportRenderer`. They come from
`[RenderWith(typeof(XRenderer))]` markers on internal placeholder classes in
**`Gtk4.Forms.Platform/Xamarin.Forms.Platform.cs`**, compiled into a platform-flavoured
`Xamarin.Forms.Platform.dll` (`Gtk4.Forms.Platform/Gtk4.Forms.Platform.Base.csproj` is the
netstandard facade Core references). Adding a control with a default renderer means editing
`Gtk4.Forms.Platform/Xamarin.Forms.Platform.cs` as well as the GTK renderer.

**`Gtk4.Forms.Platform.Gtk`** implements `IVisualElementRenderer` per control plus the platform
services (`IPlatformServices`, ticker, image sources). `FormsWindow` hosts an `Application`;
`Platform.cs` owns renderer creation and the layout pass.

**XAML** is two layers. `Gtk4.Forms.Xaml` is the runtime loader/parser (`LoadFromXaml`, markup
extensions, type converters). `Gtk4.Forms.Build.Tasks` is the build-time half: `XamlGTask`
generates the `InitializeComponent` + `x:Name` partial class, and `XamlCTask` (Mono.Cecil) compiles
XAML into IL — the `*Visitor.cs` files walk the XAML node tree emitting IL. XAML errors surface as
MSBuild errors via `BuildException`/`ErrorMessages.resx`.

Other pieces: `Gtk4.Forms.Maps` + `Gtk4.Forms.Maps.Gtk`, `Gtk4.Forms.CustomAttributes`
(test attributes such as `[Issue]`, `[Preserve]`), and `Gtk4.Forms.Controls` +
`Gtk4.Forms.ControlGallery` (the sample/QA app). `Gtk4.Forms.Controls` imports the 1369
upstream issue repros from `Gtk4.Forms.Controls.Issues/Xamarin.Forms.Controls.Issues.Shared.projitems`
(the shared-project folder itself keeps its `Xamarin.Forms.*` name), so they compile into
the gallery even though the per-platform UITest runners were pruned.

**Drop-in conversion.** `Gtk4.Forms.Base` is a meta-package that makes an existing Xamarin.Forms
project run on GTK by adding one `PackageReference` — no source changes (the preserved
`Xamarin.Forms.*` assembly/namespace identity is what lets consumer code bind unchanged). Its
auto-imported `build/Gtk4.Forms.Base.targets` runs `Gtk4.Forms.ProjectConverter --convert-project`
(a `net10.0` console tool; pure, idempotent, minimal-diff csproj rewrite covered by
`Gtk4.Forms.ProjectConverter.Tests`, ~42 xUnit v3 tests) at `BeforeBuild` to strip the official
`Xamarin.Forms*` refs, collapse platform TFMs to `net10.0` and set `OutputType=Exe`; its
`build/Gtk4.Forms.Base.props` injects a `Main` that calls `Gtk4.Forms.Host`'s reflection-based
`GtkHost.Run` (assembly `Xamarin.Forms.Host`). The csproj rewrite is a **one-time conversion** (a
build target cannot retarget the build in progress — the first build rewrites, the second is clean);
`SkipGtkConvert`/`SkipGtkHost` opt out. Still open: `Gtk4.Forms.Build.Tasks` must expose its XAML
targets via `buildTransitive/` for a fully hands-off XAML consumer.

## Coding style

.NET Foundation style with the project's exceptions (see `.editorconfig`, which is authoritative):
- **Hard tabs**, not spaces, in `.cs` files.
- **Never write `private`** — it's the C# default (`dotnet_style_require_accessibility_modifiers = never`).
  Much of the inherited GTK code does write it; leave existing declarations alone.
- Lines up to ~120 characters.
- Allman braces; `var` only when the type is apparent.
- Fields are camelCase, private/internal fields prefixed with `_`; constants PascalCase.
