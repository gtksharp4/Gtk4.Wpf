# WPF samples coverage

Measured 2026-11-20 on `main` at `db9526db0cfbf377d1f104410ef377910d0f6aed`, package `Gtk4.Forms.Wpf 1.0.0.26324`,
against the official `microsoft/WPF-Samples` (253 projects: 243 apps, 9 libraries and 1 native C++ codec). Per-project
results are in [`samples-coverage.json`](samples-coverage.json). Each row also holds the first run's result
(`previous`), so future runs can diff against it.

## Headline

**45 of 206 portable official WPF sample apps build unmodified, and 31 run. On 2026-11-19 (`3b73e4f5`, report
`fedd13c8`) the figures were 0 and 0.**

- Of the other 14 apps that build, 10 crash at startup and 4 hang without a window.
- 37 apps are Windows-only by design (WinForms, HWND interop, UIA client, COM, System.Speech, per-monitor DPI). They
  are excluded from the denominator, as in the first run.
- **No regressions.** Every project that built in the first run, with or without the diagnostic package, still builds.
  Two of its startup crashes now run (see below).
- **Both walls from the first run are gone.**
  - `CS1069` (`ApplicationSettingsBase`) was in 150 projects and is now in 2. Neither of those is
    ConfigurationManager.
  - The `{x:Type}` syntax-error cluster (`CS1026`/`CS8124`/`CS1519`/…) was in 66 projects and is now in none.
- **The next wall is the XAML generator.** 63 of the 160 failing portable apps contain generator errors. For 22 of them,
  generator errors are the only errors (15 have only content-model errors).
- `CS2002` (a generated `.g.cs` is specified twice) is a warning in 41 of the 46 successful builds. Any consumer that
  sets `TreatWarningsAsErrors` would fail on it.

### Corrections to the raw batch data

The top 15 missing symbols were checked against `Gtk4.Forms.*` source, and every crash and hang was checked against its
run log. The following are corrected here and in the JSON.

- **Two run results were swapped.**
  - MultiPage is a **hang**, not a crash: it was alive at 30 s with no window, and its `StartupUri` is a `Page`.
  - LocalizationResources is a **crash**, not a hang. It throws `'data\picture1.jpg' is not a valid value for property
    'Source'` in `InitializeComponent`.
  - The totals are unchanged: 10 crashes and 4 hangs.
- **The `.Content` symbols are generator artefacts, not API demand.** This covers 8 of the top 15: `Border`, `TextBlock`,
  `TabControl`, `Menu`, `MenuItem`, `FlowDocument`, `ComboBox`, `Paragraph`, and also `ListBox`.
  - `XamlCodeGenerator.EmitChildren` emits `.Children.Add` only for the five panels listed in `WpfXmlnsMap.HasChildren`.
    For every other parent it emits `.Content =`.
  - The layer types already declare the right content property: `Border` is `[ContentProperty("Child")]`, `TextBlock`
    is `Inlines`, `ItemsControl` is `Items` and `FlowDocument` is `Blocks`.
  - `TextBox.space` is likewise `xml:space` emitted as a property.
- **`ValueConversion` and `ValueConversionAttribute` are one symbol.** It is missing from the layer: 6 samples.
- **`Form` and `System.Windows.Forms` are Windows-only demand.** Every sample that names them is a Windows-only project,
  so they are dropped from the ranking.
- **Real gaps, confirmed:**
  - `Selector.IsSynchronizedWithCurrentItem` and the `System.Windows.Media.Media3D` types are absent.
  - `RadioButton.Checked` is missing because `RadioButton` derives from `ContentControl`, not `ToggleButton`.
  - `ToggleButton`, `CheckBox` and `MenuItem` declare `Checked` as `EventHandler`. WPF handlers take `RoutedEventArgs`,
    so the generated wiring fails with `CS0123` (11 projects).
- **Members reported "missing" on the samples' own types are generator artefacts.** Examples are
  `TransformExample.WindowTitle` and `SampleViewer.Title`.
  - For XAML with no code-behind (OpacityMasking, ClipRegion, 2DTransforms and others), the generated partial class
    declares no base type, so the class derives from `object`.
  - `Page.Background`, `Window.AllowsTransparency` and `FrameworkElement.FlowDirection`, by contrast, really are
    missing.
