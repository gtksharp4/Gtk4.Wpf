# Gtk4.Wpf — WPF on GTK 4 #

Run an **unmodified WPF** application — its `System.Windows.*` code and WPF XAML, with **no namespace
or XAML changes** — as a native desktop app on **Windows, Linux and macOS**, powered by a GTK 4
engine. The WPF types are reimplemented in their real `System.Windows.*` namespaces, so your code
binds against them unchanged; only the project file is rewritten (strip `UseWPF`, retarget to
`net10.0`) by a one-time, idempotent converter.

Lineage: this began as a fork of [`xamarin/Xamarin.Forms`](https://github.com/xamarin/Xamarin.Forms)
(MIT) and once also carried a full Xamarin.Forms→GTK 4 port; that port has been removed and the
repository is now solely the WPF-on-GTK compatibility layer.

## How it works

1. Add **one** `PackageReference` to `Gtk4.Wpf` to an existing WPF project.
2. On the first build, the package's converter rewrites the `.csproj` (removes `UseWPF`, collapses
   `net*-windows` to `net10.0`) and stops with a message to re-run.
3. The second build compiles the **unchanged** `System.Windows.*` code and WPF XAML against the
   Gtk4.Wpf reimplementation and produces a GTK 4 desktop app.

No source edits. The `[STAThread] Main` is generated from `App.xaml` by the markup compiler.

## Packages

Published under the `Gtk4.` prefix. The GTK backend binds **GtkSharp 4 (GTK 4.22)**, which also
carries the native GTK 4 runtime (see [Native GTK 4 runtime](#native-gtk-4-runtime)).

| Package id | TFM | Role |
| --- | --- | --- |
| `Gtk4.Wpf` | net10.0 | **The drop-in.** One `PackageReference` converts and runs a WPF app. Pulls in the three below + the converter. |
| `Gtk4.WindowsBase` | netstandard2.0 | `DependencyObject`/`DependencyProperty` + routed-event property system (`namespace System.Windows`). |
| `Gtk4.PresentationFramework` | net10.0 | `UIElement`→`Window`/controls, binding engine, themes — rendered on GtkSharp 4 (`namespace System.Windows`). |
| `Gtk4.MarkupCompiler` | netstandard2.0 | Build-time MSBuild task: generates `InitializeComponent` from the untouched WPF XAML. |

`Gtk4.PresentationFramework` references `Gtk4.WindowsBase`; the layer depends on **no**
external WPF or `Microsoft.WindowsDesktop` SDK.

## Build

Plain `dotnet` / MSBuild. `global.json` pins .NET SDK **10.0.100** (`rollForward: latestFeature`).

```powershell
dotnet build Gtk4.Wpf.sln                 # whole solution (11 projects)
dotnet build Gtk4.Wpf.sln -c Release
```

`Gtk4.Wpf.sln` is the only solution in the tree. Build-wide settings live in
`Directory.Build.props` / `Environment.Build.props`: `TreatWarningsAsErrors=true` everywhere, and
`UseOSSpecificOutputPaths=true` (redirects to `bin-wsl/` / `obj-wsl/` so a Windows and a WSL build of
the same tree do not fight over `obj/`).

## Tests

Five suites, all **xUnit v3 on `net10.0`**, run with `dotnet test`:

| Project | Approx. tests | GTK? | Covers |
| --- | --- | --- | --- |
| `Gtk4.WindowsBase.Tests` | ~83 | no | DP / routed-event / property system |
| `Gtk4.MarkupCompiler.Tests` | ~173 | no | XAML → code generation |
| `Gtk4.Wpf.ProjectConverter.Tests` | ~15 | no | csproj rewrite |
| `Gtk4.PresentationFramework.Tests` | ~1,290 | **yes** | controls, binding, themes on **real GTK 4 widgets** |
| `Gtk4.Integration.Tests` | few | yes | end-to-end: convert + build + run the embedded WPF sample (Windows-only, not in CI) |

```powershell
dotnet test Gtk4.Wpf.sln
```

`PresentationFramework.Tests` drives real GTK 4 windows. On **Windows** it initialises GTK on an STA
thread (the GDK Win32 backend requires it) and needs no display. On **Linux** it needs an X server:

```bash
xvfb-run -a --server-args="-screen 0 1280x1024x24" env GDK_BACKEND=x11 \
  dotnet test Gtk4.PresentationFramework.Tests
```

On **macOS** GTK's Quartz backend requires thread 0, so the test assembly is executed directly. CI is
`.github/workflows/build-and-test.yml` (Windows, Linux, macOS); per-OS logic lives in
`scripts/ci/test-{windows,linux,macos}`.

## Native GTK 4 runtime

**There is nothing to install.** The `GtkSharp4` package carries the native GTK 4 libraries for every
supported OS as runtime assets, so `dotnet restore` alone leaves a Windows, Linux or macOS machine
ready to run — no MSYS2, `apt`, `brew` or `gvsbuild` step. On Linux a *display* is still needed
(X11/Wayland, or Xvfb for a headless run); macOS renders via the Quartz GDK backend.

## GtkSharp4

The GTK 4 binding is a hard fork of GtkSharp; the native GTK 4 runtime ships with it. The version is
floated `4.22.4.*` via `$(GtkSharp4Version)` in `Directory.Build.props`. For hermetic/offline restore
the wrapper `nupkg`s are vendored under `build/gtksharp4-packages/` (feed `GtkSharp4Vendored` in
`nuget.config`); refresh them with `build/update-gtksharp4-packages.ps1`.

## Getting started

See [`docs/article-wpf-to-gtk4-forms.md`](docs/article-wpf-to-gtk4-forms.md) for a full
WPF-to-Gtk4.Wpf port walkthrough, and the `Gtk4.WpfSample` project for a runnable,
**unmodified-WPF** app that compiles through the layer:

```csharp
// App.xaml.cs — nothing GTK-specific; the [STAThread] Main is generated from App.xaml.
namespace WpfSample
{
    public partial class App : System.Windows.Application
    {
    }
}
```

## Coding style

.NET Foundation style with the project's exceptions (`.editorconfig` is authoritative):

- **Hard tabs**, not spaces, in `.cs` files.
- **Never write `private`** — it is the C# default.
- Lines up to ~120 characters; Allman braces; `var` only when the type is apparent.
- Fields camelCase, private/internal fields prefixed `_`; constants PascalCase.

## Contributing

- [How to Contribute](.github/CONTRIBUTING.md)
- Bugs: [GitHub Issues](https://github.com/pieroviano/Gtk4.Wpf/issues)
