# WPF samples coverage

Measured 2026-11-22 on `main` at `30f46d4a117f7192cdf184bf5d9f82403b4abdaf` (Rounds 49–52, the first two-lane batch,
plus the Round 52 paginated-viewer review fixes), package `Gtk4.Forms.Wpf 1.0.0.26326` from a **pinned** local feed,
against the official `microsoft/WPF-Samples` (256 projects: 243 apps, 9 libraries and 4 native C++ projects). All 252
csproj were rebuilt and every built app was smoke-run. Per-project results are in
[`samples-coverage.json`](samples-coverage.json). Each row also holds its result in report `ccd8dbb1` (main
`142b53f6`, Round 48, `previous`).

## Headline

**159 of 204 portable official WPF sample apps build unmodified, and 152 run.** That is up from 129 / 119 in report
`ccd8dbb1` (+30 build, +33 run), and from 0 / 0 in report `fedd13c8` (2026-11-19).

- **30 apps moved fail→pass**: 26 run and 4 crash at startup. **7 built apps moved crash/hang→runs.** The library
  wpfclock also builds now.
- **No regressions.** No project went pass→fail, and no app went runs→crash/hang (see Regressions for the one app
  that read as a hang).
- Of the 7 apps that build but do not run, 6 crash and 1 hangs. 4 of them have a known, small cause in the
  MarkupCompiler or the runtime converters (see Crashes).
- 162 projects build in all: the 159 portable apps, the libraries CustomApplicationClient and wpfclock, and the
  Windows-only console app FocusTracker.
- Of the 30 fail→pass, lane A (Rounds 49 and 51: animation, runtime XAML, the startup sweep) accounts for 15 and
  lane B (Rounds 50 and 52: imaging, flow documents, printing) for 14. One is not isolated. All 7 crash/hang→runs
  are lane A.

### Corrections to the raw batch data

The final build log of each of the 252 csproj was re-parsed uniformly (UTF-8 and UTF-16). Run results were taken from
each batch's per-project records, with every run's stderr scanned for unhandled exceptions. The top 15 missing symbols
were checked against the `Gtk4.Forms.*` source.

- **The aggregated totals were wrong.** They said 200 portable apps, 157 build and 64 run. The per-project data gives
  204 / 159 / 152:
  - 200 came from the batches' own regex `windowsOnly` flags. As in every earlier report, the verified
    classification is carried over unchanged, so the denominator stays 204.
  - 64 run under-counts because the batches record a run in different formats ("runs", "runs: MainWindow",
    "runs (window: …)"). Every one of the 152 is a visible top-level window in the batch's records.
  - Several batch summaries contradict their own per-project records. Batch 1 says 37 build, but its logs give 39.
    Batch 2 says 26, but its results file and logs give 29. Batch 4 says 31, but its logs give 32. Batch 5 says 22,
    but its logs give 24, and that includes Highlighter, a legacy csproj, which counts as not-built.
  - The aggregated crash list omits CustomComboBox and GeometryDesignerDemo (batch 3), LocalizationResources
    (batch 5) and the hang DragDropThumbOps.
- **CustomApplicationClass is a run, not a hang.** It showed no window in the 10 s smoke run while other batches were
  loading the machine. Re-run alone twice for this report, at 20 s and at 10 s, it showed its window within 1 s both
  times. ContextMenuOpening likewise showed no window at 10 s under load, but its window appeared within 20 s alone.
- **DragDropThumbOps was re-run alone for this report** (20 s) and is still a hang.
- **Legacy (non-SDK) csproj files stay `not-built`.** Highlighter "builds and runs" again, but as real .NET
  Framework WinForms/WPF, so it is not counted. The attempt is kept under `legacyBuild`.
