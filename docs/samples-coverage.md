# WPF samples coverage

Measured 2026-11-20 on `main` at `1b50a52853e61ac5abaebc5f106a19c192ff85bd`, package `Gtk4.Forms.Wpf 1.0.0.26324`,
against the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4 native C++ projects). This is
the first run with the local feed **pinned** (see Method). Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row holds its result in report `a37d3b17` (`previous`).

## Headline

**89 of 204 portable official WPF sample apps build unmodified, and 83 run.** That is up from 83 / 74 in report
`a37d3b17` (main `ea4c17ae`, Round 45) and from 0 / 0 in report `fedd13c8` (2026-11-19).

- 6 apps moved fail→pass (4 run, 2 crash) and 5 built apps moved from crash to running. All of it is Round 46.
- **No regressions.** No app went pass→fail, and no app went runs→crash/hang.
- Of the 6 apps that build but do not run, 5 crash at startup and 1 hangs.
- The denominator is 204, not 206: DocumentStructure and SingleInstanceDetection are now Windows-only, as `a37d3b17`
  planned. Neither builds, so the build and run counts are unaffected.

### Were the earlier, unpinned runs trustworthy? Yes.

nuget.org publishes `Gtk4.Forms.*` 1.0.0.26321–26324, the same date-based versions that local packs produce, so an
unpinned restore could have used a published package. There is no evidence that any earlier run did:

- Every earlier cache records the local feed. In `samples2`/`3`/`4` (reports `3deb4dba`, `9ca5b30b`, `a37d3b17`),
  every `gtk4.forms.*` `.nupkg.metadata` source is `C:\Temp\Gtk.Forms\Packages`. The `fedd13c8` run used the user
  cache, and its 1.0.0.26323 entry also records the local feed.
- None of the cached packages is the published one. The SHA-512 content hashes of `Gtk4.Forms.Wpf` and
  `PresentationFramework` 1.0.0.26324 in each cache differ from nuget.org's (`LixHErk8…`, `5WUMP+NS…`). `fedd13c8`'s
  26323 (`1/2pORy…`) differs from nuget.org's 26323 (`XZq81GI3…`).
- Each run's package matches its SHA. The PresentationFramework DLL grows on each run: 612,864 → 621,568 → 649,216 →
  654,848 bytes. `IsSynchronizedWithCurrentItem` (Round 45) first appears in the `a37d3b17` package.
  `FrameworkElement.AbandonInitialization` (Round 46) appears only in this run's package.
- The pinned run reproduces every `a37d3b17` result except the 11 moves below, and each move has a Round 46 cause. No
  result needs another explanation.
- Caveat: `fedd13c8` restored into the shared user cache, so its package may predate `3b73e4f5`. Its 0 / 0 came from
  `CS1069` (ConfigurationManager) and the `{x:Type}` syntax bug, so that does not change the result.

### Corrections to the raw batch data

The top 15 missing symbols were checked against the `Gtk4.Forms.*` source. Every crash and hang was re-run alone.

- **Legacy (non-SDK) csproj files were built this time, but not against the layer.** The converter leaves them on
  .NET Framework 4.x. They therefore compile against the real WPF reference assemblies with our generated `*.g.cs`
  added. They are recorded as `not-built`, as before, with the attempt kept under `legacyBuild`.
  - Highlighter "builds and runs" as real .NET Framework WPF. No Gtk4.Forms assembly is in its output, so it is not
    counted.
  - This removes 2 of the batches' top 6 symbols. `FrameworkElement.AbandonInitialization` (7) and
    `System.Windows.Markup.XamlValueConverter` (6) both exist in the layer
    (`FrameworkElement.Initialized.cs:53`, `Markup/XamlValueConverter.cs:24`). Every hit is a legacy project.
  - The same goes for `MSB3644` (3) and the `NU1201` client→converted-sibling chains (4 Accessibility clients).
- **Windows-only symbols are dropped from the ranking:** `System.Windows.Forms` (7), `Form` (6) and the
  `System.Windows.Forms` namespace (3).
- **The other top-15 symbols are real gaps**, absent from the source:
  - imaging: `BitmapCacheOption`, `BitmapCreateOptions`, `BitmapFrame`, `BitmapPalettes`, `PixelFormats.Indexed1`;
  - 3D: all of `System.Windows.Media.Media3D` (`Viewport3D`, `Vector3D`, `Quaternion`);
  - input: `KeyboardFocusChangedEventArgs`;
  - geometry: `System.Windows.Vector`;
  - automation: `System.Windows.Automation.Peers` (the layer has only `System.Windows.Automation`).
