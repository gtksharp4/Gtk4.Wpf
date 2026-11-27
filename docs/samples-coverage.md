# WPF samples coverage

Measured 2026-11-25 on `main` at `f678058f60de863827a1b973f2026f26ebf0f9cf`. This closes the Round 62–72 programme:

- Round 62 (lane A): automation attached properties, the Window adorner layer and `DataGridLength`.
- Round 63 (lane B): generator sweep IV and rendering hygiene.
- Round 64 (lane D): WPF access keys and CheckBox/RadioButton caption labels.
- Round 66 (lane G): control brushes over the GTK theme and `FlowDocumentPageViewer` pagination.
- Round 67 (lane H): HexSphereDemo's 3D textures, gradient and rotate storyboard.
- Round 68 (lane E): the Window default size, WPF `ScrollViewer` measure and defaults, and `Image` layout.
- Round 69 (lane I): WPF `Shape` layout.
- Round 70 (lane J): `VisualBrush`/`TileBrush` 2D painting and `OpacityMask`.
- Round 71 (lane K): inline UIElements in `TextBlock` and KeyFrameAnimation.
- Round 72 (lane F): WPF content resolution, implicit DataTemplates, `TabControl` templates and `Hyperlink` navigation.
- The final cleanup pass (`gf73/cleanup`).

The package is `Gtk4.Forms.Wpf 1.0.0.26329` from a **pinned** local feed. The samples are the official
`microsoft/WPF-Samples` at `00f39dbb`: 256 projects, of which 243 are apps, 9 are libraries and 4 are native C++
projects. All 252 csproj were rebuilt, and every built app was smoke-run. Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report `88c92780` (main
`17c64167`, Rounds 59–61, the last full re-measure) under `previous`.

## Headline

**201 of 204 portable official WPF sample apps build unmodified, and 195 run.** In report `88c92780` it was 198 build
and 191 run (+3 build, +4 run). In report `fedd13c8` (2026-11-19) it was 0 and 0.

- **3 apps moved fail→pass.** DataBindingDemo and ExpenseItDemo build and run. WPFGallery builds, then crashes at
  startup on the next wall: the .NET 9 Fluent theme dictionary.
- **2 built apps moved crash→runs:** PhotoStoreDemo (Window adorner layer) and SlidePuzzleDemo (object-typed literals
  stay strings). AnimationExamples still crashes, but on a later wall.
- **No regressions.** No project went pass→fail, and no app went runs→crash/hang.
- **No hangs.** 6 portable apps build but do not run, and all 6 crash:
  - WPFGallery is a new build.
  - AnimationExamples crashes further in.
  - The other 4 are known, and each is deliberate or a defect in the sample.
- **3 portable apps still fail to build.** EditingExaminerDemo needs a packaging change plus a small documents tail.
  BamlReflector and LocBaml are System.Xaml/BAML tooling, which is deliberately not next.
- **207 projects build in all.** That is the 201 portable apps, five libraries (CustomApplicationClient, wpfclock,
  HostingWpfUserControlInWf, WPFControls and EditBoxControlLibrary) and the Windows-only console app FocusTracker.
- **The count is close to its ceiling.** At most 1 more build and 2 more runs can come from layer work (see Next
  rounds). Everything else left is Windows-only, a defect in the sample, or deliberately out of scope. Most of the
  programme's work was fidelity: what the running apps look like and do.

### Corrections to the raw batch data

Every csproj's final build log was re-parsed uniformly. The run results come from the batches' per-app run records.
For this report, every crash, every app that moved and every app whose re-run evidence existed only in a batch note
was re-run alone (see Method). The top 15 missing symbols were checked against the `Gtk4.Forms.*` source.

- **The aggregated denominator was wrong.** The aggregate said 208 portable apps, with 201 building and 195 running. It
  used the batches' own regex `windowsOnly` flags. As in every earlier report, the verified classification is carried
  over unchanged, so the denominator stays 204. The numerators are the same: 201 and 195.
- **Batch 3 was rebuilt for this report.** Its scratch, including all logs, had been deleted. Its note also
  undercounted: it gave 23 builds and 20 failures. The 43 batch-3 csproj (Sample Applications, Migration and
  Interoperability) were rebuilt from fresh copies with the same recipe and feed:
  - 24 build: 20 apps and 4 libraries. 19 apps run and 1 crashes (WPFGallery).
  - The batch's progress file lost five records to a write lock (an observer held the file open). The build results
    of those five come from their logs.
  - PhotoViewerDemo, SlidePuzzleDemo and VideoViewerDemo were re-run alone for 20 s, and each showed a window from 1 s.
  - WPFGallery was re-run alone and crashed the same way.
- **Batch 2's "38 runs, 1 unreconciled" is 39 runs.** Its `smoke.txt` holds all 41 built apps. 35 had a window
  within 10 s. KeySplineAnimations, 2DTransforms, CustomBitmapEffect and OpacityMasking showed one when re-run alone
  for 20 s, which makes 39 runs. AnimationExamples and
  WDPEncoderAndDecoder crash.
