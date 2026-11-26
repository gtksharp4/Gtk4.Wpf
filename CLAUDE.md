# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository

A **WPF-on-GTK 4 compatibility layer**: it runs an **unmodified WPF** application — its
`System.Windows.*` code and WPF XAML — on a GTK 4 engine, with no namespace or XAML changes, across
**Windows, Linux and macOS**. The WPF types are reimplemented in their real `System.Windows.*`
namespaces so consumer code binds against them unchanged; only the project file is rewritten (strip
`UseWPF`, retarget to `net10.0`) by a one-time, idempotent converter.

Lineage: this started as a fork of `xamarin/Xamarin.Forms` (the GTK leg) and once also carried the
full Xamarin.Forms→GTK 4 port. That port has been **removed** — the repository is now only the
WPF-on-GTK layer. Branch/history notes for the earlier prune live under `docs/plans/`.

**Solution:** `Gtk4.Wpf.sln` is the only solution in the tree, with 13 projects (7 product + 6
test). Project/package names use the `Gtk4.*` prefix; the reimplemented WPF types keep their
`System.Windows.*` namespaces (that identity is the whole point — it is what lets WPF code compile
unchanged).

## Projects

| Project | Assembly | TFM | Role |
|---|---|---|---|
| `Gtk4.WindowsBase` | `Gtk4.WindowsBase` (`namespace System.Windows`) | netstandard2.0 | The `DependencyObject`/`DependencyProperty` property system + routed events. No GTK dependency. |
| `Gtk4.PresentationFramework` | `Gtk4.PresentationFramework` (`namespace System.Windows`) | net10.0 (+ net10.0-windows on Windows) | `UIElement`→`Window`/controls, the binding engine, the control set, themes — rendered on GtkSharp 4. |
| `Gtk4.MarkupCompiler` | `Gtk4.MarkupCompiler` | netstandard2.0 | Build-time MSBuild task: generates `InitializeComponent` from the untouched WPF XAML, System.Xaml-free. `[assembly: XmlnsDefinition]` makes the WPF xmlns resolve to our types. |
| `Gtk4.Wpf.ProjectConverter` | `Gtk4.Wpf.ProjectConverter` | net10.0 (Exe) | Pure, idempotent, minimal-diff csproj rewrite: strip `UseWPF`/`UseWindowsForms`, collapse `net*-windows`→`net10.0` (OutputType left as-is). |
| `Gtk4.Wpf` | package `Gtk4.Wpf` | net10.0 | The one-package drop-in. References the four above; its `build/`+`buildTransitive/` props+targets run the converter; it packs the converter DLL into `tools/`. |
| `Gtk4.WpfSample` | app (Exe) | net10.0 | An **unmodified WPF** app (`App : System.Windows.Application`, WPF XAML) compiled through the layer as the end-to-end proof. Its `[STAThread] Main` is generated from `App.xaml` by the MarkupCompiler. |
| `Gtk4.Wpf.Testing` | package `Gtk4.Wpf.Testing` | net10.0 | UI test driver (xUnit v3): `WpfAppHost` runs an app in the test process on the GTK thread, `By` locators, waits that pump the loop, semantic actions over UI Automation, routed `Input`, GTK-snapshot screenshots and failure evidence. References PresentationFramework only; tests live in `Gtk4.Wpf.Testing.Tests`. |

`Gtk4.PresentationFramework` references `Gtk4.WindowsBase`; everything else references
only within this set. There is no dependency on any external WPF or `Microsoft.WindowsDesktop` SDK.

## Build

Plain `dotnet` / MSBuild. `global.json` pins .NET SDK **10.0.100** (`rollForward: latestFeature`).

```powershell
dotnet build Gtk4.Wpf.sln                 # whole solution
dotnet build Gtk4.Wpf.sln -c Release
```