- **Some labels and counts are wrong:**
  - `BitmapCacheOption`, `BitmapCreateOptions` and `BitmapPalettes` are types. They are tagged `M:` because they are
    `CS0103` names.
  - `BitmapFrame` is 8 apps (6 `M:` + 2 `T:`).
  - `KeyboardFocusChangedEventArgs` is 4 apps, not 5: EditBoxControlLibrary is a library that ExpenseItDemo compiles
    again.
  - `Vector` is 5 apps.
- **The run results needed correcting:**
  - SimpleBinding was recorded as `hang` but runs: a window titled `MainWindow` appeared within 1 s, twice, when run
    alone.
  - DragDropEvents was recorded as `hang` but crashes: it exits 1 after about 1 s, twice, when run alone. The cause is
    the same as in `a37d3b17`.
  - Both batch results were noise from parallel load.
- **The error codes were re-parsed uniformly** from the final `buildN.log` of all six batches. The parse agrees with
  every batch's pass/fail (91 pass, of which 1 is legacy Highlighter).

## Per category

These are portable apps on the strict recipe. Δ is the change since `a37d3b17`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 1 | 0 | 1 | 0 |
| Animation | 11 | 0 | 11 | 2 | 0 | 2 | +2 |
| Application Management | 10 | 1 | 8 | 4 | 0 | 4 | 0 |
| Clipboard | 2 | 0 | 2 | 0 | 0 | 0 | 0 |
| Compatibility | 1 | 0 | 1 | 1 | +1 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 26 | +1 | 26 | +2 |
| Documents | 17 | 2 | 15 | 4 | 0 | 4 | 0 |
| Drag and Drop | 6 | 0 | 6 | 5 | 0 | 3 | 0 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 7 | 0 | 7 | 0 |
| Events | 4 | 0 | 4 | 4 | +1 | 4 | +1 |
| Getting Started | 8 | 0 | 8 | 5 | 0 | 5 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 4 | 0 | 2 | 0 |
| Graphics | 26 | 0 | 25 | 3 | 0 | 3 | +1 |
| Input and Commands | 18 | 1 | 16 | 8 | +1 | 8 | +1 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 1 | 0 | 1 | 0 |
| Resources | 3 | 0 | 3 | 2 | 0 | 2 | 0 |
| Sample Applications | 23 | 1 | 21 | 3 | +1 | 3 | +1 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 4 | +1 | 3 | +1 |
| Threading | 3 | 0 | 3 | 0 | 0 | 0 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 4 | 0 | 4 | 0 |
| **Total** | **256** | **39** | **204** | **89** | **+6** | **83** | **+9** |

The library CustomApplicationClient also builds, so 90 projects build in all.

## What moved

**Fail→pass, 6 apps.** In each case, the change shown removed the only blocker recorded in `a37d3b17`.

| App | Round 46 change | Run result now |
|---|---|---|
| DataTemplatingIntro | generator-v3 (`ab58191b`): template name scopes | runs |
| RoutedEventHandling | generator-v3: attached-event syntax (`Button.Click` on a parent) | runs |
| CaptureUnCaptureMouse | generator-v3: attached-event syntax (`RadioButton.Checked`, `Button.Click`) | runs |
| VideoViewerDemo | generator-v3: owner-qualified property on its own type (`TextBlock.TextTrimming`) | runs |
| AlternatingAppearanceOfItems | generator-v3: nested `{x:Type TreeViewItem}` | crash (see below) |
| AppCompat-Quirks | generator-v3: CLR conversion of `Run.FontWeight` (`FontWeight?`) | crash (see below) |

**Crash→runs, 5 apps:**

- OpacityAnimation and TargetValues: generator-v3 plus `27faa495` build `DrawingBrush`/`DrawingGroup` resources as
  objects.
- IntroToStylingAndTemplating: generator-v3 resolves `{StaticResource {x:Type TextBlock}}` to `typeof(TextBlock)`.
- Linq: startup-runtime (`ce09926c`) holds `Selector.SelectionChanged` back during `BeginInit`/`EndInit`.
- OpacityMasking: startup-runtime converts a string to a `Uri` DP value (`Frame.Source`).

The last two confirm the 76-run gate measurement of the startup-runtime branch, now on a pinned feed.

