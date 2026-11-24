# WPF samples coverage

Measured 2026-11-23 on `main` at `17c641677c8b7793e669f8c87c4711bb9e56cd15` (Rounds 59–61, the third three-lane batch:
lane A controls and freezable tail III plus the .NET 9 WPF surface; lane B the documents/imaging tail, re-applied from
`gf57b`, plus the binding engine's non-element targets; lane C startup and generator sweep III; each with its review
fixes and follow-ups). The package is `Gtk4.Forms.Wpf 1.0.0.26327` from a **pinned** local feed. The samples are the
official `microsoft/WPF-Samples` at `00f39dbb` (256 projects: 243 apps, 9 libraries and 4 native C++ projects). All
252 csproj were rebuilt and every built app was smoke-run. Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report `4fd5749d` (main
`85493c8b`, Rounds 56–58), under `previous`.

## Headline

**198 of 204 portable official WPF sample apps build unmodified, and 191 run.** That is up from 191 / 182 in report
`4fd5749d` (+7 build, +9 run), and from 0 / 0 in report `fedd13c8` (2026-11-19).

- **7 apps moved fail→pass**: 5 run and 2 crash at startup. **4 built apps moved crash→runs** (Brushes, HexSphereDemo,
  PhotoFlipperDemo, AnnotatedDocumentViewer), all four that the Round 58 report put on generator sweep III.
- **No regressions.** No project went pass→fail, and no app went runs→crash/hang.
- **No hangs.** 7 apps build but do not run, and all 7 crash. 2 are new builds that hit a second wall (PhotoStoreDemo,
  SlidePuzzleDemo). AnimationExamples crashes further in. The other 4 are known and are deliberate, out of scope or
  defects in the sample.
- **6 portable apps still fail to build.** Three of them (DataBindingDemo, WPFGallery, ExpenseItDemo) now fail only or
  mainly on `AutomationProperties` attached properties. One is a one-line packaging fix (EditingExaminerDemo), and two
  are System.Xaml/BAML tooling that is deliberately not next.
- 204 projects build in all: the 198 portable apps, the libraries CustomApplicationClient, wpfclock,
  HostingWpfUserControlInWf, WPFControls and EditBoxControlLibrary, and the Windows-only console app FocusTracker.
- By lane:
  - Round 60 lane B (documents/imaging tail, `8c49c8e7`): 6 fail→pass (DocumentMerge, PhotoViewerDemo, ParticlesDemo,
    PhotoStoreDemo, and with lane A FontDialogDemo and SlidePuzzleDemo). The binding-engine half (`64479593`) moved
    AnimationExamples past its Round 58 crash.
  - Round 61 lane C (generator sweep III, `414525fe`): Per-FrameAnimation fail→pass and all 4 crash→runs. It also
    removed WPFGallery's Grid shorthand and `ControlExample` content errors.
  - Round 59 lane A (controls/freezable tail III `e6d32fbe`, .NET 9 surface `997d7515`): with lane B, FontDialogDemo and
    SlidePuzzleDemo. It removed WPFGallery's whole .NET 9 wall (about 25 members) and ExpenseItDemo's `DataGridCell`
    wall. Both apps still fail, on the next wall.

### Corrections to the raw batch data

The final build log of each of the 252 csproj was re-parsed uniformly. Run results come from the batches' per-app run
records. Every crash and every app that moved was re-run alone (see Method). The top 15 missing symbols were checked
against the `Gtk4.Forms.*` source.

- **The aggregated totals were wrong.** They said 208 projects, 175 portable apps, 170 build and 165 run. The
  per-project data gives 252 / 204 / 198 / 191.
  - **Batch 5 was missing from the aggregate** (its batch note is empty). Its 44 csproj (Globalization and
    Localization, Windows, Accessibility, Element Tree, PerMonitorDPI) all have final logs and run records under
    `samples-remeasure-r61/batch5`. They were parsed like every other batch: 29 portable apps, 29 build, 28 run
    (LocalizationResources crashes, as before).
  - The aggregate again used the batches' own regex `windowsOnly` flags. As in every earlier report, the verified
    classification is carried over unchanged, so the denominator stays 204.
- **AppCompat-Quirks is a crash, not a run.** Batch 4's runner stops at the first window, and this app shows its
  window at 0.5 s and then exits on its known `NullReferenceException`. A solo 20 s re-run confirmed the crash.
- **Batch 3's note undercounted.** It reported 17 builds (13 apps) and 14 smoke-run apps. Its own `builds.json` and the
  logs give 21 builds: 17 apps and 4 libraries. All 17 apps have run records (15 run, 2 crash).
- **FocusTracker** (Windows-only console app) was alive with no window in batch 5, where earlier runners saw it exit. It
  is recorded as `hang` and is not in the portable denominator.
