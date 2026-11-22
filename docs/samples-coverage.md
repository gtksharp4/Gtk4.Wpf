# WPF samples coverage

Measured 2026-11-22 on `main` at `466055e33f9a38db98b8391607ddce7f04801c8a` (Rounds 53–55, the first three-lane batch,
plus the `gf52b/content-bindings-sweep` merge and the lane review follow-ups), package `Gtk4.Forms.Wpf 1.0.0.26326`
from a **pinned** local feed, against the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4
native C++ projects). All 252 csproj were rebuilt and every built app was smoke-run. Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report `f3ea89fc` (main
`30f46d4a`, Rounds 49–52, `previous`).

## Headline

**171 of 204 portable official WPF sample apps build unmodified, and 165 run.** That is up from 159 / 152 in report
`f3ea89fc` (+12 build, +13 run), and from 0 / 0 in report `fedd13c8` (2026-11-19).

- **12 apps moved fail→pass**: 9 run and 3 crash at startup. **4 built apps moved crash/hang→runs.** The libraries
  HostingWpfUserControlInWf and WPFControls also build now.
- **No regressions.** No project went pass→fail, and no app went runs→crash/hang.
- **No hangs are left.** Of the 6 apps that build but do not run, all crash. 3 are new builds whose cause is in the
  MarkupCompiler (see Crashes); the other 3 are the known, deliberate or out-of-scope ones.
- 176 projects build in all: the 171 portable apps, the libraries CustomApplicationClient, wpfclock,
  HostingWpfUserControlInWf and WPFControls, and the Windows-only console app FocusTracker.
- By lane: Round 55 lane C (startup and generator sweep, controls and media tail) accounts for 4 fail→pass and all
  4 crash/hang→runs. Round 54 lane B (Media3D, documents text tail) accounts for 5 fail→pass, Round 53 lane A
  (items and templates) for 2, and the `gf52b` merge for 1 (CommonDialogs).
- **Round 53's track 1 (`gf53a/visual-layer-input`) never reached `main`.** That one commit covers the blockers of
  12 of the 33 failing portable apps (see Next rounds, track 1).
- FontProperties and FlowDocumentProperties were already recorded as building by the Round 54b lane gate. This is the
  first full measure that includes them.

### Corrections to the raw batch data

The final build log of each of the 252 csproj was re-parsed uniformly. Run results were taken from each batch's
per-project records, and every run's stdout/stderr was scanned for unhandled exceptions. The top 15 missing symbols
were checked against the `Gtk4.Forms.*` source.

- **The aggregated totals were wrong again.** They said 202 portable apps, 168 build and 162 run. The per-project data
  gives 204 / 171 / 165:
  - 202 came from the batches' own regex `windowsOnly` flags. As in every earlier report, the verified
    classification is carried over unchanged, so the denominator stays 204.
  - The batch summaries counted builds and runs in different ways, and batch 5 lost its build exit codes. The re-parse
    of all 252 final logs is authoritative.
- **WPFGallery's batch result was not a valid measurement.** Its build 1 stopped after restore (the log ends at
  "Determining projects to restore"), so build 2 was the converting build ("converted … re-run the build"). A third
  build was run alone for this report and is recorded: only `CS1069` on
  `Microsoft.Win32.UserPreferenceChangedEventArgs` (6 files) remains, which is a missing `Microsoft.Win32.SystemEvents`
  package reference. `UserControl` and `System.Windows.Shell` no longer appear.
- **Legacy (non-SDK) csproj files stay `not-built`.** Highlighter "builds and runs" again, but as real .NET Framework,
  so it is not counted. The attempt is kept under `legacyBuild`.
- **FocusTracker** is a Windows-only console app that prints a prompt and exits. It is recorded as `crash` as before and
  is not in the portable denominator.