**Partial progress** (still failing, one blocker removed):

- Generator name scopes removed `CS0102` from 7 more apps (AnimationExamples, Brushes, Callbacks, DataBindingDemo,
  PhotoStoreDemo, DocumentMerge, GlobalizationHomepage).
- `Validation.Error` typing removed `CS0117` from ValidateItemSample.
- The routed `SelectionChanged` delegate (startup-runtime) removed `CS0123` from Concepts and BitmapEffectsGallery.
  `Slider.ValueChanged` is still an `EventHandler` (3 apps).

**Error landscape**, counted as portable failing apps per code, `a37d3b17` → now:

- `CS0102` (name scopes): 8 → 0.
- `CS0123` (delegate types): 5 → 3.
- `CS0029` (CLR conversions): 4 → 2.
- `CS0117`: 33 → 30.
- Flat: `CS0246` 57 → 58, `CS1061` 45, `CS0103` 27, `CS0234` 20 → 21, `CS0534` 9 (`CreateWidget`).
- `CS2002` (a generated `.g.cs` is specified twice) is still a warning in 83 of the 89 portable builds.

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang.

- **SimpleBinding** and **DragDropEvents** looked like regressions (runs→hang, crash→hang) but were re-run alone, twice
  each. SimpleBinding runs. DragDropEvents crashes exactly as before.
- **New error codes in 3 still-failing apps are not regressions.** A fixed generator blocker uncovered layer gaps
  further on:
  - PropertyAnimation: `NameScope` is not public, `Storyboard.SetTargetName` is static-only, and `PropertyPath` is
    missing.
  - GlobalizationHomepage: `FlowDirection`, `Border.SnapsToDevicePixels`.
  - BamlReflector: `NU1201` gave way to `CS0234`/`CS0246` once its sibling converted.

## API demand

The counts are failing portable apps that report the symbol. Windows-only and legacy-csproj demand is excluded. "Apps"
is the apps whose errors touch the theme (from `blockerThemes`, a ranking aid).