- **Batch 1's "Grid ColumnDefinitions gap" is not a layer gap.** The message `Skipped element ColumnDefinition in
  Grid.ColumnDefinitions` appears only in ADODataSet. That is a legacy csproj built against .NET Framework, where the
  generator cannot resolve the layer's types. No portable log has it.
- **FocusTracker exits; it does not hang.** It is a Windows-only console app that prints its UI Automation message and
  ends, because stdin is null and it calls `Console.ReadLine`. The run value is `exits` (it was `hang` in `88c92780`).
  It is outside the denominator.
- **Top-15 symbol check** (the aggregated `topMissing` ranking):
  - **Artefact:** `System.Windows.Markup.GtkFormsXamlRegistration` and `…Attribute` (7 each). The attribute exists
    (`Gtk4.Forms.PresentationFramework/Markup/GtkFormsXamlRegistry.cs:13`). All 7 hits are legacy csproj files
    built against .NET Framework with the layer's generated `.g.cs`. This is a converter limit, not a layer gap.
  - **Windows-only, dropped from the ranking:**
    - `System.Windows.Forms` (6) and its namespace form (4), and `Form` (6).
    - `System.Windows.Interop` (2).
    - The WinRT TouchKeyboard pair, both legacy csproj files: `Windows`, `TypedEventHandler<,>`, `InputPane` and
      `InputPaneVisibilityEventArgs` (2 each).
    - `DpiScale` (2: FormattedTextExample and ImageScaling). This is absent from the layer, but PerMonitorDPI is
      per-monitor DPI by design and is classified Windows-only.
  - **Real gaps, absent from `main`, and deliberately not next:** `System.Xaml`, `System.Windows.Baml2006`,
    `Baml2006Reader`, `IXamlLineInfoConsumer` and `NamespaceDeclaration` (2 each: BamlReflector and the library
    BamlTools). No `namespace System.Xaml` or `Baml2006Reader` exists in the layer.

## Per category

These are portable apps on the strict recipe. Δ is the change since `88c92780`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 2 | 0 | 2 | 0 |
| Animation | 11 | 0 | 11 | 11 | 0 | 10 | 0 |
| Application Management | 10 | 1 | 8 | 8 | 0 | 8 | 0 |
| Clipboard | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 30 | 0 | 30 | 0 |
| Documents | 17 | 2 | 15 | 15 | 0 | 15 | 0 |
| Drag and Drop | 6 | 0 | 6 | 6 | 0 | 6 | 0 |
| Element Tree | 2 | 0 | 2 | 2 | 0 | 2 | 0 |
| Elements | 10 | 0 | 10 | 10 | 0 | 10 | 0 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 8 | 0 | 7 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 14 | 0 | 13 | 0 |
| Graphics | 26 | 0 | 25 | 25 | 0 | 24 | 0 |
| Input and Commands | 18 | 1 | 16 | 16 | 0 | 16 | 0 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Resources | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Sample Applications | 23 | 1 | 21 | 20 | +3 | 19 | +4 |
| Speech and Media | 2 | 1 | 1 | 1 | 0 | 1 | 0 |
| Styles & Templates | 5 | 0 | 5 | 5 | 0 | 5 | 0 |
| Threading | 3 | 0 | 3 | 3 | 0 | 3 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 2 | 0 | 2 | 0 |
| Windows | 12 | 2 | 10 | 10 | 0 | 10 | 0 |
| **Total** | **256** | **39** | **204** | **201** | **+3** | **195** | **+4** |

17 categories are complete for their portable apps: every app builds and runs. In the rest:

- The build failures are in Sample Applications (EditingExaminerDemo) and Tools (2).
- The 6 non-runners are each in a different category: Animation, Compatibility, Getting Started, Globalization and
  Localization, Graphics and Sample Applications.

## What moved

Each count move is attributed by elimination: the named change removed every blocker that `88c92780` recorded for
the app. Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass: 3 apps.**

| Change | Apps (run result) |
|---|---|
| Round 62 lane A automation attached properties (merged `c24ce59e`): `AutomationProperties.HeadingLevel`, `ItemStatus`, `AcceleratorKey` and the `UIElementAutomationPeer` reads | DataBindingDemo (runs); WPFGallery (crash, see Crashes) |
| Round 62 lane A automation (`c24ce59e`) + controls tail IV (`a3120799`): `DataGridLength`, `DataGridLengthConverter`, `DataGrid.ColumnWidth` | ExpenseItDemo (runs) |

**Crash→runs: 2 apps.**

- PhotoStoreDemo: Round 62 lane A window adorner layer (`06a239df`). `Window` owns an implicit adorner layer, so
  `AdornerLayer.GetAdornerLayer` finds one.
- SlidePuzzleDemo: Round 63 lane B generator sweep IV (`20f220ff`). `Tag="1"` on an `object`-typed property stays a
  string.

**Moved blockers in apps that still fail or crash.**

- AnimationExamples: the `TextEffectRotateTransform` wall is gone, because Round 63 lane B added
  `TextBlock.TextEffects` and registers the names inside `TextEffect.Transform`. It now stops on a routed-event
  `Source` cast (see Crashes).
- WPFGallery: the build wall is gone (`AutomationProperties.HeadingLevel`). At run time it needs the Fluent theme
  dictionary.
- EditingExaminerDemo is unchanged (`PermissionSet`).

**User-reported samples.** Each status is from this measurement. "Runs" means the app starts and stays up with its
window. The visual and behavioural evidence comes from each lane's own gate (pixel tests, screen grabs) and was not
re-checked here.

| Sample | Round (merge) | What changed | Status now |
|---|---|---|---|
| CheckBox/RadioButton text | 64 lane D (`1b914771`) | A string `Content` becomes the `Gtk.CheckButton` label through the access-key parser, so the caption shows next to the indicator. 66 lane G then painted the indicators with the control brushes | Every portable app that uses them builds and runs; not inspected one by one |
| Access keys in menus/buttons | 64 lane D (`1b914771`) | WPF `AccessText` rules for Button, RepeatButton, ToggleButton, CheckBox, RadioButton, Label and the MenuItem/GroupBox/Expander/TabItem headers (`_Open` shows `Open` with key O, and `__` shows `_`). Alt+key activation goes through the window | DialogBox, FontDialogDemo, WindowActivationAndDeactivation, DocumentMerge and AnnotationsStyling build and run |
| BMP/GIF/JPEG/PNG/TIFF imaging samples | 68 lane E (`7d676dea`), Round 65 re-land | An unsized `Manual` window opens at 3/4 of the work area, as WPF's `CW_USEDEFAULT` window does, not at the content's 466x122 natural size. `Image` measures from the source's DIP size | All five run, with a window at 2 s. WDPEncoderAndDecoder still crashes (no JPEG XR codec) |
| ContentControlStyle | 72 lane F (`0d305896`) | `Label` content shows the styled `ContentControl`, and `btn2` shows its wrapped `TextBlock`. Earlier it showed the type name and an empty button | Runs. Re-run alone for this report: window at 2 s |
| Concepts (Expander headers, ListBox hyperlinks) | 72 lane F (`0d305896`) | Expander headers read "Animation", "Controls", "Databinding". List items show their links, and clicking one navigates `frame2` | Runs, with a slow first window. Re-run alone: no window in the first 20 s, then one from 1 s in a second run (as in the Round 68 gate) |
| FlowDocumentProperties | 66 lane G (`3ffb6b79`) | Button brushes override the GTK theme face (blue with white captions). A block-aware page meter lays out pages and columns from PageWidth/Height, PagePadding and the Column* properties | Runs, window at 3 s; stderr has GTK min-height warnings (1 KB) |
| HexSphereDemo | 67 lane H (`aec1ac36`) | Textured spheres, the radial gradient backdrop and the "Image 1 animate" spin. `<Resource>` images are copied to the output, `ParallelTimeline` children are generated, `Color.FromScRgb` matches WPF | Runs, window at 3 s, 0 bytes of stderr |
| DataTrigger | 72 lane F (`0d305896`) | Implicit `DataType` templates; Name and State columns; WA rows in red and Portland OR on cyan (needed `MultiDataTrigger` and a key-less `ListBoxItem` style) | Runs. Re-run alone: window at 2 s, 0 bytes of stderr |
| Graphics/Geometry | 69 lane I (`5cd4fe7f`), 70 lane J (`2f6122af`) | Shape natural size, stretch matrix, clipping, EvenOdd under transforms, live `PathGeometry` changes | Runs, window at 4 s. **Open:** the 3D pane stays dark (a `VisualBrush` 3D material is not rendered), and the combining-geometries page is empty (see the next row). stderr has 207 `gdk-frame-clock: layout continuously requested` and 48 "snapshot without a current allocation" lines in 10 s |
| ↳ Geometry, combining page | — | 4 `<Path.Data>` property elements holding a `CombinedGeometry` are skipped: `CombinedGeometry` does not exist in the layer | Found by this report, not scheduled |
| ShapesGallery | 69 lane I (`5cd4fe7f`) | WPF `Shape` layout, as above | Runs (first window at 10 s under load), 0 bytes of stderr |
| VisualBrush | 70 lane J (`2f6122af`) | Live `VisualBrush` sources hosted in the owner's window; `TileBrush` tiling; `OpacityMask` in element space. Pixel-tested on all three tabs | Runs, window at 3 s, 0 bytes of stderr |
| KeyFrameAnimation | 71 lane K (`c47d09d8`) | `InlineUIContainer` children of a `TextBlock` are hosted, so the listing lines show. Same-property clocks of one `Begin` compose, so the nested lines enlarge in turn | Runs, window at 4 s. stderr has 4 `GtkLabel ... must not decrease below min` lines (the FontSize animation) |

**Rendering hygiene (Round 63 lane B, `3a6b5e64`).** In this run, Per-FrameAnimation and ParticlesDemo wrote 0 bytes
of stderr. The `Cairo.Context is leaking` and `Gsk ... out of memory` warnings that the last report measured are gone.

**Error landscape.** Counted as portable failing apps per code, `88c92780` → now:

- `CS0117`: 3 → 0, all `AutomationProperties`.
- `CS1061`: 1 → 0 (`DataGrid.ColumnWidth`).
- `CS0246`: 3 → 3, unchanged (`PermissionSet`, BAML tooling).
- `CS0234`: 2 → 2, unchanged (`System.Xaml`, `Markup.Localizer`).

`CS2002` (a generated `.g.cs` specified twice) is a warning in 34 of 201 portable builds (35 of 198 before). Every
build-2 warning comes from sample code or from this duplicate-item pattern, except one generator-side warning:
`CS0649` in 15 projects. That is 34 fields, all in generated `.g.cs` files: an `x:Name` on a `BeginStoryboard` is
declared as a field and never assigned (see XAML generator errors).

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang.

Some apps showed no window in their first 10 s run under parallel load (six batches ran at once), then showed one when
re-run alone for 20 s. All of these are recorded as `runs`, as in earlier reports:

- Batch 1: BindingDPToDP, BindValidation, CollectionBinding, ValidateItemSample and Concepts.
- Batch 2: KeySplineAnimations, 2DTransforms, CustomBitmapEffect and OpacityMasking.
- Batch 4: FontProperties, DragDropDataFormats, DragDropEvents, DragDropOpenTextFile and KeyboardKey.
- Batch 5: CommonDialog and WindowSizingOrder.
- Batch 6: CustomApplicationClass, ExceptionHandlingSecondaryUIThread, CustomClassesWithDP, ContentControlStyle and
  EventTriggers.

For batches 1 and 6, the solo re-run result survives only in the batch note. So BindingDPToDP, Concepts and
ContentControlStyle were rebuilt and re-run alone for this report, and all three show a window. BindingDPToDP's
appeared at 11 s, a cold start.

## API demand

The counts are failing portable apps (3). Windows-only and legacy-csproj demand is excluded.

- "Apps" is the number of failing portable apps whose errors touch the theme (`blockerThemes`).
- "Only" is the number of apps whose every recorded blocker is in the theme.

| Theme | Symbols (apps) | Apps | Only |
|---|---|---:|---:|
| System.Xaml/BAML tooling | `System.Xaml` (`XamlSchemaContext`, `XamlType`, `XamlReader`/`XamlWriter`, `Baml2006Reader`, …) and `System.Windows.Markup.Localizer` (BamlReflector, LocBaml) | 2 | 2 |
| Code access security | `PermissionSet`/`PermissionSetAttribute` (EditingExaminerDemo; on .NET these come from the `System.Security.Permissions` package, not WPF) | 1 | 1 |

EditingExaminerDemo has a second wall behind its first. The held branch `gf63b/packaging-permissions` adds the
package, and its commit message records that the app then fails on `Floater`, `AnchoredBlock`, `ParserContext` and
`HeadingLevel`. `HeadingLevel` has landed since (Round 62). `Floater`, `AnchoredBlock` and `ParserContext` are still
absent from `main` (checked: `XamlReader.cs` documents that it has no `ParserContext` overload). The sample uses
`AnchoredBlock`/`Floater` in `TreeViewhelper.cs:98-101` and `new ParserContext { BaseUri = … }` in
`XamlHelper.cs:88`.

Run-time demand (built apps that crash on a layer gap):

| Theme | Gap (app) |
|---|---|
| Fluent theme | `pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml` is merged in `App.xaml`, and its keys (`ControlCornerRadius` first) are not supplied (WPFGallery) |
| Routed events | `RoutedEventArgs.Source` is not adjusted where a `Page` in a `Frame` meets the outer logical tree (AnimationExamples) |

Fidelity demand, in apps that already run (not counted in the headline):

| Theme | Gap (apps) |
|---|---|
| Generator property elements | `DataGrid.Columns` (ExpenseItDemo, ExpenseItIntro), `ListView.View` (BindingToStringFomat, EditingCollections, WPFGallery), `ListBox.GroupStyle`/`ItemsControl.GroupStyle` (CollectionViewSource, DataBindingDemo, AlternatingAppearanceOfItems, Grouping), `ListBox.ItemsSource` and `TextBlock.Text` (Concepts, CompositeCollections, PriorityBinding), `*.RenderTransform`, `*.InputBindings`, `InkCanvas.*`, `ItemsPanel`, `LinearGradientBrush.GradientStops` |
| Geometry | `CombinedGeometry` is missing (Geometry's combining page); `ImageDrawing` is missing (DrawingBrush) |
| Bitmap effects | `BitmapEffect` property elements (BitmapEffectsGallery, Brushes): obsolete in WPF, rendered as no-ops there since .NET 4.0 |

## Crashes (built, did not run)

| Sample | Cause |
|---|---|
| WPFGallery | **New build.** `InvalidOperationException: Resource with key 'ControlCornerRadius' not found` (`MainWindow.g.cs:34`, a `StaticResource` in a `Border.CornerRadius` setter). `App.xaml` merges the .NET 9 Fluent theme dictionary (`PresentationFramework.Fluent;component/Themes/Fluent.xaml`), which the layer does not supply. Expect more walls behind it (the Round 62 gate predicted this one). |
| AnimationExamples | **Further in.** `InvalidCastException` (String to `Frame`) at `SampleViewer.cs:54`. `InteractiveAnimationsExample.PageLoaded` (line 112) sets `IsChecked` on a RadioButton inside a `Page` hosted in a `Frame`. Its `Checked` event bubbles to `SampleViewer`'s `RadioButton.Checked` handler with `args.Source` still the inner RadioButton (string content), so `(Frame)theButton.Content` fails. In WPF a navigated page is not a logical child of the `Frame`, so the source is adjusted at that boundary. *(Corrected in Round 75: `Frame.AdjustEventSource` returns the `Frame` itself, so the outer handler sees the `Frame` as `Source` and the sample's `args.Source is RadioButton` check skips the cast. Fixed in Round 75; not run on WPF.)* |
| ExpenseItIntro | Unchanged: `Resource with key 'CustomNavWindowStyle' not found`. The key is defined nowhere in the sample, so WPF rejects it too. A defect in the sample (re-run alone twice for this report). |
| WDPEncoderAndDecoder | Unchanged: `NotSupportedException: JPEG XR (WDP) has no encoder in the GTK runtime`. Deliberate: gdk-pixbuf/GdkTexture have no JPEG XR codec. |
| AppCompat-Quirks | Unchanged: there is no window at 10 s, and two solo 20 s runs both crash with a `NullReferenceException` in the sample's `PopulateAppContextInformation` (`MainWindow.xaml.cs:197`, from `Window_Loaded`). It reflects over WPF-internal AppContext switch types. Not worth chasing. |
| LocalizationResources | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. The sample ships only an en-US satellite and no neutral resource. |

Two crashes are layer work: WPFGallery is track 1 below and AnimationExamples is track 2. The other four are out of
scope.

## XAML generator errors

No portable app fails to build on a generator error. Per-project messages are in the JSON (`xamlErrors`).

1. **Outside the denominator.**
   - The legacy-csproj `GtkFormsXamlRegistration` artefact: HostingWfInWpfWithXaml, PropertyMappingWithWfh,
     WpfLayoutHostingWfWithXaml, ADODataSet, UsingSpeech, WinFormsHostScaling and SingleInstanceDetection.
   - DocumentStructure, whose `fixedpage1_structure.xaml` is malformed XML in the sample:
     `xmlns:my="clr-namespace:MyCompany.MyProduct` has no closing quote. That app is Windows-only.
