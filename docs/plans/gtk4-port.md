# Port Xamarin.Forms GTK backend from GtkSharp 3 to GtkSharp 4 (GTK 4.22)

## Decisions (from the requester)

| # | Question | Decision |
|---|---|---|
| 1 | How to reference GtkSharp4 | Shared local NuGet feed. `Packages` is a symlink to `C:\Temp\gtkwindowsforms\Packages`; rebuild GtkSharp4 → its packages land in that feed → XF restores them. |
| 2 | Replace or parallel | **Replace GTK3 in place.** Rename Gtk3 → Gtk4 (assembly, package id). GTK3 support dropped. Matches the already-renamed `Xamarin.Forms.Gtk4.sln`. |
| 3 | Done bar | All three xUnit suites green on **real GTK 4** widgets **and** `ControlGallery.GTK` launches and renders. |
| 4 | Idiomatic vs compat | **Idiomatic GTK 4 end state**, retiring the fork's GTK3-compat shims. Bootstrap through the shims for an early running baseline, then rewrite family by family. |

## Blocking verification — do these BEFORE writing renderer code

1. **Publish GtkSharp4 to the shared feed.** It does not exist there yet (`C:\Temp\gtkwindowsforms\Packages` holds only `Gtk.Drawing`, `Gtk.Resources.Extensions`). GtkSharp4 packs to its own `BuildOutput/NugetPackages` (`C:\Temp\GtkSharp\build.cake:194`), package ids are project-name with `Sharp`→`Sharp4` (`Source/Libs/Directory.Build.props:16`), e.g. `GtkSharp4`, `GdkSharp4`, `GLibSharp4`, `CairoSharp4`, `GrapheneSharp4`, `GskSharp4`, `PangoSharp4`, `GioSharp4`. Either set GtkSharp4's `PackageOutputPath` to the shared feed, or copy the nupkgs there after `dotnet cake build.cake`. Confirm `GtkSharp4` restores into a throwaway `net10.0` console app before touching XF.
2. **Confirm a `netstandard2.0` library can consume `GtkSharp4`.** The bindings are `netstandard2.0` (`C:\Temp\GtkSharp\CLAUDE.md`, "wrappers build for netstandard2.0 only"), so this should work, but the package also ships `GtkSharp4.targets` (gvsbuild GTK 4 download). Verify the target flows transitively to a `netstandard2.0` consumer, or that XF's own `net10.0` test/gallery projects trigger it.
3. **Confirm the native GTK 4 runtime install path on this machine.** `GtkSharp.targets` downloads a gvsbuild GTK 4 bundle to `%LOCALAPPDATA%\Gtk\4.22.4` on Windows unless `SkipGtkInstall=True`. The 169 renderer tests and the gallery need it present. Decide: let the target install it, or point at an existing install.
4. **Pin the GtkSharp4 package version string** the feed produces (`VersionPrefix.yyDDD`, `Source/Directory.Build.props:60-62`) — it is date-derived, so the XF `PackageReference` must float (`*` / a range) or be re-pinned each GtkSharp4 rebuild. Prefer a floating range on the Local feed.

## What the fork already solves (do not rebuild these)