| Theme | Symbols (apps) | Apps |
|---|---|---:|
| Graphics (2D) | `Page.Background`/`BackgroundProperty` (9, including Page roots), `Vector` (5), `TextBlock.SetForeground` attached accessors (3), `Point.Parse`/`Add`/`Offset` (2), `SystemColors.*Brush`/`*Color` (2), `UIElement.Clip`, `CompositionTarget`, `GeometryHitTestParameters`, `Shape.GetLocalValueEnumerator`, `Canvas.LayoutUpdated` | 35 |
| Input and focus | `KeyboardFocusChangedEventArgs` (4), `Slider.AutoToolTipPlacement`/`Precision` (3), `FocusVisualStyle` (3), `Slider.DecreaseSmall`/`IncreaseSmall` (2), `RoutedCommand()` (`CS1729`, 3), `Cursors.ScrollWE`, `Cursor.Dispose`, `EditingCommands`, `ContextMenuEventArgs`, `FocusNavigationDirection`, `RangeBase.ValueChanged` routed delegate (`CS0123`, 3) | 17 |
| Documents and text | `FlowDocument.ColumnWidth` (3), `ColumnGap`/`IsColumnWidthFlexible` (2), `Table` (2), `System.Windows.Annotations` (2), `PrintDialog` (2), `TextRange.Load`/`Save`, `FlowDocument.ContentStart`/`End`, `Paragraph.Style`, `FlowDocumentScrollViewer.VerticalScrollBarVisibility` | 15 |
| Flow direction | `FrameworkElement.FlowDirection` (`TextBlock` 4, `Window` 3, `StackPanel` 2), `Inline.FlowDirection` (`Span` 3, `Run` 2), `FlowDocument.FlowDirection` (2), the `FlowDirection` enum (2), `Span.Language`, `NumberSubstitution` | 12 |
| Extensibility | `UIElement.CreateWidget()` leaking as abstract (`CS0534`, 9), overridable `LogicalChildren`/`OnRender` (`CS0115`, 6) | 12 |
| Animation | `TranslateTransform.X/YProperty` + `BeginAnimation` (3), `DoubleAnimationBase` (2), `Clock` (2), `Storyboard.GetCurrentTime`/`Seek`/`TimeSeekOrigin` (2), `SolidColorBrush.ColorProperty` (2), `PropertyPath` (2), `HandoffBehavior` | 11 |
| Imaging | `BitmapFrame` (8), `BitmapCacheOption`/`BitmapCreateOptions` (7), `PixelFormats.Indexed*` (5), `BitmapPalettes` (5), `Png`/`Jpeg`/`Gif`/`Tiff`/`Bmp`/`Wmp` codecs, `BitmapImage.Format` | 10 |
| 3D | `System.Windows.Media.Media3D` (7): `Viewport3D` (5), `Vector3D` (5), `Quaternion` (4), `Point3D` (2), `GeometryModel3D` (2) | 8 |
| Items and validation | `IEditableCollectionView`, `BindingGroup.GetValue`/`TryGetValue`, `ItemsControl.ContainerFromElement`, `ListViewItem`, `Label.Target` | 8 |
| XAML at runtime | `XamlReader` (4), `XamlWriter` (3), `XamlParseException` (3) | 7 |
| Threading | `DispatcherOperationCallback` (2), static `Dispatcher.Run` (2), `DispatcherObject.Dispatcher` on elements | 5 |
| Dialogs and clipboard | `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog` (3), `CommonDialog`, `Clipboard.ContainsAudio`/`ContainsImage`/`ContainsFileDropList` | 5 |
| Navigation and pages | `NavigationEventArgs` (3), `PageFunction<T>`/`ReturnEventArgs<T>` | 5 |
| Automation peers | `System.Windows.Automation.Peers` (4) | 4 |
| Window | `Window.DragMove` (2), `AllowsTransparency` (2) | 2 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| DragDropEvents | crash | `ArgumentOutOfRangeException` at `mainGrid.RowDefinitions[1]` in the `SizeChanged` handler. **The generator drops `<Grid.RowDefinitions>`/`<Grid.ColumnDefinitions>` silently:** `EmitPropertyElement` (`XamlCodeGenerator.cs:786`) handles only Resources, VisualStateGroups, media objects, Triggers and Binding/template children. None of the 684 generated files of this run adds a row or column definition. This also mis-lays out 23 of the 83 running apps, whose XAML declares them |
| AlternatingAppearanceOfItems | crash | `'' is not a valid value for property 'ItemTemplate'`. The `HierarchicalDataTemplate` resources `Level1Data`…`Level3Data` are still emitted as `Resources["…"] = ""` |
| AppCompat-Quirks | crash | `NullReferenceException` in the sample's `PopulateAppContextInformation` (from `Window_Loaded`). It reflects over WPF's internal AppContext switch classes, which the layer does not have, so `GetAppContextSwitchType` returns null. A compatibility gap, not worth chasing |
| LocalizationResources | crash | Unchanged: there is only an en-US satellite and no neutral resource, so `MissingManifestResourceException` under a non-en-US UI culture, as in real WPF |
| StringLocalizationSample | crash | Unchanged: `Resource with key 'LocalizedMessage' not found`. The merged `StringResources.xaml` is not reachable from `InitializeComponent` |
| DragDropThumbOps | hang | Unchanged. Alive with no top-level window after 25 s alone, and the output is empty. Not diagnosed |

## XAML generator errors

These are the remaining generator-side causes in portable apps. Per-project messages are in the JSON (`xamlErrors`).
Generator v3 cleared name scopes, attached events, nested `{x:Type}`, owner-qualified properties and the `Run.FontWeight`
conversion. What is left:

1. **Property elements dropped silently.** `Grid.RowDefinitions`/`ColumnDefinitions` cause one crash (DragDropEvents)
   and wrong layout in 23 running apps; the GTK "Trying to measure … for height" warnings are the visible symptom.
   Any other property element outside the handled set is lost the same way, with no build message.
2. **Non-media resources still emitted as `""`:** `HierarchicalDataTemplate` (AlternatingAppearanceOfItems) and
   `CollectionViewSource`.
3. **Missing `x:Name` fields:** `mediaElement` (WindowActivationAndDeactivation, which fails on nothing else),
   `myMediaElement`/`MediaElement1` (MediaGallery), keyframe names (KeySplineAnimations), and `flowReader`
   (ParagraphAndHyphenation). All are elements of types the layer lacks (`MediaElement`, 3D keyframes).
4. **Duplicate `Main`:** WPFGallery (`CS0111`), and no `Main` for a custom `Application` subclass
   (CustomApplicationClass, Concepts: `CS5001`).