- **Top-15 symbol check** (the aggregated `topMissing` ranking):
  - Windows-only, dropped from the ranking: `System.Windows.Forms` (10), `Form` (6), `System.Windows.Interop` (3) and
    `DpiScale` (3, PerMonitorDPI only).
  - Artefact: `System.Windows.Markup.GtkFormsXamlRegistration` / `…Attribute` (7 each). The attribute exists
    (`Gtk4.Forms.PresentationFramework/Markup/GtkFormsXamlRegistry.cs:13`, emitted by
    `Gtk4.Forms.MarkupCompiler/XamlCodeGenerator.ImplicitClass.cs:113`). All 7 hits are legacy, Windows-only
    csproj files built against real WPF with the layer's generated `.g.cs`. The symbol is new to the ranking because
    Round 55 (`fe22f709`) registers every `x:Class` root, so every generated root now emits it. It replaces
    `XamlValueConverter`, the previous artefact.
  - Real gaps, absent from `main`:
    - `MouseEventArgs.MouseDevice` / `MouseButtonEventArgs.MouseDevice` (4). There is no `MouseDevice` type; it is
      only mentioned in a comment in `Input/Mouse.Capture.cs`.
    - `BeginStoryboard` (3, `CS1955`). `FrameworkElement.BeginStoryboard(Storyboard)` does not exist, so the name binds
      to the trigger-action class `Triggers/BeginStoryboard.cs`.
    - `HitTestResultBehavior` (2).
    - The `EditBox.OnMouseEnter`/`OnMouseLeave`/`OnMouseUp` overrides (2 each). `UIElement` has no protected
      `OnMouse*` virtuals.
    - `CachedBitmap` and `CroppedBitmap` (2 each).
    - `HitTestResultBehavior` and the `OnMouse*` virtuals are on the unmerged `gf53a/visual-layer-input`. The others
      are not.
- **Also found while checking:** `SortDescription.PropertyName`/`Direction` are still get-only
  (`Data/SortDescription.cs:13,16`). That is AnnotatedDocumentViewer's only blocker, and the Round 54 request to
  lane A is still open. `Binding.Path` is still typed `string?` (`Data/Binding.cs:38`). That is MediaGallery's only
  blocker.

## Per category

These are portable apps on the strict recipe. Δ is the change since `f3ea89fc`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 2 | 0 | 2 | 0 |
| Animation | 11 | 0 | 11 | 9 | +1 | 8 | 0 |
| Application Management | 10 | 1 | 8 | 8 | 0 | 8 | 0 |
| Clipboard | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 30 | +1 | 30 | +1 |
| Documents | 17 | 2 | 15 | 12 | +2 | 12 | +2 |
| Drag and Drop | 6 | 0 | 6 | 5 | 0 | 5 | +1 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 9 | 0 | 9 | 0 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 7 | +1 | 6 | +1 |
| Globalization and Localization | 14 | 0 | 14 | 14 | +1 | 13 | +1 |
| Graphics | 26 | 0 | 25 | 21 | +2 | 19 | +1 |
| Input and Commands | 18 | 1 | 16 | 16 | 0 | 16 | 0 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Resources | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Sample Applications | 23 | 1 | 21 | 7 | 0 | 7 | +2 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 5 | +1 | 5 | +1 |
| Threading | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 9 | +3 | 9 | +3 |
| **Total** | **256** | **39** | **204** | **171** | **+12** | **165** | **+13** |

## What moved

Each move is attributed by elimination: the named change removed every blocker that `f3ea89fc` recorded for the app.
Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass: 12 apps and 2 libraries.**

| Change | Apps (run result) |
|---|---|
| Round 55 lane C startup-generator-sweep (`43208b5a`): root attached property through its owner type, `NavigationWindow.Source` owned from `Frame.SourceProperty` | Wizard, GlobalizationHomepage (run) |
| Round 55 lane C controls-media-tail (`33c0326c`): `MediaElement`, `UserControl` | WindowActivationAndDeactivation (runs); libraries HostingWpfUserControlInWf, WPFControls |
| Round 55 lane C, both tracks: `NavigationWindow.Source`, the `DataGrid` tail, `MenuItem` on `Control`, `Popup`, `SystemParameters` | ExpenseItIntro (crash) |
| Round 54 lane B media3d (`44cd947e`): `System.Windows.Media.Media3D`, `Viewport3D`, 3D key frames | Converter (runs); Geometry, KeySplineAnimations (crash) |
| Round 54 lane B documents-text-tail (`767cbdec`): `FontFamily`-typed `FlowDocument`/`TextElement` typography | FlowDocumentProperties, FontProperties (run) |
| Round 53 lane A items-templates (`9d6ca70d`): `Binding.UpdateSourceExceptionFilter`, `FrameworkTemplate.FindName`, `ItemContainerGenerator`, `ItemCollection.CurrentItem` | BindValidation, FindingElementsInTemplates (run) |
| `gf52b/content-bindings-sweep` merge (`b25bc71e`): `Run.TextProperty` and `SetBinding` on flow content | CommonDialogs (runs) |

**Crash/hang→runs: 4 apps, all from Round 55 lane C startup-generator-sweep (`43208b5a`).**

- ControlsAndLayout: scRGB `sc#` colours in `MediaConverters.ParseColor`.
- CustomComboBox: `{StaticResource}` in a `ResourceDictionary` root resolves against its own keys.
- GeometryDesignerDemo: `Visual` property elements of media objects are emitted, so the named `VisualBrush` is built and
  registered.