- **`AttachElementTriggers` is a generator/layer mismatch, not a missing API.** The generator emits
  `x.AttachElementTriggers()` into user code, but `FrameworkElement.AttachElementTriggers` is `internal`
  (6 portable samples).
- **The aggregate's `windowsOnly` total of 42 is replaced by 41 projects (37 apps).** The 41 are the verified first-run
  classification plus the native AITCodec. The batch heuristic disagreed on a few legacy projects.
- **The first run's correction to `CreateWidget` was itself wrong.** It is `protected abstract`, not public. It still
  leaks into user subclasses, giving `CS0534` in 9 portable apps and 1 library.
- **Accessibility/Highlighter is not a regression.** It "passed" in the first run only because it built off-layer on
  .NET Framework. Legacy csproj files were not built this time.

## Per category

These are portable apps on the strict recipe. Δ is the change since `fedd13c8`, where every value was 0. "+CM then" is
the first run's diagnostic, which added the ConfigurationManager package by hand (build / run).

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ | +CM then |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 0 | 0 | 0 | 0 | 0 / 0 |
| Animation | 11 | 0 | 11 | 1 | +1 | 0 | 0 | – |
| Application Management | 10 | 0 | 9 | 4 | +4 | 4 | +4 | 3 / 2 |
| Clipboard | 2 | 0 | 2 | 0 | 0 | 0 | 0 | 0 / 0 |
| Compatibility | 1 | 0 | 1 | 0 | 0 | 0 | 0 | – |
| Data Binding | 31 | 1 | 30 | 10 | +10 | 5 | +5 | 8 / 3 |
| Documents | 17 | 1 | 16 | 3 | +3 | 3 | +3 | – |
| Drag and Drop | 6 | 0 | 6 | 2 | +2 | 1 | +1 | 0 / 0 |
| Element Tree | 2 | 0 | 2 | 1 | +1 | 1 | +1 | 0 / 0 |
| Elements | 10 | 0 | 10 | 3 | +3 | 3 | +3 | 3 / 3 |
| Events | 4 | 0 | 4 | 2 | +2 | 2 | +2 | – |
| Getting Started | 8 | 0 | 8 | 4 | +4 | 2 | +2 | – |
| Globalization and Localization | 14 | 0 | 14 | 3 | +3 | 1 | +1 | – |
| Graphics | 26 | 0 | 25 | 2 | +2 | 2 | +2 | – |
| Input and Commands | 18 | 1 | 16 | 5 | +5 | 3 | +3 | – |
| Migration and Interoperability | 20 | 15 | 0 | 0 | 0 | 0 | 0 | – |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 | – |
| Properties | 3 | 0 | 3 | 1 | +1 | 1 | +1 | 1 / 1 |
| Resources | 3 | 0 | 3 | 1 | +1 | 1 | +1 | 1 / 1 |
| Sample Applications | 23 | 1 | 21 | 1 | +1 | 0 | 0 | – |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 | – |
| Styles & Templates | 5 | 0 | 5 | 1 | +1 | 1 | +1 | – |
| Threading | 3 | 0 | 3 | 0 | 0 | 0 | 0 | – |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 | – |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 | – |
| Windows | 12 | 2 | 10 | 1 | +1 | 1 | +1 | – |
| **Total** | **253** | **37** | **206** | **45** | **+45** | **31** | **+31** | **16 / 10** |

The library CustomApplicationClient also builds, as it did before: 46 projects build in all. The Graphics row gained a
project, the native AITCodec, which was not counted in the first run.

## What moved

All 45 builds are fail→pass. "Cause" is the change that removed the first-run blocker. Only the first error wall was
visible in the first run. For apps where the +CM diagnostic was not tried, anything behind `CS1069` was hidden, so the
evidence shows only that one change was necessary, not that it alone was sufficient.

