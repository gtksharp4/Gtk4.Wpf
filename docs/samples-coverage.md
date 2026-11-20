# WPF samples coverage

Measured 2026-11-20 on `main` at `ea4c17ae09a4d0db710525472711607a2d377f1d`, package `Gtk4.Forms.Wpf 1.0.0.26324`,
against the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4 native C++ projects).
Per-project results are in [`samples-coverage.json`](samples-coverage.json). Each row holds its result in the previous
report (`previous`, report `9ca5b30b`), so the next run can diff against it.

## Round 46 update: startup runtime (`gf46/startup-runtime`)

Re-measured on `a37d3b17` plus this branch, with the same recipe and a package packed from the worktree.
**All 84 projects still build, and 76 apps run** (was 74): Linq and OpacityMasking now run, and no app that ran
before stopped running. The per-category and per-theme tables below are the `ea4c17ae` measurement.

- Linq: `Selector.SelectionChanged` is held back while a `BeginInit`/`EndInit` scope is open and raised once, with
  the net change, when the outermost scope ends.
- OpacityMasking: the DP converter path converts a string to `Uri` (`Frame.Source`).
- DragDropEvents: an exception that escapes a GTK signal callback now reaches `Dispatcher.UnhandledException` and is
  printed if unhandled. The sample still crashes, and now says why (it needs `Grid.RowDefinitions`, a compiler gap).
- LocalizationResources and StringLocalizationSample: see the table below. Neither is fixable in the runtime alone.

## Headline

**83 of 206 portable official WPF sample apps build unmodified, and 74 run.** That is up from 59 / 47 in report
`9ca5b30b` (main `64cddd04`, Round 44) and from 0 / 0 in report `fedd13c8` (2026-11-19).

- 24 apps moved fail→pass (19 run, 5 crash) and 8 built apps moved from crash/hang to running. All of it is Round 45.
- **No regressions.** No app went pass→fail, and no app went runs→crash/hang.
- Of the 9 apps that build but do not run, 8 crash at startup and 1 hangs. 5 of the 9 are new builds.
- 37 apps are Windows-only by design and are excluded from the denominator, as before.
- Data Binding is now nearly done: 25 of 30 portable apps build and 24 run.

### Corrections to the raw batch data

The top 15 missing symbols were checked against the `Gtk4.Forms.*` source. Every crash and hang was checked against its
logs and generated code. The following are corrected here and in the JSON.

- **252 projects is 256.** The six batches enumerated `*.csproj` only, so the 4 native `.vcxproj` projects were not
  measured. They are carried over as not-built and Windows-only (`measured: false`). The denominator does not change.
- **Windows-only stays at 37 apps (206 portable), not 35 (211).** The batch heuristics disagree with the verified
  classification on 9 projects. The verified classification is kept, and each disagreement is noted on its row.
  - FocusTracker and WindowMove are UIA clients, and the 3 PerMonitorDPI apps are Windows-only by design. The
    batches called them portable.
  - DocumentMerge only has a csproj `Reference` to `System.Windows.Forms`. The batch called it Windows-only.
  - DocumentStructure (a WinForms `OpenFileDialog` via an alias) and SingleInstanceDetection (VB
    `WindowsFormsApplicationBase`) are flagged Windows-only by the batches, and the evidence supports that.
    Neither builds, so the build and run counts are the same either way. Reclassify both next run; the denominator
    becomes 204.
- **Every top-15 symbol is a real gap.** None of them is in the layer: `BitmapCreateOptions`, `BitmapCacheOption`,
  `BitmapFrame`, `BitmapPalettes`, `PixelFormats.Indexed*`, all of `System.Windows.Media.Media3D` (`Viewport3D`,
  `Vector3D`, `Quaternion`), `KeyboardFocusChangedEventArgs`, `XamlReader`, `System.Windows.Vector`,
  `System.Windows.Automation.Peers`, and the `FlowDirection` enum and properties. Three labels and counts need fixing:
  - `BitmapCreateOptions`, `BitmapCacheOption`, `BitmapPalettes` and `XamlReader` are tagged `M:` because they are
    `CS0103` names. They are types.
  - `KeyboardFocusChangedEventArgs` is 4 apps, not 5. EditBoxControlLibrary is a library that ExpenseItDemo compiles
    again.
  - `BitmapFrame` is 8 apps: 6 under `M:` and 2 under `T:`.