The GtkSharp4 fork ships a deliberate GTK3→GTK4 compatibility layer under `C:\Temp\GtkSharp\Source\Libs\GtkSharp\Compat\` (and `GdkSharp\Compat\`). It lets GTK3-shaped code compile and run on GTK 4. Confirmed coverage:

| GTK3 API the XF code uses | Shim | File |
|---|---|---|
| `Gtk.Container`, `Add/Remove/Children`, `Put/Move` | `Container : Widget` (GtkFixed-style layout, reachable `OnGetPreferredWidth/Height`, `OnSizeAllocated`) | `Compat/Container.cs` |
| `Gtk.EventBox`, `VisibleWindow`, `ChildrenFillAllocation` | `EventBox : Container` | `Compat/EventBox.cs` |
| `Box.PackStart/PackEnd/ReorderChild/SetChildPacking`, `Add/Children`, `Grid.Add`, `Fixed.Add`, `Window/ScrolledWindow/Viewport/Frame/Overlay/Expander.Add` | partials | `Compat/Children.Compat.cs`, `Members.Compat.cs` |
| `OnDrawn(Cairo.Context)`, `OnSizeAllocated(Gdk.Rectangle)`, `SizeAllocated` event | on `Window/Box/Fixed/Grid/Button/Frame/DrawingArea/EventBox` | `Compat/Drawing.Compat.cs` |
| `ButtonPress/Release`, `MotionNotify`, `Enter/Leave`, `Scroll`, `KeyPress/Release`, `FocusIn/Out`, `Focused` events + their `*EventArgs`/`Gdk.Event*` | controllers created lazily on `Gtk.Widget` | `Compat/Widget.Compat.cs`, `Compat/SignalArgs.cs` |
| `ShowAll`, `NoShowAll`, `AddEvents`, `SizeAllocate(Rectangle)`, `GetPreferredWidth/Height`, `BorderWidth`, `Margin`, `Toplevel` | `Gtk.Widget` partial | `Compat/Widget.Compat.cs`, `Members.Compat.cs` |
| `Image.Pixbuf` (write-remembers), `Button.Relief/ImagePosition`, `Label.LineWrap`, `ShadowType`, `StateType` + `ToStateFlags` | partials/enums | `Members.Compat.cs`, `Compat/StateType.cs` |
| `Gtk.Application.Init/Quit/Invoke/Run/EventsPending/RunIteration` | statics | `GtkSharp/Application.cs:59-171` |
| `Gdk.Color`, old `Gdk.Event*` structs | compat | `GdkSharp/Compat/Color.cs`, `Events.cs` |

Because of this, **Stage 1 (compile + run on shims) is small.** The bulk of the effort is Stage 2 (idiomatic rewrite), which is what decision #4 asked for.

## Residue the shims do NOT cover — must fix even for Stage 1

| Gap | GTK4 reality | XF site(s) | Fix |
|---|---|---|---|
| `Gtk.Menu` / `Gtk.MenuItem` | Removed; only `Gtk.PopoverMenu` / `GLib.Menu` exist | `Cells/CellBase.cs:200-225` (`menu.PopupForDevice`, `menu.ShowAll`, per-item `ButtonPressEvent`) | Rewrite context menu as `PopoverMenu` + `GLib.Menu`/`SimpleAction`. |
| `Gdk.Screen` | Removed; use `Gdk.Display` | `GtkThemes.cs:44,51,58`, `GtkPlatformServices.cs:77`, `GtkDeviceInfo.cs:21` | `Gdk.Display.Default`; `StyleContext.AddProviderForScreen`→`AddProviderForDisplay`; monitor/geometry via `Display.Monitors`/`Surface`. |
| `Gdk.CairoHelper.SetSourcePixbuf` | Removed in GTK 4 | `Controls/ImageControl.cs:232` | Convert pixbuf→`Cairo.ImageSurface` manually, or render via `Gdk.Texture`. |
| `OnGetPreferredWidth/Height` override on a **layout-managed** widget | A `GtkBoxLayout`/`GtkFixedLayout` answers `measure` **instead of** the widget — the override is silently never called (fork note in `Compat/Drawing.Compat.cs:599-604`) | `Controls/ShellWidget.cs:91` (`ShellWidget : Gtk.Box`, overrides both at ~784-800) | Re-base off `Container`/bare `Widget` (vfuncs reachable), set an explicit size request, or install a custom `LayoutManager`. **`HostBox : Gtk.EventBox` in `CollectionViewRenderer.cs:169` is fine** — EventBox derives from bare-Widget `Container`. |
| Hand-written P/Invoke bound to GTK3 sonames | GTK 4 / WebKitGTK 6.0 | `Controls/WebKit2.cs:27,81-144` (`libgtk-3.so.0`, `gtk_widget_show_all`, `libwebkit2gtk-4.1`) | Repoint to GTK 4 (`libgtk-4`) and WebKitGTK **6.0** (`libwebkitgtk-6.0`), whose `WebKitWebView` is a GTK 4 widget; drop `gtk_widget_show_all`. Keep lazy symbol resolution so absence degrades to a placeholder (as today). |
| `Gtk.Stock` | Removed | comments only (`GtkToolbarTracker.cs`) per audit — verify none compile | none expected. |

Also audit at Stage 1:
- `Image.Pixbuf` getter now returns only the last value **set through it** (`Members.Compat.cs:1187-1210`). `Controls/ImageControl.cs` reads `_image.Pixbuf` back for transforms — ensure every read follows a set, or cache the source pixbuf in the field the file already keeps (`_original`).
- `Pango.CairoHelper.CreateLayout/ShowLayout` (`Renderers/ImageRenderer.cs:281,295`) — still present (pangocairo), but verify against the fork's `PangoSharp4`.

## Identity / build changes (decision #2 — rename in place)

- `Xamarin.Forms.Platform.GTK/Xamarin.Forms.Platform.GTK.csproj`:
  - `PackageReference GtkSharp` (v3) → `GtkSharp4` (floating version from Local feed). Remove the `GtkSharp3Version`-based reference.
  - `AssemblyName` `Xamarin.Forms.Platform.Gtk3` → `Xamarin.Forms.Platform.Gtk4`; `PackageId` `Net4x.Xamarin.Forms.Gtk3.Platform.GTK` → `…Gtk4…`; `Description` GTK3 → GTK4. **Keep `RootNamespace = Xamarin.Forms.Platform.GTK`** (namespace unchanged, mirroring the fork keeping `namespace Gtk`).
- `Directory.Build.props:93,103-104`: replace the `GtkSharp` v3 reference block and `GtkSharp3Version`/`GtkSharp3VersionBuildSuffix` with a `GtkSharp4` reference. Keep the `_XFAssert*` pack-guard targets.
- `Xamarin.Forms.Maps.GTK` csproj — same package swap + Gtk3→Gtk4 identity rename.
- Delete the stale `Xamarin.Forms.Gtk3.sln` / `.slnx` (already removed in working tree), keep `Xamarin.Forms.Gtk4.sln`. Note the Gtk4 sln still lists the project titled `Xamarin.Forms.Platform.GTK` at the GTK csproj path — unchanged path, fine.
- `nuget.config` already exposes `Local → Packages`; no change needed.
- Update `CLAUDE.md` (repo) and `Xamarin.Forms.Platform.GTK/README.md`: "GtkSharp 3 / GTK 3" → "GtkSharp 4 / GTK 4.22", SDK/runtime notes, the Xvfb note (GTK 4 still needs an X/Wayland server on Linux).

## Strategy — two stages

### Stage 1 — compile and run on the compat shims (fast baseline)
Goal: `dotnet build Xamarin.Forms.Gtk4.sln` green, the 169 renderer tests executing (not necessarily all green) on real GTK 4, gallery launching. Work: the package swap + identity rename + the six residue fixes above + port the **test harness** (below). This proves the compat layer carries the backend and gives a regression baseline for Stage 2.

### Stage 2 — idiomatic GTK 4, retiring the shims (the main effort)
Rewrite family by family; after each family, the 169 renderer tests + a gallery smoke must stay green. Suggested order (lowest-risk, highest-reuse first):

1. **App lifecycle** — `ShowAll`→`Visible`/`Present`; `Gtk.Application.Init/Run` → a real `GtkApplication` activation in `Forms.cs`/`FormsWindow.cs`/`Platform.cs`; drop `NoShowAll`. (`Platform.cs`, `FormsWindow.cs`, `GtkPlatformServices.cs`, `GtkSynchronizationContext.cs`.)
2. **Styling** — `StyleExtensions.cs` + `GtkThemes.cs`: `Gdk.Color`/`StateType`/`StyleContext` providers → `AddCssClass`/`RemoveCssClass` + per-widget `CssProvider` + `AddProviderForDisplay`. Blocks many renderers, so do early.
3. **Images** — `ImageControl.cs`, `Extensions/ImageExtensions.cs`, `Renderers/ImageRenderer.cs`, `ImageButton`, cells: `Gdk.Pixbuf`→`Gdk.Texture`/`Gdk.Paintable`, `Gtk.Image`→`Gtk.Picture` where aspect handling is wanted.
4. **Custom drawing** — `GtkFormsContainer.cs` (the base container for every element) and the shapes/frame/radiobutton/datepicker/map `OnDrawn` overrides → `OnSnapshot` + `Snapshot.AppendCairo`. Re-base `GtkFormsContainer` off a custom `Widget`/`DrawingArea`.
5. **Input** — `VisualElementTracker.cs`, `ShellWidget.cs`, `ShellTabBar.cs`, `Carousel.cs`, `CellBase.cs`, button/imagebutton renderers: `ButtonPressEvent`/`ButtonReleaseEvent` → `GestureClick` directly; scroll/motion/key → their controllers.
6. **Layout / containers** — custom containers and `OnSizeAllocated` → `LayoutManager` or `Widget` measure/allocate vfuncs with `Gsk.Transform`. Resolve the `ShellWidget : Gtk.Box` dead-measure break here properly (custom layout manager).
7. **CollectionView / ListView / Shell / FlyoutPage** — the heavy renderers; do last, on top of the now-idiomatic primitives.
8. **WebView / OpenGLView** — `WebKit2.cs` to WebKitGTK 6.0; `OpenGLView` already wraps `Gtk.GLArea` (GTK 3.16+, carries into GTK 4) — verify only.

New shared idiomatic helpers (snapshot-draw wrapper, gesture wiring, texture-from-ImageSource) live under `Xamarin.Forms.Platform.GTK/Helpers/` and `Extensions/`, matching the existing layout; each new file ≤200 lines.

## Test harness porting (gate for every stage)

`Xamarin.Forms.Platform.GTK.UnitTests/GtkTestHost` drives real widgets and encodes GTK3 behaviour (per repo `CLAUDE.md`): `Window.Resize` is WM-request-only on Win32 so tests call `Window.SizeAllocate(...)`; `Pump`/`PumpUntil`; no `async` test bodies (`GtkSynchronizationContext` deadlocks — use `Await`); never `Destroy()` a window (`Retire`). Under GTK 4:
- `SizeAllocate(Gdk.Rectangle)` survives via the shim (`Widget.Compat.cs:2052`) but idiomatically becomes `Allocate(w,h,-1,transform)` after a `Measure`. Keep the shim call in Stage 1; revisit in Stage 2 step 6.
- The pump (`Gtk.Application.EventsPending/RunIteration`) is shimmed (`Application.cs:113-123`) — keep.
- Verify the known OS split still reproduces: `CoreControlMappingTests.BoxViewPaintsItsColourAndRepaintsOnChange` (Windows pass / Xvfb fail) — re-baseline on GTK 4 before calling the suite green.
- Core (4848) and Xaml (1046) suites are platform-independent (`MockPlatformServices`); they should pass once the solution builds. Treat any new failure there as a regression from the identity/build changes, not the port.

## Verification

1. `dotnet build Xamarin.Forms.Gtk4.sln` (Debug) — zero warnings (`TreatWarningsAsErrors=true` repo-wide).
2. `dotnet test Xamarin.Forms.Core.UnitTests` and `…Xaml.UnitTests` — unchanged counts, all green.
3. `dotnet test Xamarin.Forms.Platform.GTK.UnitTests` on Windows (GDK Win32) **and** Linux (`xvfb-run -a --server-args="-screen 0 1280x1024x24" … GDK_BACKEND=x11`) — 169 green (minus the one documented Xvfb BoxView split, re-confirmed).
4. Launch `Xamarin.Forms.ControlGallery.GTK`; smoke the core galleries (layouts, images, lists/CollectionView, Shell/FlyoutPage, buttons/entries). Capture before/after screenshots per family rewritten in Stage 2.
5. Release build sanity (`-c Release`) — the `#if !SIGNED_ASSEMBLY` InternalsVisibleTo note in repo `CLAUDE.md` still holds.
6. Every bug a rewrite exposes ships with a renderer unit test that fails pre-fix / passes post-fix (repo rule).