2. **Named `BeginStoryboard` fields are never assigned** (`CS0649`, 34 fields in 15 projects: AnimationExamples,
   AnimationTiming, KeyFrameAnimation, KeySplineAnimations, OpacityAnimation, PathAnimations and others). The field is
   declared from `x:Name`, but `InitializeComponent` does not set it. Code that reads the field gets null. XAML
   `BeginStoryboardName` references resolve through the name scope and are not affected (not re-checked per sample).

**"Skipped property element" backlog.** These are MSBuild messages, not errors, but they are a silent-drop risk at run
time. The 207 passing final logs hold 59 distinct messages in 25 projects. In `88c92780` there were 81 in 32 projects
over 205 logs, and the drop comes from Round 63 lane B. The groups that cover more than one project:

- `ListBox.GroupStyle` (3), `ListView.View` (3).
- `ListBox.ItemsSource`, `TextBlock.Text`, `DataGrid.Columns` and `Path.Data` (2 each).

Each of the rest is in a single project: `*.RenderTransform`, `*.InputBindings`, `*.BitmapEffect`, `InkCanvas.Strokes`/
`DefaultDrawingAttributes`, `ItemsPanel`, `CollectionViewSource.SortDescriptions`, `SpellCheck.CustomDictionaries`,
`LinearGradientBrush.GradientStops`, and the user-type elements `FireworkEffect.Child`, `SonicEffect.Child`,
`ExpenseReport.LineItems`, `HeaderTile.Source` and `ControlExample.XamlCode`. Two elements are not in the layer at
all: `ImageDrawing` and the `PreviewCanExecute` attribute on `CommandBinding`.