- **Top-15 symbol check** (the aggregated `topMissing` ranking):
  - Windows-only, dropped from the ranking: `System.Windows.Forms` (10 with batch 5), `Form` (6),
    `System.Windows.Interop` (3), `Windows`, `TypedEventHandler<,>`, `InputPane` and `InputPaneVisibilityEventArgs` (2
    each: the WinRT TouchKeyboard pair, legacy csproj files).
  - `System.Drawing.Bitmap` (`CS1069`): only StickyNotesDemo, which is Windows-only. The aggregate's second example,
    WPFGallery, is wrong: its final log has no `System.Drawing` error.
  - Artefact: `System.Windows.Markup.GtkFormsXamlRegistration` and `…Attribute` (7 each with batch 5). The attribute
    exists (`Gtk4.Forms.PresentationFramework/Markup/GtkFormsXamlRegistry.cs:13`). All 7 hits are legacy csproj files
    built against .NET Framework with the layer's generated `.g.cs`. This is a converter limit, not a layer gap.
  - Real gaps, absent from `main`:
    - `AutomationProperties.HeadingLevelProperty` and `SetHeadingLevel` (2, DataBindingDemo and WPFGallery).
      `AutomationProperties` declares only `Name`, `HelpText`, `AutomationId`, `LabeledBy`, `IsRequiredForForm` and
      `LiveSetting` (`Automation/AutomationProperties*.cs`). The peer side already has `GetHeadingLevelCore`,
      `GetItemStatusCore` and `GetAcceleratorKeyCore` (`Automation/Peers/AutomationPeer.Core.cs`), returning defaults.
    - `System.Xaml`, `System.Windows.Baml2006`, `Baml2006Reader` (2 each: BamlReflector and the library BamlTools).
      Deliberately not next.

## Per category

These are portable apps on the strict recipe. Δ is the change since `4fd5749d`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 2 | 0 | 2 | 0 |
| Animation | 11 | 0 | 11 | 11 | +1 | 10 | +1 |
| Application Management | 10 | 1 | 8 | 8 | 0 | 8 | 0 |
| Clipboard | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 30 | 0 | 30 | 0 |
| Documents | 17 | 2 | 15 | 15 | +1 | 15 | +2 |
| Drag and Drop | 6 | 0 | 6 | 6 | 0 | 6 | 0 |
| Element Tree | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Elements | 10 | 0 | 10 | 10 | 0 | 10 | 0 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 8 | 0 | 7 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 14 | 0 | 13 | 0 |
| Graphics | 26 | 0 | 25 | 25 | 0 | 24 | +1 |
| Input and Commands | 18 | 1 | 16 | 16 | 0 | 16 | 0 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Resources | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Sample Applications | 23 | 1 | 21 | 17 | +5 | 15 | +5 |
| Speech and Media | 2 | 1 | 1 | 1 | 0 | 1 | 0 |
| Styles & Templates | 5 | 0 | 5 | 5 | 0 | 5 | 0 |
| Threading | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 2 | 0 | 2 | 0 |
| Windows | 12 | 2 | 10 | 10 | 0 | 10 | 0 |
| **Total** | **256** | **39** | **204** | **198** | **+7** | **191** | **+9** |

Seventeen categories are now complete for their portable apps (every one builds and runs); Documents joined them in
this round, and Animation lacks only AnimationExamples. The remaining build failures are in Sample Applications (4)
and Tools (2). The 7 non-runners are spread over six categories (see Crashes).

## What moved

Each move is attributed by elimination: the named change removed every blocker that `4fd5749d` recorded for the app.
Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass: 7 apps.**

| Change | Apps (run result) |
|---|---|
| Round 60 lane B documents-imaging-tail (`8c49c8e7`, reviewed `e99d8776`): Fixed documents, `Glyphs` and XPS read/write (`eed18bd9`) | DocumentMerge (runs) |
| Round 60 lane B: freezable `ImageSource` and the `CachedBitmap`/`CroppedBitmap`/`FormatConvertedBitmap`/`TransformedBitmap` family (`af71a13f`) | PhotoViewerDemo, ParticlesDemo (run); PhotoStoreDemo (crash) |
| Round 59 lane A controls-freezable-tail-3 (`dab570ca`, merged `e6d32fbe`): `TextBlock.ContentStart`/`ContentEnd`, routed `TextBox.SelectionChanged`, typed brush clones; + Round 60 lane B freezable `TextDecorationCollection` and `DocumentViewerBase.IsMasterPage` (`40de56af`) | FontDialogDemo (runs); SlidePuzzleDemo (crash) |
| Round 61 lane C startup-generator-sweep-3 (`63570ce1`, merged `414525fe`, reviewed `6a01d4af`): content property of a project-local type | Per-FrameAnimation (runs) |

**Crash→runs: 4 apps, all from Round 61 lane C startup-generator-sweep-3.** These are the four the Round 58 report
listed under track 1.

- Brushes: length units (`12pt`) in `Thickness` parsing.
- HexSphereDemo: base-qualified property elements (`GradientBrush.GradientStops`) and nested `ModelVisual3D.Content`.
- PhotoFlipperDemo: explicit-collection property elements (`Viewport3D.Children`).
- AnnotatedDocumentViewer: prefixed clr-namespace `Type.Member` commands in `CommandConverter`.

**Moved blockers in apps that still fail or crash.**

- WPFGallery: the .NET 9 surface wall is gone (`ThemeMode`, the `SystemColors` accent keys, the new `MessageBoxButton`
  values, `GetResourceStream`, `ItemsPanel`, `ToolTipService.ToolTipProperty`, `IsFocusScope`, `RecognizesAccessKey`,
  `RequestNavigateEventArgs.Handled`, `Window.IsActive`; Round 59 lane A). So are the Grid shorthand and
  `ControlExample.Content` (Round 61 lane C). The only build errors left are 10 `CS0117` on
  `AutomationProperties.HeadingLevel`. Expect another wall at run time.
- ExpenseItDemo: `DataGridCell` and `DataGridCellsPresenter` are gone (Round 59 lane A). It now needs
  `AutomationProperties.AcceleratorKey` and `DataGrid.ColumnWidth` (a `DataGridLength`, emitted from `ColumnWidth="*"`).
- AnimationExamples (crash): the `BindingOperations` wall is gone (Round 60 lane B non-element targets). It now stops on
  "Cannot find the element named 'TextEffectRotateTransform'" (see Crashes).