- **Windows-only demand is dropped from the ranking:** `System.Windows.Forms` (7), `Form` (6), `Forms` (3),
  `AutomationElement` (3) and `DpiScale` (3). For `System.Windows.Interop` (3), only SaveWindowState is portable.
  `UserControl` (3) is 1 portable app (WPFGallery); the other 2 are Windows-only libraries.
- **Reported as generator errors, but actually layer gaps:**
  - `Button.FocusVisualStyleProperty`: the layer has no `FocusVisualStyle` at all.
  - `TextBlock.SetForeground`: WPF's `TextBlock` has static attached accessors, and the layer only has them on
    `TextElement`.
  - A Page root with no `Background`: 9 apps. These were generator artefacts in `9ca5b30b`. The generator now emits the
    `Page` base, and `Page : FrameworkElement` has no `Background`.
- **Reported as missing symbols, but actually generator bugs:**
  - `TreeViewItem`: `{x:Type}` nested in another markup extension.
  - `TextBlock.SetTextTrimming` (VideoViewerDemo): an owner-qualified property on its own type is emitted as an
    attached setter.
- **CustomApplicationClass is a recipe artefact.** Its recorded `NU1201` is the sibling client not yet converted. The
  manual rebuild fails with `CS5001` (no `Main`) and `CS2002`.
- **New error codes in 4 still-failing apps are not regressions.** Fixed blockers uncovered errors further on:
  - the two CommandSource samples (`RoutedCommand()`, `Slider.DecreaseSmall`/`IncreaseSmall`);
  - MediaGallery (`x:Name` fields, `Slider.ValueChanged` delegate);
  - AppCompat-Quirks (`Run.FontWeight` CLR conversion).

## Per category

These are portable apps on the strict recipe. Δ is the change since `9ca5b30b`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 1 | 0 | 1 | 0 |
| Animation | 11 | 0 | 11 | 2 | +2 | 0 | 0 |
| Application Management | 10 | 0 | 9 | 4 | 0 | 4 | 0 |
| Clipboard | 2 | 0 | 2 | 0 | 0 | 0 | 0 |
| Compatibility | 1 | 0 | 1 | 0 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 25 | +10 | 24 | +17 |
| Documents | 17 | 1 | 16 | 4 | +1 | 4 | +1 |
| Drag and Drop | 6 | 0 | 6 | 5 | +1 | 3 | 0 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 7 | 0 | 7 | 0 |
| Events | 4 | 0 | 4 | 3 | 0 | 3 | 0 |
| Getting Started | 8 | 0 | 8 | 5 | +2 | 5 | +2 |
| Globalization and Localization | 14 | 0 | 14 | 4 | +1 | 2 | +1 |
| Graphics | 26 | 0 | 25 | 3 | +1 | 2 | 0 |
| Input and Commands | 18 | 1 | 16 | 7 | +1 | 7 | +1 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 1 | 0 | 1 | 0 |
| Resources | 3 | 0 | 3 | 2 | 0 | 2 | +1 |
| Sample Applications | 23 | 1 | 21 | 2 | +1 | 2 | +1 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 3 | +2 | 2 | +1 |
| Threading | 3 | 0 | 3 | 0 | 0 | 0 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 4 | +2 | 4 | +2 |
| **Total** | **256** | **37** | **206** | **83** | **+24** | **74** | **+27** |

The library CustomApplicationClient also builds, so 84 projects build in all.

## What moved

**Fail→pass, 24 apps.** In each case, the change shown removed the only blocker recorded in `9ca5b30b`.

| Round 45 change | Apps | Run result now |
|---|---:|---|
| items-data-dialog (`f7938bd5`): `IsSynchronizedWithCurrentItem`, `CollectionView`, `UpdateSource`, `DataTransferEventArgs`, `DataContextChanged` | 9 | All run: CollectionBinding, Colors, MasterDetail, MasterDetailXml, MultiBinding, SortFilter, Grouping, UpdateSource, DirectionalBinding |
| long-tail-members (`4c6d28e9`): one or two members each | 10 | 8 run: HierarchicalDataTemplate, SpellChecking, GlobalizationRunDialog, HandlingEventOnCommandUsingCode, HtmlToXamlDemo, ContentControlStyle, MessageBox, ShowWindowWithoutActivation. 2 crash: DragDropEvents, IntroToStylingAndTemplating |
| generator-v2 (`50731e8b`): root base type, element triggers, CLR string→`ImageSource`/`Uri` | 5 | 2 run: HelloWorld, MultiPage (both regressions from `9ca5b30b` are fixed). 3 crash: OpacityAnimation, TargetValues, OpacityMasking |