## Next rounds

### Rounds 77-79 (executed 2026-11-27; samples not re-measured)

Merged into `main`; details are in the plan, "Rounds 77-79". The per-app results come from scratch builds made by the
lanes.

| Lane | App effect observed (scratch build) | Next wall |
|---|---|---|
| A (R77) WPFGallery startup | Alive 25 s with the main window shown (the keyed-template wall was worked around locally; R78 fixes it) | Content and navigation not verified; the generator keeps the quotes around a `StringFormat` (runtime stopgap only); `IconsPage.xaml:278` `StringFormat` is emitted truncated |
| B (R78) Item views and grouping | Grouping shows the group headers; CompositeCollections shows its CLR items and `ListBoxItem`s | `Binding.XPath` and XML-typed implicit `DataTemplate`s (Grouping, CompositeCollections); `GroupItem` containers (AlternatingAppearanceOfItems); a fixed GridView header |
| C (R79) EditingExaminerDemo, sizing | EditingExaminerDemo runs (no exception, stable 850x660 window) | GeometryDesignerDemo still grows (a NaN-size window is not clipped); only `Shape` is capped by fixed tracks |

Next candidates by value: `Binding.XPath` + XML `DataTemplate`s (2 apps); a full re-measure of the samples (none since
`f678058f`); the generator `StringFormat` unquoting; the NaN-axis clip (GeometryDesignerDemo); `GroupItem` containers.