- Unchanged: DataBindingDemo (`AutomationProperties.HeadingLevel`/`ItemStatus`), EditingExaminerDemo
  (`PermissionSet`; the packaging track was not taken), BamlReflector and LocBaml.

**Error landscape.** Counted as portable failing apps per code, `4fd5749d` → now:

- Down: `CS0246` 7 → 3, `CS1061` 5 → 1, `CS0234` 3 → 2.
- Gone: `CS0103` (2), `CS0123` (FontDialogDemo's `SelectionChanged`) and `CS0200` (WPFGallery's Grid shorthand).
- Flat: `CS0117` 3 (now all `AutomationProperties`).
- `CS2002` (a generated `.g.cs` specified twice) is a warning in 35 of 198 portable builds (29 of 191 before).

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang, so no app needed a solo re-run to separate noise.
Every crash and every app that moved was still re-run alone (see Method).

Some apps showed no window in their first 10 s run under parallel load (five other batches were running), then showed
one when re-run alone for 20 s: BindingToMethod, Concepts, AddingEventHandler, FindingSourceElement, Brushes, Converter
and GIFEncoderAndDecoder. CustomAnimation was missing from the first log and runs alone. BindingToStringFomat's exe is
`DataBindingToStringFomat.exe`, which the first pass did not find; it runs. All are recorded as `runs`, as in earlier
reports.

Round 63b (rendering hygiene) fixed both stderr sources. Measured through the lane-private package, one app at a time:

- "Cairo.Context is leaking": GtkSharp's `DrawingArea.DrawFunc` wrapper creates a `Cairo.Context` per draw and never
  disposes it. Every DrawFunc in the layer now disposes its context in a `finally` (`Media/CairoDrawFunc.cs`).
  Per-FrameAnimation went from 254 in 15 s (before, same machine) to 0 in two runs. ParticlesDemo went from 153-293 in
  15-20 s to 0 in 9 runs.
- `Gsk-WARNING ... out of memory`: Viewport3D painted textured triangles whose texel-to-pixel area scale fell below
  1e-8 (a particle dying to nearly zero size). Cairo cannot represent the inverse pattern matrix and the compositor
  rejects the frame. Such triangles are now skipped. Before: up to 128 warnings per 20 s run, in most runs; after: 0 in
  4 runs of 20 s.

## API demand

The counts are failing portable apps (6). Windows-only and legacy-csproj demand is excluded. "Apps" is the number of
failing portable apps whose errors touch the theme (`blockerThemes`). "Only" is the number whose every recorded blocker
is in the theme.

| Theme | Symbols (apps) | Apps | Only |
|---|---|---:|---:|
| Automation attached properties | `AutomationProperties.HeadingLevel` (DataBindingDemo, WPFGallery), `ItemStatus` (DataBindingDemo), `AcceleratorKey` (ExpenseItDemo) | 3 | 2 |
| System.Xaml/BAML tooling | `System.Xaml` (`XamlSchemaContext`, `XamlType`, `Baml2006Reader`, …), `System.Windows.Markup.Localizer` (BamlReflector, LocBaml) | 2 | 2 |
| Code access security | `PermissionSet`/`PermissionSetAttribute` (EditingExaminerDemo; on .NET these come from the `System.Security.Permissions` package, not WPF) | 1 | 1 |
| Controls tail | `DataGrid.ColumnWidth` and `DataGridLength` (ExpenseItDemo) | 1 | 0 |

Run-time demand (built apps that crash on a layer or generator gap):

| Theme | Gap (app) |
|---|---|
| XAML generator | An attribute literal for an `object`-typed property is emitted typed (`Tag="1"` → `Tag = 1`) (SlidePuzzleDemo) |
| Text effects | `TextBlock.TextEffects` is missing, so `<TextBlock.TextEffects>` is skipped and the names inside are never registered (AnimationExamples) |
| Adorners | `AdornerLayer.GetAdornerLayer` finds no layer inside a `Window`, which in WPF always has one through its template (PhotoStoreDemo) |

## Crashes (built, did not run)

| Sample | Cause |
|---|---|
| SlidePuzzleDemo | **New build.** `InvalidCastException` (Int32 to String) at `Puzzle.cs:207`, `(string)item.Tag` in a `SelectionChanged` handler. The MarkupCompiler emits `<ComboBoxItem Tag="1">` as `__e59.Tag = 1;` (`Puzzle.g.cs:105`). WPF keeps an attribute literal for an `object`-typed property as a string. |
| PhotoStoreDemo | **New build.** `NullReferenceException` in `MainWindow.WindowLoaded` (`MainWindow.cs:38`): `AdornerLayer.GetAdornerLayer(CurrentPhoto)` returns null. `GetAdornerLayer` (`Documents/AdornerLayer.cs:88`) finds only an explicit `AdornerDecorator` ancestor. WPF's `Window` template supplies one, so every element in a window has a layer. |
| AnimationExamples | Further in: `InvalidOperationException: Cannot find the element named 'TextEffectRotateTransform' to animate 'CenterX'` (`Storyboard.ResolveTarget`). `TextBlock` has no `TextEffects` property in the layer (only `FlowDocument` has one), so the MarkupCompiler skips `<TextBlock.TextEffects>` in four files and the named `RotateTransform` inside `TextEffect.Transform` is never created. |
| ExpenseItIntro | Unchanged: `Resource with key 'CustomNavWindowStyle' not found`. The key is defined nowhere in the sample, so WPF rejects it too. A defect in the sample. |
| WDPEncoderAndDecoder | Unchanged: `NotSupportedException: JPEG XR (WDP) has no encoder in the GTK runtime`. Deliberate: gdk-pixbuf/GdkTexture have no JPEG XR codec. |
| AppCompat-Quirks | Unchanged: a window shows at 0.5 s, then a `NullReferenceException` in the sample's `PopulateAppContextInformation` (`MainWindow.xaml.cs:197`, it reflects over WPF-internal AppContext switch types) exits the app. Not worth chasing. |
| LocalizationResources | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. The sample ships only an en-US satellite and no neutral resource. |