The native GTK 4 runtime arrives with the `GtkSharp4` package (GTK 4.22, floated `4.22.4.*` via
`$(GtkSharp4Version)` in `Directory.Build.props`, restored from the `Local`/vendored feed). On
Windows the first build downloads a gvsbuild runtime into `%LOCALAPPDATA%\Gtk`; the CI scripts warm
it up serially to avoid a parallel-download race (see below).

Build-wide settings live in `Directory.Build.props` / `Environment.Build.props`:
- **`TreatWarningsAsErrors=true` for every project** — a new warning breaks the build.
- **Strong-name signing**: every `Gtk4.*` project is signed with the repo-root `sign-key.snk` (public key
  token `a5ab967c088e8330`). Each `InternalsVisibleTo` must carry `Key="$(Gtk4FormsPublicKey)"`, or the signed
  friend loses access. Projects outside the prefix (user-app stand-ins, WPF-Samples) stay unsigned.
- `UseOSSpecificOutputPaths=true` redirects output to `bin-wsl/` / `obj-wsl/`, so a Windows build
  and a WSL build of the same tree do not fight over `obj/`.
- Versions come from GitInfo + `Version.targets`.

## Tests

Six suites, all **xUnit v3 on `net10.0`**, run with `dotnet test`:

| Project | Tests | GTK? | Covers |
|---|---|---|---|
| `Gtk4.WindowsBase.Tests` | ~83 | no | DP / routed-event / property system |
| `Gtk4.MarkupCompiler.Tests` | ~173 | no | XAML → `InitializeComponent` code generation |
| `Gtk4.Wpf.ProjectConverter.Tests` | ~46 | no GTK | csproj rewrite, plus MSBuild target tests that spawn a nested `dotnet build` (needs a working SDK on PATH) |
| `Gtk4.PresentationFramework.Tests` | ~1399 | **yes** | controls, binding, themes — on **real GTK 4 widgets** |
| `Gtk4.Wpf.Testing.Tests` | ~50 | **yes** | the driver itself, plus UI tests of `Gtk4.WpfSample` and five official WPF-Samples compiled in process (SimpleBinding, MasterDetail, DirectionalBinding, HandlingEventOnCommandUsingCode, MousePointer) |
| `Gtk4.Integration.Tests` | few | yes | end-to-end: add `Gtk4.Wpf` to the embedded WPF sample, build (converts), run, detect the window |

```powershell
dotnet test Gtk4.Wpf.sln                              # everything
dotnet test Gtk4.WindowsBase.Tests --no-build
dotnet test Gtk4.PresentationFramework.Tests --no-build
```

The first three suites need no GTK runtime or display (the converter suite needs an SDK on PATH). `PresentationFramework.Tests`
drives real GTK 4 widgets. `Integration.Tests` needs the `Gtk4.Wpf` package already packed in
the repo `Packages/` feed and the native GTK runtime; its window detection is Windows-only and it is
**not** part of CI.

### GTK renderer tests

`Gtk4.PresentationFramework.Tests` is an `Exe` with a custom `Program.cs`, and `WpfGtkFixture`
owns the single thread GTK is initialised on. GTK 4 is thread-sensitive per OS:

- **Windows** — the GDK Win32 backend calls `OleInitialize`, which needs **STA**. `WpfGtkFixture`
  runs a dedicated STA background thread; every test body runs on it. No `DISPLAY`, no Xvfb.
- **Linux** — needs an X server: `xvfb-run -a --server-args="-screen 0 1280x1024x24" dotnet test …`
  with `GDK_BACKEND=x11` (the default 640x480 would clamp the 800x600 test windows).
- **macOS** — GTK 4's Quartz backend requires **thread 0**, which `dotnet test` owns, so the
  assembly is executed directly (`dotnet exec …dll`) and `Program.RunOnThreadZero` (selected by
  `XF_GTK_MAC_RUNNER=1`) owns thread 0.

`XF_GTK_REQUIRE_NATIVE=1` turns "no display / no GTK runtime" from a harness skip into a failure, so
a broken install goes red rather than green-by-absence. Test parallelisation stays disabled.