**Crash/hang→runs, 8 apps.** These were all caused by startup resources (`50731e8b`: resource objects plus
parse-time `{StaticResource}`):

- BindConversion, BindingToMethod, BusinessLayerValidation, BindingToStringFomat, PropertyChangeNotification,
  XmlnsBind and DefiningResources were crashes.
- XmlDataSource was a hang.

**Error landscape**, counted as portable apps per code, `9ca5b30b` → now:

- `CS1061`: 64 → 45.
- `CS0246`: 65 → 57.
- `CS0029`: 9 → 4. CLR string conversions.
- Flat: `CS0117` 34 → 33, `CS0103` 27, `CS0234` 20, `CS0534` 9 (`CreateWidget`), `CS0102` 8.
- `CS2002` (a generated `.g.cs` is specified twice) is still a warning in 78 of the 83 portable builds.

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang. LocalizationResources and
StringLocalizationSample crash for the reasons in the table below. DragDropThumbOps hangs, as before.

## API demand

The counts are failing portable apps that report the symbol. Windows-only demand is excluded.

| Theme | Symbols (samples) | Apps |
|---|---|---:|
| Graphics (2D) | `Page.Background`/`BackgroundProperty` (9, including Page roots), `Vector` (5), `Point.Parse` (2), `SystemColors.*Brush` (2), `VisualBrush.Viewbox` (2), `TextBlock.SetForeground` attached accessors (2), `UIElement.Clip`, `GeneralTransform`, `CompositionTarget`, `HitTestResultBehavior`, `GeometryHitTestParameters`, `BrushConverter`/`ThicknessConverter` | 25 |
| Documents and text | `FlowDocument.ColumnWidth` (3), `ColumnGap`/`IsColumnWidthFlexible` (2), `Table`/`TableRow`/`TableCell` (3), `System.Windows.Annotations` (2), `PrintDialog` (2), `TextRange` `Load`/`Save`, `FlowDocument.ContentStart`/`End`, `TextEffect`, `FlowDocumentPageViewer` | 19 |
| Input and focus | `KeyboardFocusChangedEventArgs` (4), `FocusVisualStyle` (3), `RoutedCommand()` (3), `Slider.AutoToolTipPlacement`/`Precision` (3), `Slider.DecreaseSmall`/`IncreaseSmall` (2), `Cursors.ScrollWE`, `Cursor.Dispose`, `Mouse.SetCursor`, `EditingCommands`, `ContextMenuEventArgs`, `FocusNavigationDirection` | 17 |
| Animation | `TranslateTransform.X/YProperty` + `BeginAnimation` (3: `TranslateTransform` is not a `DependencyObject`), `DoubleAnimationBase` (2), `Clock` (2), `Storyboard.GetCurrentTime`/`Seek`/`TimeSeekOrigin` (2), `HandoffBehavior`, `SolidColorBrush.ColorProperty` | 13 |
| Extensibility | `UIElement.CreateWidget()` leaking as abstract (`CS0534`, 9), overridable `OnRender`/`LogicalChildren`/`Slider.OnValueChanged`, `ICommandSource.CommandTarget` typed as `UIElement` | 12 |
| Flow direction | `FlowDirection` enum and `FrameworkElement.FlowDirection` (`TextBlock` 4, `StackPanel` 2, `Window` 2), `Inline.FlowDirection` (`Span` 3, `Run` 2), `FlowDocument.FlowDirection` (2), `Span.Language`, `NumberSubstitution` | 11 |
| Imaging | `BitmapFrame` (8), `BitmapCreateOptions`/`BitmapCacheOption` (7), `PixelFormats.Indexed*` (5), `BitmapPalettes` (5), `Png`/`Jpeg`/`Gif`/`Tiff`/`Bmp`/`Wmp` decoders and encoders, `BitmapImage.Format`, `BitmapMetadata` | 11 |
| 3D | `System.Windows.Media.Media3D`: `Viewport3D` (5), `Vector3D` (5), `Quaternion` (4), `Point3D` (2), `GeometryModel3D` (2) | 8 |
| XAML at runtime | `XamlReader` (4), `XamlWriter` (3), `XamlParseException` (3), `Application.LoadComponent` | 8 |
| Navigation and pages | `NavigationEventArgs` (3), `PageFunction<T>`/`ReturnEventArgs<T>`, `UserControl` | 6 |
| Items and validation | `IEditableCollectionView`, `BindingGroup.GetValue`/`TryGetValue`, `Validation.ErrorProperty`, `Binding.UpdateSourceExceptionFilter`, `Label.Target`, `ItemContainerGenerator`, `ItemsControl.ContainerFromElement`, `FilterEventArgs` | 6 |
| Threading | `DispatcherOperationCallback` (2), static `Dispatcher.Run` (2), `DispatcherObject.Dispatcher` on elements (2) | 5 |
| Dialogs and clipboard | `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog` (3), `CommonDialog`, `Clipboard.ContainsAudio`/`ContainsImage`/`ContainsFileDropList` | 5 |
| Automation peers | `System.Windows.Automation.Peers` (4) | 4 |
| Window | `Window.DragMove` (2), `AllowsTransparency` (2) | 2 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| OpacityAnimation, TargetValues | crash | `'' is not a valid value for property 'Background'`. The `DrawingBrush` resources in `App.xaml` are still emitted as `Resources["…"] = "";`. The resource-object fix does not yet cover `DrawingBrush`/`DrawingGroup` |
| IntroToStylingAndTemplating | crash | `BasedOn="{StaticResource {x:Type TextBlock}}"` is emitted as `FindResource("{x:Type TextBlock")`. The nested markup extension is truncated instead of becoming `typeof(TextBlock)` |
| DragDropEvents | crash | The exception is now printed (it was swallowed before): `ArgumentOutOfRangeException` at `mainGrid.RowDefinitions[1]` in the sample's `SizeChanged` handler. No generated file of the 84 built samples contains `Grid.RowDefinitions` or `ColumnDefinitions`, so the grid has no rows. MarkupCompiler track |
| LocalizationResources | crash | Not a manifest-name defect: the converter leaves `EmbeddedResource` untouched. The sample embeds only `stringtable.en-us.restext`, which the SDK compiles into an en-US satellite; there is no neutral resource and no `UICulture`. `ResourceManager` therefore finds nothing under any other UI culture (en-GB here), as in real WPF. It runs with the UI culture forced to en-US |
| StringLocalizationSample | crash | `Resource with key 'LocalizedMessage' not found`. `ResourceDictionary.Source` now loads a registered compiled dictionary, but the markup compiler emits nothing for `Application.Resources` `MergedDictionaries` or for `StringResources.xaml`. MarkupCompiler track |
| DragDropThumbOps | hang | Unchanged. Alive with no top-level window after 38 s, and the logs are empty. Not diagnosed |

