# Plan — `Gtk4.Forms.Base`: add one package, run an existing Xamarin.Forms app on GTK 4

**Goal.** A consumer adds a single `PackageReference` to **`Gtk4.Forms.Base`** and their existing
Xamarin.Forms project builds and runs on the GTK 4 backend — **no edits to controls, XAML, `using`s
or namespaces**. The package rewrites the consumer `.csproj` and injects a GTK entry point.

The enabler already exists: this fork keeps the **assembly names and namespaces as `Xamarin.Forms.*`**
(only dirs/projects/packages are `Gtk4.Forms.*`). So `Xamarin.Forms.Core.dll`, `Xamarin.Forms.Xaml.dll`
etc. from the `Gtk4.Forms.*` packages are **binary drop-ins** for the official ones, and the consumer's
`using Xamarin.Forms;` + XAML bind unchanged. Only the `.csproj` needs rewiring.

Modelled on `C:\Temp\GtkWindowsForms` — `ResxProcessor --convert-project` +
`Gtk.Windows.Forms.Base.targets` (`Gtk.Windows.Forms.Base-Targets-Plan.md`).

## Decisions (answered — do not re-ask)

| Topic | Decision |
|---|---|
| Conversion outcome | **Convert the project in place into a runnable cross-platform GTK exe** (single project) |
| Entry point | **Inject a default GTK host** — reflection finds the `Xamarin.Forms.Application` subclass; zero consumer code |
| csproj rewrite scope | **Refs + TFMs + Exe** — strip official `Xamarin.Forms*` refs, collapse platform TFMs → one `net10.0`, set `OutputType=Exe` |
| Meta-package name | **`Gtk4.Forms.Base`** |
| Native GTK 4 runtime | **Out of scope** — ships with the GtkSharp 4 nuget package (nuget.org), restored automatically on all OSes |

## ⛔ Blocking verification — settle before any code

| # | Check | Why it blocks | How to verify |
|---|---|---|---|
| B1 | The `Gtk4.Forms.*` packages' **assembly names** are exactly `Xamarin.Forms.Core/Xaml/Platform.Gtk4/…` | The whole "no code change" premise is binary drop-in. If an id differs, the consumer's compiled refs break | `<AssemblyName>` in each csproj (Core `Xamarin.Forms.Core`, renderer `Xamarin.Forms.Platform.Gtk4`) — already pinned |
| B2 | Official `Xamarin.Forms` package and `Gtk4.Forms.*` **cannot coexist** — both emit `Xamarin.Forms.Core.dll` | If the converter does not strip the official refs, restore pulls two packages with the same assembly identity → MSB3277/NU1107 | Add both to a scratch project, restore, observe the conflict. Confirms the strip is mandatory, not cosmetic |
| B3 | `Gtk4.Forms.Build.Tasks` build assets flow transitively so consumer XAML still compiles | A converted app with `.xaml` needs `XamlGTask`/`XamlCTask` + `InitializeComponent` codegen | Build a scratch app with a `.xaml` page referencing `Gtk4.Forms.Base`; confirm XamlC runs (`_XFBuildTasksLocation` resolves) |
| B4 | A build `<Target>` **cannot** change the TFM of the build in progress | Determines the UX: conversion is **one-time** (first build rewrites, second is clean). A project whose *first* eval fails (e.g. `net*-android` on a box with no Android workload) must run the tool once by hand | Inherited constraint, proven in the WinForms plan (`Gtk.Windows.Forms.Base-Targets-Plan.md` §"Blocking decision") |

**Consequence of B4**: netstandard2.0 shared UI libraries (the common target) convert cleanly on first
build — netstandard restores everywhere. Platform-headed single projects (`net*-android`) may need one
manual `--convert-project` run before the first restore. Document, don't fight it.

## What ships (4 pieces)

```
Gtk4.Forms.Base            (new, packable)  → the meta-package the consumer adds
  ├─ depends on            Gtk4.Forms.Core, Gtk4.Forms.Xaml, Gtk4.Forms.Platform.Gtk, Gtk4.Forms.Build.Tasks
  ├─ build/ + buildTransitive/  Gtk4.Forms.Base.props, Gtk4.Forms.Base.targets   (auto-imported by NuGet)
  ├─ tools/                Gtk4.Forms.ProjectConverter.dll                        (csproj rewriter)
  └─ content/              GtkEntryPoint.g.cs                                      (injected Main)
Gtk4.Forms.Host           (new, netstandard2.0)  → GtkHost.Run(): the reflection-based launcher
Gtk4.Forms.ProjectConverter (new, net8.0 console) → CsprojConverter.Transform (pure) + --convert-project
Gtk4.Forms.ProjectConverter.Tests (new, xUnit v3 net10.0) → transform unit tests
```

