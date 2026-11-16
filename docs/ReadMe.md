# Gtk4.Forms #

A maintenance fork of **Xamarin.Forms 5.0.0**, pruned to the **GTK leg only** and retargeted at
GTK 4 and the modern .NET SDK. It keeps a Forms XAML codebase runnable as a native desktop app on
**Windows, Linux and macOS** — the one target .NET MAUI does not cover.

Origin: [`pieroviano/Xamarin.Forms`](https://github.com/pieroviano/Xamarin.Forms), branch `5.0.0`
(the default and PR-target branch). Upstream: [`xamarin/Xamarin.Forms`](https://github.com/xamarin/Xamarin.Forms) (MIT).

## Support

Xamarin.Forms reached end of support on **May 1, 2024** ([Xamarin Support Policy][xamarin-support-policy])
and was succeeded by [.NET MAUI][dotnet-maui-repo]. For new cross-platform mobile/desktop work,
follow the [official upgrade guidance](https://learn.microsoft.com/dotnet/maui/migration). This
fork exists only to keep the **GTK desktop** backend — which MAUI has no equivalent for — alive on
a supported SDK.

[xamarin-support-policy]: https://dotnet.microsoft.com/platform/support/policy/xamarin
[dotnet-maui-repo]: https://github.com/dotnet/maui/

## The rename (read this first)

Directories, project files, the solution and the **NuGet package ids** were renamed
`Xamarin.Forms.*` → `Gtk4.Forms.*`. The **assembly names, code namespaces and `InternalsVisibleTo`
strings were deliberately kept as `Xamarin.Forms.*`** — binding compatibility and the string-based
renderer resolution depend on them.

So a project lives in `Gtk4.Forms.Platform.Gtk/`, ships as NuGet package `Gtk4.Forms.Platform.Gtk`,
yet compiles to `Xamarin.Forms.Platform.Gtk4.dll` with `namespace Xamarin.Forms.Platform.GTK`.

| What | Renamed to `Gtk4.Forms.*` | Kept as `Xamarin.Forms.*` |
| --- | --- | --- |
| Directories | ✅ | — |
| Project files (`.csproj`) | ✅ | — |
| Solution | ✅ (`Gtk4.Forms.sln`) | — |
| NuGet package ids | ✅ | — |
| Assembly names (`.dll`) | — | ✅ |
| Code namespaces | — | ✅ |
| `InternalsVisibleTo` strings | — | ✅ |

One folder keeps its old name on purpose: the shared project under
`Gtk4.Forms.Controls.Issues/Xamarin.Forms.Controls.Issues.Shared/`.

## Packages

Published under the `Gtk4.Forms.` prefix. The GTK backend binds **GtkSharp 4 (GTK 4.22)**, published
on nuget.org, which also carries the native GTK 4 runtime (see [GtkSharp4](#gtksharp4) below).

| Package id | Assembly it builds | Role |
| --- | --- | --- |
| `Gtk4.Forms.Core` | `Xamarin.Forms.Core.dll` | Platform-independent core (bindings, layout, Shell, styles) |
| `Gtk4.Forms.Xaml` | `Xamarin.Forms.Xaml.dll` | Runtime XAML loader/parser |
| `Gtk4.Forms.Build.Tasks` | `Xamarin.Forms.Build.Tasks.dll` | Build-time XAML tasks (`XamlGTask`, `XamlCTask`) |
| `Gtk4.Forms.Platform.Gtk` | `Xamarin.Forms.Platform.Gtk4.dll` | GTK 4 renderers + platform services |
| `Gtk4.Forms.Platform.Base` | `Xamarin.Forms.Platform.dll` | Default-renderer markers Core references |
| `Gtk4.Forms.Maps.Base` | `Xamarin.Forms.Maps.dll` | Maps abstraction |
| `Gtk4.Forms.Maps.Gtk` | `Xamarin.Forms.Maps.Gtk.dll` | Cairo slippy-map implementation (no API key) |
| `Gtk4.Forms.CustomAttributes` | `Xamarin.Forms.CustomAttributes.dll` | Test attributes (`[Issue]`, `[Preserve]`) |

`Gtk4.Forms.Platform.Gtk` pulls in `Gtk4.Forms.Core` and depends on `GtkSharp4`.

## Build

Plain `dotnet` / MSBuild — there is **no Cake, no `build.cmd`, no `build.ps1`, no provisioning
script**; those were deleted with the non-GTK platforms. `global.json` pins .NET SDK **10.0.100**
(`rollForward: latestFeature`).

```powershell
dotnet build Gtk4.Forms.sln                 # whole solution
dotnet build Gtk4.Forms.sln -c Release
```

`Gtk4.Forms.sln` is the only solution in the tree. Library projects target `netstandard2.0`; the
three test projects and the gallery target `net10.0`.

**Ordering:** `Gtk4.Forms.Build.Tasks` supplies the XAML MSBuild tasks and must be built before
anything that consumes XAML. Building the solution handles this for you; a project-at-a-time build
after a `bin`/`obj` wipe does not.

Build-wide settings live in `Directory.Build.props` / `Environment.Build.props`:
`TreatWarningsAsErrors=true` everywhere, `LangVersion` 8.0 (tests use `latest`), and
`UseOSSpecificOutputPaths=true` (redirects to `bin-wsl/` / `obj-wsl/` so a Windows and a WSL build
of the same tree do not fight over `obj/`).

## Tests

Three suites, all **xUnit v3 on `net10.0`**, run with `dotnet test`:

| Project | Approx. tests | Covers |
| --- | --- | --- |
| `Gtk4.Forms.Core.UnitTests` | ~4,856 | Platform-independent core |
| `Gtk4.Forms.Xaml.UnitTests` | ~1,029 | XAML loader + XamlC compiler |
| `Gtk4.Forms.Platform.Gtk.UnitTests` | ~191 (+3 skip) | **Real GTK 4 widgets** |

```powershell
dotnet test Gtk4.Forms.sln
```

The GTK suite drives real GTK 4 windows. On **Windows** it works directly against the GDK Win32
backend. On **Linux** it needs an X server:

```bash
xvfb-run -a --server-args="-screen 0 1280x1024x24" env GDK_BACKEND=x11 \
  dotnet test Gtk4.Forms.Platform.Gtk.UnitTests
```

CI is `.github/workflows/build-and-test.yml` (Windows, Linux, macOS); per-OS logic lives in
`scripts/ci/test-{windows,linux,macos}`.

## Native GTK 4 runtime

**There is nothing to install.** The `GtkSharp4` package carries the native GTK 4 libraries for every
supported OS as runtime assets, so `dotnet restore` alone leaves a Windows, Linux or macOS machine
ready to run — no MSYS2, `apt`, `brew` or `gvsbuild` step. On Linux a *display* is still needed
(X11/Wayland, or Xvfb for a headless run); macOS renders via the Quartz GDK backend.

## GtkSharp4

The GTK 4 binding is a hard fork of GtkSharp, published on **nuget.org**; the native GTK 4 runtime
ships with it. The version is floated `4.22.4.*` via `$(GtkSharp4Version)` in `Directory.Build.props`.
For hermetic/offline restore the wrapper `nupkg`s are also vendored under `build/gtksharp4-packages/`
(feed `GtkSharp4Vendored` in `nuget.config`); refresh them with `build/update-gtksharp4-packages.ps1`.

## Getting started

See [`docs/article-wpf-to-gtk4-forms.md`](docs/article-wpf-to-gtk4-forms.md) for a full
WPF-to-Gtk4.Forms port walkthrough, [`docs/article-gtk4-forms-gtk.md`](docs/article-gtk4-forms-gtk.md)
for the backend architecture, and the `Gtk4.Forms.ControlGallery` project for a runnable app. The
entry point is small:

```csharp
[STAThread]
static void Main()
{
    GtkThemes.Init();
    Gtk.Application.Init();
    Forms.Init();

    var window = new FormsWindow();
    window.LoadApplication(new App());
    window.SetApplicationTitle("My App");
    window.Show();

    Gtk.Application.Run();
}
```

## Coding style

.NET Foundation style with the project's exceptions (`.editorconfig` is authoritative):

- **Hard tabs**, not spaces, in `.cs` files.
- **Never write `private`** — it is the C# default (much inherited GTK code does; leave it alone).
- Lines up to ~120 characters; Allman braces; `var` only when the type is apparent.
- Fields camelCase, private/internal fields prefixed `_`; constants PascalCase.

## Contributing

- [How to Contribute](.github/CONTRIBUTING.md)
- Bugs: [GitHub Issues](https://github.com/pieroviano/Xamarin.Forms/issues)