Two of the three layer crashes are MarkupCompiler work (track 2 below); PhotoStoreDemo is adorner work (track 3).

## XAML generator errors

No portable app fails to build on a generator error any more. The remaining generator-side causes are at run time or
in Windows-only and legacy projects. Per-project messages are in the JSON (`xamlErrors`).

1. **Typed literals for `object` properties** (SlidePuzzleDemo, crash). `Tag="1"` becomes an `int`. The literal should
   stay a string unless the property type asks for a conversion.
2. **Skipped `TextBlock.TextEffects`** (AnimationExamples, crash). This needs the layer property first; the generator
   then has to emit the `TextEffect` collection and register the names inside `TextEffect.Transform`.
3. **Not generator bugs, though surfaced in `.g.cs`:** `AutomationProperties.HeadingLevel` (DataBindingDemo,
   WPFGallery), `AutomationProperties.AcceleratorKey` and `DataGrid.ColumnWidth` (ExpenseItDemo). All are missing layer
   members.
4. **Outside the denominator:** the legacy-csproj `GtkFormsXamlRegistration` artefact (HostingWfInWpfWithXaml,
   PropertyMappingWithWfh, WpfLayoutHostingWfWithXaml and 4 more), and DocumentStructure, whose
   `fixedpage1_structure.xaml` is malformed XML in the sample (`xmlns:my="clr-namespace:MyCompany.MyProduct` has no
   closing quote); that app is Windows-only.

**"Skipped property element" backlog.** These are MSBuild messages, not errors, but they are a silent-drop risk at
run time. The 205 final logs that build hold 81 distinct messages in 32 projects (121 in 39 across 198 builds in
`4fd5749d`). The largest groups (projects):