## Component 1 — `Gtk4.Forms.ProjectConverter` (the csproj rewriter)

New console project, adapted from `C:\Temp\GtkWindowsForms\ResxProcessor\CsprojConverter.cs` (pure,
idempotent, minimal-diff, BOM-preserving regex transform) and `ResxProcessor\Program.cs:36`
(`--convert-project` mode, **no instance guard** so `dotnet <dll>` cannot deadlock).

`CsprojConverter.Transform(string content, out bool changed)` applies, in order:

| Step | Rule | Notes |
|---|---|---|
| TFM collapse | In `<TargetFramework(s)>`, drop monikers `-android`, `-ios`, `-maccatalyst`, `-windows[…]`, `uap10.0*`, `monoandroid*`, `xamarin.ios*`; map `netstandard2.\d` → `net10.0`; dedupe; collapse list to single `net10.0` | Reuse the WinForms `WindowsMoniker`/split-dedupe shape; add the extra monikers |
| Strip clashing refs | Remove `<PackageReference Include="Xamarin.Forms" …/>`, `Xamarin.Forms.Maps`, `Xamarin.Forms.Visual.Material`, and any `Xamarin.Forms.Platform.*` line (whole line incl. children) | **B2** — else duplicate `Xamarin.Forms.*.dll`. Regex removes the element whether self-closed or with a `</PackageReference>` body |
| Output type | Ensure `<OutputType>Exe</OutputType>` (replace `Library`/absent). Add `<StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>` | StartupObject makes the injected Main unambiguous even if the project already has one (avoids CS0017) |
| Idempotent | If TFM already `net10.0`, OutputType `Exe`, no `Xamarin.Forms*` refs → `changed=false`, byte-for-byte unchanged | Second build is a no-op; `--convert-project` logs "Already converted" |

`Program.Main`: `--convert-project <csproj>` dispatched first, validates the path, calls
`CsprojConverter.TryConvertFile`, prints `Converted:`/`Already converted:`, returns 0. (A `--help`/no-arg
path prints usage.) No resx handling — Xamarin.Forms uses `.xaml`, not the WinForms `.resx` convention.

## Component 2 — `Gtk4.Forms.Host` (the injected default host)

New `netstandard2.0` library, referenced by `Gtk4.Forms.Base`. Templatised from the gallery entry point
`Gtk4.Forms.ControlGallery/Program.cs:16-37`, using the confirmed API:
`Gtk4.Forms.Platform.Gtk/Forms.cs:31` `Forms.Init(IEnumerable<Assembly>)`,
`FormsWindow.cs:8/35/49`, `GtkThemes.cs:23`, base type `Gtk4.Forms.Core/Application.cs:12`.

```csharp
namespace Xamarin.Forms.Platform.GTK   // namespace stays Xamarin.Forms.*; type lives in the Gtk4.Forms.Host assembly
{
    public static class GtkHost
    {
        public static int Run(string[] args)
        {
            GtkThemes.Init();
            Gtk.Application.Init();
            Forms.Init(RendererAssemblies());              // Platform.Gtk4 + Maps.GTK if present
            var app = ResolveApplication();                // reflect the Application subclass
            var window = new FormsWindow();
            window.LoadApplication(app);
            window.SetApplicationTitle(
                System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "Xamarin.Forms");
            window.Show();
            Gtk.Application.Run();
            return 0;
        }
        // ResolveApplication(): scan GetEntryAssembly() types for a concrete Xamarin.Forms.Application
        // subclass with a public parameterless ctor. 0 → throw with guidance; 1 → use it; >1 → require
        // [assembly: GtkMainApplication(typeof(App))] (new opt-in attribute) else prefer the type named "App".
        // RendererAssemblies(): Assembly.Load("Xamarin.Forms.Platform.Gtk4") + optional "Xamarin.Forms.Maps.GTK".
    }
}
```

The injected **entry point** is a packed source file `content/GtkEntryPoint.g.cs`, added to the
consumer compile by `Base.props` (so there is a real `Main`; the consumer wrote none):

```csharp
// <auto-generated/> injected by Gtk4.Forms.Base
namespace Gtk4.Forms.Host
{
    internal static class GtkEntryPoint
    {
        [System.STAThread]
        static int Main(string[] args) => global::Xamarin.Forms.Platform.GTK.GtkHost.Run(args);
    }
}
```

## Component 3 — `Gtk4.Forms.Base.props` (auto-imported, consumer-visible at eval)

Named `$(PackageId).props` so NuGet auto-imports it; packed to `build/` **and** `buildTransitive/`
(mirrors `Gtk.Windows.Forms.csproj:97`).