- DragDropThumbOps: every `x:Class` root is registered in `GtkFormsXamlRegistry`. This was the last hang.

**Moved blockers in apps that still fail.** Media3D and the controls tail cleared walls that exposed the next ones:

- CubeAnimationDemo, HexSphereDemo and PhotoFlipperDemo now need `MouseDevice` and `FrameworkElement.BeginStoryboard`.
- ParticlesDemo now needs `RenderTargetBitmap.Freeze`.
- GraphingCalculatorDemo now needs `MouseDevice`/`KeyboardDevice`, the `Rect` edges and `TextBox.MaxLines`.
- SlidePuzzleDemo now needs `DocumentViewerBase.IsMasterPage`, `ComboBoxItem.Tag` and `BeginStoryboard`.
- DataBindingDemo now needs `CollectionViewSource.SetBinding` and `AutomationProperties.HeadingLevel`/`ItemStatus`.
- MediaGallery, AnimationExamples, Concepts, DialogBox, AnnotationsStyling and AnnotatedDocumentViewer are each down to a
  single blocker.

**Error landscape.** Counted as portable failing apps per code, `f3ea89fc` → now:

- Down: `CS0246` 25 → 12, `CS0234` 13 → 5, `CS1061` 16 → 12, `CS0117` 11 → 4, `CS0103` 7 → 3, `CS0029` 5 → 2,
  `CS0400` 2 → 1.
- Gone: `CS5001` (Concepts) and `CS1729`.
- Flat: `CS0115` 4 → 4 (the overridable `LogicalChildren`, `OnRender` and `OnMouse*`, all on `gf53a`), `CS0200` 1,
  `CS0123` 1, `CS1069` 1 (WPFGallery), `CS1503` 1.
- New: `CS1955` 0 → 3 (`BeginStoryboard` called as a method, revealed by Media3D).
- `CS2002` (a generated `.g.cs` specified twice) is a warning in 25 of 171 portable builds (23 of 159 before).

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang.

Nine apps showed no window in their first 10 s run under parallel load and showed one when re-run for 20 s:
FindingSourceElement, AnimationTiming, PathAnimations, ClipboardViewer, ShapeElements, KeyStrokeCounter, FindText,
FlowDirection and Wizard. They are recorded as `runs`, as in earlier reports.

## API demand

The counts are failing portable apps (33). Windows-only and legacy-csproj demand is excluded. "Apps" is the number of
failing portable apps whose errors touch the theme (from `blockerThemes`, a regex ranking aid). "Only" is the number
whose every recorded blocker is in the theme.

| Theme | Symbols (apps) | Apps | Only |
|---|---|---:|---:|
| 2D graphics, visual layer and input | **On `gf53a`:** `RoutedEventArgs.OriginalSource`, `GeneralTransform`/`TranslatePoint`, hit testing (`HitTestResultBehavior`, `PointHitTestParameters`, `GeometryHitTestParameters`/`Result`), `CompositionTarget`, `ThicknessConverter`/`BrushConverter`/`RectConverter`, `Mouse.SetCursor`/`Synchronize`, `AddVisualChild`, `Rect.Size`/`Location`/edges, `ColorInterpolationMode`, the `BitmapEffect` family, the overridable `LogicalChildren`/`OnRender`/`OnMouse*`, `Adorner.GetDesiredTransform`. **Not on `gf53a`:** `MouseDevice`/`KeyboardDevice` on the event args (4), `FrameworkElement.BeginStoryboard` (4), `RenderTargetBitmap.Freeze` | 19 | 16 |
| Controls tail | `Hyperlink.TargetName` (Concepts), `FontStyleConverter`/`FontWeightConverter` (DialogBox), `InkCanvasEditingMode` (AnnotationsStyling), `TextBox.TextAlignment`/`MaxLines`, `ComboBoxItem.Tag` | 6 | 3 |
| Documents and text | Fixed/XPS (`FixedDocument`, `FixedPage`, `PageContent`, `DocumentReference`, `XpsDocument`: DocumentMerge), `DocumentViewerBase.IsMasterPage`, `TextBlock.ContentStart`/`ContentEnd`, `TextDecorationCollection.Freeze` | 3 | 1 |
| Items and data | `SortDescription` settable (AnnotatedDocumentViewer), `Binding.Path` typed `PropertyPath` (MediaGallery), `CollectionViewSource.SetBinding` | 3 | 2 |
| XAML generator | `{Binding}` on an animation's `To` emitted as a string (AnimationExamples); `SetBinding` emitted on a non-`FrameworkElement` `DependencyObject` (DataBindingDemo) | 2 | 1 |
| Imaging tail | `CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`, `TransformedBitmap` (PhotoViewerDemo, PhotoStoreDemo) | 2 | 1 |
| System.Xaml/BAML tooling | `System.Xaml` (`XamlSchemaContext`, `XamlType`, `Baml2006Reader`, …), `System.Windows.Markup.Localizer` | 2 | 2 |
| Packaging | `Microsoft.Win32.SystemEvents` (WPFGallery, `CS1069`) | 1 | 1 |
| Automation | `AutomationProperties.HeadingLevel`/`ItemStatus` (DataBindingDemo, with `CollectionViewSource.SetBinding`) | 1 | 0 |
| Code access security | `PermissionSet`/`PermissionSetAttribute` (EditingExaminerDemo; on .NET these come from the `System.Security.Permissions` package, not WPF) | 1 | 1 |

