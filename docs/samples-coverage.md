# WPF samples coverage

Measured 2026-11-23 on `main` at `85493c8bb2200bda4e3efd1cc73c9caaf1920939` (Rounds 56–58, the second three-lane batch:
lane A visual layer, input devices and `BeginStoryboard`; lane B items and data tail; lane C generator sweep II and
controls tail II, each with its review fixes). The package is `Gtk4.Forms.Wpf 1.0.0.26327` from a **pinned** local
feed. The samples are the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4 native C++
projects). All 252 csproj were rebuilt and every built app was smoke-run. Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report `270cdb7e` (main
`466055e3`, Rounds 53–55), under `previous`.

## Headline

**191 of 204 portable official WPF sample apps build unmodified, and 182 run.** That is up from 171 / 165 in report
`270cdb7e` (+20 build, +17 run), and from 0 / 0 in report `fedd13c8` (2026-11-19).

- **20 apps moved fail→pass**: 15 run and 5 crash at startup. **2 built apps moved crash→runs** (Geometry,
  KeySplineAnimations). The library EditBoxControlLibrary also builds now.
- **No regressions.** No project went pass→fail, and no app went runs→crash/hang.
- **No hangs.** 9 apps build but do not run, and all 9 crash. 5 are new builds: 4 crash on MarkupCompiler or converter
  gaps and 1 on non-element bindings (see Crashes). The other 4 are known: 3 are deliberate or out of scope, and
  ExpenseItIntro's remaining cause is a defect in the sample.
- 197 projects build in all: the 191 portable apps, the libraries CustomApplicationClient, wpfclock,
  HostingWpfUserControlInWf, WPFControls and EditBoxControlLibrary, and the Windows-only console app FocusTracker.
- By lane:
  - Round 56 lane A (`gf56a`, the visual layer and input surface plus input devices and `BeginStoryboard`): 13
    fail→pass (10 run, 3 crash), the library, and with lane C GraphingCalculatorDemo.
  - Round 58 lane C (generator sweep II, controls tail II and packaging): 4 fail→pass and both crash→runs.
  - Round 57 lane B (items and data tail): 2 fail→pass.
- **Round 57's documents-and-imaging track never reached `main`.** `gf57b/documents-imaging-tail` (4 commits) carries
  the blockers of 4 of the 13 failing portable apps (see Next rounds, track 2).

### Corrections to the raw batch data

The final build log of each of the 252 csproj was re-parsed uniformly. Run results come from the batches' per-app run
records, and each record was scanned for unhandled exceptions. The top 15 missing symbols were checked against the
`Gtk4.Forms.*` source.

- **The aggregated totals were wrong again.** They said 205 portable apps, 189 build and 180 run. The per-project data
  gives 204 / 191 / 182.
  - The aggregate used the batches' own regex `windowsOnly` flags. For example, batch 5 flagged all 12 Accessibility
    projects as Windows-only, which drops FindText and InvokePattern/Target. Both of those build and run, which
    accounts for 191 − 2 = 189 and 182 − 2 = 180.
  - As in every earlier report, the verified classification is carried over unchanged, so the denominator stays 204.
- **WPFGallery is a valid two-build measurement this time.** Build 1 converted the project and build 2 is the one
  recorded. The `Microsoft.Win32.SystemEvents` wall (`CS1069`) is gone, and the next wall is visible (see API demand).
- **Legacy (non-SDK) csproj files stay `not-built`** (21 projects). Highlighter builds again, but as real .NET Framework,
  so it is not counted. The attempt is kept under `legacyBuild`.
- **FocusTracker** is a Windows-only console app that prints a prompt and exits. It is recorded as `crash` as before and
  is not in the portable denominator.
- **Probe hash typo.** The probe summary truncated the WindowsBase hash as `2777984d…dd54b43`, which is the
  MarkupCompiler suffix. The full hashes were re-computed from the feed and the shared batch cache, and all four match
  (see Method).
- **Top-15 symbol check** (the aggregated `topMissing` ranking):
  - Windows-only, dropped from the ranking: `System.Windows.Forms` (10), `Form` (5), `System.Windows.Interop` (3),
    `System.Windows.Forms.Integration` (3) and `DpiScale` (3). `DpiScale` is absent from `main`, but it is wanted only
    by the PerMonitorDPI apps, which are Windows-only by design.
  - Artefact: `System.Windows.Markup.GtkFormsXamlRegistration` and `…Attribute` (7 each). The attribute exists
    (`Gtk4.Forms.PresentationFramework/Markup/GtkFormsXamlRegistry.cs:13`). All 7 hits are legacy csproj files built
    against .NET Framework with the layer's generated `.g.cs`: ADODataSet, UsingSpeech, HostingWfInWpfWithXaml,
    PropertyMappingWithWfh, WpfLayoutHostingWfWithXaml, WinFormsHostScaling and SingleInstanceDetection. This is a
    converter limit, not a layer gap.
  - Real gaps, absent from `main`:
    - `AutomationProperties.HeadingLevelProperty` and `SetHeadingLevel` (2, DataBindingDemo and WPFGallery).
      `HeadingLevel` exists only on `AutomationPeer` (`Automation/Peers/AutomationPeer.Core.cs`), not as an attached
      property. This belongs to the UI automation plan.
    - `CachedBitmap`, `TransformedBitmap`, `FormatConvertedBitmap` and `CroppedBitmap` (2 each, PhotoStoreDemo and
      PhotoViewerDemo). All four are implemented on the unmerged `gf57b/documents-imaging-tail` (`b55ad49e`).
    - `FixedDocument` and `FixedDocumentSequence` (2 each). They are on `gf57b` (`e3e9699a`). Only DocumentMerge is
      portable; DocumentSerialization is Windows-only (XPS printing).