```xml
<Project>
  <PropertyGroup>
    <!-- opt-outs -->
    <SkipGtkConvert Condition="'$(SkipGtkConvert)'==''">false</SkipGtkConvert>
    <SkipGtkHost    Condition="'$(SkipGtkHost)'==''">false</SkipGtkHost>
  </PropertyGroup>
  <ItemGroup Condition="'$(SkipGtkHost)' != 'true'">
    <Compile Include="$(MSBuildThisFileDirectory)../content/GtkEntryPoint.g.cs"
             Link="GtkEntryPoint.g.cs" />
  </ItemGroup>
</Project>
```

(No global `<Using>` needed — unlike the WinForms base, the Xamarin.Forms namespaces are already the
consumer's own `using`s.)

## Component 4 — `Gtk4.Forms.Base.targets` (runs the converter)

Named `$(PackageId).targets`, packed to `build/` + `buildTransitive/`. One target, same shape as
`Gtk.Windows.Forms.Base.targets`'s `GtkConvertProject`:

```xml
<Project>
  <Target Name="GtkConvertProject" BeforeTargets="BeforeBuild"
          Condition="'$(DesignTimeBuild)' != 'true' and '$(SkipGtkConvert)' != 'true'">
    <!-- dotnet <dll> (RID-agnostic, convert mode has no instance guard); no '--' in XML comments (MSB4025) -->
    <Exec Command='dotnet "$(MSBuildThisFileDirectory)../tools/Gtk4.Forms.ProjectConverter.dll" --convert-project "$(MSBuildProjectFullPath)"' />
  </Target>
</Project>
```

`BeforeBuild`, not `Restore`: a package's `build/*.targets` is imported only *after* the first restore
(via `nuget.g.targets`), so a `Restore` hook never fires on first add. `BeforeBuild` is the earliest
reliably-imported consumer hook.

## Component 5 — `Gtk4.Forms.Base.csproj` (the meta-package)

New project under `Gtk4.Forms.Base/`. Follows the repo's packing conventions
(`Gtk4.Forms.Platform.Gtk/Gtk4.Forms.Platform.Gtk.csproj`): `GeneratePackageOnBuild=True`,
`PackageOutputPath=$(XFPackageOutputPath)`, `Version=$(VersionPrefix).$(VersionSuffix)`,
`PackageId=Gtk4.Forms.Base`.

```xml
<ItemGroup><!-- nuspec deps (flow Build assets for XAML) -->
  <ProjectReference Include="..\Gtk4.Forms.Core\Gtk4.Forms.Core.csproj" />
  <ProjectReference Include="..\Gtk4.Forms.Xaml\Gtk4.Forms.Xaml.csproj" />
  <ProjectReference Include="..\Gtk4.Forms.Platform.Gtk\Gtk4.Forms.Platform.Gtk.csproj" />
  <ProjectReference Include="..\Gtk4.Forms.Build.Tasks\Gtk4.Forms.Build.Tasks.csproj" />
  <ProjectReference Include="..\Gtk4.Forms.Host\Gtk4.Forms.Host.csproj" />
  <!-- Maps is optional; consumers add Gtk4.Forms.Maps.Gtk themselves if they use Map -->
</ItemGroup>
<ItemGroup><!-- pack the wiring + tool + injected host -->
  <None Include="Gtk4.Forms.Base.props"   Pack="true" PackagePath="\build\;\buildTransitive\" />
  <None Include="Gtk4.Forms.Base.targets" Pack="true" PackagePath="\build\;\buildTransitive\" />
  <None Include="content\GtkEntryPoint.g.cs" Pack="true" PackagePath="\content\" />
  <None Include="..\Gtk4.Forms.ProjectConverter\bin\$(Configuration)\net8.0\**"
        Pack="true" PackagePath="\tools\" />
</ItemGroup>
```

Build-order note: `Gtk4.Forms.ProjectConverter` must build before `Gtk4.Forms.Base` packs (so `tools/`
is populated) — a `ProjectReference … ReferenceOutputAssembly="false"` or an explicit solution
dependency. Add all new projects to `Gtk4.Forms.sln`.

## What happens when a consumer adds the package

1. `dotnet add package Gtk4.Forms.Base` → restore brings Core/Xaml/Platform.Gtk/Build.Tasks/Host and
   their **native GTK 4 runtime via GtkSharp 4**.
2. First `dotnet build`: `Base.targets` runs the converter → `.csproj` rewritten (TFM→`net10.0`, refs
   stripped, `OutputType=Exe`, `StartupObject` set). That build may still fail/partial (old eval).
3. Second `dotnet build` (or the same build if the project was already `net*`): clean. `Base.props`
   adds `GtkEntryPoint.g.cs`; the app compiles to an exe whose `Main` calls `GtkHost.Run`, which
   reflects out the consumer's `App : Application` and runs it on GTK 4.
4. `dotnet run` → the app window appears. Zero source edits.

## Files

| File | Change |
|---|---|
| `Gtk4.Forms.ProjectConverter/CsprojConverter.cs` | **new** — pure `Transform` (adapt `GtkWindowsForms\ResxProcessor\CsprojConverter.cs`) |
| `Gtk4.Forms.ProjectConverter/Program.cs` | **new** — `--convert-project` dispatch (adapt `ResxProcessor\Program.cs:36`) |
| `Gtk4.Forms.ProjectConverter/Gtk4.Forms.ProjectConverter.csproj` | **new** — net8.0 console, `InternalsVisibleTo` the Tests |
| `Gtk4.Forms.ProjectConverter.Tests/…` | **new** — xUnit v3 (repo stack); transform cases below |
| `Gtk4.Forms.Host/GtkHost.cs` (+ `GtkMainApplicationAttribute.cs`) | **new** — netstandard2.0 launcher + opt-in app selector |
| `Gtk4.Forms.Base/Gtk4.Forms.Base.csproj` | **new** — meta-package |
| `Gtk4.Forms.Base/Gtk4.Forms.Base.props` / `.targets` | **new** — git-tracked, packed |
| `Gtk4.Forms.Base/content/GtkEntryPoint.g.cs` | **new** — injected Main |
| `Gtk4.Forms.sln` | add the 4 new projects |
| `docs/article-gtk4-forms-gtk.md` | add a "Drop-in conversion: add one package" section |
| `CLAUDE.md` | document `Gtk4.Forms.Base` + the one-time-conversion UX |

Every new source/test file ≤ 200 lines (per repo rule); split the converter by behaviour if needed.

## Verification

| Step | Check |
|---|---|
| Unit (converter) | TFM strip (android/ios/windows/uap), `netstandard2.0`→`net10.0`, multi-TFM collapse+dedupe, `Xamarin.Forms`/`Xamarin.Forms.Maps` ref removal (self-closed + bodied), OutputType/StartupObject insert, **idempotent no-op**, BOM preserved, minimal diff |
| CLI e2e | `dotnet run -- --convert-project <tmp.csproj>` on a captured real XF shared-lib csproj → expected diff; re-run → "Already converted" |
| Scratch consumer (netstandard2.0 lib) | App.xaml + a ContentPage + `App:Application`; add `Gtk4.Forms.Base`; `dotnet build` ×2 → exe; `dotnet run` shows the window on Windows and (Xvfb) Linux |
| B2 regression | Confirm the scratch consumer has **no** duplicate-assembly warning after the official `Xamarin.Forms` ref is stripped |
| Repo suites | `Gtk4.Forms.sln` still builds; Core/XAML/renderer suites green; new ProjectConverter.Tests green |

A failing fix needs a test that fails without it (repo rule): the converter is a pure function — pin
every transform rule in `CsprojConverterTests`.

## Risks

| Risk | Mitigation |
|---|---|
| First-build-too-late (B4) | One-time, idempotent; `SkipGtkConvert` opt-out; documented manual `--convert-project` for projects whose first eval fails |
| Rewriting the consumer's csproj is surprising/destructive | Minimal diff, idempotent, BOM-preserving, clear `Converted:` log line, opt-out property |
| Duplicate `Xamarin.Forms.*` assemblies (B2) | Converter strips the official refs; scratch-consumer test asserts no conflict |
| Multiple `Main` → CS0017 | `StartupObject=Gtk4.Forms.Host.GtkEntryPoint` forces ours; `SkipGtkHost` for consumers keeping their own |
| App discovery ambiguous (0 or >1 `Application` subclass) | Clear throw with guidance; `[assembly: GtkMainApplication(typeof(App))]` opt-in; prefer type named `App` |
| Tool not cross-platform / deadlock | Invoke the packed `tools/…dll` via `dotnet` (RID-agnostic); convert mode runs no instance guard |
| XAML won't compile at consumer (B3) | Meta-package depends on `Gtk4.Forms.Build.Tasks`; its build assets flow; verified by the scratch consumer |

## Follow-ups / open

1. `<UseMaui>`/`<EnableDefaultXamlItems>` and platform-only MSBuild items (Android `Resource`, iOS
   `BundleResource`) — strip too, or leave (they are inert once the TFM is `net10.0`)? Default: **leave**
   (minimal diff) and revisit if a real consumer trips.
2. Maps: auto-add `Gtk4.Forms.Maps.Gtk` when a stripped `Xamarin.Forms.Maps` ref is seen, or leave it to
   the consumer? Default: **leave** (keeps the package lean).
3. Title/icon: host uses the entry assembly name; expose `[assembly: GtkMainApplication(Title=…, Icon=…)]`
   if consumers want control.