5. **Value conversion (`CS0029`):** `int` → `string` (2DTransforms). FontProperties' `FontFamily` → `string` is in
   the code-behind (`TextElement.FontFamily` typing), so it is a layer gap.
6. **Not generator bugs, though surfaced in `.g.cs`:** `Page.Background` (9 apps), `TextBlock.SetForeground` (3),
   `FlowDirection`, `Slider.AutoToolTip*`, and `FocusVisualStyle`. All are missing layer members.
   - `Slider.ValueChanged` wired as `EventHandler` (`CS0123`, 3 apps) is the layer's `RangeBase` delegate type,
     fixed on the held `gf46/input-focus` branch.
   - DocumentStructure's `fixedpage1_structure.xaml` is malformed XML, and that app is now Windows-only.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole remaining blocker is the track.

1. **Rework `gf46/input-focus` (held): 10 only, plus 3 `CS0123` apps.**
   - Only: ContextMenuOpening, FocusVisualStyle, CursorType, CustomRoutedCommand, EditingCommands, KeyStrokeCounter,
     ProgrammaticFocusControl, CalculatorDemo, and both CommandSource samples. It also removes `CS0123` from
     KeySplineAnimations, ParagraphAndHyphenation and MediaGallery.
   - The branch already has the code: keyboard-focus events and `KeyboardFocusChangedEventArgs`, `FocusVisualStyle`,
     `MoveFocus`/`PredictFocus`/`FocusNavigationDirection`, `RoutedCommand()`, `EditingCommands`, the full `Cursors`
     set and `Cursor.Dispose`, `ContextMenuOpening`/`ContextMenuEventArgs`, routed `RangeBase.ValueChanged`, the
     `Slider` commands and `AutoToolTip*`, `KeyConverter`, and the `Preview*` events.
   - On its own (pre-v3) base, it got 4 of these to build and run and FocusVisualStyle to build (crash).
     ContextMenuOpening and ProgrammaticFocusControl failed only on generator gaps that v3 has since fixed.
     CalculatorDemo needs `x:Name` fields for `local:` types and `FrameworkElement.Parent`.
   - It was held on 3 important findings, which the rework must fix:
     - Enter/Esc is taken from an open ComboBox drop-down: GTK never sets `IsDropDownOpen`.
     - A focused element whose route does not reach the Window loses the key.
     - GTK focus moves are reported via null, so `LostFocus`/`GotFocus` arrive in the wrong order.
   - Rebase onto `main`, where generator-v3 overlaps.
2. **Generator v4, property elements and remaining startup crashes: up to 3 runs, and layout in 23 running apps.**
   - Emit `Grid.RowDefinitions`/`ColumnDefinitions`, then other collection property elements. Report any dropped
     property element as a build message, as media properties already are.
   - Emit `HierarchicalDataTemplate`/`CollectionViewSource` resources as objects (AlternatingAppearanceOfItems).
   - Make merged `ResourceDictionary` XAML reachable at parse time (StringLocalizationSample).
   - Fix the `Main` handling for custom `Application` subclasses and `x:Class` duplicates (CustomApplicationClass,
     WPFGallery).
   - Gate: all 90 building projects, plus a visual check that the 23 grid apps lay out.
3. **Flow direction: 12 involved, 7 only.**
   - Only: FlowDirection, Gradient, Image, LTRRTL, Paths, RunSpan, Span. LangAttribute (`Span.Language`),
     FlowContentProperty and NumbersMultipleLangauges (`NumberSubstitution`) are close.
   - Covers: the `FlowDirection` enum, inherited `FrameworkElement.FlowDirection`, `Inline.FlowDirection`, and
     `FlowDocument.FlowDirection`.
   - **Done in Round 47 (pinned feed, package 1.0.0.26325).** All 90 building projects still build and all 83
     running apps still run. The 7 only-flow-direction apps now build and run, as do FlowContentProperty and
     NumbersMultipleLangauges (9 new). GlobalizationHomepage (`Frame.Source` on a Window class) still fails.
     LangAttribute failed because the generator emitted `Language="ar-SA"` as a string; after the review fix
     (`XmlLanguage` is converter-backed) it builds and runs.
   - GTK gaps: `FrameworkElement.FlowDirection` sets `Widget.Direction` per element (GTK does not inherit it), and
     so does every widget the control owns (an Entry's text child, a TextView, a DropDown's parts). `Gtk.Box`-based
     panels (StackPanel, DockPanel) and `Gtk.Grid` mirror; panels placed by this layer (Canvas) do not. `Inline`/`Block`/`FlowDocument` are not
     `DependencyObject`s, so their `FlowDirection`/`Language` are plain properties rendered only when set (bidi
     isolate, Pango `lang`) and do not inherit. `FrameworkElement.Language` and `NumberSubstitution` are stored
     only.
