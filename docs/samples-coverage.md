# WPF samples coverage

Measured 2026-11-20 on `main` at `64cddd04c2a2849ec572495acdf96b6700bc1bca`, package `Gtk4.Forms.Wpf 1.0.0.26324`,
against the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4 native C++ projects).
Per-project results are in [`samples-coverage.json`](samples-coverage.json). Each row holds its result in the previous
report (`previous`, report `3deb4dba`), so the next run can diff against it.

## Headline

**59 of 206 portable official WPF sample apps build unmodified, and 47 run.** That is up from 45 / 31 in report
`3deb4dba` (main `db9526db`, earlier the same day) and from 0 / 0 in report `fedd13c8` (2026-11-19).

- 16 apps moved fail→pass and 4 built apps moved from crash/hang to running. All of it is Round 44.
- **2 regressions (pass→fail): TargetValues and MultiPage.** Neither ran before (both hung). Round 44 now compiles
  their x:Class-less `Page`, which hits two old generator gaps. See [Regressions](#regressions).
- Of the other 12 apps that build, 10 crash at startup and 2 hang without a window. 9 of the 12 fail on one
  generator bug class: `{StaticResource}` lookups and resource objects.
- 37 apps are Windows-only by design and are excluded from the denominator, as before.
- The content-model wall is gone: no `.Content`/`xml:space` errors on layer types are left (63 apps before). The
  generator is still involved in 42 failing apps, and 14 fail on nothing else.

### Corrections to the raw batch data

The top 15 missing symbols were checked against the `Gtk4.Forms.*` source, and every crash, hang and regression was
checked against its logs and generated code. The following are corrected here and in the JSON.

- **259 projects is 256.** Batch 4 counted the three native Migration projects twice: once for the VS2008 `.vcproj`
  and once for the `.vcxproj` of the same project. One row per project is kept. The 3 Migration native projects are
  new in the inventory (`change: new-in-inventory`). They are Windows-only, so the denominator does not change.
- **Windows-only is 44 projects (37 apps), and portable apps are 206, not 209.** The verified classification from
  the earlier runs is kept, plus the 3 natives. The batch heuristics disagreed on a few legacy projects. Either way,
  the portable build and run counts (59 and 47) are the same.
- **`Page.Content`, `Page.WindowTitle`, `Page.Triggers`, `Page.FindResource` and most of `Page.Background` are
  generator artefacts.** These are 6, 4, 2, 2 and 5 samples.
  - The errors are on the samples' own root classes (`TransformExample`, `OpacityAnimationExample`, …). For an x:Class
    with no code-behind, the generated partial declares no base type.
  - The layer's `Page` already has `Content`, `Title` and `WindowTitle`.
  - Only `Page.Background`/`BackgroundProperty` is a real gap (2 samples): `Page : FrameworkElement` has no
    `Background`.
- **`AttachElementTriggers` is a generator/layer mismatch, not a missing API.** The generator emits
  `x.AttachElementTriggers()`, but `FrameworkElement.AttachElementTriggers` is `internal`. It affects 13 portable
  apps, not 8.
- **`ValueConversion` and `ValueConversionAttribute` are one symbol** (6 samples), and it is missing.
- **`System.Windows.Forms` (7) and `Form` (5) are Windows-only demand.** Every sample that names them is a
  Windows-only project, so they are dropped from the ranking.
- **`TreeViewItem` is not missing.** AlternatingAppearanceOfItems fails because the generator emits
  `{x:Type TreeViewItem}` inside a `RelativeSource` as an unqualified `typeof(TreeViewItem)`. This is a generator bug.
- **WPFWithWFAndDatabinding is not a casing bug.** The converter's `UseWPF` regex is `IgnoreCase`. The
  `NETSDK1005` comes from the sample declaring both `TargetFrameworks=net472` and `TargetFramework=net10.0-windows`.
  The project is Windows-only, so no count changes.
- **XmlDataSource moved crash→hang, but its cause did not change.** It is the same unhandled `FindResource` failure.
  The process stayed alive after the exception, so the recipe classifies it as a hang. This is not a regression.
- **Real gaps, confirmed absent from the layer:** `Selector.IsSynchronizedWithCurrentItem`, `ValueConversionAttribute`,
  `BitmapCacheOption`/`BitmapCreateOptions`/`BitmapFrame`/`BitmapPalettes`, `PixelFormats.Indexed*`, all of
  `System.Windows.Media.Media3D` (`Viewport3D`, `Vector3D`, `Quaternion`), `KeyboardFocusChangedEventArgs`, `Vector`,
  `UserControl`, and the `FlowDirection` enum itself.

## Per category

These are portable apps on the strict recipe. Δ is the change since `3deb4dba`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 1 | +1 | 1 | +1 |
| Animation | 11 | 0 | 11 | 0 | −1 | 0 | 0 |
| Application Management | 10 | 0 | 9 | 4 | 0 | 4 | 0 |
| Clipboard | 2 | 0 | 2 | 0 | 0 | 0 | 0 |
| Compatibility | 1 | 0 | 1 | 0 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 15 | +5 | 7 | +2 |
| Documents | 17 | 1 | 16 | 3 | 0 | 3 | 0 |
| Drag and Drop | 6 | 0 | 6 | 4 | +2 | 3 | +2 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 7 | +4 | 7 | +4 |
| Events | 4 | 0 | 4 | 3 | +1 | 3 | +1 |
| Getting Started | 8 | 0 | 8 | 3 | −1 | 3 | +1 |
| Globalization and Localization | 14 | 0 | 14 | 3 | 0 | 1 | 0 |
| Graphics | 26 | 0 | 25 | 2 | 0 | 2 | 0 |
| Input and Commands | 18 | 1 | 16 | 6 | +1 | 6 | +3 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 1 | 0 | 1 | 0 |
| Resources | 3 | 0 | 3 | 2 | +1 | 1 | 0 |
| Sample Applications | 23 | 1 | 21 | 1 | 0 | 1 | +1 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 1 | 0 | 1 | 0 |
| Threading | 3 | 0 | 3 | 0 | 0 | 0 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 2 | +1 | 2 | +1 |
| **Total** | **256** | **37** | **206** | **59** | **+14** | **47** | **+16** |

The library CustomApplicationClient also builds, so 60 projects build in all. Migration gained the 3 native projects.

## What moved

**Fail→pass, 16 apps.** In each case, the change shown removed the only blocker recorded in `3deb4dba`.

| Cause | Apps | Run result now |
|---|---:|---|
| XAML content model, `[ContentProperty]` (`2e11abf4`) | 14 | 10 run: Target, BindingDPToDP, SimpleBinding, DragDropTextOps (also `xml:space`), HeightProperties, UsingElements, VisibiltyChanges, WidthProperties, MousePointer, WindowSizingOrder. 4 crash: BindingToMethod, Linq, XmlnsBind, DefiningResources |
| Content model + event surface (`2e11abf4`, `a812d07b`: `CheckBox.Click`) | 1 | DragDropOpenTextFile runs |
| Event surface (`a812d07b`: `ButtonBase.OnClick` overridable) | 1 | CustomRoutedEvents runs |

**Crash/hang→runs, 4 apps** (they built before too):

- ComplexLayout: length units such as `30px` (`19a2a945`).
- DropShadow: string→`Transform` (`19a2a945`).
- HandlingEventOnCommand: command name→`ICommand` (`19a2a945`).
- EventOnGainAndLooseFocus: hang→runs. This was never diagnosed and was not isolated now. The window is a plain
  `StackPanel` of `Button`s with text content, so the content-model text handling is the likely cause.

LocalizationResources still crashes, but further in: the `ImageSource` crash is fixed (`19a2a945`), and it now
fails on a manifest resource name.

The Page-`StartupUri` hosting (`19a2a945`/`64d3f2fe`) does not yet turn either Page-startup hang into a run. Both
now fail to build (below).

**Error landscape**, counted as portable apps per code, `3deb4dba` → now:

- `CS1061`: 90 → 64. This is the content model.
- `CS0123`: 11 → 4. This is the event surface. The 4 left are `Slider.ValueChanged`/`SelectionChanged` delegates.
- `CS0246`: 66 → 65.
- Flat: `CS0117` 34, `CS0103` 27, `CS0234` 20, `CS0534` 9 (`CreateWidget`), `CS0102` 8.
- `CS0029`: 8 → 9.
- `CS2002` (a generated `.g.cs` is specified twice) is now a warning in 55 of the 59 portable builds. The 4 without it
  are the three code-only apps and CustomDictionaries.

## Regressions

There are 2 pass→fail regressions. Both apps hung before, so the run count does not drop, but the build count does.
Both have the same trigger: `64d3f2fe` ("StartupUri registry for x:Class-less pages",
`XamlCodeGenerator.ImplicitClass.cs`) now generates a class for a `Page`/`Window` XAML that has no `x:Class`.

- Before, these pages produced no code at all. The app built, and then hung because the page never loaded.
- Now the page is compiled, and its generated code hits generator gaps that were already there:

| App | New error | Root cause |
|---|---|---|
| Animation/TargetValues | `CS1061 'Button' does not contain a definition for 'AttachElementTriggers'` in `AnimationTargetValuesExample.g.cs` | The generator emits a call to `FrameworkElement.AttachElementTriggers()`, which is `internal` (13 portable apps hit it) |
| Getting Started/MultiPage | `CS0029 Cannot implicitly convert type 'string' to 'System.Uri'` in `Page1.g.cs` (`__e11.NavigateUri = "Page2.xaml";`) | `Hyperlink.NavigateUri` is a CLR property of type `Uri?`. The CLR-property path emits the raw string with no conversion |

Each fix is small. Make element triggers reachable from generated code (public/`EditorBrowsable(Never)`, or emit
`Triggers` wiring the layer applies itself), and convert strings for `Uri`-typed CLR properties. With both fixed,
the two apps exercise the new `NavigationWindow` Page host end to end.

## API demand

The counts are failing portable apps that report the symbol. Generator artefacts and Windows-only demand are
excluded. They are floors: a missing base type on generated roots still hides errors in 16 apps.

| Theme | Symbols (samples) | Apps |
|---|---|---:|
| Items, data and dialog semantics | `Selector.IsSynchronizedWithCurrentItem` (10), `ValueConversionAttribute` (6), `Button.IsDefault`/`IsCancel` (3), `CollectionView` (2), `BindingExpressionBase.UpdateSource` (2), `DataTransferEventArgs` (2), `ContentPresenter.ContentSource` (2), settable `Window.DialogResult` (2), concrete `HeaderedContentControl` (2), `Grid.ShowGridLines` (2), `Panel.IsItemsHost` (2), `MenuItem.ItemsSourceProperty`, `Grid.IsSharedSizeScope`, `ContentControl.HasContent` | 31 |
| Documents and text | `FlowDocument.ColumnWidth`/`ColumnGap`/`IsColumnWidthFlexible` (2–3), `Run`/`Span.SetValue` (`Inline` as a `DependencyObject`, 2), `Table` (2), `PrintDialog` (2), `System.Windows.Annotations` (2), `TextRange` `Load`/`Save`, `FlowDocument.ContentStart`/`End`, `TextElement.BackgroundProperty` | 21 |
| Graphics (2D) | `Vector` (4), `Point.Parse`/`Add`/`Multiply` (2), `Matrix` operations, `SystemColors.*Brush`/`*Color` (2), `VisualBrush.Viewbox` (2), `TextBlock.SetForeground` (2), `CompositionTarget`, `GeometryHitTestParameters` | 15 |
| Input | `KeyboardFocusChangedEventArgs` (4), `Expander.FocusVisualStyle` (2), `Cursors.ScrollWE`, `Cursor.Dispose`, `Mouse.SetCursor`, `EditingCommands`, `RoutedCommand()`, `ContextMenuEventArgs` | 13 |
| Extensibility | `UIElement.CreateWidget()` leaking as abstract (`CS0534`, 9), `RoutedPropertyChangedEventArgs<T>` (3), overridable `OnApplyTemplate`/`OnRender`/`LogicalChildren` | 12 |
| Flow direction | `FlowDirection` enum and `FrameworkElement.FlowDirection` (`TextBlock` 4, `StackPanel` 2), `Inline.FlowDirection` (`Span` 3, `Run` 2), `FlowDocument.FlowDirection` (2) | 11 |
| Imaging | `BitmapCreateOptions`/`BitmapCacheOption` (7), `BitmapFrame` (6), `PixelFormats.Indexed*` (5), `BitmapPalettes` (5), `Png`/`Jpeg`/`Gif`/`Tiff`/`Bmp`/`Wmp` decoders and encoders | 10 |
| Animation | `TranslateTransform.X/YProperty` + `BeginAnimation` (3), `DoubleAnimationBase` (2), `Clock` (2), `Storyboard.Seek`/`TimeSeekOrigin`/`HandoffBehavior`, `SolidColorBrush.ColorProperty` | 9 |
| 3D | `System.Windows.Media.Media3D`: `Viewport3D` (5), `Vector3D` (5), `Quaternion` (4), `Point3D` (2), `GeometryModel3D` (2) | 8 |
| Navigation and pages | `NavigationEventArgs` (3), `Page.Background(Property)` (2), `UserControl`, `PageFunction<T>` | 8 |
| XAML at runtime | `XamlReader` (4), `XamlWriter` (3), `XamlParseException` (3), `Application.LoadComponent` | 6 |
| Dialogs, clipboard, threading, automation | `Microsoft.Win32.OpenFileDialog`/`SaveFileDialog` (3), `MessageBoxOptions`, `Clipboard.Contains*`; static `Dispatcher.Run` (2), `DispatcherOperationCallback` (2), `DispatcherObject.Dispatcher` on elements; `System.Windows.Automation.Peers` (4) | 5 / 5 / 4 |
| Window | `Window.DragMove` (2), `Window.ShowActivated`, `AllowsTransparency` | 3 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| BindConversion, BusinessLayerValidation, BindingToStringFomat, PropertyChangeNotification, XmlnsBind, DefiningResources | crash | `Resource with key '…' not found` from the generated `InitializeComponent` |
| XmlDataSource | hang | Same `FindResource` failure (`InventoryData`). The process stays alive after the unhandled exception |
| BindingToMethod | crash | `InvalidCastException`: `string` → `IValueConverter` at `MainWindow.g.cs:38` |
| Linq | crash | `NullReferenceException` in the sample's `ListBox_SelectionChanged`. It is raised during `InitializeComponent`, before the fields it uses are assigned; WPF does not raise it there |
| LocalizationResources | crash | `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources` (resource naming of a localised `.resx`) |
| StringLocalizationSample | crash | `Resource with key 'LocalizedMessage' not found` (a merged/localised dictionary) |
| DragDropThumbOps | hang | Alive with no top-level window, empty logs. Not diagnosed |

The 8 resource failures have two generator root causes, verified in the generated code of BindConversion,
BindingToMethod, XmlnsBind and DefiningResources:

1. **Resource objects of user or unsupported types are emitted as an empty-string placeholder.** For example,
   `<local:MyData x:Key="MyDataSource"/>`, `<local:DoubleToString/>` and `XmlDataProvider` all become
   `Resources["key"] = "";`. That is the `InvalidCastException` in BindingToMethod.
2. **`{StaticResource}` is resolved with `FindResource` on the wrong scope.**
   - It is called on `this` when the dictionary is on an inner element. In BindConversion, the dictionary is on a
     `StackPanel`.
   - Or it is called on an element before it is parented. In DefiningResources, `__e15.FindResource` runs before
     `__e15` is in the tree.
   - WPF resolves it at parse time against the enclosing dictionaries.

## XAML generator errors

The counts are failing portable apps; per-project messages are in the JSON (`xamlErrors`). The generator is involved
in 42 apps, and 14 fail on nothing else:

- OpacityAnimation, PropertyAnimation, TargetValues, DataTemplatingIntro, RoutedEventHandling, HelloWorld, MultiPage;
- GlobalizationHomepage, 2DTransforms, OpacityMasking, ShapeElements, CaptureUnCaptureMouse;
- AlternatingAppearanceOfItems, WindowActivationAndDeactivation.

The error classes:

1. **No base type for an x:Class with no code-behind: 16 apps.** For example: 2DTransforms, OpacityMasking,
   ClipRegion, KeyFrameAnimation, OpacityAnimation, VisualBrush, ShapeElements, Concepts, ControlsAndLayout.
2. **`AttachElementTriggers` is `internal`: 13 apps.** This includes the TargetValues regression.
3. **Name scopes: 10 apps.** An `x:Name` inside a template or resource becomes a root field that collides
   (`CS0102`):
   - AnimationExamples, DataTemplatingIntro, Brushes, Callbacks, DataBindingDemo, PhotoStoreDemo, DocumentMerge,
     GlobalizationHomepage;
   - WPFGallery has a duplicate `Main` (`CS0111`);
   - PropertyAnimation's Page partial conflicts with the user partial (`CS0111`/`CS0260`/`CS0262`).
4. **Value conversion (`CS0029`): 8 apps.**
   - `Polygon.Points` → `PointCollection`.
   - `Window.Icon` and `ImageBrush.ImageSource` → `ImageSource`.
   - `Hyperlink.NavigateUri` → `Uri`: the MultiPage regression.
   - `int` → `string`.
5. **Delegate types (`CS0123`): 4 apps.** `Slider.ValueChanged` is wired as `EventHandler`, and
   `ComboBox`/`ListBox.SelectionChanged` as `EventHandler<SelectionChangedEventArgs>`.
6. **Attached routed-event syntax: 2 apps.** `Button.Click="…"` or `RadioButton.Checked="…"` on a non-button element is
   emitted as an attached property (`Button.SetClick`, `RadioButton.CheckedProperty`), and `PreviewMouseDown`/`Up` are
   missing on `StackPanel` (RoutedEventHandling, CaptureUnCaptureMouse).
7. **`{x:Type}` inside a nested markup extension** is emitted unqualified, e.g. `typeof(TreeViewItem)`
   (AlternatingAppearanceOfItems).
8. **Missing `x:Name` fields** for some elements: `mediaElement`, `FlowDocRdr` and keyframe names
   (WindowActivationAndDeactivation, FindText, KeySplineAnimations).
9. **Not a generator bug:** DocumentStructure's `fixedpage1_structure.xaml` is malformed XML.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole remaining blocker is the track.

1. **Startup resources.** Up to 11 more runs; every one of these apps already builds.
   - Instantiate resource elements of `clr-namespace` and unsupported types (`XmlDataProvider`) instead of emitting
     `""`.
   - Resolve `{StaticResource}` at parse time against the enclosing dictionaries, not with `FindResource` on `this`
     or on an unparented element. This covers 9 apps: BindConversion, BindingToMethod, BusinessLayerValidation,
     BindingToStringFomat, PropertyChangeNotification, XmlnsBind, XmlDataSource, DefiningResources, and
     StringLocalizationSample if its dictionary path is the same.
   - Don't raise `SelectionChanged` during `InitializeComponent` (Linq).
   - Use the localised `.resx` manifest name (LocalizationResources).
2. **XAML generator v2.** This fixes both regressions. It is involved in 42 apps, and 14 fail only on it.
   - Emit the root base type for an x:Class with no code-behind.
   - Make element triggers reachable from generated code.
   - Template and resource name scopes.
   - CLR-property string conversion (`Uri`, `ImageSource`, `PointCollection`).
   - Routed delegate types for `Slider.ValueChanged` and `SelectionChanged`.
   - Attached routed-event syntax and `PreviewMouseDown`/`Up`.
   - `{x:Type}` in nested markup extensions.
   - Missing `x:Name` fields.
3. **Items, data and dialog semantics.** Involved in 31 apps, 12 only.
   - Only: CollectionBinding, Colors, DirectionalBinding, EditingCollections, Grouping, MasterDetail,
     MasterDetailXml, MultiBinding, UpdateSource, ValidateItemSample and two CommandSource samples.
   - Covers: `IsSynchronizedWithCurrentItem`, `ValueConversionAttribute`, `IsDefault`/`IsCancel`, `CollectionView`,
     `UpdateSource`, `DataTransferEventArgs`, `ContentSource`, settable `DialogResult`, concrete
     `HeaderedContentControl`.
4. **Long tail: one or two members each.** About 12 apps are blocked by nothing else.
   - `MenuItem.ItemsSourceProperty` (HierarchicalDataTemplate)
   - `TextBox.SpellCheck`/`GetSpellingError` (SpellChecking)
   - `RowDefinition.ActualHeight` (DragDropEvents)
   - `Grid.IsSharedSizeScope` (GlobalizationRunDialog)
   - `AddChild` (HandlingEventOnCommandUsingCode)
   - `TextBox.Copy` and `TextElement.BackgroundProperty` (HtmlToXamlDemo)
   - `RoutedPropertyChangedEventArgs<T>` (MediaGallery)
   - `ContentControl.HasContent` and `Grid.ShowGridLines` (ContentControlStyle)
   - `Panel.IsItemsHost` (IntroToStylingAndTemplating)
   - `MessageBoxOptions` (MessageBox)
   - `Window.ShowActivated` (ShowWindowWithoutActivation)
   - `FontFamily` typed as `string` (FontProperties)
5. **Input and focus.** Involved in 13 apps, 7 only.
   - Only: ContextMenuOpening, FocusVisualStyle, CursorType, CustomRoutedCommand, EditingCommands, KeyStrokeCounter,
     CalculatorDemo.
   - Covers: `KeyboardFocusChangedEventArgs` and the `(Preview)Got/LostKeyboardFocus` overrides,
     `FocusVisualStyle`, `Cursors.*`, `Cursor.Dispose`, `EditingCommands`, `RoutedCommand()`,
     `ContextMenuEventArgs`.
   - Also hide `CreateWidget` from user subclasses (`CS0534`, 9 apps).
6. **Flow direction and flow documents.** Involved in about 29 apps, 8 only.
   - Covers: the `FlowDirection` enum, `FrameworkElement`/`Inline`/`FlowDocument.FlowDirection`, `FlowDocument`
     columns, `Inline` as a `DependencyObject` and `Table`.

Deliberately not next:

- Imaging codecs and Media3D: 18 apps, 10 only, but large.
- System.Xaml/BAML tooling.
- Legacy (non-SDK) csproj conversion: 21 projects, only 1 of them a portable app.
- The packaging hygiene item from the previous report is still open: `CS2002` in 55 of 59 builds, and `NU1201` in
  converted ProjectReference chains (CustomApplicationClass, BamlReflector).

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches. Each batch was copied under
  `.claude/samples3/batchN`, with empty `Directory.Build.props`/`.targets` (so `TreatWarningsAsErrors` was off) and an
  isolated NuGet cache. No original or tracked file was modified.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26324` from the repo `Packages/` feed.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again; this result is recorded.
  - Nothing else is changed. The diff is saved as `csproj.diff` beside each copy.
  - Exception: CustomApplicationClass was rebuilt once by hand after its sibling client had converted.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. The error codes and
  messages in the JSON were re-parsed uniformly from the final logs of all six batches (UTF-8 and UTF-16), and
  deduplicated by code and message. The parse agreed with every batch's own pass/fail.
- **Run.** Each app that built got a smoke run on Windows of about 10 s.
  - "Runs" means alive with a visible top-level window.
  - "Crash" means an unhandled exception or an exit with no window.
  - "Hang" means alive with no window.
  - The run proves nothing about visual or behavioural correctness.
- **Windows-only.** The verified classification from the earlier runs, plus the native C++ projects.
- **Limits.**
  - Only Windows was measured; Linux and macOS were not run.
  - Legacy (non-SDK) csproj files are not converted, so they were not built: 21 projects.
  - The theme counts come from a regex classification of compiler messages (`blockerThemes` in the JSON). They are a
    ranking aid, not an exact partition.
  - The attribution of the 16 moves is by elimination: the removed blocker was the only one recorded before. It was
    not bisected.

## History

| Date | Main SHA | Package | Portable build | Portable run | Note |
|---|---|---|---:|---:|---|
| 2026-11-19 | `3b73e4f5` | 1.0.0.26323 | 0 / 206 | 0 | +CM diagnostic: 16 build, 10 run |
| 2026-11-20 | `db9526db` | 1.0.0.26324 | 45 / 206 | 31 | 10 crash, 4 hang; no regressions |
| 2026-11-20 | `64cddd04` | 1.0.0.26324 | 59 / 206 | 47 | Round 44; 10 crash, 2 hang; 2 regressions (TargetValues, MultiPage: x:Class-less Page now compiled) |