| Cause | Apps | Run result now |
|---|---:|---|
| ConfigurationManager dependency (`3dcb00ff`) | 31 | 21 run; crash: BindConversion, BusinessLayerValidation, PropertyChangeNotification, ComplexLayout, DropShadow, HandlingEventOnCommand, LocalizationResources, StringLocalizationSample; hang: MultiPage |
| ConfigurationManager + 141 named colours (`a501daeb`) | 3 | PriorityBinding and ProcessingCommandLineArguments: **crash → run**. XmlDataSource got past the colour crash and now crashes on a resource lookup. |
| ConfigurationManager + text-control members (`a501daeb`: `TextWrapping`, `AppendText`, `ScrollToEnd`, scroll-bar visibility) | 2 | DragDropDataFormats runs; BindingToStringFomat crashes on a resource lookup |
| ConfigurationManager + converter pin raise (`b67b8f88`, NU1605) | 1 | UnhandledExceptionHandling runs |
| ConfigurationManager + qualified type names (`05d4353c`, `TargetType="ListBoxItem"`) | 1 | DataTrigger runs |
| ConfigurationManager + presentation xmlns mappings (`0c4bd3f7`: `Primitives.Thumb` resolves, so the `myThumb` field is generated) | 1 | DragDropThumbOps hangs |
| `{x:Type}` in `TargetType` (`05d4353c`) | 5 | SearchingForElement, AddingEventHandler, FindingSourceElement run; TargetValues and EventOnGainAndLooseFocus hang |
| Qualified names + SpellCheck/xmlns (`05d4353c`, `0c4bd3f7`) | 1 | CustomDictionaries runs |

CLR value typing (`70d8a271`): no move can be attributed to it alone from this data. It was not isolated, because
proving it would mean rebuilding at `0c4bd3f7`.

### Error landscape

The counts below are portable apps that report each code, first run → now.

- **Gone:**
  - `CS1069`: 135 → 1. The remaining one is WPFGallery, which needs `Microsoft.Win32.SystemEvents` for
    `UserPreferenceChangedEventArgs`. It is a packaging gap of the same kind as ConfigurationManager.
  - The syntax cluster `CS1026`/`CS8124`/`CS1519`: 62 → 0.
- **Unmasked:** these are semantic errors that the syntax errors and `CS1069` had hidden, not regressions.
  - `CS1061`: 20 → 90.
  - `CS0246`: 30 → 66.
  - `CS0117`: 2 → 34.
  - `CS0103`: 5 → 27.
  - `CS0123`: 0 → 11.
  - `CS0534` (`CreateWidget`): 9.
  - `CS0102` (name scopes): 8.
  - `CS0029` (value conversion): 8.
- `NU1201` (restore order in ProjectReference chains): 9 → 2. The two are BamlReflector and CustomApplicationClass.

## Regressions

**None.**

- All 16 apps that built with the first run's +CM diagnostic build now.
- The 10 that ran still run.
- Of the 6 that crashed, 2 now run (PriorityBinding, ProcessingCommandLineArguments). The other 4 still crash, and
  XmlDataSource's crash cause moved from colours to a resource lookup.
- The two first-run as-is "passes":
  - CustomApplicationClient (a library) still builds.
  - Highlighter (built off-layer) is a scope change, not a regression (see the corrections).

## API demand

The counts are portable projects that report the symbol. Generator artefacts and Windows-only demand are excluded.
Error masking is much lower than in the first run, but the counts are still floors.