- **Also found while checking:**
  - `MagnitismCanvas.Content` (Per-FrameAnimation) and `ControlExample.Content` (WPFGallery) have one cause. The
    MarkupCompiler does not read the content property of a type declared in the project being compiled:
    `MagnitismCanvas : Canvas` inherits `Children`, and `ControlExample` declares
    `[ContentProperty(nameof(ExampleContent))]`. It falls back to `.Content`.
  - `Freezable.Color` (SlidePuzzleDemo) comes from `((SolidColorBrush)…).Clone().Color`. `SolidColorBrush` has no
    typed `Clone()` (`GradientBrush.Clone.cs` and `TileBrush.Clone.cs` exist; `SolidColorBrush` has none), so
    `Freezable.Clone()` is returned.
  - `TextBox.SelectionChanged` is an `EventHandler` (`Controls/TextBox.cs:23`). WPF's is a `RoutedEventHandler`, so
    FontDialogDemo gets `CS0123`.

## Per category

These are portable apps on the strict recipe. Δ is the change since `270cdb7e`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 2 | 0 | 2 | 0 |
| Animation | 11 | 0 | 11 | 10 | +1 | 9 | +1 |
| Application Management | 10 | 1 | 8 | 8 | 0 | 8 | 0 |
| Clipboard | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 30 | 0 | 30 | 0 |
| Documents | 17 | 2 | 15 | 14 | +2 | 13 | +1 |
| Drag and Drop | 6 | 0 | 6 | 6 | +1 | 6 | +1 |
| Element Tree | 2 | 0 | 2 | 2 | +1 | 2 | +1 |
| Elements | 10 | 0 | 10 | 10 | +1 | 10 | +1 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 8 | +1 | 7 | +1 |
| Globalization and Localization | 14 | 0 | 14 | 14 | 0 | 13 | 0 |
| Graphics | 26 | 0 | 25 | 25 | +4 | 23 | +4 |
| Input and Commands | 18 | 1 | 16 | 16 | 0 | 16 | 0 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Resources | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Sample Applications | 23 | 1 | 21 | 12 | +5 | 10 | +3 |
| Speech and Media | 2 | 1 | 1 | 1 | +1 | 1 | +1 |
| Styles & Templates | 5 | 0 | 5 | 5 | 0 | 5 | 0 |
| Threading | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 2 | +2 | 2 | +2 |
| Windows | 12 | 2 | 10 | 10 | +1 | 10 | +1 |
| **Total** | **256** | **39** | **204** | **191** | **+20** | **182** | **+17** |

Sixteen categories are now complete for their portable apps (every one builds and runs). The remaining build failures
are concentrated in Sample Applications (9), Tools (2), Animation (1) and Documents (1).

## What moved

Each move is attributed by elimination: the named change removed every blocker that `270cdb7e` recorded for the app.
Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass: 20 apps and 1 library.**

| Change | Apps (run result) |
|---|---|
| Round 56 lane A visual-layer-input (`a1bb8214`, reviewed `7a1dad6e`): `OriginalSource`, `GeneralTransform`, hit testing, `CompositionTarget`, converters, overridable `LogicalChildren`, the `BitmapEffect` family, `AdornerLayer.Update` | DragDropObjects, OverridingLogicalTree, ThicknessConverter, BitmapEffectsGallery, ImageBrush, VisualBrush, LayoutTransitionsDemo, CompositionTarget, DrawingVisual (run); Brushes (crash) |
| Round 56 lane A input-devices-storyboard (`2e7985f3`): `MouseDevice`/`KeyboardDevice` on the event args, `FrameworkElement.BeginStoryboard` | CubeAnimationDemo (runs); HexSphereDemo, PhotoFlipperDemo (crash); with visual-layer-input, the library EditBoxControlLibrary |
| Round 56 lane A + Round 58 lane C: `Rect` edges, input devices, `TextBox.MaxLines` | GraphingCalculatorDemo (runs) |
| Round 58 lane C controls-tail-packaging-2 (`22e6dd58`): `Hyperlink.TargetName`, font converters, `InkCanvas` shell | Concepts, DialogBox, AnnotationsStyling (run) |
| Round 58 lane C startup-generator-sweep-2 (`e8937f6e`): `{Binding}` on an animation value emitted through `BindingOperations` | AnimationExamples (crash) |
| Round 57 lane B items-data-tail (`7237e943`, reviewed `97277fb2`): settable `SortDescription`, `Binding.Path` typed `PropertyPath` | MediaGallery (runs); AnnotatedDocumentViewer (crash) |