### Rounds 74-76 (executed 2026-11-27; samples not re-measured)

The six tracks below ran as Round 74 (lane A), Round 75 (lane B) and Round 76 (lane C) and are merged into `main`.
Details are in `docs/plans/gtk-forms-wpf-on-gtk-1to1.md`, "Rounds 74-76". The counts in this report are still those of
`f678058f`. The per-app results below come from scratch builds made by the lanes, not from the pinned-feed Method.

| Track | Status | App effect observed (scratch build) | Next wall |
|---|---|---|---|
| 1 Fluent theme | partial | WPFGallery passes `ControlCornerRadius` (119 of 124 Fluent keys resolve) | keyed `ItemsPanelTemplate` emitted as `""` (re-check after track 4); a window shown before `Run` needs lazy GTK init; `{}`-escaped `StringFormat` `FormatException` |
| 2 Routed-event source | done | AnimationExamples ran 25 s with no crash | none seen |
| 3 Packaging + documents | done | EditingExaminerDemo builds | `WindowLoaded` reflects over the WPF-internal `ComboBox.EditableTextBoxSite` (sample defect class) |
| 4 Generator sweep V | partial | 45 → 35 skipped-element messages in the corpus | `ListView.View`/`GridView`, `GroupStyle`, `CompositeCollection` need runtime types |
| 5 Window sizing | partial | Min/Max cap `SizeToContent`; a `ScrollViewer` window content sizes from the managed Measure | a `ScrollViewer` nested in a Grid/StackPanel still gets the GTK natural size; fixed-track capping; `Stretch` shapes `ActualWidth`; the GeometryDesignerDemo layout loop (reproduced, cause not found) |
| 6 Legacy csproj | done (refuse, exit 20) | — | `SkipGtkFormsConvert=true` bypasses the refusal; legacy siblings reached through a `ProjectReference` are not checked |

The original plan for these rounds follows.

These are ranked by portable samples unblocked. "Only" means samples whose sole recorded blocker is the track. A later
phase (a second wall at run time) can still hold an app back, so treat the counts as upper bounds.

The count is close to its ceiling. Done in full, the three count-moving tracks would take it to at most 202 build and
197 run. Tracks 4–6 move no count: they are fidelity and converter work, ranked by the apps they touch.

**Excluded by decision:** the DrawingBrush rendering lane was not run (user decision), and it is not proposed again
here.

1. **Fluent theme dictionary: 1 crash→runs (WPFGallery), upper bound.**
   - Supply `PresentationFramework.Fluent;component/Themes/Fluent.xaml` (and its Light/Dark variants if the sample
     asks for them) as a layer resource dictionary, starting with the keys WPFGallery reads: `ControlCornerRadius`
     first. Then iterate wall by wall.
   - Owns: new `Themes/Fluent/**`; the `PresentationFramework.Fluent` pack-URI case in `Resources/ResourceStreamLocator.cs`;
     `Themes/ThemeManager.cs`.