| Theme | Symbols (samples) | Portable apps |
|---|---|---:|
| Items, data and dialog semantics | `Selector.IsSynchronizedWithCurrentItem` (10), `ValueConversionAttribute` (6), `Button.IsDefault`/`IsCancel` (3), concrete `HeaderedContentControl` (2), settable `Window.DialogResult` (2), `ContentPresenter.ContentSource` (2), `CollectionView` (2), `BindingExpressionBase.UpdateSource` (2), `DataTransferEventArgs` (2), `ListViewItem` (2), `Validation.ErrorProperty`, `BindingGroup.TryGetValue`, `IEditableCollectionView` | 26 (10 only this) |
| Event surface | `RoutedEventHandler`-typed `Checked`/`Unchecked`/`Click`/`SelectionChanged` (`CS0123`, 11), `RadioButton.Checked` (7), `CheckBox.Click` (3), `RadioButton.Click` (2), `Button.ClickEvent` as a routed event (2), overridable `OnClick` (2) | 23 |
| Documents and text | `FlowDocument.ColumnWidth`/`ColumnGap`/`IsColumnWidthFlexible` (3), `Run` as a `DependencyObject` (2), `Table` (2), `PrintDialog` (2), `System.Windows.Annotations` (2), `TextRange` ctor/`Load`/`Save`, `FlowDocument.ContentStart/End`, `Typeface`, `XmlLanguage` | 23 |
| Graphics (2D) | `Vector` (4), `Point`/`Matrix` operators, `Parse`, `Add`, `Multiply` (2–3), `SystemColors.*Brush`/`*Color` (2), `VisualBrush.Viewbox`, `ImageBrush.Viewbox` (2), `CompositionTarget`, `HitTestResultBehavior`, `GeneralTransform` | 17 |
| Input | `KeyboardFocusChangedEventArgs` and the `(Preview)Got/LostKeyboardFocus` overrides (5), `FocusVisualStyle` (2), `ContextMenuEventArgs`, `Cursors.*`, `EditingCommands`, `RoutedCommand()` | 16 |
| Extensibility | `UIElement.CreateWidget()` leaking as abstract (9 apps + 1 library), overridable `OnMouseEnter/Leave/Up`, `OnApplyTemplate` (2), `LogicalChildren` (2), `OnRender(DrawingContext)` | 12 |
| Flow direction | `FrameworkElement.FlowDirection` (`TextBlock` 4, `StackPanel` 2, `Window` 2), `Inline.FlowDirection` (`Span` 3, `Run` 2), `FlowDocument.FlowDirection` (2), `NumberSubstitution` | 11 (3 only this) |
| Imaging | `BitmapCreateOptions`/`BitmapCacheOption` (7), `BitmapFrame` (6), `PixelFormats.Indexed*` (5), `BitmapPalettes` (5), `Png`/`Jpeg`/`Gif`/`Tiff`/`Bmp`/`Wmp` `Decoder`+`Encoder` | 10 (4 only this) |
| Animation | `TranslateTransform.X/YProperty` and `BeginAnimation` (3), `SolidColorBrush.ColorProperty` (2), `Storyboard.Seek`/`TimeSeekOrigin`/`HandoffBehavior` (2), `DoubleAnimationBase` (2), `Clock` (2), `Storyboard.Begin(x, bool)` (2) | 10 |
| 3D | `System.Windows.Media.Media3D`: `Viewport3D` (5), `Vector3D` (5), `Quaternion` (4), `Point3D`, `GeometryModel3D` | 9 (6 only this) |
| Navigation and pages | `NavigationEventArgs` (3), `Page.Background` (2), `PageFunction<T>`, `ReturnEventArgs<T>`, `UserControl` (2) | 9 |
| XAML at runtime | `XamlReader` (4), `XamlWriter` (3), `XamlParseException` (3), `Application.LoadComponent` | 7 |
| Dialogs and clipboard | `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog` (3), `Clipboard.ContainsAudio`/`FileDropList`/`Image` | 5 |
| Threading | static `Dispatcher.Run` (2), `DispatcherOperationCallback` (2) | 4 |
| Automation peers | `System.Windows.Automation.Peers` (4), `IExpandCollapseProvider`, `PatternInterface` | 4 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| BindConversion, BusinessLayerValidation, BindingToStringFomat, PropertyChangeNotification, XmlDataSource, StringLocalizationSample | crash | `Resource with key '…' not found` from `FindResource` in the generated `InitializeComponent`. The lookup runs before the element is parented, or against the wrong scope: BindConversion registers the resource on an inner StackPanel. |
| DropShadow | crash | `'1,0,0,1,3,3' is not a valid value for property 'RenderTransform'`. A string is not converted to a `Transform`. |
| ComplexLayout | crash | `Cannot convert "30px" for Button.Height`. LengthConverter units are not supported. |
| LocalizationResources | crash | `'data\picture1.jpg' is not a valid value for property 'Source'`. A string is not converted to an `ImageSource`. |
| HandlingEventOnCommand | crash | `'ApplicationCommands.Open' is not a valid value for property 'Command'`. A command name is not converted to an `ICommand`. |
| TargetValues, MultiPage | hang | The process is alive with no window. `StartupUri` is a `Page`, which was not hosted in a window. Round 44 hosts it in a `NavigationWindow`, but both are still hangs (not re-measured), so 3 of the 6 samples that track targeted are fixed. |
| EventOnGainAndLooseFocus, DragDropThumbOps | hang | The process is alive with no window and the logs are empty. Both have a `Window` startup. Not diagnosed. |

There are two root causes:

1. `{StaticResource}` resolution order.
2. Missing string→object conversion for DP values: `Transform`, `ImageSource`, `ICommand` and length units.

Together they account for all 10 crashes.

## XAML generator errors (regression corpus for `gf39/xaml-content-model`)

Per-project messages are in the JSON (`xamlErrors`). The counts are portable apps.

1. **Content property ignored: 63 apps, 15 of which fail on nothing else.** Children are assigned to `.Content` for
   every parent except `Grid`, `StackPanel`, `DockPanel`, `WrapPanel` and `Canvas`. Affected parents include
   `Border` (44), `TextBlock` (14), `TabControl` (12), `Menu`/`MenuItem` (8), `FlowDocument`/`Paragraph` (7),
   `ComboBox`/`ListBox`/`TreeView`/`TreeViewItem` and `UserControl` roots.
   - Examples: BindingDPToDP, Linq, SimpleBinding, XmlnsBind, HeightProperties, WidthProperties, UsingElements,
     DragDropTextOps, MousePointer, DefiningResources, WindowSizingOrder, PathAnimations, Target.
2. **The x:Class root: 20 apps.**
   - For XAML with no code-behind (OpacityMasking, ClipRegion, 2DTransforms), the partial class declares no base type.
   - Root members that the layer lacks also land here: `Page.Background`, `Window.AllowsTransparency`, `FlowDirection`.
3. **Name scopes: 10 apps.**
   - An `x:Name` inside a template or resource becomes a root field that collides with code-behind (`CS0102`):
     DataTemplatingIntro, Callbacks, AnimationExamples, Brushes, DataBindingDemo, PhotoStoreDemo, DocumentMerge,
     GlobalizationHomepage.
   - WPFGallery gets a duplicate generated `Main` (`CS0111`).
4. **Event wiring: 11 apps with `CS0123`.**
   - `Checked`/`Unchecked`/`Click`/`SelectionChanged` handlers are wired as `EventHandler` or
     `EventHandler<SelectionChangedEventArgs>`.
   - `Button.Click` on a non-button is emitted as an attached setter (`Button.SetClick`, `Button.ClickProperty`).
5. **Value conversion: 7 apps.**
   - `Polygon.Points` → `PointCollection` (4).
   - `Image.Source`/`ImageBrush.ImageSource` → `ImageSource` (2).
   - `TextBox.Text="0"` is emitted unquoted, giving `int` → `string`.
   - A nullable-enum CLR property (`Block.TextAlignment`) is emitted as a string. `ClrPropertyExpression` skips
     generic types.
6. **Element triggers: 6 apps.** The generated code calls `FrameworkElement.AttachElementTriggers()`, which is
   `internal`.
7. **`xml:space`** is emitted as a `space` property: 6 apps.
8. **Not a generator bug:** DocumentStructure's `fixedpage1_structure.xaml` is malformed XML.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole remaining blocker is the track.

1. **XAML content model and generator fixes (`gf39/xaml-content-model`).**
   - Covers:
     - honour `ContentPropertyAttribute`: `Border.Child`, `ItemsControl.Items`, `TextBlock`/`Paragraph.Inlines`,
       `FlowDocument.Blocks`, `HeaderedItemsControl.Items`, `Panel.Children` for every `Panel`;
     - `xml:space`;
     - emit the base type for XAML with no code-behind;
     - template name scopes;
     - the value conversions in item 5 above;
     - make `AttachElementTriggers` reachable from generated code.
   - Impact: 22 apps fail only on generator errors, and the generator is involved in 63.
2. **Startup robustness.**
   - Covers:
     - `{StaticResource}` resolved against the parse-time scope, not `FindResource` on an unparented element;
     - string conversion for `Transform`, `ImageSource`, `ICommand` (`ApplicationCommands.X`) and `LengthConverter`
       units;
     - a `Page` as `StartupUri`, hosted in a window.
   - Impact: 10 crashes and 2 hangs. These apps already build, so this is the cheapest gain in the run count.
3. **Event surface.**
   - Covers:
     - `RoutedEventHandler` for `ToggleButton`/`CheckBox`/`MenuItem` `Checked`/`Unchecked`/`Click`;
     - `RadioButton : ToggleButton`;
     - `ButtonBase.Click`;
     - `Selector.SelectionChanged` delegate type;
     - `Button.ClickEvent` as a routed event.
   - Impact: involved in 23 apps (2 only).