## XAML generator errors

These counts are failing portable apps. Per-project messages are in the JSON (`xamlErrors`). Generator v2 removed the
two largest classes: the root base type for x:Class with no code-behind (16 apps), and internal
`AttachElementTriggers` (13 apps). What is left:

1. **Name scopes (`CS0102`/`CS0111`/`CS0260`/`CS0262`): 10 apps.** An `x:Name` inside a template or resource becomes
   a root field, and it collides.
   - DataTemplatingIntro and GlobalizationHomepage fail on nothing else.
   - AnimationExamples, Brushes, Callbacks, DataBindingDemo, PhotoStoreDemo and DocumentMerge also hit it.
   - WPFGallery has a duplicate `Main`.
   - PropertyAnimation's code-behind class lacks `partial`. WPF tolerates this.
2. **Routed delegate and attached-event syntax: 6 apps.**
   - `Slider.ValueChanged` is wired as `EventHandler` and `SelectionChanged` as `EventHandler<…>`: KeySplineAnimations,
     ParagraphAndHyphenation, MediaGallery, BitmapEffectsGallery.
   - `Button.Click`/`RadioButton.Checked` on another element are emitted as attached properties. RoutedEventHandling
     and CaptureUnCaptureMouse fail on nothing else.
3. **Root base type when the code-behind partial declares none: 3 apps.** ControlsAndLayout (`Scene`), FindText (`App`)
   and MediaGallery. Generator v2 emits the base only when there is no code-behind.