2. **Routed-event source adjustment: 1 crash→runs (AnimationExamples).**
   - Adjust `RoutedEventArgs.Source` where the route crosses a logical-tree boundary (a navigated `Page` in a `Frame`),
     as WPF does. Check WPF's rule (`RoutedEventArgs.Source` re-targeting along the logical tree) before coding. Expect
     no other wall: the next frames in the sample are plain storyboards.
   - Gate: every Data Binding, Events and Input and Commands app, because they all route events.
   - Owns: `Gtk4.Forms.WindowsBase/RoutedEventHost*.cs`, `Gtk4.Forms.WindowsBase/RoutedEventArgs.cs`,
     `Controls/Frame*.cs`, `Navigation/**`.
3. **Packaging and documents tail: 1 fail→pass (EditingExaminerDemo), upper bound.**
   - Rebase `gf63b/packaging-permissions` (`28737070`): it adds `System.Security.Permissions` as a `Gtk4.Forms.Wpf`
     dependency and pins the package dependencies.
   - Then add `AnchoredBlock`, `Floater` and `ParserContext`, with `XamlReader.Load(Stream, ParserContext)` (`BaseUri`).
   - Check first: that `SYSLIB0003` stays a warning in the consumer.
   - Owns: `Gtk4.Forms.Wpf/Gtk4.Forms.Wpf.csproj` and its package tests; new `Documents/AnchoredBlock.cs` and
     `Documents/Floater.cs`; new `Markup/ParserContext.cs`; `Markup/XamlReader.cs`.
4. **Generator sweep V (fidelity): 0 count, 25 projects with skipped elements, 15 with unassigned fields.**
   - `DataGrid.Columns` (ExpenseItDemo shows a grid with no columns), `ListView.View`, `*.GroupStyle`,
     `ListBox.ItemsSource`, `TextBlock.Text` and `Path.Data`, with a new `CombinedGeometry` for the Geometry sample.
   - Then `*.RenderTransform`, `*.InputBindings` and `ItemsPanel`, and assign named `BeginStoryboard` fields.
   - Fold `gf39/xaml-content-model` in as a test-only change (the compile-checked corpus); the backlog is its corpus.
   - Owns: `Gtk4.Forms.MarkupCompiler/**`, `Gtk4.Forms.MarkupCompiler.Tests/**`,
     `Gtk4.Forms.WindowsBase/Markup/XamlValueConverter*.cs`, `Gtk4.Forms.PresentationFramework/Markup/**` (except
     `ParserContext.cs` and `XamlReader.cs`, which belong to track 3), and new `Media/CombinedGeometry*.cs`.
5. **Window and layout sizing (fidelity): 0 count.**
   - The known sizing gaps:
     - `SizeToContent` around a `ScrollViewer`.
     - Min/Max not capping a `SizeToContent` axis.
     - Fixed Grid tracks and explicit parent sizes not capping a child's natural size.
     - `Stretch != None` shapes reporting the slot as `ActualWidth`/`ActualHeight`.
   - The GTK layout loop behind Geometry's 207 and GeometryDesignerDemo's `gdk-frame-clock: layout continuously
     requested` lines, and the measure warnings in Concepts, ContentControlStyle, DragDropEvents (215 KB of stderr in
     20 s) and FlowDocumentProperties.
   - Owns: `Window.DefaultSize.cs` and the `SizeToContent` code of `Window.cs` (move it to a new `Window.SizeToContent.cs`
     partial first), `Controls/ScrollViewer*.cs`, `Controls/Grid.cs` and `Controls/Grid.Layout.cs`,
     `FrameworkElement.GtkLayout.cs`, and `Shapes/Shape.Layout.cs`/`Shape.NaturalLayout.cs`.
6. **Legacy (non-SDK) csproj conversion: 0 portable (21 projects, all Windows-only).**
   - Convert old-style csproj files (`ToolsVersion`, `Microsoft.CSharp.targets`, `TargetFrameworkVersion v4.x`) to SDK
     style, or refuse them with a clear message. Today the converter says "Already converted" and the build compiles
     the layer's `.g.cs` against .NET Framework. This also removes the `GtkFormsXamlRegistration` artefact (7) from the
     symbol ranking and the `NU1201` sibling-reference failures.
   - Owns: `Gtk4.Forms.Wpf.ProjectConverter/**` and its tests, and `Gtk4.Forms.Wpf/build/**` and
     `buildTransitive/**`.

**Lanes (three in parallel, two tracks each, file-disjoint):**

| Lane | Tracks | Only | Boundary to watch |
|---|---|---:|---|
| A | 1 (Fluent theme) + 5 (window and layout sizing) | 1 crash→runs | Track 1 owns `Themes/**` and `ResourceStreamLocator.cs`. Track 5 owns `Window.DefaultSize.cs` and the new `Window.SizeToContent.cs`, `ScrollViewer*.cs`, `Grid.cs`/`Grid.Layout.cs`, `FrameworkElement.GtkLayout.cs` and the two `Shape` layout partials. A Fluent `CornerRadius` must not reach for layout code |
| B | 2 (routed-event source) + 3 (packaging and documents tail) | 1 crash→runs, 1 fail→pass | Track 2 owns `RoutedEventHost*.cs`, `RoutedEventArgs.cs`, `Frame*.cs` and `Navigation/**`. Track 3 owns the `Gtk4.Forms.Wpf` csproj, the two new `Documents/` files, `Markup/ParserContext.cs` and `Markup/XamlReader.cs` |
| C | 4 (generator sweep V) + 6 (legacy csproj conversion) | 0 | Track 4 owns `Gtk4.Forms.MarkupCompiler/**`, `Markup/**` minus the two track-3 files, `XamlValueConverter*.cs` and `CombinedGeometry*.cs`. Track 6 owns `Gtk4.Forms.Wpf.ProjectConverter/**` and the package's `build/`/`buildTransitive/` targets, not the csproj |

Held branches:

- **`gf63b/packaging-permissions` (1 commit `28737070`, 114 behind): take it into track 3.**
  - The plan doc says the Round 63 packaging track "delivered nothing". The ref does carry one commit: the dependency,
    pinned package dependencies and nuspec tests (45 tests, by its message; not re-run here).
  - Its message also records EditingExaminerDemo's second wall (see API demand).
  - Rebase it on `main` and re-gate it. Do not merge it as is.
- **`gf46/input-focus` (6 commits, 342 behind): 0 samples.** Every Input and Commands app builds and runs. What remains
  is focus-transaction behaviour, owned by the focus-behaviour design. A rework must start from `main`'s
  `Input/FocusManager*.cs` (`IsFocusScope`/`GetFocusScope` landed in Round 59), not from the branch. It is not in the
  lanes.
- **`gf39/xaml-content-model` (3 commits, 395 behind): 0 samples on its own.** Fold it into track 4 as a test-only
  change. Do not rebase the product commits.

Deliberately not next:

- JPEG XR encoding (WDPEncoderAndDecoder). There is no codec in the GTK stack.
- System.Xaml/BAML tooling (LocBaml, BamlReflector/BamlTools).
- AppCompat-Quirks (WPF-internal AppContext switches), LocalizationResources (the sample ships no neutral resource) and
  ExpenseItIntro (the sample references an undefined resource key).
- DrawingBrush rendering (user decision, see Known gaps).

## Known gaps

From the end of the Round 62–72 programme (`docs/plans/gtk-forms-wpf-on-gtk-1to1.md`, "Known gaps"):

- SizeToContent around a ScrollViewer does not size to its content.
- Window Min/MaxHeight/Width do not cap an axis SizeToContent sizes.
- Fixed Grid tracks and explicit parent sizes do not cap a child's natural size (Shape, TextBlock).
- Stretch != None shapes report the slot as ActualWidth/Height.
- External http(s) links in Frame/NavigationWindow raise NavigationFailed and stay (WPF hosts web content).
- DrawingBrush rendering is not implemented beyond what exists (DrawingBrush fills).
- VisualBrush as a 3D material is not rendered (Graphics/Geometry 3D pane stays dark).
- VisualBrush self-reference via BorderBrush/Control/TextBlock brushes relies on the pixel storm breaker.
- A Click-only Hyperlink without NavigateUri is unsupported.
- BaselineAlignment other than Baseline and FlowDocument InlineUIContainers are store-only.
- An InlineUIContainer added to a TextBlock after its widget exists is not hosted.
- macOS/Linux GTK runs were not measured in this programme.
- **DrawingBrush rendering lane not run (user decision).**

Found by this report:

- `CombinedGeometry` and `ImageDrawing` are missing (Graphics/Geometry, DrawingBrush).
- Named `BeginStoryboard` fields stay null (`CS0649` in 15 projects).
- `RoutedEventArgs.Source` is not adjusted at a `Frame`/`Page` boundary (AnimationExamples).