4. **Items, data and dialog semantics.**
   - Covers: `IsSynchronizedWithCurrentItem`, `ValueConversionAttribute`, `IsDefault`/`IsCancel`, concrete
     `HeaderedContentControl`, settable `DialogResult`, `ContentSource`, `CollectionView`/`IEditableCollectionView`,
     `UpdateSource`, `DataTransferEventArgs`, `ListViewItem`.
   - Impact: involved in 26 apps (10 only).
5. **Extensibility and flow direction.**
   - Covers:
     - hide `CreateWidget` from user subclasses (an internal factory or a virtual default);
     - overridable `OnMouse*`, `OnApplyTemplate`, `LogicalChildren`, `OnClick`, `OnRender`;
     - `FrameworkElement.FlowDirection` and `Inline.FlowDirection`.
   - Impact: involved in 23 apps (4 only), including CalculatorDemo and ExpenseIt.
6. **Packaging hygiene.** These are cheap fixes, and they matter for real consumers.
   - Covers:
     - fix the duplicate generated `Compile` items behind `CS2002` (41 of 46 builds);
     - add a `Microsoft.Win32.SystemEvents` dependency (WPFGallery);
     - the restore order of converted ProjectReference chains (`NU1201`, 2 projects).

Deliberately not next: Media3D (9 apps, 6 only, large), the imaging codecs (10 apps, 4 only), System.Xaml/BAML
tooling, and legacy (non-SDK) csproj conversion (21 projects, only 1 of them a portable app).

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches. Each batch was copied under
  `.claude/samples2/batchN`, with empty `Directory.Build.props`/`.targets` (so `TreatWarningsAsErrors` was off) and an
  isolated NuGet cache. No original or tracked file was modified.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26324` from the repo `Packages/` feed.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again; this result is recorded.
  - Nothing else is changed. The csproj diff is the PackageReference plus the converter rewrite, and it is saved as
    `csproj.diff` beside each copy.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Exit codes from `Start-Process`
  were unreliable in several batches. The error codes and messages in the JSON were re-parsed uniformly from the
  final logs for all six batches.
- **Run.** Each app that built got a smoke run on Windows, polled for 8–30 s depending on the batch.
  - "Runs" means alive with a visible top-level window.
  - "Crash" means an unhandled exception or an exit with no window.
  - "Hang" means alive with no window.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** Carried over from the first run's verified classification, plus AITCodec (native C++).
- **Limits.**
  - Only Windows was measured; Linux and macOS were not run.
  - Legacy (non-SDK) csproj files are not converted, so they were not built: 21 projects.
  - The theme counts come from a regex classification of compiler messages (`blockerThemes` in the JSON). They are a
    ranking aid, not an exact partition.

## History

| Date | Main SHA | Package | Portable build | Portable run | Note |
|---|---|---|---:|---:|---|
| 2026-11-19 | `3b73e4f5` | 1.0.0.26323 | 0 / 206 | 0 | +CM diagnostic: 16 build, 10 run |
| 2026-11-20 | `db9526db` | 1.0.0.26324 | 45 / 206 | 31 | 10 crash, 4 hang; no regressions |

### Round 44 Track C (event surface), spot check

Built the 22 portable projects whose recorded blockers mention `CS0123`, `Click`, `Checked` or `RadioButton`, with the
strict recipe against the package from the track branch.

- `RoutedEventHandler`-typed `Checked`/`Unchecked`/`Indeterminate`/`Click`, `ButtonBase`, `ToggleButton : ButtonBase`
  and `RadioButton : ToggleButton` are in. No `Checked`/`Click` `CS0123` or missing-member error is left.
- 1 more app builds: `Events/CustomRoutedEvents` (`Button.ClickEvent`). The other 21 still fail on other blockers.
- Remaining `CS0123` are `Slider.ValueChanged` and `ComboBox.SelectionChanged` delegate types, not this track.
- Open generator gap: attached routed-event syntax (`RadioButton.Checked="..."`, `Button.Click="..."` on a non-button
  element) is emitted as an attached property (`RadioButton.CheckedProperty`/`SetChecked`). Seen in
  `CaptureUnCaptureMouse` and `RoutedEventHandling`.