## Risks

| Risk | Impact | Mitigation |
|---|---|---|
| GtkSharp4 not yet in the shared feed; version is date-derived | Restore fails or drifts | Blocking step 1 + floating version range on the Local feed. |
| `GtkSharp4.targets` (gvsbuild download) may not flow to a `netstandard2.0` consumer | No native GTK 4 at runtime | Blocking steps 2-3; if needed, add the runtime install to the `net10.0` test/gallery projects instead. |
| Dead `OnMeasure`/`OnGetPreferred*` on layout-managed subclasses is **silent** (renders tiny, not an error) | Shell/other sizing wrong with no build signal | Audit every `class … : Gtk.(Box|Grid|Fixed)` overriding measure (found: `ShellWidget`); prefer `Container`/custom `LayoutManager`. |
| WebKitGTK 6.0 API/soname differs from 4.1 | WebView broken | Keep lazy resolution + placeholder fallback already in `WebKit2.cs`; gate on symbol availability. |
| Snapshot vs Cairo clipping differences | Over-paint / missing paint regressions (the `GtkFormsContainer` over-paint note at `GtkFormsContainer.cs:54-63` is exactly this class of bug) | `AppendCairo(bounds)` clips to the widget; port the existing clip reasoning, keep the renderer tests that pin it. |
| Idiomatic rewrite is large (26k LOC, 62 GTK-touching files) | Schedule | Stage 1 gives a working shim-backed baseline early; Stage 2 is incremental and independently verifiable per family. |

## Key references

- Renderer backend: `Xamarin.Forms.Platform.GTK/` (145 files, ~26k LOC). Maps: `Xamarin.Forms.Maps.GTK/`. Sample: `Xamarin.Forms.ControlGallery.GTK`. Tests: `Xamarin.Forms.Platform.GTK.UnitTests/`.
- Fork compat layer: `C:\Temp\GtkSharp\Source\Libs\GtkSharp\Compat\`, `…\GdkSharp\Compat\`. Samples (idiomatic GTK 4 patterns): `C:\Temp\GtkSharp\Source\Samples\`. Fork guidance: `C:\Temp\GtkSharp\CLAUDE.md`.
- Feed: `C:\Temp\Xamarin.Forms\Packages` → `C:\Temp\gtkwindowsforms\Packages`. GtkSharp4 pack output: `C:\Temp\GtkSharp\BuildOutput\NugetPackages`.
</content>
</invoke>