## Crashes (built, did not run)

| Sample | Cause |
|---|---|
| ExpenseItIntro | **New build.** `InvalidOperationException: Resource with key 'NavButtonFocusVisualStyle' not found`, from `NavigationWindowKeyboardAccessibleStyle.InitializeComponent` (a merged `ResourceDictionary`). The key is defined nowhere in the sample. It is used only by `NavigationWindowForwardButtonStyle`, which is reached only through a template. WPF creates dictionary values lazily, so the lookup never runs. The generated code builds every keyed value eagerly, and `StaticResourceScope.Find` throws. |
| Graphics/Geometry | **New build.** `NullReferenceException` at `SampleViewer.SampleSelected` (`SampleViewer.cs:72`, `myGeometry.Positions = points`). The MarkupCompiler drops the `Viewport3D.Camera` and `ModelVisual3D.Content` property elements, and it never assigns the `x:Name` field of the nested `MeshGeometry3D`. This is the generator limit that Round 54 lane B recorded. |
| KeySplineAnimations | **New build.** `InvalidOperationException: Cannot find the element named 'TimeProgressTransform' to animate 'X'`. The target is a `TranslateTransform` named inside `LineGeometry.Transform` in a `GeometryDrawing` (`SplineExample.xaml:190`). Names on freezables nested in Geometry/Drawing property elements are not registered, so `Storyboard.TargetName` does not resolve. |
| WDPEncoderAndDecoder | Unchanged: `NotSupportedException: JPEG XR (WDP) has no encoder in the GTK runtime`. This is a deliberate limit: gdk-pixbuf/GdkTexture have no JPEG XR codec. |
| AppCompat-Quirks | Unchanged: `NullReferenceException` in the sample's `PopulateAppContextInformation`, which reflects over WPF-internal AppContext switch types. A compatibility gap, not worth chasing. |
| LocalizationResources | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. There is only an en-US satellite and no neutral resource. |

The three new crashes are all MarkupCompiler work (track 4 below).

## XAML generator errors

These are the remaining generator-side causes in portable apps. Per-project messages are in the JSON (`xamlErrors`).

1. **`{Binding}` on an animation property** (AnimationExamples, its only blocker).
   `To="{Binding ElementName=SampleDisplayBorder, Path=ActualWidth}"` on a `DoubleAnimation` is emitted as
   `__anim.To = "{Binding …}"` (`CS0029`, string to `double?`). It needs `BindingOperations.SetBinding` on
   `DoubleAnimation.ToProperty`.
2. **`SetBinding` on a non-`FrameworkElement`** (DataBindingDemo). A `CollectionViewSource` resource with
   `Source="{Binding …}"` is emitted as `__e.SetBinding(…)`. `CollectionViewSource` derives from `DependencyObject`, so
   the call needs `BindingOperations.SetBinding`.
3. **Media3D property elements dropped** (Geometry crash). `Viewport3D.Camera`, `ModelVisual3D.Content` and
   `ModelVisual3D.Transform` are not emitted, and `x:Name` on nested freezables is declared but never assigned. The
   runtime `XamlReader` builds the same scene correctly (`Media3DXamlTests`).
4. **Names inside Geometry/Drawing property elements not registered** (KeySplineAnimations crash).
5. **Eager `StaticResource` in dictionary values** (ExpenseItIntro crash). See Crashes.
6. **Not generator bugs, though surfaced in `.g.cs`:** `Hyperlink.TargetName` (Concepts), `InkCanvasEditingMode`
   (AnnotationsStyling), `TextBox.TextAlignment` (FontDialogDemo), `TextBox.MaxLines` (GraphingCalculatorDemo),
   `DocumentViewerBase.IsMasterPage` and `ComboBoxItem.Tag` (SlidePuzzleDemo), `AutomationProperties.HeadingLevel`
   (DataBindingDemo). All are missing layer members.
   - DocumentStructure's `fixedpage1_structure.xaml` is malformed XML, and that app is Windows-only.
   - The "Skipped property element … not supported by the markup compiler" lines (e.g. `Button.ContextMenu`,
     `Expander.Header` in Concepts; `RowDefinition`/`ColumnDefinition` in UsingSpeech) are MSBuild messages, not
     errors. They are a silent-drop risk at run time, so treat them as a generator backlog.