Known intermittent: on the macOS runner GTK occasionally never allocates a newly shown window (it stays 0x0 and
inactive), so `TextGapsTests.TextBox_wrapping_without_AcceptsReturn_is_content_sized_in_a_StackPanel` times out.
It passed on re-run (run 44899539173). Treat a repeat as real.

The macOS CI job does not gate (`continue-on-error`): on the hosted runner the test process freezes for up to ~30 s at
a time, with App Nap off (measured by the `GtkPump STALL` lines, ~135 per run), so a different time-bounded test
fails each run. Read its failures against the stall lines before treating one as a regression.

CI is `.github/workflows/build-and-test.yml` (Windows, Linux, macOS); per-OS logic lives in
`scripts/ci/test-{windows,linux,macos}`.

## Drop-in conversion

`Gtk4.Wpf` makes an existing WPF project run on GTK by adding one `PackageReference`:

1. NuGet auto-imports `build/Gtk4.Wpf.{props,targets}` (and `buildTransitive/` copies for
   transitive consumers).
2. The `GtkFormsConvertProject` target runs `Gtk4.Wpf.ProjectConverter` (packed under
   `tools/`) at `BeforeBuild`, rewriting the consumer `.csproj` on disk.
3. Because that build was already evaluated as a WPF project (the WindowsDesktop SDK), the converter
   **returns exit code 10 when it changed the project** and the target stops this build with an
   actionable message. The **next** build re-evaluates the converted (non-WPF) project and compiles
   against `Gtk4.Wpf`. Exit 0 = already converted → build proceeds. (`SkipGtkFormsConvert=true`
   opts out of the rewrite only; design-time/IntelliSense builds are skipped. `<Resource>` files are
   copied to the output either way; `SkipGtkFormsCopyResources=true` opts out of that.)

The csproj rewrite is a **one-time conversion** — a build target cannot retarget the build in
progress, so the first build rewrites and the second is clean. `Gtk4.Integration.Tests` proves
the whole loop end-to-end against the embedded sample.

## Architecture

- **`Gtk4.WindowsBase`** (`namespace System.Windows`, netstandard2.0) is the property engine:
  `DependencyObject`/`DependencyProperty` (value precedence, coercion, change callbacks), the routed
  event system, and `Dispatcher`. No GTK.
- **`Gtk4.PresentationFramework`** (`namespace System.Windows`) builds the visual tree on top:
  `UIElement`→`FrameworkElement`→controls, `Window`, layout/measurement, the binding engine, default
  styles/`ControlTemplate`s and theming (`System.Windows.Themes`), and the GtkSharp 4 rendering.
  Each WPF control is backed by a GTK 4 widget (e.g. `TextBox`→`Gtk.Entry`, `ComboBox`→`Gtk.DropDown`
  + `Gtk.StringList`, `Border`→`Gtk.Frame`). GTK 4-native widgets are preferred over the deprecated
  GTK 3 compat shims (`[Obsolete]` in GtkSharp4), which `TreatWarningsAsErrors` would otherwise
  reject.
- **`Gtk4.MarkupCompiler`** is the build-time half of XAML: a System.Xaml-free MSBuild task
  that generates the `InitializeComponent` + `x:Name` partial class (and, for `App.xaml`, the
  `[STAThread] Main`) from the untouched presentation-namespace XAML.

**The GTK binding is GtkSharp 4 (GTK 4.22), not GtkSharp 3.** The fork keeps assembly name
`GtkSharp.dll` and `namespace Gtk` but exposes the GTK 4 API, restored from the `Local` feed
(`nuget.config` → `Packages`) or the vendored `build/gtksharp4-packages/` feed.

## Coding style

.NET Foundation style with the project's exceptions (see `.editorconfig`, which is authoritative):
- **Hard tabs**, not spaces, in `.cs` files.
- **Never write `private`** — it's the C# default (`dotnet_style_require_accessibility_modifiers = never`).
- Lines up to ~120 characters.
- Allman braces; `var` only when the type is apparent.
- Fields are camelCase, private/internal fields prefixed with `_`; constants PascalCase.