4. **Value conversion (`CS0029`): 3 apps.**
   - `Run.FontWeight` (a CLR `FontWeight?`) gets a raw string. AppCompat-Quirks fails on nothing else.
   - `int` → `string` (2DTransforms).
   - `FontFamily` → `string` (FontProperties).
5. **`{x:Type}` nested in another markup extension.** It is emitted unqualified at compile time
   (AlternatingAppearanceOfItems, which fails on nothing else). At run time it is truncated (the
   IntroToStylingAndTemplating crash).
6. **An owner-qualified property on its own type** (`<TextBlock TextBlock.TextTrimming=…>`) is emitted as
   `TextBlock.SetTextTrimming` (VideoViewerDemo, which fails on nothing else).
7. **Missing `x:Name` fields:** `mediaElement` (WindowActivationAndDeactivation, which fails on nothing else),
   `myMediaElement` (MediaGallery), keyframe names (KeySplineAnimations), `trg1` (TableRows) and `root`
   (MergedResources).
8. **Not a generator bug:** DocumentStructure's `fixedpage1_structure.xaml` is malformed XML.
9. **Media resources that still fall back to a plain value (Round 46 review).** A keyed media resource is built as an
   object only when every attribute converts and every nested element resolves. `{StaticResource}`/`{DynamicResource}`
   now convert on a settable media reference property (`DynamicResource` is resolved once, there is no dynamic media
   binding), and `DrawingImage`, `PathGeometry` (`Figures` markup, `Transform`), `GradientStopCollection` and
   `GeometryGroup` are built. Left as a plain value: a `<Color>` resource (kept as text, coerced at the use site),
   `CollectionViewSource` and `HierarchicalDataTemplate` (not media objects; their emission is unchanged), and any media
   resource with an attribute or nested type the layer lacks. A skipped media property or child element is now reported
   as a build message (`Skipped ... on X: not available in the layer`) instead of vanishing silently.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole remaining blocker is the track.

1. **Startup crashes: up to 6 more runs, and every one of these apps already builds.**
   - Instantiate `DrawingBrush`/`DrawingGroup` resource objects (OpacityAnimation, TargetValues).
   - Resolve `{StaticResource {x:Type T}}` (IntroToStylingAndTemplating).
   - Emit `Application.Resources` `MergedDictionaries` and register dictionary XAML for `ResourceDictionary.Source`
     (StringLocalizationSample, MarkupCompiler).
   - Emit `Grid.RowDefinitions`/`ColumnDefinitions` (DragDropEvents; no generated file of the 84 built samples contains them, MarkupCompiler).
2. **XAML generator v3: 9 only.**
   - Only: DataTemplatingIntro, GlobalizationHomepage, RoutedEventHandling, CaptureUnCaptureMouse,
     AlternatingAppearanceOfItems, VideoViewerDemo, AppCompat-Quirks, WindowActivationAndDeactivation, PropertyAnimation.
   - Template and resource name scopes.
   - Routed delegate types and attached-event syntax.
   - A base type even when the code-behind partial has none.
   - Nested `{x:Type}`.
   - Owner-qualified properties.
   - Nullable/struct CLR conversions.
   - Missing `x:Name` fields.
3. **Flow direction: 11 involved, 9 essentially only.**
   - Only: FlowDirection, Gradient, Image, LTRRTL, Paths; plus FlowContentProperty, LangAttribute (`Span.Language`),
     RunSpan, Span.
   - Covers: the `FlowDirection` enum, `FrameworkElement.FlowDirection` (inherited), `Inline.FlowDirection`, and
     `FlowDocument.FlowDirection`.
4. **Input and focus: 17 involved, 8 only.**
   - Only: ContextMenuOpening, FocusVisualStyle, CursorType, CustomRoutedCommand, EditingCommands, KeyStrokeCounter,
     CalculatorDemo, ProgrammaticFocusControl.
   - Covers: `KeyboardFocusChangedEventArgs` and the `(Preview)Got/LostKeyboardFocus` overrides, `FocusVisualStyle`,
     `RoutedCommand()`, `Cursors.ScrollWE`, `Cursor.Dispose`, `EditingCommands`, `ContextMenuEventArgs`,
     `FocusNavigationDirection`, and the `Slider` `AutoToolTip*`/`DecreaseSmall`/`IncreaseSmall` members.