**Crash→runs: 2 apps, both from Round 58 lane C startup-generator-sweep-2 (`e8937f6e`).** These are the two the
Round 58 lane gate predicted.

- Geometry: `Viewport3D.Camera` and `ModelVisual3D.Content`/`Transform`/`Children` are emitted, and the nested `x:Name`
  fields are assigned.
- KeySplineAnimations: key-frame storyboards are emitted, and names inside Geometry/Drawing property elements register.

**Moved blockers in apps that still fail.**

- Per-FrameAnimation: hit testing and `LogicalChildren` are gone. It now fails on `MagnitismCanvas.Content`, the
  generator's content-property lookup for a project-local type.
- ExpenseItDemo: the `OnMouse*` overrides are gone. It now needs `DataGridCell` and `DataGridCellsPresenter`.
- FontDialogDemo: `TextBox.TextAlignment`, `CS0029` and `CS1729` are gone. It still needs `TextBlock.ContentStart`
  and `ContentEnd`, `TextDecorationCollection.Freeze`, and a routed `TextBox.SelectionChanged`.
- PhotoStoreDemo: `AddVisualChild` and `Rect.Size`/`Location` are gone. Only the imaging family is left (on `gf57b`).
- SlidePuzzleDemo: `ComboBoxItem.Tag` and `BeginStoryboard` are gone. It still needs
  `DocumentViewerBase.IsMasterPage` (on `gf57b`) and a typed `SolidColorBrush.Clone()`.
- DataBindingDemo: `CollectionViewSource.SetBinding` is gone. Only `AutomationProperties.HeadingLevel`/`ItemStatus`
  remain, which belong to the UI automation plan.
- WPFGallery: `SystemEvents` is gone. It now hits the .NET 9 WPF surface (see API demand).
- Unchanged: DocumentMerge, PhotoViewerDemo and ParticlesDemo (all three are on `gf57b`), EditingExaminerDemo,
  BamlReflector and LocBaml.

**Error landscape.** Counted as portable failing apps per code, `270cdb7e` → now:

- Down: `CS0246` 12 → 7, `CS1061` 12 → 5, `CS0234` 5 → 3, `CS0117` 4 → 3, `CS0103` 3 → 2.
- Gone: `CS0115` (4: the overridable `LogicalChildren`, `OnRender` and `OnMouse*`), `CS1955` (3: `BeginStoryboard`),
  `CS0029` (2), `CS0400`, `CS1069` and `CS1503`.
- Flat: `CS0123` 1 (FontDialogDemo), `CS0200` 1 (now WPFGallery's Grid shorthand; AnnotatedDocumentViewer's
  `SortDescription` one is fixed).
- `CS2002` (a generated `.g.cs` specified twice) is a warning in 29 of 191 portable builds (25 of 171 before).

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang. With no regressions, no app needed a solo re-run to
separate noise.

Eleven apps showed no window in their first 10 s run under parallel load, then showed one when re-run alone for 20 s:
KeyFrameAnimation, Vector, FlowContentElements, SpellChecking, DragDropObjects, DragDropTextOps, CustomWindowUI,
WindowSizingOrder, FocusVisualStyle, SettingMargins and UsingElements. They are recorded as `runs`, as in earlier
reports. A few more (CursorType, FlowContentProperty, KeyStrokeCounter, HandlingEventOnCommandUsingCode and some Data
Binding apps) showed their window only at 7–9.5 s under load.

## API demand

The counts are failing portable apps (13). Windows-only and legacy-csproj demand is excluded. "Apps" is the number of
failing portable apps whose errors touch the theme (`blockerThemes`). "Only" is the number whose every recorded blocker
is in the theme.