- `Rectangle.OpacityMask` (4), `Window.CommandBindings` (4), `VisualBrush` attribute `Visual` (4, "not available in
  the layer").
- `ListBox.GroupStyle`, `ListBox.ItemsSource`, `ListView.View`, `ItemsControl.ItemContainerStyle`, `TextBlock.Text`,
  `Path.Data`, `Canvas.OpacityMask`, `StackPanel.CommandBindings` and `Window.DataContext` (2 each).
- `TextBlock.TextEffects` (1 project, 4 files: AnimationExamples, the crash above).

Fixed since `4fd5749d` (Round 61 lane C): length units, base-qualified and explicit-collection property elements,
prefixed commands, user-type content properties, Grid definition shorthand (also on Grid subclasses), the plain `Name`
rule, `RenderOptions` defaults, `Timeline` runtime names and keyed resources no longer stringified.

## Media3D: known gaps against WPF

These come from Round 54 lane B (`gf54b/media3d`). They are documented, not fixed:

- Hidden surfaces use a back-to-front sort of whole triangles (painter's algorithm), not a per-pixel z-buffer.
- Lighting is flat per triangle, with Blinn-Phong specular. Brush textures use a per-triangle affine mapping, not a
  perspective-correct one.
- `Paced` key times are treated as `Uniform`. `ParallelTimeline.SlipBehavior` is stored only.
- Typed 3D animations (`Rotation3DAnimation`, `Point3DAnimation`, `Vector3DAnimation`, `QuaternionAnimation`) are
  deferred.
- `MeshGeometry3D.TextureCoordinates` is a plain list: editing it in place does not repaint.
- The generated-code path now emits an explicit `Viewport3D.Children` and nested `ModelVisual3D.Content` too
  (Round 61), so PhotoFlipperDemo and HexSphereDemo run.

Carry-overs that are still open:

- `Color.FromScRgb` skips the sRGB transfer (`Media/Color.cs`, re-checked: byte normalization only).
- `ScrollBar` is still on `Control`, not `RangeBase` (`Controls/Primitives/ScrollBar.cs:16`, re-checked).
- `SystemParameters` `*Key` objects do not resolve through `DynamicResource` (carried over, not re-checked).
- `Int32RectConverter` and `PixelFormatConverter` are still unregistered; `Int32Rect`, `PixelFormat` and
  `BitmapPalette` stay on the audited unconverted list (`Round49aConverterTests`). No sample sets them from text.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole recorded blocker is the track. The
compiler reports nearly all errors in one pass, but a later phase (generated code, a second wall at run time) can
still hold an app back. Treat "only" as an upper bound for build and an estimate for run. Done in full, the six
tracks would take the count to at most 202 build and about 197 run.

**The automation exclusion is lifted.** The Round 56–58 reports kept `AutomationProperties.HeadingLevel`/`ItemStatus`
out of the lanes because `docs/plans/ui-automation.md` owned `Automation/**`. That plan records Phases 1–5 as done, so
its "parity scout must not pick" rule no longer applies. Its open follow-ups touch `Automation/Peers/**` and the
client, so track 1 keeps to the attached properties and the default `Get*Core` reads.

1. **Automation attached properties: 2 only, 1 involved.**
   - Only: DataBindingDemo (`HeadingLevel`, `ItemStatus`) and WPFGallery (`HeadingLevel`; expect a run-time wall
     behind it).
   - Involved: ExpenseItDemo (`AcceleratorKey`; it also needs track 4).
   - Add `HeadingLevel` (`AutomationHeadingLevel` already exists in `Peers/PeerEnums.cs`), `ItemStatus` and
     `AcceleratorKey` as attached properties. Make `UIElementAutomationPeer`'s `GetHeadingLevelCore`,
     `GetItemStatusCore` and `GetAcceleratorKeyCore` read them, as WPF does.
   - Owns: `Automation/AutomationProperties*.cs` (new partials preferred) and the three `Get*Core` overrides in
     `Automation/Peers/UIElementAutomationPeer*.cs`. Not `Controls/DataGrid.Automation.cs` (track 4).
2. **Startup and generator sweep IV: 2 crash→runs.**
   - Runs: SlidePuzzleDemo (keep `object`-typed attribute literals as strings) and AnimationExamples (add
     `TextBlock.TextEffects`, emit the `TextEffect` collection, register names inside `TextEffect.Transform`).
   - Also: work down the "Skipped property element" backlog, starting with `*.OpacityMask`, `*.CommandBindings`,
     `ListBox.GroupStyle`/`ItemsSource` and `ListView.View`, and register `Int32RectConverter`/`PixelFormatConverter`
     (the Round 60 lane B hand-off) if it stays cheap.
   - Fold `gf39/xaml-content-model` in as a test-only change (the compile-checked generator test); the backlog is its
     regression corpus.
   - Gate: all 204 building projects, plus the 7 non-runners.
   - Owns: `Gtk4.Forms.MarkupCompiler/**` and `Gtk4.Forms.MarkupCompiler.Tests/**`,
     `Gtk4.Forms.WindowsBase/Markup/XamlValueConverter*.cs`, `Gtk4.Forms.PresentationFramework/Markup/**` except
     `NonElementBinding.cs`, and a new additive `Controls/TextBlock.TextEffects.cs` (no other `TextBlock*.cs` edits).
3. **Window adorner layer: 1 crash→runs.**
   - Runs: PhotoStoreDemo. Give `Window` (and any other root that has a template layer in WPF) an implicit
     `AdornerDecorator`, so `AdornerLayer.GetAdornerLayer` finds a layer for every element in a window. Expect a
     further wall in the sample's `RubberbandAdorner` crop path.
   - Check every caller of `GetAdornerLayer` (validation error templates, drag adorners) for behaviour changes when a
     layer appears where there was none.
   - Owns: `Documents/Adorner*.cs` and the window content hosting in `Window*.cs`.
4. **Controls tail IV: 0 only, 1 involved.**
   - Involved: ExpenseItDemo (`DataGrid.ColumnWidth` as a `DataGridLength`, with its converter for `"*"`, `"Auto"`,
     `SizeToHeader` and pixel values; also needs track 1).
   - Owns: `Controls/DataGrid*.cs` (including `DataGrid.Automation.cs`) and new `Controls/DataGridLength*.cs`. The
     XAML converter registration belongs to track 2's `XamlValueConverter*.cs`. Hand it over as a note, or use a
     `[TypeConverter]` on `DataGridLength`, which needs no generator change.
5. **Packaging: 1 only.**
   - Only: EditingExaminerDemo. Add `System.Security.Permissions` as a `Gtk4.Forms.Wpf` dependency for
     `PermissionSet`/`PermissionSetAttribute`, as Round 58 did with `Microsoft.Win32.SystemEvents`. Unchanged from the
     Round 58 plan; Round 61 did not take it.
   - Check first: that the package's `net10.0` reference assembly carries both types, and that `SYSLIB0003` stays a
     warning in the consumer.
   - Owns: `Gtk4.Forms.Wpf/Gtk4.Forms.Wpf.csproj` and its package tests.
6. **Rendering hygiene: 0 samples (both apps already run).**
   - Find the undisposed `Cairo.Context` behind the GtkSharp finalizer warnings in Per-FrameAnimation and ParticlesDemo
     (per-frame `OnRender`/`DrawingContext` paths and the `RenderTargetBitmap` surface), and the `GskTransformNode` "out
     of memory" failures in ParticlesDemo.
   - Owns: `Media/DrawingContextRenderer*.cs`, `Media/Imaging/RenderTargetBitmap.cs`, `Media/DrawingBrush.cs`,
     `Media/VisualBrush.cs`, `Controls/Viewport3D.cs`, and the `OnRender` draw hookup in `UIElement*.cs`
     (rendering partials only).

**Lanes (three in parallel, two tracks each, file-disjoint):**

| Lane | Tracks | Only | Boundary to watch |
|---|---|---:|---|
| A | 1 (automation attached properties) + 4 (controls tail IV) | 2, and ExpenseItDemo needs both | Track 1 owns `Automation/AutomationProperties*.cs` and the `UIElementAutomationPeer` `Get*Core` reads. Track 4 owns `Controls/DataGrid*.cs`, `DataGrid.Automation.cs` included. Together they finish ExpenseItDemo |
| B | 3 (window adorner layer) + 6 (rendering hygiene) | 1 crash→runs | Track 3 owns `Documents/Adorner*.cs` and `Window*.cs`. Track 6 owns the `Media/` renderers, `RenderTargetBitmap.cs`, `Viewport3D.cs` and the `OnRender` hookup. Neither touches `Documents/` outside `Adorner*.cs` |
| C | 2 (generator sweep IV) + 5 (packaging) | 2 crash→runs, and 1 | Track 2 owns `Gtk4.Forms.MarkupCompiler/**`, `Markup/**` minus `NonElementBinding.cs`, `XamlValueConverter*.cs` and the new `TextBlock.TextEffects.cs`. Track 5 owns only the `Gtk4.Forms.Wpf` package project |

Held branches:

- **`gf52b/content-bindings-sweep`: merged** (`b25bc71e`, before `270cdb7e`). The branch ref is gone. Nothing to do.
- **`gf46/input-focus` (held, 6 commits, 227 behind): 0 samples.** Every Input and Commands app builds and runs. What
  remains is focus-transaction behaviour, owned by the focus-behaviour design. `FocusManager.IsFocusScope` and
  `GetFocusScope` landed on `main` in Round 59 (`a99dfb4b`), so a rework must start from `main`'s
  `Input/FocusManager*.cs`, not from the branch. It is not in the lanes.
- **`gf39/xaml-content-model` (held, 3 commits, 280 behind): 0 samples on its own.** Fold it into track 2 as a
  test-only change.
- **`gf57b/documents-imaging-tail` (4 commits, 82 behind): superseded.** Round 60 lane B re-applied 3 of its 4 commits
  and dropped `06cd79ce` on purpose (see below). Delete the ref.
- **`gf53a/visual-layer-input` (1 commit, 122 behind): superseded.** Its commit landed through
  `gf56a/visual-layer-input`. Delete the ref after confirming that nothing unique remains on it.

**Round 60 (lane B) hand-offs, status.**

- Done: ParticlesDemo's `x:Name` inside a `Model3D` tree (it runs), DocumentMerge (runs), FontDialogDemo (runs), and
  SlidePuzzleDemo's typed `SolidColorBrush.Clone()` (it builds; the next wall is track 2).
- Open: AnimationExamples' `TextEffect.Transform` name (track 2), PhotoStoreDemo's `AdornerLayer` (track 3),
  `Int32RectConverter`/`PixelFormatConverter` (track 2, optional).
- Known documents/imaging gaps (not scheduled, not re-checked): `DocumentPageView` does not paint a `FixedPage`;
  `TransformedBitmap` handles only 90-degree rotations and scales; the derived bitmaps do not follow later pixel
  changes in their `Source`; XPS resource-dictionary parts are copied verbatim; `Brushes.*` returns new unfrozen
  instances.
- Binding engine gaps (not scheduled, not re-checked): a freezable shared by several holders has no inheritance
  context, and a single-segment path to a non-dependency property of an element source is read once.
- Rejected in the Round 60 triage: the null-owner `InvalidOperationException` in `TextTreeEdit.Split.cs`. `main`'s
  detached-`List` fix (`2f4a566d`) covers it, so `06cd79ce` has no genuine delta to port.

Deliberately not next:

- JPEG XR encoding (WDPEncoderAndDecoder). There is no codec in the GTK stack.
- System.Xaml/BAML tooling (LocBaml, BamlReflector/BamlTools).
- AppCompat-Quirks (WPF-internal AppContext switches), LocalizationResources (the sample ships no neutral resource) and
  ExpenseItIntro (the sample references an undefined resource key).
- Legacy (non-SDK) csproj conversion: 21 projects. That is a converter feature, not a layer gap. It would also remove
  the `GtkFormsXamlRegistration` artefact from the symbol ranking.

## Round 62 (lane A) gate

Measured on `lane/a` (Round 62 tracks 1, 3 and 4 with their review fixes), package `1.0.0.26327` from a lane-private
pinned feed. All 204 projects that built at `17c64167` plus DataBindingDemo, ExpenseItDemo and WPFGallery were built
twice each (the first build converts). 4 of the first builds failed once with no error text (MousePointer,
MergedResources, Matrix, CubeAnimationDemo) and passed on a solo rebuild: contention, not the layer.

- **207 of 207 projects build** (+3). All 191 apps that ran at `17c64167` still run, with one visible top-level window each (26
  had no window handle in the crowded first pass and were re-checked alone for 20 s: all alive with a window).
- **DataBindingDemo: fail→runs.** **ExpenseItDemo: fail→runs** (it builds and its dialog opens). **PhotoStoreDemo:
  crash→runs.** **WPFGallery: fail→builds, then crashes** on the expected wall: `InvalidOperationException: Resource
  with key 'ControlCornerRadius' not found` (`MainWindow.g.cs:34`).
- Portable apps: 201 of 204 build and 194 run. Three of the six apps that failed at `17c64167` now build;
  EditingExaminerDemo and the two System.Xaml/BAML tools still fail.
- **ExpenseItDemo, `CreateExpenseReportDialogBox`:** the dialog opens, and `ColumnWidth="*"` reaches the grid
  (`ColumnWidth=*`). The grid has **no columns**: the generator does not emit the `DataGrid.Columns` property element
  (lane B territory, MarkupCompiler). With three text columns added in code the headers fill the grid (103, 94 and 56
  px of 253, all `Hexpand`), so star width renders.
- **PhotoStoreDemo:** a native (OS-level) click on the image reaches `OnMouseDown` (`CropButton` goes from disabled to
  enabled, `CaptureMouse` and `StartSelection` run).
  Capture on a placed adorner re-targets input (unit-tested); no native drag was driven.
- New walls: none beyond the two above (`DataGrid.Columns` generator gap; WPFGallery `ControlCornerRadius`).

## Method

- **Source.** The local `WPF-Samples` tree (`00f39dbb`), split into six category batches:
  1. Data Binding, Getting Started, Events.
  2. Graphics, Animation, Visual Layer, Speech and Media, Clipboard.
  3. Sample Applications, Migration and Interoperability.
  4. Input and Commands, Documents, Drag and Drop, Compatibility.
  5. Globalization and Localization, Windows, Accessibility, Element Tree, PerMonitorDPI.
  6. Application Management, Elements, Properties, Resources, Styles & Templates, Threading, Tools.

  Each batch was copied under `.claude/samples-remeasure-r61/batchN` without `bin`/`obj`, `NuGet.config` or
  `global.json`. The scratch root has empty `Directory.Build.props`/`.targets`, so `TreatWarningsAsErrors` was off. No
  original or tracked file was modified: `git status` is clean in the worktree and in `WPF-Samples`.
  - Batches 1, 2, 3 and 4 each discarded or repeated a first pass that had a script bug (log paths, exe filter,
    PowerShell precedence that skipped the `PackageReference`). Only the clean runs are used.
- **Layer build.** `dotnet build Gtk4.Wpf.sln -c Debug -nodeReuse:false`: 0 errors, 3 warnings. Every product and test
  project is warning-clean. The 3 warnings come from GitInfo inside the WPF-Samples `HelloWorld.csproj` that the
  solution includes (`MSB3073` exit 128, "Could not retrieve last commit", "GitInfo.txt is outside repository"),
  because `WPF-Samples` is a separate nested git repo. They are environmental, but strictly the solution build is not
  warning-free. `dotnet pack <proj> -c Debug --no-build -o <feed>` for WindowsBase, PresentationFramework,
  MarkupCompiler and Wpf: no warnings or errors.
- **Feed, pinned.** The version is date-based and equal to the Round 58 one (1.0.0.26327) with different bytes, so
  pinning is required. `samples-remeasure-r61/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-remeasure-r61/.nuget-packages`, which started empty) and cleared
    `fallbackPackageFolders`.
  - Cleared `packageSources`, then the pinned feed (`samples-remeasure-r61/feed`) and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the recipe `nuget.config`, but with
  `globalPackagesFolder` pointed at an empty probe cache, so the shared cache stayed untouched.
  - Build 1 exited 1, as expected, with the converter's "re-run the build". Build 2 exited 0 with 0 warnings and 0
    errors.
  - The cached SHA-256 values equal the feed's. They were re-computed for this report from the feed and the shared
    batch cache:
    - `Gtk4.Forms.Wpf` `db803c7742fd8d4833e6a0263852bf984154720294c233fe5f7a2dcc10d65874`
    - `PresentationFramework` `02b53da7525a36f500f4c0d00ad4519c09220b65345e3ef8cd05a8d3f0f92b4f`
    - `WindowsBase` `29c308206371d216a286e6a88702f00235e912e6974c103c56e466966454704f`
    - `MarkupCompiler` `e6f75573357642f0e2af405653ac6f34bf14f7f0f83918f78eba810c53bae8d7`
  - Every `gtk4.forms.*` `.nupkg.metadata` source is the scratch feed.
  - `project.assets.json` lists only the isolated probe cache as a package folder and no fallback folders. Its sources
    are the feed, nuget.org and the SDK-implicit `library-packs`.
  - `gtksharp4 4.22.4.26327` came from nuget.org.
  - Smoke: the 10 s run was alive with 1 visible unowned top-level window (first seen at 8 s, cold start) and 0 bytes
    of stderr. Alone for 20 s it was alive with 1 window from 1 s and 0 bytes of stderr.
  - The probe directory, its cache and its logs were deleted afterwards. Batches 1–4 and 6 re-checked the cached
    `Gtk4.Forms.Wpf` hash.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26327`.
  2. Build once with `-nodeReuse:false`. The converter rewrites the csproj and stops.
  3. Build again. This result is recorded.
  - Nothing else is changed. Original csproj files are kept beside the copies (`*.csproj.orig`).
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes, symbols and messages were
  re-parsed uniformly from the final logs of all six batches (252 logs, one per csproj; UTF-16 and UTF-8). In batch 5,
  38 of 44 build-1 logs carry the converter's "re-run the build"; the 6 without it are the legacy csproj files.
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime
  (`%LOCALAPPDATA%\Gtk\4.22.4\bin`) on `PATH`.
  - "Runs" means alive with a visible, unowned top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception, or an exit with or without a window.
  - "Hang" means alive with no window and no exception.
  - Apps with no window at 10 s were re-run alone for 20 s.
  - Every built app has a per-app run record (batch 1 `smoke-*.txt`, batch 2 `run.10.txt`/`run.20.txt`, batch 3
    `smoke.json`/`smoke2.json`, batch 4 `results.json`, batch 5 `run.results`, batch 6 `batch6-results.json`).
  - For this report, every crash and every app that moved (13 apps) was re-run alone for 15–20 s, polling every
    0.5 s, and killed by its own PID. This caught AppCompat-Quirks (see Corrections). The other runners stop at the
    first window, so a crash after the first window can still hide in a `runs` row that was not re-run.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification of the earlier reports, unchanged.
- **Limits.**
  - Only Windows was measured.
  - Legacy csproj files are not converted: 21 projects.
  - Moves are attributed by elimination, not bisected.
  - `blockerThemes` is assigned per project for the 7 failing portable projects (6 apps and the library BamlTools).

## History

| Date | Main SHA | Package | Pinned | Portable build | Portable run | Note |
|---|---|---|---|---:|---:|---|
| 2026-11-19 | `3b73e4f5` | 1.0.0.26323 | no | 0 / 206 | 0 | Shared user cache; local source, not the published package |
| 2026-11-20 | `db9526db` | 1.0.0.26324 | no | 45 / 206 | 31 | 10 crash, 4 hang; no regressions |
| 2026-11-20 | `64cddd04` | 1.0.0.26324 | no | 59 / 206 | 47 | Round 44; 10 crash, 2 hang; 2 regressions (TargetValues, MultiPage) |
| 2026-11-20 | `ea4c17ae` | 1.0.0.26324 | no | 83 / 206 | 74 | Round 45; 8 crash, 1 hang; no regressions |
| 2026-11-20 | `a37d3b17` + `gf46/startup-runtime` | 1.0.0.26324 | no | 83 / 206 | 76 | Gate run: Linq and OpacityMasking run |
| 2026-11-20 | `1b50a528` | 1.0.0.26324 | **yes** | 89 / 204 | 83 | Round 46; 5 crash, 1 hang; no regressions; the earlier runs are confirmed local-feed |
| 2026-11-21 | `2c67b72c` + generator-v4 | 1.0.0.26325 | **yes** | 90 / 204 | 87 | Gate: generator v4 |
| 2026-11-21 | `5f92acd0` + Round 47 review fixes | 1.0.0.26325 | **yes** | 91 / 204 | 88 | Gate, lower bounds |
| 2026-11-21 | `28d1eef2` + Round 48 review fixes | 1.0.0.26325 | **yes** | 92 / 204 | 89 | Gate, lower bounds (94 / 91 counting Round 47 extras) |
| 2026-11-21 | `40db59fe` + Round 48 follow-up fixes | 1.0.0.26325 | **yes** | 92 / 204 | 89 | Gate, lower bounds |
| 2026-11-21 | `142b53f6` | 1.0.0.26325 | **yes** | 129 / 204 | 119 | Round 48; full re-measure; 8 crash, 2 hang; no regressions |
| 2026-11-22 | `30f46d4a` | 1.0.0.26326 | **yes** | 159 / 204 | 152 | Rounds 49–52 (first two-lane batch) + Round 52 paginated-viewer fixes; full re-measure; 6 crash, 1 hang; no regressions |
| 2026-11-22 | `b25bc71e` + `gf54b/documents-text-tail` | 1.0.0.26326 | **yes** | 161 / 204 | 154 | Lane B gate, lower bounds; no regressions; FontProperties and FlowDocumentProperties build and run |
| 2026-11-22 | `466055e3` | 1.0.0.26326 | **yes** | 171 / 204 | 165 | Rounds 53–55 (first three-lane batch) + `gf52b` merge; full re-measure; 6 crash, 0 hang; no regressions |
| 2026-11-22 | `270cdb7e` + `gf58c/startup-generator-sweep-2` | 1.0.0.26326 | **yes** | 172 / 204 | 167 | Lane C gate (as reported then: 176 + AnimationExamples build, 165 + Geometry, KeySplineAnimations run); no regressions |
| 2026-11-23 | `85493c8b` | 1.0.0.26327 | **yes** | 191 / 204 | 182 | Rounds 56–58 (second three-lane batch); full re-measure; 9 crash, 0 hang; no regressions |
| 2026-11-23 | `17c64167` | 1.0.0.26327 | **yes** | **198 / 204** | **191** | Rounds 59–61 (third three-lane batch); full re-measure; 7 crash, 0 hang; no regressions |
| 2026-11-24 | `lane/a` (Round 62 lane A) | 1.0.0.26327 | **yes** | 201 / 204 portable (207 / 207 built projects) | 194 | Lane A gate; DataBindingDemo, ExpenseItDemo build and run, PhotoStoreDemo runs, WPFGallery builds then crashes; no regressions |

## Known layer defects found by the UI tests

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`. WPF re-queries commands afterwards, so the button is enabled. The layer calls nothing like
  `CommandManager.InvalidateRequerySuggested` there, so the button stays disabled until another requery happens.
  `CommandSampleTests` keeps a workaround test (it requeries itself) and a skipped test,
  `The_button_is_enabled_without_a_manual_requery`, that asserts the correct behaviour. Un-skip it when fixed.

## Round 70 lane J: Graphics/VisualBrush by pixels

`Graphics/VisualBrush` (`WPF-Samples` at the submodule revision) now compiles unmodified in
`Gtk4.Forms.Wpf.Testing.Tests` (`Samples/VisualBrushSampleTests`) and is checked by pixels on all three tabs:

- **Painting with Visuals**: the 50x50 tile rectangle shows the red 25x25 squares and the tiled text and button, and no
  black fill.
- **Magnifying Glass**: after a synthesized `MouseMove` over `magnifiedPanel` the Ellipse moves to mouse+5 and shows
  the magnified grid; a second move shows text instead. The root cause of the empty interior was
  `DrawingGroup.Children` items having no inheritance context, so `Visual="{Binding ElementName=magnifiedPanel}"` on
  the nested `VisualBrush` never resolved.
- **Reflection**: the lower canvas holds the vertically flipped upper canvas and fades towards the bottom. The lower
  canvas is sized by `Height`/`Width` bindings that resolve after layout; `FrameworkElement` now re-pushes a late
  Width/Height to the GTK widget.

Pinned recipe (lane-private feed from this worktree, `Gtk4.Forms.Wpf 1.0.0.26328`, sample copy with only the package
reference, `<clear/>` nuget.config with `Gtk4.*` mapped to the feed, isolated `globalPackagesFolder`, empty
`Directory.Build.props`, two builds with `-nodeReuse:false`): VisualBrush builds with 0 warnings and 0 errors and stays
alive; Win32 reports the app window plus the shared host window, which is a 1 %-opacity, non-interactive window (it
is `WS_VISIBLE` but not drawn). Gate subset built and smoke-run (about 25 s start-up, alone): Brushes, DrawingBrush,
ImageBrush, OpacityMasking, 2DTransforms, BindingDPToDP, SimpleBinding all build (0 errors) and stay up with their
window. The full 198-app gate was not run in this step.