Still open from earlier reports (re-checked where noted):

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`, and the button stays disabled until another requery. `CommandSampleTests.The_button_is_enabled_without_a_manual_requery`
  is still skipped (re-checked).
- `ScrollBar` is still on `Control`, not `RangeBase` (`Controls/Primitives/ScrollBar.cs:16`, re-checked).
- Media3D:
  - Painter's-algorithm triangle sort, not a z-buffer.
  - Flat lighting per triangle.
  - Affine texture mapping.
  - `Paced` key times are treated as `Uniform`.
  - Typed 3D animations are deferred.
  - `MeshGeometry3D.TextureCoordinates` edits do not repaint.
- Round 72 content model:
  - `ComboBox` popup items are strings (`Gtk.DropDown` model).
  - `MenuItem` element headers show their `Text`.
  - An implicit template for a string content applies only on a later content change.
  - `XmlNode` data types are not matched.
- Round 66: Aero2 hover/pressed fades and the focus rectangle are not reproduced. `FlowDocumentPageViewer` lays out
  tables, lists and floaters as one block, and Zoom is stored only.
- Round 62:
  - Window's implicit adorner layer is not in `VisualTreeHelper`.
  - Popups and `ScrollContentPresenter` have no implicit adorner layer.
  - Adorner content is not pickable.
  - A Pixel `DataGridColumn.Width` is a minimum.
- `SystemParameters` `*Key` objects do not resolve through `DynamicResource` (not re-checked).

## Method

- **Source.** The local `WPF-Samples` tree (`00f39dbb`), split into six category batches:
  1. Data Binding, Getting Started, Events.
  2. Graphics, Animation, Visual Layer, Speech and Media, Clipboard.
  3. Sample Applications, Migration and Interoperability.
  4. Input and Commands, Documents, Drag and Drop, Compatibility.
  5. Globalization and Localization, Windows, Accessibility, Element Tree, PerMonitorDPI.
  6. Application Management, Elements, Properties, Resources, Styles & Templates, Threading, Tools.

  Each batch was copied under `.claude/samples-remeasure-r72/batchN` without `bin`/`obj`, `NuGet.config` or
  `global.json`. The scratch root has empty `Directory.Build.props`/`.targets`, so `TreatWarningsAsErrors` was off. No
  original or tracked file was modified: `git status` is clean in the worktree and in `WPF-Samples`.
  - Batches 1–4 each discarded a first pass that had a script bug (execution policy, PowerShell `-replace` precedence
    that skipped the `PackageReference`, path tags, a duplicated launch). Only the clean runs are used. In batch 2, a
    few csproj had been converted by the killed first pass, so their build 1 exited 0 ("already converted"). Their
    build 2 ran from fresh `bin`/`obj`.
  - Batch 3 was rebuilt for this report from fresh copies (script `tools/batch3r.ps1`). Its logs are kept under
    `batch3/_logs`.
- **Layer build.** `dotnet build` of `layer.slnf` (`Gtk4.Wpf.sln` without the `WPF-Samples-GTK4` HelloWorld entry, whose
  known `MSB3202`/converter failure belongs to the user's tree) with `-nodeReuse:false`: 0 errors, 3 warnings. The 3
  warnings are the known GitInfo warnings from `WPF-Samples\Getting Started\HelloWorld` (`MSB3073` exit 128, "Could not
  retrieve last commit", "GitInfo.txt is outside repository"), because `WPF-Samples` is a nested git repo. Every
  product and test project is warning-clean. `dotnet pack --no-build` produced the four packages; the only message is
  the "missing a readme" notice for `Gtk4.Forms.Wpf`.
- **Feed, pinned.** `samples-remeasure-r72/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-remeasure-r72/.nuget-packages`, which started empty) and cleared
    `fallbackPackageFolders`.
  - Cleared `packageSources`, then the pinned feed (`samples-remeasure-r72/feed`) and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the recipe `nuget.config`, but with
  `globalPackagesFolder` pointed at an empty probe cache, so the shared cache stayed untouched.
  - Build 1 exited 1, as expected: the converter exited 10 with "re-run the build", and the csproj became `net10.0`
    without `UseWpf`. Build 2 exited 0 with 0 warnings and 0 errors.
  - The cached SHA-256 values equal the feed's. They were re-computed for this report from the feed and from the shared
    batch cache, and all four match:
    - `Gtk4.Forms.Wpf` `6035c9507d9f75d85b37cf5112ba040aa8771e33552c239007f4f1d4dacfde21`
    - `PresentationFramework` `bcffce118e4def9cb5c74bb4f801f46b35d4fcc73b4791c124fce3b445ac6311`
    - `WindowsBase` `2f0030c421fa0f214dabbe545ef59890d0784a8431d53b2c2a5b3f045537dd06`
    - `MarkupCompiler` `26e4b72c27aad90a155250a97d8eabeb14a8c2e459038a4e46654fa94c5e0ea4`
  - Every `gtk4.forms.*` `.nupkg.metadata` source is the scratch feed, in the probe cache and in the shared cache.
  - `project.assets.json` lists only the probe cache as a package folder and no fallback folders. Its sources are the
    feed, nuget.org and the SDK-implicit `library-packs`.
  - `gtksharp4 4.22.4.26327` came from nuget.org.
  - Smoke: the first 10 s run was alive with 0 windows (cold start). Alone for 20 s it was alive with 1 visible unowned
    window from 3 s. A second 10 s run had 1 window from 1 s. All three had 0 bytes of stderr.
  - The probe directory, its cache and its logs were deleted afterwards. All six batches re-checked the cached
    `Gtk4.Forms.Wpf` hash.
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26329`.
  2. Build once with `-nodeReuse:false`. The converter rewrites the csproj and stops.
  3. Build again. This result is recorded.
  - Nothing else is changed. The original csproj files are kept beside the copies (`*.csproj.orig`).
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes, symbols and messages were
  re-parsed uniformly from the 252 final logs (UTF-16 and UTF-8).
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime
  (`%LOCALAPPDATA%\Gtk\4.22.4\bin`) on `PATH`. Each process was killed by its own PID.
  - "Runs" means alive with a visible, unowned top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception, or an exit with or without a window.
  - "Hang" means alive with no window and no exception.
  - Apps with no window at 10 s were re-run alone for 20 s.
  - The run records:
    - batch 1: `batch1-keep/batch1-summary.txt`
    - batch 2: `batch2/logs/smoke.txt` and `*.smoke20.*`
    - batch 3: `batch3/_progress.txt` and `_progress2.txt`
    - batch 4: `batch4/smoke10_all.txt` and `smoke20*.txt`
    - batch 5: `batch5-results.json` and `_logs/*.run2.*`
    - batch 6: `batch6-logs/run.txt`
  - Re-run alone for this report, in a separate `verify` copy built with the same recipe:
    - BindingDPToDP, Concepts, ContentControlStyle, DataTrigger and ExpenseItIntro, for 20 s each.
    - PhotoViewerDemo, SlidePuzzleDemo, VideoViewerDemo and WPFGallery (batch 3).
    - The batches' own solo runs covered AppCompat-Quirks (twice), WDPEncoderAndDecoder, AnimationExamples and
      LocalizationResources.
  - The runners stop at about 10–20 s, so a crash after the first window can still hide in a `runs` row.
  - The run proves nothing about visual or behavioural correctness. That evidence lives in the lane gates cited in
    What moved.
- **Windows-only.** The verified classification of the earlier reports, unchanged.
- **Limits.**
  - Only Windows was measured.
  - Legacy csproj files are not converted: 21 projects.
  - Moves are attributed by elimination, not bisected.
  - `blockerThemes` is assigned per project for the 4 failing portable projects: 3 apps and the library BamlTools.
  - The baseline is report `88c92780`, the last full re-measure. `HEAD`'s copy of the JSON carried 4 rows edited by the
    Round 62 lane A gate (DataBindingDemo, ExpenseItDemo, PhotoStoreDemo, WPFGallery). This run confirms all four, and
    the gate text is kept as `round62LaneAGate`.

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
| 2026-11-23 | `17c64167` | 1.0.0.26327 | **yes** | 198 / 204 | 191 | Rounds 59–61 (third three-lane batch); full re-measure; 7 crash, 0 hang; no regressions |
| 2026-11-24 | `lane/a` (Round 62 lane A) | 1.0.0.26327 | **yes** | 201 / 204 portable (207 / 207 built projects) | 194 | Lane A gate; DataBindingDemo, ExpenseItDemo build and run, PhotoStoreDemo runs, WPFGallery builds then crashes; no regressions |
| 2026-11-25 | `f678058f` | 1.0.0.26329 | **yes** | **201 / 204** | **195** | Rounds 62–72 programme + cleanup; full re-measure; 6 crash, 0 hang; no regressions |