| Theme | Symbols (apps) | Apps | Only |
|---|---|---:|---:|
| Imaging tail | `CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`, `TransformedBitmap` (PhotoViewerDemo, PhotoStoreDemo); `RenderTargetBitmap.Freeze`, i.e. a freezable `ImageSource` (ParticlesDemo). **All on `gf57b`** | 3 | 3 |
| Documents and text | Fixed/XPS: `FixedDocument`, `FixedDocumentSequence`, `FixedPage`, `PageContent`, `DocumentReference`, `XpsDocument` (DocumentMerge, on `gf57b`); `DocumentViewerBase.IsMasterPage` (SlidePuzzleDemo, on `gf57b`); `TextBlock.ContentStart`/`ContentEnd` (FontDialogDemo) | 3 | 1 |
| Freezable completeness | `TextDecorationCollection.Freeze` (FontDialogDemo, on `gf57b`); typed `SolidColorBrush.Clone()` (SlidePuzzleDemo); `RenderTargetBitmap.Freeze` (ParticlesDemo) | 3 | 0 |
| Controls tail | `DataGridCell`, `DataGridCellsPresenter` (ExpenseItDemo); routed `TextBox.SelectionChanged` (FontDialogDemo); `ItemsControl.ItemsPanelProperty`, `ToolTipService.ToolTipProperty`, `ContentPresenter.RecognizesAccessKey` (WPFGallery) | 3 | 1 |
| .NET 9 WPF surface | `Application.ThemeMode`/`ThemeMode`, `SystemColors.AccentColor*BrushKey` (7), `MessageBoxButton.AbortRetryIgnore`/`RetryCancel`/`CancelTryContinue`, `Application.GetResourceStream`, `FocusManager.IsFocusScope`, `RequestNavigateEventArgs.Handled`, `Window.IsActive` (WPFGallery) | 1 | 0 |
| XAML generator | Content property of a project-local type (Per-FrameAnimation; WPFGallery's `ControlExample`); Grid `RowDefinitions="Auto,*"` attribute shorthand emitted as a string assignment (WPFGallery, `CS0200`) | 2 | 1 |
| Automation | `AutomationProperties.HeadingLevel`/`ItemStatus` (DataBindingDemo, WPFGallery) | 2 | 1 |
| System.Xaml/BAML tooling | `System.Xaml` (`XamlSchemaContext`, `XamlType`, `Baml2006Reader`, …), `System.Windows.Markup.Localizer` (BamlReflector, LocBaml) | 2 | 2 |
| Code access security | `PermissionSet`/`PermissionSetAttribute` (EditingExaminerDemo; on .NET these come from the `System.Security.Permissions` package, not WPF) | 1 | 1 |

## Crashes (built, did not run)

| Sample | Cause |
|---|---|
| AnimationExamples | **New build.** `NotSupportedException: BindingOperations only supports FrameworkElement and FrameworkContentElement targets`. The sample's own `SampleViewer` constructor (`SampleViewer.cs:35`) binds an animation. The binding engine needs non-element targets (`Data/**`), which is the gap the Round 58 gate recorded for `NonElementBinding`. |
| Brushes | **New build.** `FormatException: Cannot convert value "12pt" for property FrameworkElement.Margin`. `PrimitiveConverters.ParseThickness` (`Gtk4.Forms.WindowsBase/Markup/XamlValueConverter.Primitives.cs:20`) has no unit suffixes. Fix: parse each token with `LengthUnits.Parse`. |
| HexSphereDemo | **New build.** `ArgumentException: '' is not a valid value for property 'Background'` in `InitializeComponent`. The `RadialGradientBrush` resource uses the base-qualified property element `GradientBrush.GradientStops`. The MarkupCompiler skips it and emits `Resources["GridBackground"] = ""`. `ModelVisual3D.Content` is also skipped in this file. |
| PhotoFlipperDemo | **New build.** `ArgumentOutOfRangeException` in `Visual3DCollection.get_Item` from `OnLoaded`. The explicit `<Viewport3D.Children>` property element is skipped ("not supported by the markup compiler"), so `myViewport3D.Children` is empty. |
| AnnotatedDocumentViewer | **New build.** `NotSupportedException: CommandConverter cannot convert from "annotations:AnnotationService.CreateTextStickyNoteCommand"` (`MainWindow.g.cs:75`). A prefixed clr-namespace `Type.Member` command is not resolved (`Markup/CommandConverters.cs`). |
| ExpenseItIntro | Still crashes, further in: `InvalidOperationException: Resource with key 'CustomNavWindowStyle' not found`. `MainWindow.xaml` uses `Style="{StaticResource CustomNavWindowStyle}"`, a key defined nowhere in the sample, so WPF rejects it too. Round 58 fixed both earlier generator causes. What remains is a defect in the sample, not a layer gap. |
| WDPEncoderAndDecoder | Unchanged: `NotSupportedException: JPEG XR (WDP) has no encoder in the GTK runtime`. This is a deliberate limit: gdk-pixbuf/GdkTexture have no JPEG XR codec. |
| AppCompat-Quirks | Unchanged: a window shows at 1.5 s, then a `NullReferenceException` in the sample's `PopulateAppContextInformation` (it reflects over WPF-internal AppContext switch types) exits the app with code 1. A compatibility gap, not worth chasing. |
| LocalizationResources | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. There is only an en-US satellite and no neutral resource. |

Four of the five new crashes are MarkupCompiler or converter work (track 1 below). AnimationExamples is binding-engine
work (track 4).

## XAML generator errors

These are the remaining generator-side causes in portable apps. Per-project messages are in the JSON (`xamlErrors`).

1. **Content property of a project-local type** (Per-FrameAnimation, its only blocker; WPFGallery). For a type declared
   in the project being compiled, the resolver (`XamlCodeGenerator.SymbolResolver.ContentProperty.cs`) finds no
   `[ContentProperty]`, neither inherited (`MagnitismCanvas : Canvas` → `Children`) nor declared
   (`ControlExample` → `ExampleContent`), and it emits `.Content = …` (`CS1061`).
2. **Grid attribute shorthand** (WPFGallery). `RowDefinitions="Auto,*,Auto"` and `ColumnDefinitions="100,2*,*"`, the
   .NET 9 form, are emitted as `__e.RowDefinitions = "Auto,*,Auto"` (`CS0200`). They need a parse into the
   collection.
3. **Skipped property elements that crash at run time** (HexSphereDemo, PhotoFlipperDemo): base-qualified
   `GradientBrush.GradientStops`, an explicit `Viewport3D.Children`, and `ModelVisual3D.Content` in some contexts.
4. **Prefixed command values** (AnnotatedDocumentViewer): `Command="prefix:Type.Member"` with a clr-namespace prefix.
5. **Not generator bugs, though surfaced in `.g.cs`:** `DocumentViewerBase.IsMasterPage` (SlidePuzzleDemo),
   `AutomationProperties.HeadingLevel` (DataBindingDemo, WPFGallery), and `ItemsControl.ItemsPanelProperty` and
   `ToolTipService.ToolTipProperty` (WPFGallery). All are missing layer members.
   - DocumentStructure's `fixedpage1_structure.xaml` is malformed XML, and that app is Windows-only.

**"Skipped property element" backlog.** These are MSBuild messages, not errors, but they are a silent-drop risk at
run time. The 198 final logs that build hold 121 distinct messages in 39 projects; the Round 58 gate counted 84 in 30
across 177 builds, and the 21 new builds account for the rest. The largest groups:

- `Path.Data` (15), `Rectangle.OpacityMask` (8), `TabItem.Header` (8), `Rectangle.Fill` (8), `MenuItem.Header` (5),
  `Image.OpacityMask` (5).
- `TextBlock.TextEffects` (4), `GradientBrush.GradientStops` (4), `Window.CommandBindings` (4).

Fixed since `270cdb7e` (Round 58 lane C): deferred `StaticResource` in dictionary values, `BasedOn` type-key
tolerance, Media3D property elements and nested names (Geometry), names in Geometry/Drawing property elements
(KeySplineAnimations), bound animation values and non-element `SetBinding` emitted through `BindingOperations`,
`x:Shared="False"`, and `Button.ContextMenu`, `Expander.Header` and the `Grid.RowDefinitions`/`ColumnDefinitions`
element form.

## Media3D: known gaps against WPF

These come from Round 54 lane B (`gf54b/media3d`). They are documented, not fixed:

- Hidden surfaces use a back-to-front sort of whole triangles (painter's algorithm), not a per-pixel z-buffer.
- Lighting is flat per triangle, with Blinn-Phong specular. Brush textures use a per-triangle affine mapping, not a
  perspective-correct one.
- `Paced` key times are treated as `Uniform`. `ParallelTimeline.SlipBehavior` is stored only.
- Typed 3D animations (`Rotation3DAnimation`, `Point3DAnimation`, `Vector3DAnimation`, `QuaternionAnimation`) are
  deferred.
- `MeshGeometry3D.TextureCoordinates` is a plain list: editing it in place does not repaint.
- The generated-code path now emits `Viewport3D.Camera` and `ModelVisual3D.Content`/`Transform`/`Children`. It still
  skips an explicit `Viewport3D.Children` element and some nested `ModelVisual3D.Content` (PhotoFlipperDemo,
  HexSphereDemo).

Carry-overs that are still open:

- `Color.FromScRgb` skips the sRGB transfer (`Media/Color.cs:61`, re-checked).
- `ScrollBar` is still on `Control`, not `RangeBase` (`Controls/Primitives/ScrollBar.cs:16`, re-checked).
- `SystemParameters` `*Key` objects do not resolve through `DynamicResource` (carried over, not re-checked).

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole recorded blocker is the track. The
compiler reports nearly all errors in one pass, but a later phase (generated code, a second wall at run time) can
still hold an app back. Treat "only" as an upper bound for build and an estimate for run. Done in full, the six
tracks would take the count to at most 200 build and about 196 run.

**Not eligible: `System.Windows.Automation.Peers` and the `AutomationProperties.HeadingLevel`/`ItemStatus` attached
properties.** They belong to `docs/plans/ui-automation.md`. They block DataBindingDemo alone and WPFGallery in part.

1. **Startup and generator sweep III: 4 crash→runs, 1 only, 1 involved.**
   - Runs: Brushes (unit suffixes in `ParseThickness`), HexSphereDemo (base-qualified `GradientBrush.GradientStops`,
     nested `ModelVisual3D.Content`), PhotoFlipperDemo (explicit `Viewport3D.Children`) and AnnotatedDocumentViewer
     (`prefix:Type.Member` in `CommandConverter`).
   - Builds: Per-FrameAnimation (content property of a project-local type). Involved: WPFGallery (the same lookup for
     `ControlExample`, and the Grid `RowDefinitions`/`ColumnDefinitions` attribute shorthand).
   - Also: work down the "Skipped property element" backlog, starting with `Path.Data`, `*.OpacityMask` and
     `TabItem`/`MenuItem.Header`.
   - Re-scope `gf39/xaml-content-model` as a test-only change (the compile-checked generator test) on top of this
     track. The backlog is its regression corpus.
   - Gate: all 197 building projects, plus the 9 non-runners.
   - Owns: `Gtk4.Forms.MarkupCompiler/**` and `Gtk4.Forms.MarkupCompiler.Tests/**`,
     `Gtk4.Forms.WindowsBase/Markup/XamlValueConverter*.cs`, and `Gtk4.Forms.PresentationFramework/Markup/**` except
     `NonElementBinding.cs` (track 4).
2. **Land `gf57b/documents-imaging-tail`: 4 only, 2 involved.**
   - Only: DocumentMerge (Fixed documents, `Glyphs`, XPS package read/write), PhotoViewerDemo and PhotoStoreDemo
     (`CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`, `TransformedBitmap`), and ParticlesDemo (a freezable
     `ImageSource`, so `RenderTargetBitmap.Freeze`).
   - Involved: SlidePuzzleDemo (`DocumentViewerBase.IsMasterPage`; it still needs track 3) and FontDialogDemo
     (`TextDecorationCollection.Freeze`; it still needs track 3).
   - State: re-applied onto `main` in Round 60 (lane B) without `06cd79ce`, whose detached-List fix `main` already has,
     reviewed and fixed (see the Round 60 hand-offs for the remaining walls of the 6 targets).
   - Owns: `Documents/**` except `Documents/Hyperlink.cs` and `Documents/Adorner*.cs`, `Media/Imaging/**`,
     `Media/ImageSource.cs`, `Media/DrawingImage.cs`, new `System.Windows.Xps/**`, and
     `Controls/Primitives/DocumentViewerBase*.cs`.
3. **Controls and freezable tail III: 1 only, plus 2 once track 2 lands.**
   - Only: ExpenseItDemo (`DataGridCell`, `DataGridCellsPresenter`).
   - With track 2: FontDialogDemo (`TextBlock.ContentStart`/`ContentEnd`, `TextBox.SelectionChanged` as a
     `RoutedEventHandler`) and SlidePuzzleDemo (a typed `SolidColorBrush.Clone()`/`CloneCurrentValue()`).
   - `TextBox.SelectionChanged` is a public-API type change. Check every subscriber in the layer and the tests.
   - Owns: `Controls/DataGrid*.cs` plus new `Controls/DataGridCell*.cs` and
     `Controls/Primitives/DataGridCellsPresenter.cs`, `Controls/TextBlock*.cs` (it reads the `Documents/` pointer API
     and does not edit it), `Controls/TextBox*.cs`, and new `Media/SolidColorBrush.Clone.cs` (or similar typed-clone
     partials outside `Media/Imaging/**`).
4. **Non-element binding targets: 1 crash→runs.**
   - Runs: AnimationExamples (`BindingOperations.SetBinding` on an animation).
   - It also makes the bindings that `NonElementBinding.TrySetBinding` drops today apply their values (freezables,
     animations, `CollectionViewSource`). Those apps already run, but they show the unbound value.
   - Owns: `Data/**`, `Markup/NonElementBinding.cs` (carved out of track 1), and the binding storage in
     `Gtk4.Forms.WindowsBase/Freezable.cs`/`DependencyObject` if the engine needs it.
5. **Packaging: 1 only.**
   - Only: EditingExaminerDemo. Add `System.Security.Permissions` as a `Gtk4.Forms.Wpf` dependency for
     `PermissionSet`/`PermissionSetAttribute`, as Round 58 did with `Microsoft.Win32.SystemEvents`.
   - Check first: that the package's `net10.0` reference assembly carries both types, and that `SYSLIB0003` stays a
     warning. This reverses the earlier "deliberately not next" call for this sample, because the fix is now a
     one-line dependency.
   - Owns: `Gtk4.Forms.Wpf/Gtk4.Forms.Wpf.csproj` and its package tests.
6. **.NET 9 WPF surface for WPFGallery: 0 only, 1 involved.**
   - Covers: `Application.ThemeMode` and `ThemeMode`, the `SystemColors.AccentColor*BrushKey` family, the
     `MessageBoxButton` values `AbortRetryIgnore`/`RetryCancel`/`CancelTryContinue`, `Application.GetResourceStream`,
     `ItemsControl.ItemsPanelProperty`, `ToolTipService.ToolTipProperty`, `FocusManager.IsFocusScopeProperty`,
     `ContentPresenter.RecognizesAccessKey`, a settable `RequestNavigateEventArgs.Handled`, and `Window.IsActive`.
   - WPFGallery also needs track 1 (Grid shorthand, `ControlExample`) and the UI automation plan (`HeadingLevel`).
     Expect a further wall behind this one.
   - Owns: `Application*.cs`, new `ThemeMode.cs`, `Media/SystemColors*.cs`, `MessageBox*.cs`,
     `Controls/ItemsControl*.cs`, `Controls/ToolTipService*.cs`, `Input/FocusManager*.cs`,
     `Controls/ContentPresenter*.cs`, `Navigation/RequestNavigateEventArgs.cs`, and `Window*.cs`.

**Round 60 (lane B) hand-offs.** Tracks 2 and 4 are in `lane/b` (the documents/imaging tail re-applied from `gf57b`,
and the binding engine). The walls below were measured after that round. None of them is in lane B's files.

- **For lane C (MarkupCompiler and `XamlValueConverter`; notes only, no code was written in lane B):**
  - Register `Int32RectConverter` and `PixelFormatConverter` in `XamlValueConverter`, add `Int32Rect` and `PixelFormat`
    to the MarkupCompiler `ConverterBackedStructs` list (a drift test enforces the mirror), then remove both types from
    the Round49a audited list in `Round49aConverterTests`.
  - ParticlesDemo throws a `NullReferenceException` on an `x:Name` inside a `Model3D` tree (generated code).
  - AnimationExamples now gets past the binding wall and stops on "Cannot find the element named
    TextEffectRotateTransform": an `x:Name` inside `TextEffect.Transform` is not registered in the name scope.
- **For lane A:**
  - DocumentMerge needs `UIElement.TranslatePoint`.
  - PhotoStoreDemo now builds but exits: `AdornerLayer.GetAdornerLayer(CurrentPhoto)` returns null (`MainWindow.cs:38`).
    The `AddVisualChild` and `Rect.Size`/`Rect.Location` walls are gone.
  - SlidePuzzleDemo needs a typed `SolidColorBrush.Clone()` (`PuzzleGrid.cs:216`).
  - FontDialogDemo needs `TextBlock.ContentStart`/`ContentEnd` and a `RoutedEventHandler`-typed
    `TextBox.SelectionChanged`.
- **Known documents/imaging gaps (lane B, not scheduled):**
  - `DocumentPageView` does not paint a `FixedPage`; only `DocumentViewer` hosts one, as a widget.
  - `TransformedBitmap` handles only 90-degree rotations and scales.
  - `CroppedBitmap`, `FormatConvertedBitmap` and `TransformedBitmap` do not follow later pixel changes in their
    `Source`.
  - XPS resource-dictionary parts are copied verbatim by `CopyPage`; only the page's own references are rewritten.
  - Brushes returned by `Brushes.*` are new, unfrozen instances here (WPF returns frozen shared ones).
- **Binding engine gaps (lane B, not scheduled):**
  - A freezable shared by several holders has no inheritance context, as in WPF. A binding on it resolves only when
    its context comes from one holder (a `ResourceDictionary` owner counts as a holder).
  - A single-segment path to a non-dependency property of an element source is read once.
- **Rejected in the Round 60 triage: the null-owner `InvalidOperationException` in `TextTreeEdit.Split.cs`.** Not a
  defect. After dropping `06cd79ce`, `main`'s detached-`List` fix (Round 57, `2f4a566d`) covers
  `ListItemCollection.Remove`/`Clear`, and the `ListItemCollectionClear` tests pass in the merged lane (975 passed). The
  null owner has no reachable path: a paragraph in a table cell fails earlier with "The element is not in a text
  container". There is no failing test and no genuine delta to port.

**Lanes (three in parallel, two tracks each, file-disjoint):**

| Lane | Tracks | Only | Boundary to watch |
|---|---|---:|---|
| A | 3 (controls and freezable tail III) + 6 (.NET 9 surface) | 1 (+2 after track 2) | Track 3 owns `DataGrid*`, `TextBlock*`, `TextBox*` and the brush typed-clone partials. Track 6 owns `ItemsControl*` (so `DataGrid`'s base), `ContentPresenter*`, `ToolTipService*`, `FocusManager*`, `Application*`, `Window*`, `MessageBox*` and `SystemColors*` |
| B | 2 (land `gf57b`) + 4 (non-element bindings) | 4 + 1 crash→runs | Track 2 owns `Documents/**` minus `Hyperlink.cs`/`Adorner*.cs`, `Media/Imaging/**` and `Media/ImageSource.cs`. Track 4 owns `Data/**` and `Markup/NonElementBinding.cs` |
| C | 1 (generator sweep III) + 5 (packaging) | 1 + 4 crash→runs, and 1 | Track 1 owns `Gtk4.Forms.MarkupCompiler/**` and `Markup/**` minus `NonElementBinding.cs`. Track 5 owns only the `Gtk4.Forms.Wpf` package project |

Held branches:

- **`gf57b/documents-imaging-tail`: re-applied in Round 60 (lane B)** (3 of its 4 commits, onto `main`). The old
  branch ref is superseded; delete it once `lane/b` is merged.
- **`gf52b/content-bindings-sweep`: merged** (`b25bc71e`, before `270cdb7e`). The branch ref is gone. Nothing to do.
- **`gf46/input-focus` (held, 6 commits, 182 behind): 0 samples.** Every Input and Commands app builds and runs. What
  remains is focus-transaction behaviour, owned by the focus-behaviour design. It overlaps track 6 on
  `Input/FocusManager*.cs`, so if it is reworked, do it after lane A and against `main`. It is not in the lanes.
- **`gf39/xaml-content-model` (held, 3 commits, 235 behind): 0 samples on its own.** Fold it into track 1 as a
  test-only change.
- **`gf53a/visual-layer-input` (1 commit, 77 behind): superseded.** Its commit `d4e0ee3c` landed as `a700a7ef` through
  `gf56a/visual-layer-input` (`a1bb8214`). Delete the ref after confirming that nothing unique remains on it.

Deliberately not next:

- JPEG XR encoding (WDPEncoderAndDecoder). There is no codec in the GTK stack.
- System.Xaml/BAML tooling (LocBaml, BamlReflector/BamlTools).
- AppCompat-Quirks (WPF-internal AppContext switches), LocalizationResources (the sample ships no neutral resource) and
  ExpenseItIntro (the sample references an undefined resource key).
- Legacy (non-SDK) csproj conversion: 21 projects. That is a converter feature, not a layer gap. It would also remove
  the `GtkFormsXamlRegistration` artefact from the symbol ranking.

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches:
  1. Data Binding, Getting Started, Events.
  2. Graphics, Animation, Visual Layer, Speech and Media, Clipboard.
  3. Sample Applications, Migration and Interoperability.
  4. Input and Commands, Documents, Drag and Drop, Compatibility.
  5. Globalization and Localization, Windows, Accessibility, Element Tree, PerMonitorDPI.
  6. Application Management, Elements, Properties, Resources, Styles & Templates, Threading, Tools.

  Each batch was copied under `.claude/samples-remeasure-r58/batchN` without `bin`/`obj`, `NuGet.config` or
  `global.json`. The scratch root has empty `Directory.Build.props`/`.targets`, so `TreatWarningsAsErrors` was off. No
  original or tracked file was modified: `git status` is clean in the worktree and in `WPF-Samples`.
  - Batches 1, 3 and 5 each discarded a first attempt that had a script bug and wrote no logs. Each re-ran from fresh
    copies, and only the clean runs are used.
- **Feed, pinned.** `samples-remeasure-r58/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-remeasure-r58/.nuget-packages`, which started empty) and cleared
    `fallbackPackageFolders`.
  - Cleared `packageSources`, then the pinned feed (`samples-remeasure-r58/feed`) and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the recipe `nuget.config`, but with
  `globalPackagesFolder` pointed at an empty probe cache, so the shared cache stayed untouched.
  - Build 1 exited 1, as expected: the converter gave `MSB3073` (exit 10), then "re-run the build". Build 2 exited 0
    with 0 warnings and 0 errors.
  - The cached SHA-256 values equal the feed's. They were re-computed for this report from the feed and the shared
    batch cache:
    - `Gtk4.Forms.Wpf` `3c623a4db14856ab9693366cc71c594b170febb144109c5d630b843d37b389d1`
    - `PresentationFramework` `b2ecf2a42768018eeb5b91d5144fb564d3408c45e2e04434a6c35241c4a3bb5f`
    - `WindowsBase` `2777984d2739279a25c887eec2123f59a094c306e96ddcc42a86dbc499b0e433`
    - `MarkupCompiler` `332a962d55b21f797526d02b871698407e860fe767c2f5affea77ecf0dd54b43`
  - Every `gtk4.forms.*` `.nupkg.metadata` source is the scratch feed.
  - `project.assets.json` lists only the isolated probe cache as a package folder and no fallback folders. Its sources
    are the feed, nuget.org and the SDK-implicit `library-packs`.
  - `gtksharp4 4.22.4.26325` came from nuget.org.
  - Smoke: a cold first 10 s run showed no window. Alone for 20 s, and again for 10 s, the probe was alive with 1
    visible unowned top-level window (from t=2 s) and 0 bytes of stderr.
  - The probe directory and its cache were deleted afterwards. Every batch re-checked the cached `Gtk4.Forms.Wpf` hash.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26327`.
  2. Build once with `-nodeReuse:false`. The converter rewrites the csproj and stops with exit 10.
  3. Build again. This result is recorded.
  - Nothing else is changed. Original csproj files are kept beside the copies (`*.csproj.orig`).
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes, symbols and messages were
  re-parsed uniformly from the final logs of all six batches (252 logs, one per csproj; UTF-16 and UTF-8). Batch 6's
  script lost its exit codes, which does not matter because the logs are authoritative.
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime
  (`%LOCALAPPDATA%\Gtk\4.22.4\bin`) on `PATH`.
  - "Runs" means alive with a visible, unowned top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception, or an exit with no window.
  - "Hang" means alive with no window and no exception.
  - Apps with no window at 10 s were re-run alone for 20 s.
  - Batches 1, 2, 4, 5 and 6 left per-app run records, and every built app in them was cross-checked against those
    records. Batch 3's 12 run results come from its report: 10 runs, 2 crashes, none without a window.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification of the earlier reports, unchanged.
- **Limits.**
  - Only Windows was measured.
  - Legacy csproj files are not converted: 21 projects.
  - Moves are attributed by elimination, not bisected.
  - `blockerThemes` is assigned per project for the 14 failing portable projects (13 apps and the library BamlTools).

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
| 2026-11-23 | `85493c8b` | 1.0.0.26327 | **yes** | **191 / 204** | **182** | Rounds 56–58 (second three-lane batch); full re-measure; 9 crash, 0 hang; no regressions |

## Known layer defects found by the UI tests

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`. WPF re-queries commands afterwards, so the button is enabled. The layer calls nothing like
  `CommandManager.InvalidateRequerySuggested` there, so the button stays disabled until another requery happens.
  `CommandSampleTests` keeps a workaround test (it requeries itself) and a skipped test,
  `The_button_is_enabled_without_a_manual_requery`, that asserts the correct behaviour. Un-skip it when fixed.