- **Top-15 symbol check** (the aggregated `topMissing` ranking):
  - Artefacts. `System.Windows.Markup.XamlValueConverter` (6) and `FrameworkElement.AbandonInitialization` (6) exist
    in the layer (`Markup/XamlValueConverter.cs`, `FrameworkElement.Initialized.cs`). Every hit is a legacy csproj,
    built against real WPF with the layer's generated `.g.cs`.
  - Windows-only, dropped from the ranking. `System.Windows.Forms` (6 + 4 as the namespace), `Form` (6) and `DpiScale`
    (3, PerMonitorDPI only).
  - Real gaps, absent from the source:
    - 3D: `System.Windows.Media.Media3D` (7), `Viewport3D` (6), `Vector3D` (5), `Quaternion` (4);
    - controls: `UserControl` (3 counted; 2 real, since FormsControlLibrary's is the WinForms one);
      `ListBox.ScrollIntoView` (3); `Frame.NavigationUIVisibility` (2); `SystemParameters` (2, via `CS0400`);
    - `MenuItem.Padding` (2): `MenuItem` derives from `FrameworkElement`, not `Control`
      (`Controls/MenuItem.cs`), so it has no `Padding`.
  - Labels. `M:CS0103:mySplineKeyFrame`/`myVector3DSplineKeyFrame` are `x:Name` fields that are missing because the
    3D keyframe types are missing.
- **Also found while checking:** `RoutedEventArgs.OriginalSource` does not exist (`Gtk4.Forms.WindowsBase/RoutedEventArgs.cs`
  has only `RoutedEvent`, `Source` and `Handled`). `FlowDocument.FontFamily` and `TextElement.FontFamily` are typed
  `string?`, not `FontFamily`, which causes the `CS0029` in FontProperties and FlowDocumentProperties.
  `ItemsControl.Items` is an `IList<object?>`, not an `ItemCollection` view, so `Items.CurrentItem` does not bind.
- **windowsOnly is carried over unchanged.** Batch 2 again flagged CustomBitmapEffect and KeyFrameAnimation as
  Windows-only, but both build and run on the layer.

## Per category

These are portable apps on the strict recipe. Δ is the change since `ccd8dbb1`.

| Category | Projects | Windows-only apps | Portable apps | Build | Δ | Run | Δ |
|---|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 2 | +1 | 2 | +1 |
| Animation | 11 | 0 | 11 | 8 | +5 | 8 | +5 |
| Application Management | 10 | 1 | 8 | 8 | +1 | 8 | +1 |
| Clipboard | 2 | 0 | 2 | 2 | +1 | 2 | +1 |
| Compatibility | 1 | 0 | 1 | 1 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 29 | 0 | 29 | 0 |
| Documents | 17 | 2 | 15 | 10 | +5 | 10 | +5 |
| Drag and Drop | 6 | 0 | 6 | 5 | 0 | 4 | 0 |
| Element Tree | 2 | 0 | 2 | 1 | 0 | 1 | 0 |
| Elements | 10 | 0 | 10 | 9 | 0 | 9 | +2 |
| Events | 4 | 0 | 4 | 4 | 0 | 4 | 0 |
| Getting Started | 8 | 0 | 8 | 6 | +1 | 5 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 13 | 0 | 12 | 0 |
| Graphics | 26 | 0 | 25 | 19 | +9 | 18 | +11 |
| Input and Commands | 18 | 1 | 16 | 16 | 0 | 16 | +1 |
| Migration and Interoperability | 23 | 15 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 3 | +1 | 3 | +1 |
| Resources | 3 | 0 | 3 | 3 | +1 | 3 | +1 |
| Sample Applications | 23 | 1 | 21 | 7 | +4 | 5 | +2 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 4 | 0 | 4 | 0 |
| Threading | 3 | 0 | 3 | 3 | +1 | 3 | +1 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 6 | 0 | 6 | +1 |
| **Total** | **256** | **39** | **204** | **159** | **+30** | **152** | **+33** |

## What moved

Each move is attributed by elimination: the named change removed every blocker that `ccd8dbb1` recorded for the app.
Nothing was bisected. Per-row attribution is in the JSON (`unblockedBy`).

**Fail→pass, 30 apps.**

| Change | Apps (run result) |
|---|---|
| Round 50 imaging-codecs (`2063d423`): pixel model, `BitmapFrame`, the six codecs, `BitmapMetadata`, `BitmapCacheOption`/`BitmapCreateOptions`, palettes and indexed `PixelFormats`, `BitmapImage.Format` | BitmapMetadata, BMPEncoderAndDecoder, GIFEncoderAndDecoder, ImageView, JPEGEncoderAndDecoder, PNGEncoderAndDecoder, TIFFEncoderAndDecoder (run); WDPEncoderAndDecoder (crash) |
| Round 50 flow-document-tables (`adbdfb87`) and text-ranges-typefaces (`d969770b`): `FlowDocument` column/page/typography properties, the `Table` family, `FlowDocumentReader`, `TextRange.Load`/`Save`, `ContentStart`/`ContentEnd`. Round 52 paginated-viewers-printing (`e45d5695`): `PrintDialog` | ClipboardViewer, FlowContentElements, FlowDocumentNewsClient, ParagraphAndHyphenation, TableBuilder, TableRows (all run) |
| Round 51 animation-clocks (`4a1a9325`, landing `gf49a/animation-clocks`): `Clock`, typed animation bases, `BeginAnimation`, DP-backed `TranslateTransform.X/Y` and `SolidColorBrush.Color`, `PropertyPath`, `HandoffBehavior`, `Storyboard.Seek` | CustomAnimation, LocalAnimations, PropertyAnimation, ConcentricRingsDemo, AnimationTiming (with the Round 49 `CreateWidget` fold) (all run); the library wpfclock builds |
| Round 49 startup-build-sweep (`9a38e05e`): MarkupCompiler value conversion, `local:` element fields, `FrameworkElement.Parent`, the `CreateWidget` CS0534 fold, weak `LayoutUpdated`, the prefixed `RoutedEvent` owner | Callbacks, 2DTransforms, CalculatorDemo, KeyFrameAnimation (run); CustomComboBox (crash) |
| Round 49 runtime-xaml-navigation (`8c44b817`): `XamlReader`/`XamlWriter`/`XamlParseException`, `Application.LoadComponent` | SkinnedApplication, MergedResources, FindText (also needs Round 50's `FlowDocumentReader`) (run); ControlsAndLayout (crash) |
| Rounds 49 and 51 together: `Parent`, `XamlWriter`, `BeginAnimation`, `Geometry.Clone` | GeometryDesignerDemo (crash) |
| Rounds 49–52, change not isolated (`CS1501` on a `Dispatcher` overload) | UsingDispatcher (runs) |

**Crash/hang→runs, 7 apps:**

- Round 49 startup-build-sweep, with the Round 51 startup-run-sweep runtime conversion fixes (the two were not
  separated):
  - ShapeElements: string → `DoubleCollection`.
  - CustomWindowUI: string → `Cursor`.
  - ICommandSourceImplementation: markup-extension arguments are trimmed.
  - FocusVisualStyle: `{DynamicResource}` on a `Style`-typed DP is deferred.
- Round 51 animation-clocks: storyboard paths into sub-object values (ClipRegion) and multi-step indexed paths into a
  brush (DrawingBrush).
- Round 51 startup-run-sweep: ContextMenuOpening, one of the two empty-output hangs, now opens its window.

**Error landscape.** Counted as portable failing apps per code, `ccd8dbb1` → now:

- Down: `CS0246` 44 → 25, `CS0103` 23 → 7, `CS1061` 19 → 16, `CS0117` 16 → 11, `CS0234` 16 → 13, `CS0534` 9 → 0,
  `CS1501` 2 → 0.
- Gone: the `CS1002`/`CS1026`/`CS1513`/… syntax family (CustomComboBox), and `NU1201` (BamlReflector restores now).
- Flat: `CS0115` 4 → 4 (overridable `LogicalChildren`, `OnRender`, `OnMouseEnter`/`Leave`/`Up`).
- Up: `CS0029` 2 → 5. The new ones are revealed by earlier fixes: `FontFamily` typed `string` (2 apps), and
  conversions in AnimationExamples, FontDialogDemo and MediaGallery.
- `CS2002` (a generated `.g.cs` specified twice) is a warning in 23 of 159 portable builds (21 of 129 before).

## Regressions

None. No project went pass→fail, and no app went runs→crash/hang.

- **CustomApplicationClass** read as a hang (no window at 10 s) under parallel load. Re-run alone twice, it showed
  its window within 1 s, so it is unchanged (runs).
- **DragDropThumbOps** is still a hang. Round 51 diagnosed it (the MarkupCompiler must register the `x:Class` root in
  `GtkFormsXamlRegistry`), and the fix was carried to Round 52 but is not on `main`.

## API demand

The counts are failing portable apps (45). Windows-only and legacy-csproj demand is excluded. "Apps" is the number of
failing portable apps whose errors touch the theme (from `blockerThemes`, a regex ranking aid); "only" is the number
whose every recorded blocker is in the theme.

| Theme | Symbols (apps) | Apps | Only |
|---|---|---:|---:|
| Controls and media tail | `MediaElement` (3), `UserControl` + `System.Windows.Shell` (WPFGallery), `ListViewItem` (ExpenseItDemo), `RangeBase.ValueProperty` (2), `MenuItem.Padding` (2), `SystemParameters` (2), `Frame.NavigationUIVisibility`/`ContentRendered`/`CurrentSource` (2), `TabItem.IsSelected`, `TabControl.TabStripPlacement`, `Expander.ExpandDirection`, `TreeView.SelectedValuePath`, `WrapPanel.ItemWidth/ItemHeight`, `GridSplitter.ShowsPreview`, `TextBox.TextAlignment`, `ScrollViewer.CanContentScroll`, `Hyperlink.TargetName`, `Popup.AllowsTransparency`, the `DataGrid` tail (`DataGridRow`, `DataGridColumnHeader`, `HeadersVisibility`, `CanUserResize*`), `FontStyleConverter`/`FontWeightConverter` | 14 | 4 |
| 2D graphics, visual layer and input | `RoutedEventArgs.OriginalSource`, `GeneralTransform`, `HitTestResultBehavior` and the hit-test parameter/result types, `CompositionTarget`, `ThicknessConverter`/`BrushConverter`/`RectConverter`, `Mouse.SetCursor`/`Synchronize`, `UIElement.TranslatePoint`, `AddVisualChild`, `Rect.Size`/`Location`, `LinearGradientBrush.ColorInterpolationMode`, the `BitmapEffect` family (`BevelBitmapEffect`, `EdgeProfile`), and the overridable `LogicalChildren`/`OnRender`/`OnMouse*` (`CS0115`) | 12 | 10 |
| 3D | `System.Windows.Media.Media3D` (7): `Viewport3D` (6), `Vector3D` (5), `Quaternion` (4), `Point3D` (2), `GeometryModel3D` (2), `MeshGeometry3D`, `Model3DGroup`, the 3D keyframe types | 9 | 8 |
| Documents and text | `Section`/`Paragraph` `ContentStart`/`ContentEnd`/`BringIntoView`, `FontFamily`-typed `FlowDocument`/`TextElement.FontFamily` (2), `Run.TextProperty` + `SetBinding` (on the held `gf52b/content-bindings-sweep`), `FlowDocument.FlowDirectionProperty`/`SetBinding`, `SystemFonts.Message*Key`, `Paragraph.FontStretch`, `TextDecorationCollection.Freeze`, Fixed/XPS documents (`FixedDocument`, `FixedPage`, `XpsDocument`) | 7 | 5 |
| XAML generator | attached-property binding on the root (`this.FocusManager`, 2), `NavigationWindow.Source` from a string (2), `CS5001` (Concepts) | 5 | 2 |
| Items, data and templates | `ListBox.ScrollIntoView` (3), `ItemCollection` (`Items.CurrentItem`, 2), `ItemContainerGenerator`, `ControlTemplate`/`DataTemplate.FindName`, `FilterEventArgs`, `Binding.UpdateSourceExceptionFilter`, `Binding.XmlNamespaceManager` | 7 | 3 |
| Imaging tail | `CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`, `TransformedBitmap` (2: PhotoStoreDemo, PhotoViewerDemo, which also need controls) | 2 | 0 |
| System.Xaml/BAML tooling | `System.Xaml` (`XamlSchemaContext`, `XamlType`, `Baml2006Reader`, …), `System.Windows.Markup.Localizer` | 2 | 2 |
| Code access security | `PermissionSet`/`PermissionSetAttribute` (EditingExaminerDemo; on .NET these come from the `System.Security.Permissions` package, not WPF) | 1 | 1 |

## Crashes and hangs (built, did not run)

| Sample | Result | Cause |
|---|---|---|
| ControlsAndLayout | crash | `Cannot parse Color from "sc#1.000000, 0.769689, 0.831936, 1.000000"` (a `Grid.Background` setter in `Scene.xaml`). `MediaConverters.ParseColor` has no scRGB (`sc#`) form. Small fix |
| CustomComboBox | crash | `Resource with key 'stringConverter' not found` in `Styles.InitializeComponent`. For a `ResourceDictionary` root the generator stores the keys in a temporary `__owner` `Border`'s `Resources`, but emits `StaticResourceScope.Find("stringConverter", this, this)`, which searches the dictionary itself. Generator fix |
| GeometryDesignerDemo | crash | `Cannot find the element named 'LogoBrush' to animate 'Opacity'`. `Storyboard.TargetName` names an `x:Name`'d `VisualBrush` (a Freezable), and Freezables are not registered in the name scope. Unhandled in a GTK callback after the window loads |
| WDPEncoderAndDecoder | crash | `NotSupportedException: JPEG XR (WDP) has no encoder in the GTK runtime`. The decoder side exists. This is a deliberate limit: gdk-pixbuf/GdkTexture have no JPEG XR codec |
| AppCompat-Quirks | crash | Unchanged: `NullReferenceException` in the sample's `PopulateAppContextInformation`. It reflects over WPF-internal AppContext switch types. A compatibility gap, not worth chasing |
| LocalizationResources | crash | Unchanged: `MissingManifestResourceException` for `LocalizationResources.data.stringtable.resources`. There is only an en-US satellite and no neutral resource |
| DragDropThumbOps | hang | Unchanged: alive with no window and empty output, also alone at 20 s (twice). Diagnosed in Round 51: the `x:Class` root must be registered in `GtkFormsXamlRegistry` by the MarkupCompiler |

## XAML generator errors

These are the remaining generator-side causes in portable apps. Per-project messages are in the JSON (`xamlErrors`).

1. **Attached property with a binding on the root element** (Wizard: 3 pages; DialogBox: 3 dialogs).
   `FocusManager.FocusedElement="{Binding ElementName=…}"` on the root is emitted as
   `this.SetBinding(this.FocusManager.FocusedElementProperty, …)`. The owner type is resolved as a member of `this`
   instead of `System.Windows.Input.FocusManager`. Wizard's Round 49 blockers (`PageFunction<T>`) are gone, so this is
   now its only blocker.
2. **`NavigationWindow Source="Homepage.xaml"`** (GlobalizationHomepage, ExpenseItIntro) is emitted as
   `this.Source = "Homepage.xaml"`. `NavigationWindow` has no `Source` in the layer, and the string needs a `Uri`
   conversion. GlobalizationHomepage fails on nothing else.
3. **`ResourceDictionary` root scope** (CustomComboBox, a crash). See Crashes.
4. **`x:Class` root registration in `GtkFormsXamlRegistry`** (DragDropThumbOps, a hang). See Crashes.
5. **Missing `x:Name` fields for elements of missing types:** `mediaElement` (WindowActivationAndDeactivation, which
   needs only `MediaElement`), `myMediaElement`/`MediaElement1` (MediaGallery), the 3D keyframe names (KeySplineAnimations,
   AnimationExamples), `InteractiveEffect` (BitmapEffectsGallery, a `BevelBitmapEffect`). These follow from the layer
   gaps, not from the generator.
6. **No `Main` (`CS5001`, Concepts).** `App.xaml` is an explicit `ApplicationDefinition` item. Not diagnosed. Concepts
   also needs 7 control and binding members.
7. **Not generator bugs, though surfaced in `.g.cs`:** `TabItem.IsSelected`, `TabControl.TabStripPlacement`,
   `WrapPanel.ItemWidth`/`ItemHeight(Property)`, `GridSplitter.ShowsPreview`, `TextBox.TextAlignment`,
   `Run.TextProperty`, `Frame.NavigationUIVisibility`, `Expander.ExpandDirection`, `TreeView.SelectedValuePath`,
   `SystemFonts.Message*Key`, `DataGridRow`/`DataGridColumnHeader`, and `ConeControl.Content` (the library
   HostingWpfUserControlInWf, from the missing `UserControl`). All are missing layer members.
   - DocumentStructure's `fixedpage1_structure.xaml` is malformed XML, and that app is Windows-only.

## Round 54 lane B, Track 2 part A: Media3D (`gf54b/media3d`)

Adds `System.Windows.Media.Media3D` from nothing, plus the key-frame animation types it needs. Measured 2026-11-22
on the branch with `Gtk4.Forms.Wpf 1.0.0.26326` packed from the worktree into a lane-private feed (same recipe as
Method). All 8 target samples were built and the ones that built were smoke-run.

**What exists now**

- Math structs: `Point3D`, `Vector3D`, `Size3D`, `Point4D`, `Quaternion`, `Matrix3D`, `Rect3D`, with converters and
  `Parse`. `default(Quaternion)` and `default(Matrix3D)` are identity.
- Object model: transforms, rotations, models, materials, lights, cameras, `Geometry3D`/`MeshGeometry3D`,
  `Visual3D`/`ModelVisual3D`, `Viewport3D`. Freezable collections forward `Changed`, so any change raises one redraw.
- Animation: `KeyTime`, `KeySpline`, `DoubleAnimationUsingKeyFrames` (+ the four key-frame types) and
  `ParallelTimeline.SlipBehavior`. `System.Windows.PointConverter`, `VectorConverter` and
  `System.Windows.Media.MatrixConverter` were added to WindowsBase.
- Rendering: a managed software rasterizer on Cairo inside a `Gtk.DrawingArea` (GSK has no textured-triangle
  primitive, and Cairo lets textures reuse `Brush.ApplyToCairo`).

**Gaps against WPF (documented, not fixed)**

- Hidden surfaces use a back-to-front sort of whole triangles (painter's algorithm), not a per-pixel z-buffer.
  Intersecting triangles can overlap wrongly.
- Lighting is flat per triangle (the mean of the vertex normals), with Blinn-Phong specular.
- Brush textures are mapped with a per-triangle affine matrix, not perspective-correct.
- `Paced` key times are treated as `Uniform`. `ParallelTimeline.SlipBehavior` is stored only.
- Typed 3D animations (`Rotation3DAnimation`, `Point3DAnimation`, `Vector3DAnimation`, `QuaternionAnimation`) are
  deferred: every target sample animates through `DoubleAnimation` on `AxisAngleRotation3D.Angle` or
  `TranslateTransform3D.OffsetX`.
- `MeshGeometry3D.TextureCoordinates` is the existing `System.Windows.Shapes.PointCollection`, a plain list: editing
  it in place does not repaint. Moving `PointCollection` to `System.Windows.Media` also needs the MarkupCompiler's
  `ConverterBackedClasses` entry, which this lane may not edit.
- `Point`/`Vector`/`Matrix` carry no `[TypeConverter]` attribute (their files are not owned by this lane); the new
  converters work when instantiated.
- `FreezableList<T>` is a public base of the 3D collections; WPF has no such type.

**Generator limits found (lane C, recorded, not worked around)**

A probe app with a `Viewport3D` scene in XAML (camera, `ModelVisual3D.Content`, `ModelVisual3D.Transform`, keyed
`MeshGeometry3D`/`DiffuseMaterial` resources) compiles without error, but the generated `InitializeComponent`
silently drops it: `Viewport3D.Camera`, `ModelVisual3D.Content` and `ModelVisual3D.Transform` property elements are
not emitted, only `__e0.Children.Add(new ModelVisual3D())`, and the keyed resources become
`this.Resources["Tri"] = "";`. `x:Name` on a nested freezable (`AxisAngleRotation3D`) is declared as a field but never
assigned, so using it throws `NullReferenceException`. The runtime `XamlReader` path builds the same scene
correctly (`Media3DXamlTests`).

**Target samples**

| Sample | Builds | Runs 10 s with a window | 3D scene paints |
|---|---|---|---|
| Graphics/Converter | yes | yes | n/a (type-conversion demo, no `Viewport3D`) |
| Animation/KeySplineAnimations | yes | no: crashes at startup, `Cannot find the element named 'TimeProgressTransform' to animate 'X'` (storyboard target resolution, lane C) | no |
| Graphics/Geometry | no: 4 x CS1061 (`Frame.ContentRendered`, `Frame.NavigationUIVisibility`) | not run | not run |
| CubeAnimationDemo | no: CS1955 `BeginStoryboard` called as a method; CS1061 `MouseEventArgs.MouseDevice` | not run | not run |
| HexSphereDemo | no: same as CubeAnimationDemo | not run | not run |
| PhotoFlipperDemo | no: same as CubeAnimationDemo | not run | not run |
| ParticlesDemo | no: CS1061 `RenderTargetBitmap.Freeze` | not run | not run |
| GraphingCalculatorDemo | no: 70 x CS1061 (`Rect.Left/Right/Top/Bottom`), 4 x CS1503 (`Point` to `Size` argument) | not run | not run |

None of the build errors in the six failing samples mentions a 3D type. They belong to lane A (`MouseDevice`) and
lane C (`BeginStoryboard`, `Frame`), or are WindowsBase gaps (`Rect.Left`, `Freeze` on `RenderTargetBitmap`).
KeySplineAnimations' 3D key-frame fields compile now, but its fields are never assigned (see above) and its
storyboard does not resolve.

A scratch app that loads the probe scene through the runtime `XamlReader` and paints it through the shipped package
reads back an opaque red pixel in the middle of the lit triangle.

**Samples gate**

All 162 projects with `build=pass` still build (2 builds each, lane-private feed, isolated cache). All 152 apps with
`run=runs` still start with a window: 147 on the first 10 s run, and 5 (EditingCollections,
ValidateItemsInItemsControl, Paths, VideoViewerDemo, WindowSizingOrder) showed no window at 10 s under load from the
other lanes and ran at once when re-run alone for 20 s.

## Next rounds

These are ranked by portable samples unblocked. "Only" means samples whose sole recorded blocker is the track. The
compiler reports nearly all errors in one pass, but a later phase (generated code, a second wall at run time) can
still hold an app back, so treat "only" as an upper bound for build and an estimate for run.

**Not eligible: `System.Windows.Automation.Peers`.** It belongs to `docs/plans/ui-automation.md` (Phases 0–5 merged in
`f977d105`). No portable sample is blocked on it now; the parity scout must not pick it.

1. **2D graphics, visual layer and input: 10 only, 12 involved.**
   - Only: ThicknessConverter, VisualBrush, ImageBrush, LayoutTransitionsDemo, CompositionTarget, DrawingVisual,
     Per-FrameAnimation, DragDropObjects, OverridingLogicalTree, BitmapEffectsGallery. Involved: Brushes and
     ExpenseItDemo (both also need controls).
   - Covers: `RoutedEventArgs.OriginalSource`; `GeneralTransform` and `UIElement.TranslatePoint`; `VisualTreeHelper`
     hit testing (`HitTestResultBehavior`, `PointHitTestParameters`, `GeometryHitTestParameters`/`Result`);
     `CompositionTarget.Rendering`; `ThicknessConverter`, `BrushConverter` and `RectConverter`; `Mouse.SetCursor`/
     `Synchronize`; `AddVisualChild`; `Rect.Size`/`Location`; `LinearGradientBrush.ColorInterpolationMode`; the
     overridable `FrameworkElement.LogicalChildren`, `UIElement.OnRender` and `OnMouseEnter`/`Leave`/`Up`; and the
     obsolete `BitmapEffect` family as no-op types.
   - Owns: `Gtk4.Forms.WindowsBase/RoutedEventArgs.cs`, `Input/Mouse*.cs`, `UIElement*.cs`,
     `LogicalTreeHelper.cs` and the `Controls/*.LogicalChildren.cs` partials (including `ItemsControl`'s, carved out
     of track 6), `Media/VisualTreeHelper.cs`, new `Media/HitTest*.cs`, `Media/GeneralTransform*.cs`,
     `Media/CompositionTarget.cs` and `Media/Effects/*BitmapEffect*.cs`, and new `ThicknessConverter`/`BrushConverter`/
     `RectConverter` files. It must not touch `Markup/XamlValueConverter*.cs`, which belongs to track 3.
2. **Media3D: 8 only, 9 involved.**
   - Only: Converter, Geometry, KeySplineAnimations, CubeAnimationDemo, GraphingCalculatorDemo, HexSphereDemo,
     ParticlesDemo, PhotoFlipperDemo. Involved: AnimationExamples (also needs `Frame` members, track 5).
   - Covers: `Viewport3D`, cameras, lights, `Model3D`/`Model3DGroup`/`GeometryModel3D`, `MeshGeometry3D`,
     `Vector3D`/`Point3D`/`Quaternion`/`Matrix3D`, `Transform3D`/`RotateTransform3D`, the 3D animations and keyframes,
     rendered through GSK 3D transforms or a `Gtk.GLArea`. The largest surface.
   - Owns: new `Media/Media3D/**`, new `Controls/Viewport3D.cs`, new 3D animation files under `Media/Animation/`
     (no edits to existing `Storyboard*.cs`), and the `XmlnsDefinition` line for `System.Windows.Media.Media3D`.
3. **Startup and generator sweep: 4 crash→runs, 2 only builds, 2 partial.**
   - Runs: ControlsAndLayout (scRGB `sc#` colours), CustomComboBox (`ResourceDictionary` root scope),
     GeometryDesignerDemo (Freezables in the name scope), DragDropThumbOps (`x:Class` root registration, the Round 51
     carry-over).
   - Builds: Wizard (root attached-property binding), GlobalizationHomepage (`NavigationWindow.Source` with a string →
     `Uri` conversion). Partial: DialogBox, ExpenseItIntro.
   - Also: diagnose Concepts' missing `Main` (`CS5001`).
   - Gate: all 162 building projects, plus the 7 non-runners.
   - Owns: `Gtk4.Forms.MarkupCompiler/**`, `Markup/XamlValueConverter*.cs` (`MediaConverters` is in
     `XamlValueConverter.Media.cs`), `Markup/GtkFormsXamlRegistry*.cs`, `Navigation/NavigationWindow.cs`,
     `NameScope.cs`, and the
     target-resolution part of `Media/Animation/Storyboard*.cs`.
4. **Documents and text tail: 4 only (5 with XPS), 7 involved.**
   - First, land the held `gf52b/content-bindings-sweep` (`Run.TextProperty`, bindings on flow content,
     `BitmapImage.Metadata`). It unblocks CommonDialogs.
   - Only: CommonDialogs, AnnotatedDocumentViewer (`Section`/`Paragraph` `ContentStart`/`ContentEnd`/`BringIntoView`),
     FontProperties and FlowDocumentProperties (`FontFamily`-typed `FontFamily`), and DocumentMerge if the Fixed/XPS
     model (`FixedDocument`, `FixedPage`, `PageContent`, `XpsDocument`) is in scope.
   - Involved: AnnotationsStyling (also needs `MenuItem.Padding`, `SystemParameters`) and FontDialogDemo (also needs
     items).
   - Owns: `Documents/**`, `FrameworkContentElement*.cs`, `SystemFonts.cs`, new `System.Windows.Xps/**`. The branch
     also edits `Data/BindingEngine*.cs`, `BindingExpression.cs`, `BindingOperations.cs` and `TargetChangeTracker.cs`,
     so **merge it to `main` before the lanes fork**, and drop its stale `docs/samples-coverage.md` hunk.
5. **Controls and media tail: 4 only, 14 involved.**
   - Only: SlidePuzzleDemo, MediaGallery and WindowActivationAndDeactivation (`MediaElement` on
     `Gtk.MediaFile`/`Gtk.Video`, plus `RangeBase.ValueProperty`), and WPFGallery (`UserControl`,
     `System.Windows.Shell.WindowChrome`/`NonClientFrameEdges`).
   - Involved: Brushes, ExpenseItIntro, ExpenseItDemo (`ListViewItem`), AnnotationsStyling, PhotoViewerDemo, Concepts,
     AnimationExamples.
   - Covers: `MenuItem` on `Control` (`Padding`), `SystemParameters`, the `Frame`/`TabControl`/`TabItem`/`Expander`/
     `TreeView`/`WrapPanel`/`GridSplitter`/`TextBox`/`ScrollViewer`/`Hyperlink`/`Popup` members, the `DataGrid` tail,
     and `FontStyleConverter`/`FontWeightConverter`.
   - Owns: new `Controls/MediaElement.cs`, `Controls/UserControl.cs`, `Controls/ListViewItem.cs`,
     `System.Windows.Shell/**`, `Controls/Primitives/RangeBase.cs`, and the existing `Controls/` files of the controls
     named above (not `ItemsControl*`/`ListBox*`), plus `SystemParameters.cs`.
6. **Items, data and templates: 3 only, 7 involved.**
   - Only: BindValidation (`Binding.UpdateSourceExceptionFilter`), DataBindingDemo (`CollectionViewSource.Filter` with
     `FilterEventArgs`), FindingElementsInTemplates (`FrameworkTemplate.FindName`, `ItemContainerGenerator`,
     `ItemCollection.CurrentItem`).
   - Involved: DialogBox, FontDialogDemo, PhotoStoreDemo (`ListBox.ScrollIntoView`), Concepts
     (`Binding.XmlNamespaceManager`).
   - Owns: `Controls/ItemsControl*.cs` (except `ItemsControl.LogicalChildren.cs`), `Controls/ListBox*.cs`, new
     `Controls/ItemCollection.cs` and `Controls/ItemContainerGenerator.cs`, `Templates/FrameworkTemplate.cs`/
     `ControlTemplate.cs`/`DataTemplate.cs`, `Data/Binding.cs`, `Data/CollectionViewSource.cs`.
     It starts after `gf52b/content-bindings-sweep` is on `main`.

**Lanes (three in parallel, two tracks each, file-disjoint):**

| Lane | Tracks | Only | Boundary to watch |
|---|---|---:|---|
| A | 1 (2D graphics, visual layer and input) + 6 (items, data and templates) | 13 | Track 1 owns `UIElement`/`FrameworkElement` virtuals; track 6 owns `ItemsControl`/`ListBox`/`Data` |
| B | 2 (Media3D) + 4 (documents and text, after `gf52b` lands) | 12–13 | Track 2 adds only new files under `Media/Animation/` |
| C | 3 (startup and generator sweep) + 5 (controls and media tail) | 10 | Track 3 owns `Markup/**` and `NavigationWindow`; track 5 owns `Frame` and the other `Controls/` files |

Held branches:

- **`gf52b/content-bindings-sweep` (held): 1 sample only (CommonDialogs), 1 commit on `main`'s Round 50 base.** Merge it
  first (see track 4). Its `Data/` edits overlap track 6.
- **`gf46/input-focus` (held): 0 samples.** Every Input and Commands app builds and all 16 portable ones run. What
  remains on the branch is focus-transaction behaviour, owned by the focus-behaviour design. Rework it there against
  `main`. It moves no sample count and is not in the lanes.
- **`gf39/xaml-content-model` (held, blocked 3 times): 0 samples on its own.** Re-scope it as a test-only change
  (the compile-checked generator test) on top of track 3, which owns `Gtk4.Forms.MarkupCompiler/**`.

Deliberately not next:

- The imaging tail (`CachedBitmap`, `CroppedBitmap`, `FormatConvertedBitmap`, `TransformedBitmap`): 2 apps, and both
  also need controls. Fold it in after tracks 5 and 6.
- JPEG XR encoding (WDPEncoderAndDecoder). There is no codec in the GTK stack.
- System.Xaml/BAML tooling (LocBaml, BamlReflector/BamlTools) and code access security (EditingExaminerDemo).
- Legacy (non-SDK) csproj conversion: 21 projects. That is a converter feature, not a layer gap.

## Method

- **Source.** The local `WPF-Samples` tree, split into six category batches:
  1. Data Binding, Getting Started, Events.
  2. Graphics, Animation, Visual Layer, Speech and Media, Clipboard.
  3. Sample Applications, Migration and Interoperability.
  4. Input and Commands, Documents, Drag and Drop, Compatibility.
  5. Globalization and Localization, Windows, Accessibility, Element Tree, PerMonitorDPI.
  6. Application Management, Elements, Properties, Resources, Styles & Templates, Threading, Tools.

  Each batch was copied under `.claude/samples-remeasure-r52/batchN` without `bin`/`obj`. The scratch root has empty
  `Directory.Build.props`/`.targets`, so `TreatWarningsAsErrors` was off. No original or tracked file was modified.
- **Feed, pinned.** `samples-remeasure-r52/nuget.config` sets the following:
  - An isolated `globalPackagesFolder` (`samples-remeasure-r52/.nuget-packages`, which started empty) and cleared
    `fallbackPackageFolders`.
  - Cleared `packageSources`, then the pinned feed (`samples-remeasure-r52/feed`) and nuget.org.
  - `packageSourceMapping` that maps `Gtk4.*` to the local feed only. Everything else, including `GtkSharp4`, maps to
    nuget.org.
  - Why: nuget.org publishes `Gtk4.Forms.*` with the same date-based versions as local packs.
- **Pinning proven by a probe.** Getting Started/HelloWorld was built with the full recipe from an empty cache.
  - Build 1 exited 1, as expected: the converter gave `MSB3073` (exit 10), then "re-run the build". Build 2 exited 0
    with 0 warnings and 0 errors.
  - The cached SHA-256 values equal the feed's: `Gtk4.Forms.Wpf` `1a7a9dac…29e0dd`, `PresentationFramework`
    `41331669…b38e12`, `WindowsBase` `4a9b2bf3…6c25`, `MarkupCompiler` `66aef909…c236`.
  - Every `Gtk4.Forms.*` `.nupkg.metadata` source is the scratch feed.
  - `project.assets.json` lists only the isolated cache as a package folder.
  - `gtksharp4 4.22.4.26325` came from nuget.org.
  - Every batch re-checked the cached `Gtk4.Forms.Wpf` hash
    (`1a7a9dacf4e01b69896fc59c6212e9a6a1f974e12a6e9b697631b3597829e0dd`).
- **Recipe (strict).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26326`.
  2. Build once with `-nodeReuse:false`. The converter rewrites the csproj and stops with exit 10.
  3. Build again. This result is recorded.
  - Nothing else is changed. Original csproj files are kept beside the copies (`*.csproj.orig`).
  - The six batches ran concurrently against the shared cache.
  - Four projects in batch 3 never reached the converter (`NU1201`/`NETSDK1005`; all Windows-only or legacy). A third
    build changed nothing, so the second build is recorded.
- **Pass/fail.** Pass means `Build succeeded` and `0 Error(s)` in the final build log. Codes, symbols and messages were
  re-parsed uniformly from the final logs of all six batches (252 logs, one per csproj).
- **Run.** Each app that built got a smoke run on Windows of about 10 s, with the GTK runtime
  (`%LOCALAPPDATA%\Gtk\4.22.4\bin`) on `PATH`.
  - "Runs" means alive with a visible top-level window (`EnumWindows`).
  - "Crash" means an unhandled exception (including one printed by a process that stays alive), or an exit with no
    window. Every run's stdout/stderr was scanned for unhandled exceptions; only the 6 crashes have one.
  - "Hang" means alive with no window and no exception.
  - Apps with no window at 10 s were re-run alone for 20 s. The batch 2 script stopped an app once a window had been
    seen after 3 s, so those apps were not kept up for the full 10 s.
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
| 2026-11-22 | `30f46d4a` | 1.0.0.26326 | **yes** | **159 / 204** | **152** | Rounds 49–52 (first two-lane batch) + Round 52 paginated-viewer fixes; full re-measure; 6 crash, 1 hang; no regressions |

## Known layer defects found by the UI tests

- **No command requery after `Loaded`.** `HandlingEventOnCommandUsingCode` adds its button and `CommandBinding` in
  `Loaded`. WPF re-queries commands afterwards, so the button is enabled. The layer calls nothing like
  `CommandManager.InvalidateRequerySuggested` there, so the button stays disabled until another requery happens.
  `CommandSampleTests` keeps a workaround test (it requeries itself) and a skipped test,
  `The_button_is_enabled_without_a_manual_requery`, that asserts the correct behaviour. Un-skip it when fixed.