4. **2D graphics, Page and Window long tail: about 11 only.**
   - `Page.Background`: PathAnimations, ShapeElements.
   - `UIElement.Clip`: ClipRegion.
   - `Vector`/`Point` operators: Point, Vector.
   - `SystemColors` brushes: SystemBrushesAndColors.
   - `Shape.GetLocalValueEnumerator`: RestoringDefaultValues.
   - `CompositionTarget`.
   - `TextBlock` attached accessors: DrawingBrush.
   - `DragMove`/`AllowsTransparency`: CustomWindowUI, NonRectangularWindow.
   - `Page.Background` also unblocks part of 9 apps.
5. **Threading, dialogs, clipboard and items: 9 only.**
   - Threading only: ExceptionHandlingSecondaryUIThread, ExceptionHandlingSecondaryWorkerThread,
     MultiThreadingWebBrowser, SingleThreadedApplication. Covers `DispatcherOperationCallback`, static
     `Dispatcher.Run` and `DispatcherObject.Dispatcher` on elements.
   - ClipboardSpy (`Clipboard.Contains*`) and CommonDialogs (`CommonDialog`).
   - Items: EditingCollections (`IEditableCollectionView`), ValidateItemSample and ValidateItemsInItemsControl
     (`BindingGroup.GetValue`/`TryGetValue`, `ItemsControl.ContainerFromElement`).

Deliberately not next:

- Imaging codecs and Media3D: 18 apps, 7 only, but large.
- Animation clocks (`DoubleAnimationBase`, `Clock`, an animatable `TranslateTransform`): 11 involved, 2 only.
- Extensibility (`CreateWidget` leaking as abstract): 9 apps, nearly always alongside other gaps. Fold it into
  whichever round touches `UIElement`.
- System.Xaml/BAML tooling (LocBaml, BamlReflector).
- Legacy (non-SDK) csproj conversion: 21 projects. They compile as .NET Framework against real WPF, so converting
  them is a converter feature, not a layer gap.
- Packaging hygiene, still open: `CS2002` in 83 of 89 builds.

## Generator v4 (gate run)

Gate on the pinned feed with `Gtk4.Forms.Wpf 1.0.0.26325` packed from the generator-v4 branch: all 90 build, and 86 of the 89 apps
run (was 83). No regressions after one fix: `CollectionViewSource.Source` is now `object` (as in WPF), so a data
provider such as `XmlDataProvider` is accepted. Without it the `CollectionViewSource` sample crashed.

| App | Change | Result |
|---|---|---|
| DragDropEvents | `Grid.RowDefinitions`/`ColumnDefinitions` emitted | crash → runs |
| StringLocalizationSample | merged `ResourceDictionary` files compiled and loaded at parse time | crash → runs |
| AlternatingAppearanceOfItems | `HierarchicalDataTemplate`, `CollectionViewSource`, `AlternationConverter` as objects | crash → runs |
| CustomApplicationClass | `Main` for a custom `Application` subclass root | fail → builds, runs (confirmed again in the Round 47 gate) |
| WPFGallery | no duplicate `Main` (`CS0111` gone); still fails on layer gaps (`UserControl`, `Automation.Peers`, ...) | fail |

A property element the generator cannot emit is now reported as a build message with file, line and name.

## Round 47 review gate

Pinned feed, `Gtk4.Forms.Wpf 1.0.0.26325` packed from `main` (`5f92acd0`) plus the Round 47 review fixes. All 90
projects recorded as `build=pass` in `samples-coverage.json` (89 apps and the CustomApplicationClient library) build
through the two-build recipe. Of the 89 apps, 86 run (alive with a window after about 10 s): all 83 recorded as
`runs`, plus DragDropEvents, StringLocalizationSample and AlternatingAppearanceOfItems. ProcessingCommandLineArguments
and RoutedEventHandling showed as hangs in the serial pass and run when re-run alone (twice each). The same recipe also
builds and runs CustomApplicationClass and LangAttribute, which were not in the recorded set. That is 91 apps built
and 88 running, so **91 / 204 build and 88 run** by what was re-measured. The rest of the 204 (including the 9 apps
that the flow-direction round moved) was not re-run, so these are lower bounds. `samples-coverage.json` is stale as of
`1b50a528` and was not regenerated.

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches. Each batch was copied under
  `.claude/samples5/batchN`, with empty `Directory.Build.props`/`.targets` (so `TreatWarningsAsErrors` was off). No
  original or tracked file was modified.
