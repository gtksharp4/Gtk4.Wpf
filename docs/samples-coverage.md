# WPF samples coverage

Measured 2026-11-21 on `main` at `142b53f66090d7167376e06ab6d0a9fd580c0f63` (Round 48 follow-up), package
`Gtk4.Forms.Wpf 1.0.0.26325` from a **pinned** local feed, against the official `microsoft/WPF-Samples` (256 projects:
243 apps, 9 libraries and 4 native C++ projects). All 252 csproj were rebuilt and every built app was smoke-run.
Per-project results are in [`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report
`2c67b72c` (main `1b50a528`, Round 46, `previous`).

## Headline

**129 of 204 portable official WPF sample apps build unmodified, and 119 run.** That is up from 89 / 83 in report
`2c67b72c` (+40 build, +36 run) and from 0 / 0 in report `fedd13c8` (2026-11-19).

- **40 apps moved fail→pass**: 33 run, 5 crash at startup and 2 hang. **3 built apps moved crash→runs.** All of it
  comes from Rounds 47 and 48.
- **No regressions.** No project went pass→fail and no app went runs→crash/hang.
- Of the 10 apps that build but do not run, 8 crash at startup and 2 hang. 6 of the 8 crashes have a known,
  small cause in the layer or the MarkupCompiler (see Crashes).
- The library CustomApplicationClient also builds, so 130 projects build in all.
- `CS2002` (a generated `.g.cs` specified twice) dropped from 83 of 89 portable builds to 21 of 129, after the
  Round 47 output-collision fix.
- The Round 47 and 48 gates had reported lower bounds of 94 / 91 by re-running only selected apps. This full
  re-measure supersedes them.

### Corrections to the raw batch data

The final build log of each of the 252 csproj was re-parsed uniformly (UTF-8 and UTF-16). The parse agrees with every
batch's own per-project pass/fail list. Every crash and hang was re-run alone by the batches. The top 15 missing
symbols were checked against the `Gtk4.Forms.*` source.

- **The aggregated batch totals are slightly off.** They said 253 projects, 203 portable apps, 128 build and 117 run.
  The inventory is unchanged from `2c67b72c` (the same 252 csproj plus the 4 native projects), so the denominator is
  still 204. The per-project logs give 129 / 119:
  - Batch 2's summary says 14 builds, but its logs and its own per-category counts (Graphics 10, Animation 3) give 13.
  - The aggregated crash list omits ClipRegion and DrawingBrush (crash) and ShapeElements.
- **Apps that stay alive with no window but print an unhandled startup exception are crashes, not hangs.** The Method
  defines a crash as an unhandled exception. That reclassifies 3 apps the smoke run read as hangs: ShapeElements,
  ICommandSourceImplementation and LocalizationResources. That leaves 2 real hangs, ContextMenuOpening and
  DragDropThumbOps, with empty output.
- **Legacy (non-SDK) csproj files stay `not-built`.** The converter leaves them on .NET Framework 4.x. Highlighter
  "builds and runs" again, but as real .NET Framework WPF (`TargetFrameworkVersion v4.7.2`, no Gtk4.Forms assembly
  in its output), so it is not counted. The attempt is kept under `legacyBuild`.
- **Top-15 symbol check:**
  - Artefacts. `System.Windows.Markup.XamlValueConverter` (7) and `FrameworkElement.AbandonInitialization` (7) exist in
    the layer (`Markup/XamlValueConverter.cs`, `FrameworkElement.Initialized.cs`). Every hit is a legacy csproj built
    against real WPF.
  - Windows-only, dropped from the ranking. `System.Windows.Forms` (13) and `Form` (6). The 7 portable "Form" hits
    in the codec samples are a substring match on `PixelFormats`.
  - Real gaps, absent from the source:
    - imaging: `BitmapFrame`, `BitmapCacheOption`, `BitmapCreateOptions`, `BitmapPalettes`, `PixelFormats.Indexed1`;
    - 3D: `System.Windows.Media.Media3D` (`Viewport3D`, `Vector3D`, `Quaternion`);
    - navigation: `NavigationEventArgs`;
    - animation: `TranslateTransform.XProperty`/`YProperty`/`BeginAnimation`. `TranslateTransform` exists, but its
      `X`/`Y` are not DPs, and nothing has `BeginAnimation`.
  - Labels. `BitmapCacheOption`, `BitmapCreateOptions`, `BitmapPalettes` and `BitmapFrame` are types reported as
    `CS0103` names, so the batches tagged them `M:`.
- **windowsOnly is carried over unchanged.** Batch 2 flagged CustomBitmapEffect as Windows-only (BitmapEffect), but it
  builds and runs on the layer, so it stays portable.

## Per category

These are portable apps on the strict recipe. Δ is the change since `2c67b72c`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 1 | 0 | 1 | 0 |
| Animation | 11 | 0 | 11 | 3 | +1 | 3 | +1 |
| Application Management | 10 | 1 | 8 | 7 | +3 | 7 | +3 |
| Clipboard | 2 | 0 | 2 | 1 | +1 | 1 | +1 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 29 | +3 | 29 | +3 |
| Documents | 17 | 2 | 15 | 5 | +1 | 5 | +1 |
| Drag and Drop | 6 | 0 | 6 | 5 | 0 | 4 | +1 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 9 | +2 | 7 | 0 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 5 | 0 | 5 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 13 | +9 | 12 | +10 |
| Graphics | 26 | 0 | 25 | 10 | +7 | 7 | +4 |
| Input and Commands | 18 | 1 | 16 | 16 | +8 | 15 | +7 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 2 | +1 | 2 | +1 |
| Resources | 3 | 0 | 3 | 2 | 0 | 2 | 0 |
| Sample Applications | 23 | 1 | 21 | 3 | 0 | 3 | 0 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 4 | 0 | 4 | +1 |
| Threading | 3 | 0 | 3 | 2 | +2 | 2 | +2 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 6 | +2 | 5 | +1 |
| **Total** | **256** | **39** | **204** | **129** | **+40** | **119** | **+36** |

## What moved

Each move is attributed by elimination: the named change removed every blocker that `2c67b72c` recorded for the app.
Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass, 40 apps.**

| Change | Apps (run result) |
|---|---|
| Round 47 flow-direction (`5f92acd0`): `FlowDirection` enum, inherited `FrameworkElement.FlowDirection`, `Inline`/`FlowDocument` direction, `Language`/`NumberSubstitution` | FlowDirection, Gradient, Image, LTRRTL, Paths, RunSpan, Span, LangAttribute, NumbersMultipleLangauges, FlowContentProperty (all run) |
| Round 47 generator-v4 (`d78faffa`): `Main` for a custom `Application` subclass | CustomApplicationClass (runs) |
| Round 48 input-api (`ecf1d70d`): keyboard-focus events, `MoveFocus`/`PredictFocus`, `RoutedCommand()`, `EditingCommands`, full `Cursors`, Slider commands and `AutoToolTip*`, `ContextMenuEventArgs`, `FocusVisualStyle` | CursorType, CustomRoutedCommand, EditingCommands, KeyStrokeCounter, ProgrammaticFocusControl, CommandSourceControlWithDispatcherTimer (run); FocusVisualStyle, ICommandSourceImplementation (crash); ContextMenuOpening (hang) |
| Round 48 input-api plus threading (`Dispatcher.Invoke(DispatcherPriority, Delegate)`) | CommandSourceControlUsingSystemTimer (runs) |
| Round 48 graphics-window-tail (`5ebf5285`): `Vector`/`Point`/`Matrix`, `SystemColors`, `GetLocalValueEnumerator`, `Page.Background`, `UIElement.Clip`, `TextBlock` attached accessors, `DragMove`/`AllowsTransparency` | Matrix, Point, Vector, SystemBrushesAndColors, RestoringDefaultValues, PathAnimations, NonRectangularWindow (run); ShapeElements, ClipRegion, DrawingBrush, CustomWindowUI (crash) |
| Round 48 threading-dialogs-items (`f585c6c8`) plus the review fixes: `DispatcherOperationCallback`, static `Dispatcher.Run`, `DispatcherObject.Dispatcher`, `Clipboard.Contains*`, `IEditableCollectionView`, `BindingGroup.GetValue`/`TryGetValue`, `ItemBindingGroup` | ExceptionHandlingSecondaryUIThread, ExceptionHandlingSecondaryWorkerThread, MultiThreadingWebBrowser, SingleThreadedApplication, ClipboardSpy, EditingCollections, ValidateItemSample, ValidateItemsInItemsControl (all run) |

**Crash→runs, 3 apps, all Round 47 generator-v4:**

- DragDropEvents: `Grid.RowDefinitions`/`ColumnDefinitions` are now emitted.
- StringLocalizationSample: merged `ResourceDictionary` files are compiled and loaded at parse time.
- AlternatingAppearanceOfItems: `HierarchicalDataTemplate`, `CollectionViewSource` and `AlternationConverter`
  resources are built as objects.

**Error landscape.** Counted as portable failing apps per code, `2c67b72c` → now:

- Down: `CS0246` 58 → 44, `CS1061` 45 → 19, `CS0117` 30 → 16, `CS0234` 21 → 16, `CS0103` 27 → 23, `CS1729` 6 → 3,
  `CS0115` 6 → 4, `CS5001` 2 → 1.
- Flat: `CS0534` 9 → 9. `UIElement.CreateWidget()` still leaks as an abstract member to user subclasses.
- New: the `CS1002`/`CS1026`/`CS1513`/… syntax family in one app (CustomComboBox). The MarkupCompiler emits invalid
  C#, now reached because earlier blockers are gone. See XAML generator errors.

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang.

- **LocalizationResources** reads crash in `2c67b72c` and hang in this smoke run. It has the same
  `MissingManifestResourceException` in stderr both times, so it is unchanged (classified crash).
- **Apps that first read as hangs ran when re-run alone**, so the first reading was noise from parallel load:
  UnhandledExceptionHandling, EventTriggers and Span.

## API demand

The counts are failing portable apps that report the symbol. Windows-only and legacy-csproj demand is excluded. "Apps"
is the number of failing portable apps whose errors touch the theme (from `blockerThemes`, a regex ranking aid).

| Theme | Symbols (apps) | Apps |
|---|---|---:|
| Imaging | `BitmapFrame` (8), `BitmapCacheOption`/`BitmapCreateOptions` (7), `PixelFormats.Indexed*` (5), `BitmapPalettes` (5), the `Png`/`Jpeg`/`Gif`/`Tiff`/`Bmp`/`Wmp` decoders and encoders, `BitmapMetadata`, `BitmapImage.Format`, `Rotation`, `PngInterlaceOption`, `TiffCompressOption` | 10 |
| Animation | `TranslateTransform.X/YProperty` + `BeginAnimation` (3), `UIElement`/`Animatable.BeginAnimation` (4), `DoubleAnimationBase` (2), `Clock` (2), `Storyboard.GetCurrentTime`/`Seek`/`TimeSeekOrigin` (2–3), `SolidColorBrush.ColorProperty` (2), `PropertyPath` (2), `HandoffBehavior` (2), public `NameScope` | 12 |
| Documents and text | `FlowDocument.ColumnWidth` (3), `ColumnGap`/`IsColumnWidthFlexible` (2), page and column props, `Table`/`TableRow`/`TableCell`/`TableRowGroup` (4), `System.Windows.Annotations` (2), `PrintDialog` (2), `TextRange.Load`/`Save`/`CanLoad`, `FlowDocument.ContentStart`/`End`, `Paragraph.FontSize`/`Style`, `Typeface`, `TextEffect`, `DynamicDocumentPaginator`, Fixed/XPS documents | 14 |
| 3D | `System.Windows.Media.Media3D` (7): `Viewport3D` (5), `Vector3D` (5), `Quaternion` (4), `Point3D` (2), `GeometryModel3D` (2), `MeshGeometry3D`, `Model3DGroup` | 8 |
| Extensibility | `UIElement.CreateWidget()` leaking as abstract (`CS0534`, 10), overridable `LogicalChildren`/`OnRender` (`CS0115`, 4) | 10 |
| XAML at runtime | `XamlReader` (5), `XamlWriter` (4), `XamlParseException` (3), `Application.LoadComponent`, BAML localizer (LocBaml) | 7 |
| 2D graphics long tail | `RectConverter`, `ThicknessConverter`/`BrushConverter`, `GeneralTransform`, `CompositionTarget`, `HitTestResultBehavior` and the hit-test parameter types, `Geometry.Clone`, `Canvas.LayoutUpdated`, `Border.SnapsToDevicePixels` | 9 |
| Tree and items | `FrameworkElement.Parent` (3: CalculatorDemo, GeometryDesignerDemo, LayoutTransitionsDemo), `ControlTemplate`/`DataTemplate.FindName`, `ItemContainerGenerator`, `ListBox.ScrollIntoView`, `FilterEventArgs`, `Binding.UpdateSourceExceptionFilter`, `Label.Target` | 7 |
| Controls | `UserControl` (WPFGallery), `ListViewItem` (ExpenseItDemo), `MediaElement` (2), `FlowDocumentPageViewer`, `TabControl.TabStripPlacement`, `Frame.Refresh` | 6 |
| Navigation and pages | `NavigationEventArgs` (3), `PageFunction<T>`/`ReturnEventArgs<T>` | 4 |
| Automation peers | `System.Windows.Automation.Peers` (3) | 3 |
| Dialogs and fonts | `PrintDialog`, `FontStyleConverter`/`FontWeightConverter`, `Fonts`, `SystemFonts`, `Mouse.SetCursor` | 3 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| ShapeElements | crash | `'2,1' is not a valid value for property 'StrokeDashArray'`. `XamlValueConverter.Convert` has no string → `DoubleCollection` conversion and falls through to the raw string (`XamlValueConverter.cs`, "No converter available"), which `DependencyProperty.ThrowIfInvalid` rejects. The process stays alive with no window |
| CustomWindowUI | crash | `'SizeAll' is not a valid value for property 'Cursor'`. Same fall-through: there is no string → `Cursor` conversion, although `Cursors.SizeAll` exists |
| ICommandSourceImplementation | crash | `Resource with key '{   StaticResource StringConverterResource}' not found`. The MarkupCompiler takes the whole text of a `{ StaticResource … }` with inner whitespace and line breaks as the key. Alive with no window |
| FocusVisualStyle | crash | `Resource with key 'MyFocusVisual' not found`. `{DynamicResource}` on a `Style`-typed DP is emitted as an eager `FindResource` in `InitializeComponent`, before the resource exists |
| ClipRegion | crash | `Storyboard.Begin: TargetProperty '(EllipseGeometry.RadiusX)' could not be resolved` on the `Image`. The storyboard animates a property of the element's `Clip` geometry, and property paths into a sub-object value are not resolved |
| DrawingBrush | crash | `Storyboard.Begin: TargetProperty '(DockPanel.Background).(DrawingBrush.Drawing).(DrawingGroup.Children)[0]…(SolidColorBrush.Color)'`: multi-step, indexed property paths into a brush are not resolved |
| AppCompat-Quirks | crash | Unchanged: `NullReferenceException` in the sample's `PopulateAppContextInformation`. It reflects over WPF-internal AppContext switch types, so `GetAppContextSwitchType` returns null. A compatibility gap, not worth chasing |
| LocalizationResources | crash | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. There is only an en-US satellite and no neutral resource. Alive with no window |
| ContextMenuOpening | hang | Alive with no top-level window and empty output, in the first pass and when re-run alone. Not diagnosed |
| DragDropThumbOps | hang | Unchanged. Alive with no top-level window and empty output, also when re-run alone. Not diagnosed |

## XAML generator errors

These are the remaining generator-side causes in portable apps. Per-project messages are in the JSON (`xamlErrors`).

1. **Invalid C# from `Styles.xaml` (CustomComboBox), the only syntax-error build.**
   - `<EventTrigger RoutedEvent="local:ExpandableToggleButton.Expanded">` is emitted as
     `typeof(local:ExpandableToggleButton)`. The xmlns prefix of a `RoutedEvent` owner is not resolved.
   - `{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}` is emitted with the inner extension as a string
     key. It compiles, but the lookup is wrong.
2. **Markup-extension key text not trimmed** (ICommandSourceImplementation, a crash). See Crashes.
3. **Eager `{DynamicResource}` on `Style`-typed DPs** (FocusVisualStyle, a crash). See Crashes.
4. **Missing runtime string conversions** (`DoubleCollection`, `Cursor`) in `XamlValueConverter` (ShapeElements,
   CustomWindowUI, both crashes). Any other DP type without a registered converter fails the same way at startup,
   not at build time.
5. **Value conversion (`CS0029`):** `int` → `string` in a generated setter (2DTransforms). FontProperties' `FontFamily`
   → `string` is in the code-behind (`TextElement.FontFamily` typing), so it is a layer gap.
6. **Missing `x:Name` fields:** `mediaElement` (WindowActivationAndDeactivation, which fails on nothing else),
   `myMediaElement`/`MediaElement1` (MediaGallery), keyframe names (KeySplineAnimations), `flowReader`
   (ParagraphAndHyphenation), `trg1` (TableRows), and `DisplayBox`/`PaperBox` (CalculatorDemo, `local:` types).
   Most of these elements are of types the layer lacks (`MediaElement`, 3D keyframes, `Table`). The CalculatorDemo
   case is a generator gap.
7. **Not generator bugs, though surfaced in `.g.cs`:** `Label.TargetProperty` (BindValidation),
   `Border.SnapsToDevicePixels` and `Window.Source` (GlobalizationHomepage). All are missing layer members.
   - DocumentStructure's `fixedpage1_structure.xaml` is malformed XML, and that app is Windows-only.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole remaining blocker is the track.

**Not eligible: `System.Windows.Automation.Peers` (4 samples).** It belongs to `docs/plans/ui-automation.md`; the parity scout must not pick it.

1. **Startup-crash sweep (MarkupCompiler and runtime conversion): up to 6 runs, no new API surface.**
   - Fixes:
     - string → `DoubleCollection` and `Cursor` (and an audit of every DP type with no converter) in
       `XamlValueConverter`: ShapeElements, CustomWindowUI;
     - trim whitespace in markup-extension arguments: ICommandSourceImplementation;
     - defer `{DynamicResource}` on `Style`-typed DPs: FocusVisualStyle;
     - resolve storyboard property paths into sub-objects and indexed collections: ClipRegion, DrawingBrush;
     - diagnose the two empty-output hangs (ContextMenuOpening, DragDropThumbOps);
     - CustomComboBox's prefixed `RoutedEvent` owner and the nested `{DynamicResource {x:Static}}` key;
     - 2DTransforms' `int` → `string`;
     - `x:Name` fields for `local:` types (with `FrameworkElement.Parent`, CalculatorDemo).
   - Gate: all 130 building projects, plus the 10 non-runners.
2. **Imaging: 8 only, 10 involved.**
   - Only: BitmapMetadata, BMP/GIF/JPEG/PNG/TIFF/WDP EncoderAndDecoder, ImageView.
   - Involved: PhotoStoreDemo and PhotoViewerDemo also need `CS0534`.
   - Covers: `BitmapFrame`, `BitmapDecoder`/`BitmapEncoder` and the six codecs (on gdk-pixbuf/GdkTexture),
     `BitmapCacheOption`, `BitmapCreateOptions`, `BitmapPalette(s)`, indexed `PixelFormats`, `BitmapMetadata`,
     `BitmapImage.Format`.
3. **Animation clocks and animatable properties: 5 only, 12 involved, plus 2 crash→runs.**
   - Only: AnimationExamples, CustomAnimation, LocalAnimations, PropertyAnimation, ConcentricRingsDemo.
   - Covers:
     - `DoubleAnimationBase` and the typed bases, and `Clock`/`AnimationClock`;
     - `BeginAnimation` on `UIElement` and `Animatable`;
     - DP-backed `TranslateTransform.X/Y` and `SolidColorBrush.Color`;
     - `PropertyPath`, a public `NameScope`, and `HandoffBehavior`;
     - `Storyboard.Seek`/`GetCurrentTime`/`TimeSeekOrigin`.
   - Shares the property-path resolver with track 1 (ClipRegion, DrawingBrush). Fold `CreateWidget` (`CS0534`, 10
     apps: AnimationTiming, Per-FrameAnimation, Callbacks, …) into this round, since it touches `UIElement`.
4. **Documents and text: 7 only, 14 involved.**
   - Only: ClipboardViewer, AnnotatedDocumentViewer, AnnotationsStyling, DocumentMerge, ParagraphAndHyphenation,
     TableBuilder, FontDialogDemo.
   - Close: FlowContentElements, FlowDocumentProperties, TableRows, CommonDialogs.
   - Covers: the `FlowDocument` column/page/typography properties, `Table`/`TableRowGroup`/`TableRow`/`TableCell`,
     `TextRange.Load`/`Save`, `TextPointer` ranges, `Typeface`, and `System.Windows.Annotations`.
   - Fixed/XPS documents (DocumentMerge) are the largest single piece.
5. **XAML at runtime, navigation and the converter: about 8.**
   - `XamlReader`/`XamlWriter`/`XamlParseException` and `Application.LoadComponent`: ControlsAndLayout, MergedResources,
     SkinnedApplication; Concepts and FindText in part.
   - `NavigationEventArgs`, `PageFunction<T>` and `ReturnEventArgs<T>`: Brushes, Wizard.
   - Converter: convert ProjectReferences before restore. A client whose sibling is converted at `BeforeBuild`
     fails restore with `NU1201` (BamlReflector, whose BamlTools sibling still advertises `net10.0-windows7.0` at
     restore).
6. **Media3D: 2 only, 8 involved.** Converter and Geometry only. With imaging and animation it also unblocks the
   five 3D demos and KeySplineAnimations. It is the largest surface: `Viewport3D`, cameras, `Model3D`/`GeometryModel3D`,
   `MeshGeometry3D`, `Vector3D`/`Point3D`/`Quaternion`, rendered through GSK's 3D transforms or a GL area. Schedule it
   after tracks 2 and 3.

Held branches:

- **`gf46/input-focus` (held): 0 samples.** Its API surface reached `main` through `gf48/input-api`, and every Input
  and Commands app now builds. What remains on the branch is focus-transaction behaviour: GTK-originated focus moves
  reported as two transactions, and a `Preview*` cancel that cannot restore GTK focus. That is owned by the Round 49
  focus-behaviour design. Rework it there against `main`; it does not move a sample count.
- **`gf39/xaml-content-model` (held, blocked 3 times): 0 samples on its own.** Generator v3/v4 on `main` have since
  delivered its property-element and converter scope. Its remaining value is the compile-checked generator test. Re-scope
  it as a test-only change on top of track 1 rather than rebasing the old generator changes.
**Not eligible: `System.Windows.Automation.Peers` (4 samples).** It belongs to `docs/plans/ui-automation.md` (branch `ua/automation`, Phase 1 landed the peers); the parity scout must not pick it.

Deliberately not next:

- Automation peers (3 apps, 2 only: ExpenseItIntro, DataBindingDemo). They belong with the UI automation plan
  (`3698f1cc`).
- `UserControl`/`MediaElement` (WPFGallery, SlidePuzzleDemo, MediaGallery, WindowActivationAndDeactivation). Each
  app also needs other gaps, except WindowActivationAndDeactivation, which needs only `MediaElement`.
- System.Xaml/BAML tooling (LocBaml, BamlTools).
- Legacy (non-SDK) csproj conversion: 21 projects. That is a converter feature, not a layer gap.

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches. Each batch was copied under
  `.claude/samples-r48/batchN`, with empty `Directory.Build.props`/`.targets` (so `TreatWarningsAsErrors` was off). No
  original or tracked file was modified.
- **Feed, pinned.** `samples-r48/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-r48/.nuget-packages`) and cleared `fallbackPackageFolders`.
  - Cleared `packageSources`, then the worktree `Packages/` feed (`C:\Temp\Gtk.Forms\.claude\wt\main\Packages`) and
    nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the full recipe from an empty cache.
  - Build 1 exited 1, as expected: the converter exited 10. Build 2 succeeded with 0 warnings and 0 errors.
  - The cached SHA-256 values equal the local feed's: `Gtk4.Forms.Wpf` `be36a23e…1e02`, `PresentationFramework`
    `780ac22a…33af`, `WindowsBase` `a2933775…2b38`, `MarkupCompiler` `afc02796…809d`.
  - Every `.nupkg.metadata` source is the worktree `Packages` feed.
  - `project.assets.json` lists only the isolated cache as a package folder.
  - `gtksharp4 4.22.4.26325` came from nuget.org.
  - Every batch re-checked the cached `Gtk4.Forms.Wpf` hash.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26325`.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again. This result is recorded.
  - Nothing else is changed. Original csproj files are kept beside the copies.
  - Batches built 3–4 projects in parallel against the shared cache, with no `NETSDK1064` or missing-file errors.
  - WPFGallery was built by its csproj path. Compatibility's csproj is at the category root, so the whole folder was
    copied as one project.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes and messages were
  re-parsed uniformly from the final logs of all six batches.
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime on `PATH`.
  - "Runs" means alive with a visible top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception (including one printed by a process that stays alive), or an exit with no
    window.
  - "Hang" means alive with no window and no exception.
  - Every crash and hang was re-run alone.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification of `2c67b72c`, unchanged.
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
| 2026-11-21 | `142b53f6` | 1.0.0.26325 | **yes** | **129 / 204** | **119** | Round 48; full re-measure; 8 crash, 2 hang; no regressions |

## Known layer defects found by the UI tests

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`. WPF re-queries commands afterwards, so the button is enabled. The layer calls nothing like
  `CommandManager.InvalidateRequerySuggested` there, so the button stays disabled until another requery happens.
  `CommandSampleTests` keeps a workaround test (it requeries itself) and a skipped test,
  `The_button_is_enabled_without_a_manual_requery`, that asserts the correct behaviour. Un-skip it when fixed.