Fixed since `f3ea89fc` (Round 55 lane C): root attached-property bindings (Wizard), `NavigationWindow Source="…"`
(GlobalizationHomepage), the `ResourceDictionary` root scope (CustomComboBox), `x:Class` root registration
(DragDropThumbOps), `Visual` property elements of media objects (GeometryDesignerDemo), and `CS5001` for an
`ApplicationDefinition` without `x:Class` (Concepts).

## Media3D: known gaps against WPF

From Round 54 lane B (`gf54b/media3d`). These are documented, not fixed:

- Hidden surfaces use a back-to-front sort of whole triangles (painter's algorithm), not a per-pixel z-buffer.
- Lighting is flat per triangle, with Blinn-Phong specular. Brush textures use a per-triangle affine mapping, not a
  perspective-correct one.
- `Paced` key times are treated as `Uniform`. `ParallelTimeline.SlipBehavior` is stored only.
- Typed 3D animations (`Rotation3DAnimation`, `Point3DAnimation`, `Vector3DAnimation`, `QuaternionAnimation`) are
  deferred.
- `MeshGeometry3D.TextureCoordinates` is a plain list: editing it in place does not repaint.
- The generated-code path drops 3D property elements (see XAML generator errors, item 3).

Carry-overs from the Round 55 lane C review that are still open:

- `Color.FromScRgb`/`ScR`/`ScG`/`ScB` skip the sRGB transfer (`Media/Color.cs`).
- `SystemParameters` `*Key` objects do not resolve through `DynamicResource`.
- `ScrollBar` is still on `Control`, not `RangeBase`.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole recorded blocker is the track. The
compiler reports nearly all errors in one pass, but a later phase (generated code, a second wall at run time) can
still hold an app back. Treat "only" as an upper bound for build and an estimate for run.

**Not eligible: `System.Windows.Automation.Peers`.** It belongs to `docs/plans/ui-automation.md`. The
`AutomationProperties.HeadingLevel`/`ItemStatus` attached properties (DataBindingDemo) belong there too. Hand them to
that plan; they are not a parity track.

1. **Land the visual layer and input track (`gf53a/visual-layer-input`): 12 only.**
   - Only: Per-FrameAnimation, DragDropObjects, OverridingLogicalTree, ThicknessConverter, BitmapEffectsGallery, Brushes,
     ImageBrush, VisualBrush, ExpenseItDemo, LayoutTransitionsDemo, CompositionTarget, DrawingVisual. Also clears part of
     PhotoStoreDemo (`AddVisualChild`, `Rect.Size`/`Location`) and GraphingCalculatorDemo (`Rect` edges).
   - **Round 56a result (`gf56a/visual-layer-input`, cherry-pick of `d4e0ee3c` plus the review fixes F1-F5).** Gate on
     the 176 building projects plus the 12 targets, package `1.0.0.26326` packed from the worktree, pinned feed:
     186 of 188 build, and 174 of 175 run (all 165 earlier runners still run).
     - **9 of the 12 build and run:** DragDropObjects (needed a new `AdornerLayer.Update`), OverridingLogicalTree,
       ThicknessConverter, BitmapEffectsGallery, ImageBrush, VisualBrush, LayoutTransitionsDemo, CompositionTarget,
       DrawingVisual.
     - **Brushes builds but crashes at startup.** A `Setter` for `Margin="12pt"` reaches `PrimitiveConverters.ParseThickness`,
       which has no unit suffixes. Fix in lane C's `Markup/XamlValueConverter.Primitives.cs`: parse each token with
       `LengthUnits.Parse`. (Its `F1 M ...` geometry was fixed here: the path parser now reads `F0`/`F1`.)
     - **Per-FrameAnimation still fails to build.** The generator emits `.Content = ...` for children of
       `MagnitismCanvas : Canvas`, a user subclass of a panel. Lane C (MarkupCompiler content-property lookup).
     - **ExpenseItDemo still fails to build.** `EditBox.cs` uses `Mouse.PrimaryDevice.LeftButton`; that needs `MouseDevice`
       (track 2).
   - Lane B can now apply `CI-2` (the `Context` shim is kept until then).
   - Owns: `Gtk4.Forms.WindowsBase/RoutedEventArgs.cs`, `Rect.*.cs`, `RectConverter.cs`, `ThicknessConverter.cs`,
     `ConverterParsing.cs`; `UIElement.*.cs` (`MouseVirtuals`, `OnRender`, `RenderLayer`, `VisualGeometry`,
     `BitmapEffect`); `FrameworkElement.LogicalChildren.cs`, `LogicalTreeHelper.cs` and the
     `Controls/*.LogicalChildren.cs` partials; `Media/Visual*.cs`, `Media/VisualTreeHelper*.cs`, `Media/HitTestTypes.cs`,
     `Media/GeneralTransform*.cs`, `Media/CompositionTarget.cs`, `Media/BrushConverter.cs`, `Media/Effects/**`,
     `Media/DrawingContext*.cs`, `Input/Mouse.Cursor.cs`, and `Documents/Adorner.cs` + `Adorner.DesiredTransform.cs`
     (the `CI-3` carve-out).
   - After it lands, lane B applies `CI-2`: `FlowDocumentPaginator` uses the `DrawingContext` that `RenderOpen` returns,
     and the `Context` shim is deleted.
2. **Input devices and storyboard entry points: 3 only, 2 involved.**
   - Only: CubeAnimationDemo, HexSphereDemo, PhotoFlipperDemo (`MouseEventArgs.MouseDevice`, `FrameworkElement.BeginStoryboard`).
     Involved: GraphingCalculatorDemo (also `KeyEventArgs.KeyboardDevice`, `TextBox.MaxLines` from track 3, and
     track 1's `Rect` edges) and SlidePuzzleDemo (`BeginStoryboard`, plus tracks 3 and 6).
   - Covers: a `MouseDevice`/`KeyboardDevice` surface on `InputDevice`, exposed from the input event args;
     `FrameworkElement.BeginStoryboard(Storyboard[, HandoffBehavior, bool])`.
   - Owns: `Input/MouseEventArgs.cs`, `Input/KeyboardEventArgs.cs`, `Input/InputDevice.cs`, new `Input/MouseDevice.cs`,
     new `FrameworkElement.BeginStoryboard.cs`. It must not touch `Input/Mouse.Cursor.cs` or `UIElement.*` (track 1).
   - **Round 56a, done (branch `gf56a/input-devices-storyboard`, 16 tests).** `MouseDevice`/`Mouse.PrimaryDevice`;
     `MouseEventArgs : InputEventArgs` and `KeyEventArgs : KeyboardEventArgs` with WPF's device constructors (args built
     that way read the device live); `FrameworkElement.BeginStoryboard` (3 overloads). Gaps kept: no `StylusDevice`
     overload, no `PresentationSource` constructor on `KeyEventArgs`, and no `FrameworkContentElement.BeginStoryboard`
     (`Storyboard.Begin` has no content-element overload).
   - Measured: CubeAnimationDemo builds and runs. HexSphereDemo and PhotoFlipperDemo now build (CS1955 and the device
     errors are gone) but exit at start-up: HexSphere on `'' is not a valid value for property 'Background'`,
     PhotoFlipper on a `Visual3DCollection` index in `OnLoaded`. GraphingCalculatorDemo and SlidePuzzleDemo no longer report the
     `MouseDevice` or `BeginStoryboard` error as their first error (only the first error was read); they fail on `Rect` edges (CS1503) and `ComboBoxItem.Tag`.
     All other apps that built and ran still do (CustomApplicationClass was not reproducible per-project: NU1201/MSB3107
     on its sibling project reference, unrelated to these files).
3. **Controls tail II and packaging: 4 only, 3 involved.**
   - Only: Concepts (`Hyperlink.TargetName`), DialogBox (`FontStyleConverter`/`FontWeightConverter`), AnnotationsStyling
     (`InkCanvasEditingMode`, plus an `InkCanvas` shell), WPFGallery (`Microsoft.Win32.SystemEvents` as a
     `Gtk4.Forms.Wpf` dependency, like `System.Configuration.ConfigurationManager`).
   - Involved: FontDialogDemo (`TextBox.TextAlignment`, `TextBlock.ContentStart`/`ContentEnd`), GraphingCalculatorDemo
     (`TextBox.MaxLines`), SlidePuzzleDemo (`ComboBoxItem.Tag`).
   - Owns: `Controls/TextBox*.cs`, `Controls/TextBlock*.cs`, `Controls/ComboBoxItem*.cs`, new `Controls/InkCanvas*.cs`,
     `Documents/Hyperlink.cs` (carved out of track 6), new `FontStyleConverter`/`FontWeightConverter` files, and
     `Gtk4.Forms.Wpf/Gtk4.Forms.Wpf.csproj` (package dependencies).
4. **Startup and generator sweep II: 3 crash→runs, 1 only, 1 involved.**
   - Runs: ExpenseItIntro (defer `StaticResource` in dictionary values, or resolve a missing key lazily as WPF does),
     Geometry (emit `Viewport3D.Camera`/`ModelVisual3D.Content`/`Transform` and assign nested freezable names),
     KeySplineAnimations (register names inside Geometry/Drawing property elements).
   - Builds: AnimationExamples (`{Binding}` on an animation property). Involved: DataBindingDemo (`SetBinding` on a
     non-`FrameworkElement` via `BindingOperations`).
   - Also: work down the "Skipped property element" backlog, starting with `ContextMenu` and `Expander.Header`.
   - Gate: all 176 building projects, plus the 6 non-runners.
   - Owns: `Gtk4.Forms.MarkupCompiler/**`, `Markup/StaticResourceScope*.cs`, `Markup/XamlValueConverter*.cs`,
     `Markup/GtkFormsXamlRegistry*.cs`, `NameScope.cs`, and the target-resolution part of
     `Media/Animation/Storyboard*.cs`.
5. **Items and data tail: 2 only, 1 involved.**
   - Only: AnnotatedDocumentViewer (`SortDescription.PropertyName`/`Direction` settable), MediaGallery (`Binding.Path`
     typed `PropertyPath`, with a string-compatible path for existing callers).
   - Involved: DataBindingDemo (with track 4 and the UI automation plan).
   - Owns: `Data/**` (`SortDescription.cs`, `Binding.cs`, `BindingEngine*.cs`, `CollectionViewSource*.cs`).
     `Binding.Path` is a public-API type change: check every `Binding.Path` reader in `Markup/**` and coordinate with
     track 4.
6. **Documents and imaging tail: 3 only, 3 involved.**
   - Only: DocumentMerge (Fixed/XPS: `FixedDocument`, `FixedPage`, `PageContent`, `DocumentReference`,
     `FixedDocumentSequence`, then `XpsDocument` on a ZIP package), PhotoViewerDemo (`CachedBitmap`, `CroppedBitmap`,
     `FormatConvertedBitmap`, `TransformedBitmap`), ParticlesDemo (`RenderTargetBitmap.Freeze`).
   - Involved: PhotoStoreDemo (imaging plus track 1), SlidePuzzleDemo (`DocumentViewerBase.IsMasterPage`),
     FontDialogDemo (`TextDecorationCollection.Freeze`).
   - Owns: new `System.Windows.Xps/**`, the Fixed types under `Documents/**` (not `Documents/Hyperlink.cs` or
     `Documents/Adorner*.cs`), `Controls/Primitives/DocumentViewerBase*.cs`, `Media/Imaging/**`, and
     `Media/TextDecoration*.cs`.

**Lanes (three in parallel, two tracks each, file-disjoint):**

| Lane | Tracks | Only | Boundary to watch |
|---|---|---:|---|
| A | 1 (land `gf53a`) + 2 (input devices, `BeginStoryboard`) | 15 | Track 1 owns `UIElement.*`, `Input/Mouse.Cursor.cs` and the visual layer. Track 2 owns the event-args and device files only |
| B | 6 (documents and imaging) + 5 (items and data) | 5 | Track 6 owns `Documents/**` minus `Hyperlink.cs` and `Adorner*.cs`. Track 5 owns `Data/**` |
| C | 4 (generator sweep II) + 3 (controls tail II, packaging) | 5 + 3 crash→runs | Track 4 owns `Gtk4.Forms.MarkupCompiler/**` and `Markup/**`. Track 3 owns the named `Controls/` files, `Documents/Hyperlink.cs` and the package |

Held branches:

- **`gf53a/visual-layer-input` (unmerged since Round 53): 12 samples.** This is track 1. Land it first.
- **`gf52b/content-bindings-sweep`: merged** (`b25bc71e`). CommonDialogs builds and runs. The branch ref is gone.
- **`gf46/input-focus` (held, 6 commits, 144 behind `main`): 0 samples.** Every Input and Commands app builds and all 16
  portable ones run. What remains on the branch is focus-transaction behaviour, owned by the focus-behaviour design.
  Rework it there against `main`. It moves no sample count and is not in the lanes.
- **`gf39/xaml-content-model` (held, blocked 3 times; 3 commits, 197 behind): 0 samples on its own.** Re-scope it as a
  test-only change (the compile-checked generator test) on top of track 4, which owns
  `Gtk4.Forms.MarkupCompiler/**`. The "Skipped property element" backlog is its natural regression corpus.

Deliberately not next:

- JPEG XR encoding (WDPEncoderAndDecoder). There is no codec in the GTK stack.
- System.Xaml/BAML tooling (LocBaml, BamlReflector/BamlTools) and code access security (EditingExaminerDemo).
- AppCompat-Quirks (WPF-internal AppContext switches) and LocalizationResources (the sample ships no neutral resource).
- Legacy (non-SDK) csproj conversion: 21 projects. That is a converter feature, not a layer gap.

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches:
  1. Data Binding, Getting Started, Events.
  2. Graphics, Animation, Visual Layer, Speech and Media, Clipboard.
  3. Sample Applications, Migration and Interoperability.
  4. Input and Commands, Documents, Drag and Drop, Compatibility.
  5. Globalization and Localization, Windows, Accessibility, Element Tree, PerMonitorDPI.
  6. Application Management, Elements, Properties, Resources, Styles & Templates, Threading, Tools.

  Each batch was copied under `.claude/samples-remeasure-r55/batchN` without `bin`/`obj`, `NuGet.config` or
  `global.json`. The scratch root has empty `Directory.Build.props`/`.targets`, so `TreatWarningsAsErrors` was off. No
  original or tracked file was modified: `git status` is clean in the worktree and in the `WPF-Samples` submodule.
- **Feed, pinned.** `samples-remeasure-r55/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-remeasure-r55/.nuget-packages`, which started empty) and cleared
    `fallbackPackageFolders`.
  - Cleared `packageSources`, then the pinned feed (`samples-remeasure-r55/feed`) and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the full recipe from an empty cache.
  - Build 1 exited 1, as expected: the converter gave `MSB3073` (exit 10), then "re-run the build". Build 2 exited 0
    with 0 warnings and 0 errors.
  - The cached SHA-256 values equal the feed's: `Gtk4.Forms.Wpf` `670a821c…cb81c`, `PresentationFramework`
    `acf7fab6…ff8f17`, `WindowsBase` `d9e92ace…604811`, `MarkupCompiler` `3f4c3c7e…d755f`.
  - Every `Gtk4.Forms.*` `.nupkg.metadata` source is the scratch feed.
  - `project.assets.json` lists only the isolated cache as a package folder and no fallback folders. Its sources are
    the feed, nuget.org and the SDK-implicit `library-packs`.
  - `gtksharp4 4.22.4.26325` came from nuget.org.
  - The probe's 10 s smoke run stayed alive with 1 visible top-level window and no stderr.
  - Every batch re-checked the cached `Gtk4.Forms.Wpf` hash
    (`670a821c197092b0a158fc352f0ac10a0ef43776bb8968005eb2995e919cb81c`). It was checked again after the WPFGallery
    re-build.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26326`.
  2. Build once with `-nodeReuse:false`. The converter rewrites the csproj and stops with exit 10.
  3. Build again. This result is recorded.
  - Nothing else is changed. Original csproj files are kept beside the copies (`*.csproj.orig`).
  - Exception: WPFGallery's build 1 stopped after restore, so its third build is recorded (see Corrections).
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes, symbols and messages were
  re-parsed uniformly from the final logs of all six batches (252 logs, one per csproj).
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime
  (`%LOCALAPPDATA%\Gtk\4.22.4\bin`) on `PATH`.
  - "Runs" means alive with a visible, unowned top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception, or an exit with no window. Every run's stdout/stderr was scanned for
    unhandled exceptions; only the 6 crashes (and FocusTracker's exit) lack a window.
  - "Hang" means alive with no window and no exception.
  - Apps with no window at 10 s were re-run alone for 20 s.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification of the earlier reports, unchanged.
- **Limits.**
  - Only Windows was measured.
  - Legacy csproj files are not converted: 21 projects.
  - The theme counts are a regex ranking aid (`blockerThemes`), not an exact partition.
  - Moves are attributed by elimination, not bisected.
  - The three shorter package hashes are recorded truncated, as the probe reported them.

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
| 2026-11-22 | `466055e3` | 1.0.0.26326 | **yes** | **171 / 204** | **165** | Rounds 53–55 (first three-lane batch) + `gf52b` merge; full re-measure; 6 crash, 0 hang; no regressions |

## Known layer defects found by the UI tests

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`. WPF re-queries commands afterwards, so the button is enabled. The layer calls nothing like
  `CommandManager.InvalidateRequerySuggested` there, so the button stays disabled until another requery happens.
  `CommandSampleTests` keeps a workaround test (it requeries itself) and a skipped test,
  `The_button_is_enabled_without_a_manual_requery`, that asserts the correct behaviour. Un-skip it when fixed.