- **Feed, pinned.** `samples5/nuget.config` sets the following, and why:
  - An isolated `globalPackagesFolder` (`samples5/.nuget-packages`) and cleared `fallbackPackageFolders`, so no
    pre-extracted copy from the user or VS cache can be used.
  - Cleared `packageSources`, then the repo `Packages/` feed and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only (everything else, including `GtkSharp4`, to
    nuget.org).
  - Why: nuget.org publishes `Gtk4.Forms.*` 1.0.0.26321–26324, the same date-based versions as local packs. Without
    the mapping, a restore may resolve the published package for an identical version.
- **Pinning proven.** A probe (Getting Started/HelloWorld, with the full recipe) and every batch checked the restored
  packages.
  - The cached `.nupkg` SHA-256 values equal the local feed's: `Gtk4.Forms.Wpf` `0cbf37aa…6830`,
    `PresentationFramework` `cbe1ee0c…f0bc`, and `WindowsBase` and `MarkupCompiler` match too.
  - Every `.nupkg.metadata` source is `C:\Temp\Gtk.Forms\Packages`.
  - The probe was re-run from a wiped cache with the fallback-folder clear: same hashes, and `project.assets.json` no
    longer lists the VS fallback folder.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26324`, preserving encoding and line endings.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again; this result is recorded.
  - Nothing else is changed. The diff is saved as `csproj.diff` beside each copy.
  - The caches were shared by batches that ran in parallel. Where a build hit a restore race (`NETSDK1064`, a missing
    file in the cache), it was rebuilt, and the last log (`build3.log`) is the one recorded: AnnotatedDocumentViewer
    and several Input and Commands projects.
  - WPFGallery was built by its csproj path, because its folder holds two `.sln` files.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes and messages were
  re-parsed uniformly from the final logs (UTF-8 and UTF-16).
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime on `PATH`.
  - "Runs" means alive with a visible top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception or an exit with no window.
  - "Hang" means alive with no window.
  - Every crash and hang, and every apparent regression, was re-run alone.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification from the earlier runs, plus the native C++ projects. DocumentStructure
  and SingleInstanceDetection are now Windows-only.
- **Limits.**
  - Only Windows was measured.
  - Legacy csproj files are not converted: 21 projects.
  - The theme counts are a regex ranking aid (`blockerThemes`), not an exact partition.
  - Moves are attributed by elimination, not bisected.

## History

| Date | Main SHA | Package | Pinned | Portable build | Portable run | Note |
|---|---|---|---|---:|---:|---|
| 2026-11-19 | `3b73e4f5` | 1.0.0.26323 | no | 0 / 206 | 0 | Shared user cache; local source, not the published package |
| 2026-11-20 | `db9526db` | 1.0.0.26324 | no | 45 / 206 | 31 | 10 crash, 4 hang; no regressions |
| 2026-11-20 | `64cddd04` | 1.0.0.26324 | no | 59 / 206 | 47 | Round 44; 10 crash, 2 hang; 2 regressions (TargetValues, MultiPage) |
| 2026-11-20 | `ea4c17ae` | 1.0.0.26324 | no | 83 / 206 | 74 | Round 45; 8 crash, 1 hang; no regressions |
| 2026-11-20 | `a37d3b17` + `gf46/startup-runtime` | 1.0.0.26324 | no | 83 / 206 | 76 | Gate run: Linq and OpacityMasking run |
| 2026-11-20 | `1b50a528` | 1.0.0.26324 | **yes** | 89 / 204 | 83 | Round 46; 5 crash, 1 hang; no regressions; the earlier runs are confirmed local-feed |
| 2026-11-21 | `2c67b72c` + generator-v4 | 1.0.0.26325 | **yes** | 89 / 204 | 86 | Generator v4; 90 projects incl. 1 library; no regressions |
| 2026-11-21 | `5f92acd0` + Round 47 review fixes | 1.0.0.26325 | **yes** | 91 / 204 | 88 | Lower bounds: the recorded 90 projects, CustomApplicationClass and LangAttribute re-run; no regressions |