5. **2D graphics, Page and Window long tail: about 9 only.**
   - Only: ShapeElements, PathAnimations (`Page.Background`), DrawingBrush (`TextBlock` attached accessors),
     ClipRegion (`UIElement.Clip`), Vector, SystemBrushesAndColors, CompositionTarget, CustomWindowUI and
     NonRectangularWindow (`DragMove`, `AllowsTransparency`).
   - Also: `Page.Background` unblocks part of 9 apps.
6. **Threading, dialogs and clipboard: 7 only.**
   - Threading only: ExceptionHandlingSecondaryUIThread, ExceptionHandlingSecondaryWorkerThread,
     MultiThreadingWebBrowser, SingleThreadedApplication. Covers `DispatcherOperationCallback`, static `Dispatcher.Run`
     and `DispatcherObject.Dispatcher` on elements.
   - Dialogs and clipboard only: ClipboardSpy (`Clipboard.Contains*`) and CommonDialogs (`Microsoft.Win32.CommonDialog`
     with `OpenFileDialog`/`SaveFileDialog`).

Deliberately not next:

- Imaging codecs and Media3D: 19 apps, 9 only, but large.
- Animation clocks (`TranslateTransform` as an animatable `DependencyObject`, `Clock`, `DoubleAnimationBase`): 13
  involved, 1 only.
- Extensibility (`CreateWidget` leaking as abstract): 9 apps, but always alongside other gaps. Fold it into whichever
  round touches `UIElement`.
- System.Xaml/BAML tooling.
- Legacy (non-SDK) csproj conversion: 21 projects.
- Packaging hygiene, still open: `CS2002` in 78 of 83 builds, and `NU1201` in converted ProjectReference chains
  (CustomApplicationClass, BamlReflector).

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches. Each batch was copied under
  `.claude/samples4/batchN`, with empty `Directory.Build.props`/`.targets` (so `TreatWarningsAsErrors` was off) and a
  fresh isolated NuGet cache (`samples4/.nuget-packages`). No original or tracked file was modified.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26324` from the repo `Packages/` feed.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again; this result is recorded.
  - Nothing else is changed. The diff is saved as `csproj.diff` beside each copy.
  - Exception: CustomApplicationClass was rebuilt once by hand after its sibling client had converted.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. The error codes and messages in
  the JSON were re-parsed uniformly from the final logs of all six batches (UTF-8 and UTF-16). The parse agreed with
  every batch's own pass/fail.
- **Run.** Each app that built got a serial smoke run on Windows of about 10 s.
  - "Runs" means alive with a visible top-level window.
  - "Crash" means an unhandled exception or an exit with no window.
  - "Hang" means alive with no window.
  - The run proves nothing about visual or behavioural correctness.
  - DragDropEvents was re-run alone for this report.
- **Windows-only.** The verified classification from the earlier runs, plus the native C++ projects.
- **Limits.**
  - Only Windows was measured; Linux and macOS were not run.
  - Legacy (non-SDK) csproj files are not converted, so they were not built: 21 projects.
  - The theme counts come from a regex classification of compiler messages (`blockerThemes` in the JSON). They are a
    ranking aid, not an exact partition.
  - The attribution of the moves is by elimination: the removed blocker was the only one recorded before. It was not
    bisected.

## History

| Date | Main SHA | Package | Portable build | Portable run | Note |
|---|---|---|---:|---:|---|
| 2026-11-19 | `3b73e4f5` | 1.0.0.26323 | 0 / 206 | 0 | +CM diagnostic: 16 build, 10 run |
| 2026-11-20 | `db9526db` | 1.0.0.26324 | 45 / 206 | 31 | 10 crash, 4 hang; no regressions |
| 2026-11-20 | `64cddd04` | 1.0.0.26324 | 59 / 206 | 47 | Round 44; 10 crash, 2 hang; 2 regressions (TargetValues, MultiPage: x:Class-less Page now compiled) |
| 2026-11-20 | `ea4c17ae` | 1.0.0.26324 | 83 / 206 | 74 | Round 45; 8 crash, 1 hang; no regressions (both Round 44 regressions fixed) |
| 2026-11-20 | `a37d3b17` + `gf46/startup-runtime` | 1.0.0.26324 | 83 / 206 | 76 | Round 46 startup runtime: Linq and OpacityMasking run; no regressions; 6 crash, 1 hang |
