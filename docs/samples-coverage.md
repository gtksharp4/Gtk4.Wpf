# WPF samples coverage

Measured 2026-11-19 on `main` at `3b73e4f56142c68a74380ca172c6c760bd80f7d9`, package `Gtk4.Forms.Wpf 1.0.0.26323`,
against the official `microsoft/WPF-Samples` (252 projects: 243 apps and 9 libraries). Per-project results are in
[`samples-coverage.json`](samples-coverage.json); future runs should diff against it.

## Headline

**0 of 206 portable official WPF sample apps build and 0 run unmodified on Gtk4.Forms.**

- 37 more apps (40 projects counting libraries) are **Windows-only by design**: they use WinForms, HWND/Win32 interop,
  the UI Automation client, COM/ActiveX, WinRT, System.Speech, OLE DB or per-monitor DPI manifests. They are excluded
  from the denominator.
- **One package reference changes the picture.** 135 of the 206 portable apps fail on `CS1069`
  (`Properties/Settings.Designer.cs` derives from `System.Configuration.ApplicationSettingsBase`). The WindowsDesktop
  shared framework supplies `System.Configuration.ConfigurationManager`, but Gtk4.Forms.Wpf does not. As a diagnostic,
  that package was added to 56 portable apps. 16 then built: **10 showed a window and 6 crashed at startup**. The other
  ~150 portable apps were not retried, so 16 builds and 10 runs is a lower bound for "add that one package".
- The second wall is a MarkupCompiler bug. `TargetType="{x:Type X}"` is emitted as `typeof(Type X})`, which is a syntax
  error. It blocks 62 portable apps outright.

### Corrections to the raw batch data

The per-batch numbers were checked against the source in `Gtk4.Forms.*`. These were wrong or misleading and are
corrected here and in the JSON:

- **Pass counts mixed two recipes.** The raw aggregate gave "12 build / 6 run". Ten of those builds (batch 1) needed the
  extra ConfigurationManager package, so they are not unmodified passes. They are reported as
  `buildWithConfigurationManager`, the same as the six batch-6 builds that the aggregate had left out.
- **Accessibility/Highlighter is not a layer pass.** It is a legacy net472 csproj that the converter does not touch, so
  it built and ran on .NET Framework (`onLayer: false`). The other aggregate "pass", CustomApplicationClient, is a
  library.
- **Some "missing" types exist in the layer.** The generated code names them without a namespace, so this is a
  generator bug, not missing API:
  - `ListBoxItem` (`System.Windows.Controls.ListBoxItem` exists, but the generator emits `typeof(ListBoxItem)` for
    `<Style TargetType="ListBoxItem">`);
  - `Shape`, `Ellipse`, `Rectangle`, `Polygon` (`System.Windows.Shapes.*` all exist);
  - `ScrollViewer`.
- **`System.Windows.Markup.XamlValueConverter` exists.** It is reported "missing" only by non-retargeted legacy net472
  projects (ADODataSet, SingleInstanceDetection, WinFormsHostScaling, the WindowsFormsHost samples), which compile
  against .NET Framework references. That is a converter gap, not an API gap.
- **`HeaderedContentControl` exists but is `abstract` in the layer.** In WPF it is concrete, which is why the samples
  fail with `CS0144`. The fix is to make it concrete, not to add the type.
- **`Border.Content`, `ListBox.Content`, `Menu.Content`, `TabControl.Content` are not API demand.** WPF has no such
  members. The generator assigns content children to `.Content` instead of honouring each type's content property
  (`Child`, `Items`).
- **The `EditBox.*`, `EditBoxAdorner.*` and `SimpleCircleAdorner.*` entries are the samples' own overrides.** The layer
  gaps behind them are:
  - `UIElement.OnMouseEnter/OnMouseLeave/OnMouseUp` and `FrameworkElement.OnApplyTemplate` are not overridable;
  - `UIElement.OnRender(DrawingContext)` is missing;
  - the layer's public `abstract UIElement.CreateWidget()` leaks into user subclasses (`CS0534`). WPF has no such
    member.
- **The "named colours not supported" crash text was wrong.** `MediaConverters.ParseColor` does resolve names, but
  `System.Windows.Media.Colors` defines 41 of WPF's 141 named colours, so it fails on `Honeydew` and `WhiteSmoke`.
- **"Fails only on CS1069" does not mean "builds after the fix".** In batch 6, 22 apps failed only on `CS1069`. With the
  package added, 6 of them built and 16 surfaced new errors that the compiler had not reported while `CS1069` was
  present.
- `App.StartupUri` / `App.Run` "missing" in FindText was not investigated. `Application` has both, so it is excluded
  from the demand list.

## Per category

"Portable" means apps that are not Windows-only. "+CM" means built or run with the System.Configuration.ConfigurationManager
package added, out of the apps where that was tried.

| Category | Projects | Windows-only apps | Portable apps | Build as-is | Run as-is | +CM tried | +CM build | +CM run |
|---|---:|---:|---:|---:|---:|---:|---:|---:|
| Accessibility | 12 | 10 | 2 | 0 | 0 | 1 | 0 | 0 |
| Animation | 11 | 0 | 11 | 0 | 0 | 0 | 0 | 0 |
| Application Management | 10 | 0 | 9 | 0 | 0 | 8 | 3 | 2 |
| Clipboard | 2 | 0 | 2 | 0 | 0 | 2 | 0 | 0 |
| Compatibility | 1 | 0 | 1 | 0 | 0 | 0 | 0 | 0 |
| Data Binding | 31 | 1 | 30 | 0 | 0 | 25 | 8 | 3 |
| Documents | 17 | 1 | 16 | 0 | 0 | 0 | 0 | 0 |
| Drag and Drop | 6 | 0 | 6 | 0 | 0 | 5 | 0 | 0 |
| Element Tree | 2 | 0 | 2 | 0 | 0 | 1 | 0 | 0 |
| Elements | 10 | 0 | 10 | 0 | 0 | 9 | 3 | 3 |
| Events | 4 | 0 | 4 | 0 | 0 | 0 | 0 | 0 |
| Getting Started | 8 | 0 | 8 | 0 | 0 | 0 | 0 | 0 |
| Globalization and Localization | 14 | 0 | 14 | 0 | 0 | 0 | 0 | 0 |
| Graphics | 25 | 0 | 25 | 0 | 0 | 0 | 0 | 0 |
| Input and Commands | 18 | 1 | 16 | 0 | 0 | 0 | 0 | 0 |
| Migration and Interoperability | 20 | 15 | 0 | 0 | 0 | 0 | 0 | 0 |
| PerMonitorDPI | 4 | 4 | 0 | 0 | 0 | 0 | 0 | 0 |
| Properties | 3 | 0 | 3 | 0 | 0 | 2 | 1 | 1 |
| Resources | 3 | 0 | 3 | 0 | 0 | 3 | 1 | 1 |
| Sample Applications | 23 | 1 | 21 | 0 | 0 | 0 | 0 | 0 |
| Speech and Media | 2 | 1 | 1 | 0 | 0 | 0 | 0 | 0 |
| Styles & Templates | 5 | 0 | 5 | 0 | 0 | 0 | 0 | 0 |
| Threading | 3 | 0 | 3 | 0 | 0 | 0 | 0 | 0 |
| Tools | 3 | 0 | 2 | 0 | 0 | 0 | 0 | 0 |
| Visual Layer | 3 | 1 | 2 | 0 | 0 | 0 | 0 | 0 |
| Windows | 12 | 2 | 10 | 0 | 0 | 0 | 0 | 0 |
| **Total** | **252** | **37** | **206** | **0** | **0** | **56** | **16** | **10** |

The +CM apps that run (window shown) are:

- **Data Binding:** CodeOnlyBinding, CollectionViewSource, CompositeCollections.
- **Properties:** CustomClassesWithDP.
- **Resources:** ApplicationResources.
- **Application Management:** ApplicationShutdown, CodeOnlyWindowsApplication.
- **Elements:** FindingElementInPanel, LoadedEvent, SettingMargins.

## API demand

The counts below are portable projects (apps and libraries) that report the symbol as an error. They cover only the
roughly 70 portable projects whose compile got past the first wall: `CS1069` without the package, or the `{x:Type}`
syntax errors. Demand from the other ~135 is still hidden. Treat these as a floor and a ranking, not a total.
Generator artefacts and samples' own types are excluded (see the corrections above).

| Theme | Symbol | Samples | Examples |
|---|---|---:|---|
| Packaging | `System.Configuration.ApplicationSettingsBase` (System.Configuration.ConfigurationManager) | 135 | LocalAnimations, ApplicationShutdown, ClipboardSpy, FlowContentElements |
| Text controls | `TextBlock.TextWrapping` | 12 | BindingToStringFomat, EditingCollections, DragDropDataFormats, HeightProperties |
| Text controls | `TextBox.VerticalScrollBarVisibility` / `HorizontalScrollBarVisibility` (TextBoxBase) | 5 / 3 | ClipboardSpy, ClipboardViewer, DragDropEvents, DragDropOpenTextFile |
| Text controls | `TextBox.AppendText` | 4 | ClipboardSpy, ClipboardViewer, DragDropDataFormats, DragDropEvents |
| Text controls | `TextBox.ScrollToEnd` | 2 | DragDropDataFormats, DragDropEvents |
| Text controls | `RichTextBox.Horizontal/VerticalScrollBarVisibility` | 1 | ClipboardViewer |
| Items / selection | `Selector.IsSynchronizedWithCurrentItem` | 4 | CollectionBinding, MasterDetail, MasterDetailXml, SortFilter |
| Items / selection | `ListViewItem` | 2 | ExpenseItDemo, EditBoxControlLibrary |
| Items / selection | `CollectionView`, `IEditableCollectionView` | 1 each | Grouping, EditingCollections |
| Items / selection | `ItemsControl.ContainerFromElement`, `BindingGroup.GetValue/TryGetValue`, `Validation` | 1 each | ValidateItemsInItemsControl, ValidateItemSample |
| Input | `KeyboardFocusChangedEventArgs` (+ `OnGotKeyboardFocus` family) | 4 | KeyStrokeCounter, CalculatorDemo, ExpenseItDemo, EditBoxControlLibrary |
| Input | `CheckBox.Click` (ButtonBase.Click); `MenuItem.Click` is `EventHandler`, WPF uses `RoutedEventHandler` | 1 each | DragDropOpenTextFile, FindText |
| Input | `ContextMenuEventArgs`, `Button.FocusVisualStyleProperty` | 1 each | ContextMenuOpening, FocusVisualStyle |
| Dialog semantics | `Button.IsDefault`, `Button.IsCancel`, `ContentPresenter.ContentSource`, concrete `HeaderedContentControl`, settable `Window.DialogResult` | 2 | EditingCollections, ValidateItemSample |
| 3D | `Viewport3D`, `Vector3D`, `Quaternion`, `Point3D`, `Model3D`, `GeometryModel3D` (System.Windows.Media.Media3D) | 4 | CubeAnimationDemo, HexSphereDemo, ParticlesDemo, PhotoFlipperDemo |
| Extensibility | overridable `OnMouseEnter/Leave/Up`, `OnApplyTemplate`, `OnRender(DrawingContext)`; no public abstract `CreateWidget` | 4 | ExpenseItDemo, EditBoxControlLibrary, DragDropObjects, DrawingVisual |
| Layout / visuals | `UIElement.ClipToBounds`, `RowDefinition.ActualHeight`, `GeneralTransform`, `HitTestResultBehavior`, `FrameworkElement.LogicalChildren` override, `DependencyObject.GetLocalValueEnumerator` | 1–2 each | HeightProperties, WidthProperties, DragDropEvents, OverridingLogicalTree, RestoringDefaultValues |
| XAML at runtime | `XamlReader`, `XamlWriter`, `XamlParseException`, `Application.LoadComponent`, settable `Application.Resources` | 4 | FindText, MergedResources, SkinnedApplication |
| XAML at runtime | System.Xaml node stream (`XamlSchemaContext`, `XamlType`, `XamlMember`, `Baml2006Reader`, ...), BAML Localizer | 3 | BamlReflector, BamlTools, LocBaml |
| Documents | `Table`/`TableRowGroup`, `FlowDocument.ContentStart/End`, `TextRange.Load/Save/CanLoad` | 1–2 each | TableBuilder, EditingExaminerDemo, ClipboardViewer |
| Documents | `SpellCheck`, `TextBox.GetSpellingError` | 2 | SpellChecking, CustomDictionaries |
| Documents | `System.Windows.Annotations.*`, `DynamicDocumentPaginator`, `TextEffect` | 1 each | AnnotatedDocumentViewer, FlowDocumentProperties |
| Typography | `Typeface`, `XmlLanguage`, `FontEastAsian*`, `FontNumeralAlignment`, `LanguageSpecificStringDictionary` | 1 | FontDialogDemo |
| Dialogs / clipboard | `Microsoft.Win32.OpenFileDialog`, `SaveFileDialog`, `CommonDialog`; `Clipboard.ContainsAudio/Image/FileDropList` | 1–3 | FindText, ClipboardViewer, CommonDialogs, ClipboardSpy |
| Threading | `DispatcherOperationCallback`, static `Dispatcher.Run` | 2 | ExceptionHandlingSecondaryUIThread, ExceptionHandlingSecondaryWorkerThread |
| Automation peers | `AutomationPeer`, `ButtonAutomationPeer`, `IExpandCollapseProvider`, `PatternInterface` | 2 | CustomComboBox, DataBindingDemo |
| Navigation | `PageFunction<T>`, `ReturnEventArgs<T>`, `NavigationEventArgs` | 1 each | Wizard, EditingExaminerDemo |
| Animation | `Storyboard.Seek`, `TimeSeekOrigin`, `HandoffBehavior` | 1 | wpfclock |
| Data | `ValueConversionAttribute`, `FilterEventArgs`, `DataTransferEventArgs` | 1 | DataBindingDemo, CustomComboBox |

## Crashes (built, did not run)

All six were built with the ConfigurationManager package. Each throws an unhandled exception in `InitializeComponent`
before a window appears.

| Sample | Cause |
|---|---|
| BindConversion | `Resource with key 'MyDataSource' not found`. The resource is registered on the inner StackPanel but looked up via `this.FindResource`. Also `<local:MyData x:Key=...>` is emitted as `Resources["MyDataSource"] = ""`, so the object is lost. |
| BusinessLayerValidation | `Resource 'TextBoxInError' not found`. `{StaticResource}` resolves via `FindResource` on the TextBox before it is added to the tree. |
| PropertyChangeNotification | `Resource 'BidItemDataTemplate' not found`. Same unparented-lookup order. |
| PriorityBinding | `Cannot parse Color from "Honeydew"`. `Colors` has 41 of WPF's 141 names. |
| XmlDataSource | `Cannot parse Color from "Honeydew"`. Same cause. |
| ProcessingCommandLineArguments | `Cannot parse Color from "WhiteSmoke"`. Same cause. |

In short, there are two causes:

- `{StaticResource}` lookups are emitted as `FindResource` calls that run before the element has a parent, and
  custom-object resources are emitted as `""`.
- The named-colour table is incomplete.

## XAML generator errors (regression corpus for `gf39/xaml-content-model`)

The full per-project list is in the JSON (`xamlErrors`). Grouped:

1. **`{x:Type X}` in `Style`/`ControlTemplate` `TargetType`.** Emitted as `new Style(typeof(Type X}))`, giving `CS1026`,
   `CS8124` and a large cascade. **66 projects (62 portable apps).** Typical examples are every Animation and Graphics
   sample, Events, Getting Started, WPFGallery, PhotoStoreDemo, DialogBox, CursorType and TextFormatting. These are
   syntax errors, so they hide every semantic error behind them, `CS1069` included.
2. **Content property ignored.** Children are assigned to `.Content` on `Border` (`Child`), on `ItemsControl`s such as
   `ListBox`, `Menu`, `MenuItem`, `TreeView`, `TabControl` and `ComboBox` (`Items`), and on a `UserControl`-derived
   root. **21 projects**, including ClipboardSpy, BindingDPToDP, Linq, SortFilter, DefiningResources, the Drag and Drop
   and Elements width/height samples, HierarchicalDataTemplate, wpfclock and HostingWpfUserControlInWf.
3. **Unqualified type names.** `TargetType="ListBoxItem"`, `Ellipse` and `Rectangle` become `typeof(ListBoxItem)` with
   no namespace. Attached-property owners `SpellCheck.*` and `ScrollViewer.*` are also emitted unqualified. Projects:
   CollectionBinding, DataTrigger, RestoringDefaultValues, SortFilter, SpellChecking, CustomDictionaries.
4. **Name scopes.** One field is generated per `x:Name` across template name scopes, so DataTemplatingIntro gets a
   duplicate `border` (`CS0102`). DataBindingDemo gets a generated field that collides with an `App` field. Some
   `x:Name` fields are not generated at all: `myThumb`, `FlowDocRdr`, `root`, and the WindowsFormsHost `wfHost`,
   `host1` and `flowLayoutHost`.
5. **Value conversion.**
   - Object-element resources (`<local:Person x:Key=...>`) are emitted as `""`.
   - `Polygon.Points` is emitted as a string assigned to a `PointCollection` (`CS0029`).
   - `xml:space` becomes `TextBox.space`.
   - `DockPanel.DataContextChanged` is emitted as a member assignment, but it is an event.
   - `TextBlock.SetTextAlignment(...)` calls an attached setter that does not exist.
6. **Not a generator bug.** DocumentStructure's `fixedpage1_structure.xaml` is malformed XML (an unterminated `xmlns:my`
   attribute).

## Next rounds

These are ranked by how many portable samples each round would unblock.

1. **Ship System.Configuration.ConfigurationManager with Gtk4.Forms.Wpf.** Add it as a package dependency, or have the
   converter add the PackageReference.
   - Covers: `ApplicationSettingsBase`.
   - Impact: the first wall for 135 of 206 portable apps (94 have no other reported error). Measured +CM yield: 16 of the
     32 tried CS1069-only apps built (10 run). 62 CS1069-only apps were not tried, so expect on the order of 30 more
     builds.
   - It is the cheapest change by far.
2. **MarkupCompiler `{x:Type}` and type qualification.**
   - Covers: parse markup extensions in `TargetType` (Style, ControlTemplate, DataTemplate `DataType`), and emit
     fully-qualified type names for `TargetType` strings and attached-property owners.
   - Impact: unblocks compilation for 62 portable apps plus 5 more. Their real API demand only becomes visible after
     this round, so re-measure afterwards.
3. **XAML content model (`gf39/xaml-content-model`).**
   - Covers: `ContentPropertyAttribute` (`Border.Child`, `ItemsControl.Items`, `UserControl`), template name scopes,
     object-element resources, `PointCollection`/`xml:space`/event members, and `{StaticResource}` resolution against
     the parse-time resource scope rather than `FindResource` on an unparented element.
   - Impact: 21 build blockers and 3 of the 6 crashes. Use the corpus above as its regression tests.
4. **Text controls and startup robustness.**
   - Covers: `TextBlock.TextWrapping`; `TextBoxBase.AppendText/ScrollToEnd/Horizontal/VerticalScrollBarVisibility` on
     TextBox and RichTextBox; `UIElement.ClipToBounds`; the full 141-entry `Colors` table.
   - Impact: 16 build blockers and the 3 colour crashes.
5. **Items, selection and dialog semantics.**
   - Covers: `Selector.IsSynchronizedWithCurrentItem`, `ListViewItem`, `CollectionView`/`IEditableCollectionView`,
     `ItemsControl.ContainerFromElement`, `BindingGroup.GetValue/TryGetValue`, `Button.IsDefault/IsCancel`, concrete
     `HeaderedContentControl`, `ContentPresenter.ContentSource`, settable `Window.DialogResult`, `ButtonBase.Click` on
     CheckBox, and `MenuItem.Click` as `RoutedEventHandler`.
   - Impact: about 12 portable projects, mostly Data Binding.
6. **Input and extensibility surface.**
   - Covers: `KeyboardFocusChangedEventArgs` and the `(Preview)Got/LostKeyboardFocus` overrides; overridable
     `OnMouse*`/`OnApplyTemplate`; `OnRender(DrawingContext)`; hiding `CreateWidget` from user subclasses;
     `DispatcherOperationCallback` and `Dispatcher.Run`; `XamlReader`/`XamlWriter`.
   - Impact: about 12 projects, including the flagship CalculatorDemo and ExpenseIt.

Deliberately not next: Media3D (4 samples, large), Annotations/Spell checking/Typography (1–2 each), the System.Xaml and
BAML tools, and legacy net472 csproj conversion (19 projects, but only 1 portable app).

## Method

- **Source.** The local `WPF-Samples` tree (official microsoft/WPF-Samples), split into six batches by category. Each
  batch was copied under `.claude/samples/batchN`, with empty `Directory.Build.props`/`.targets`/`Packages.props` so
  the repo's `TreatWarningsAsErrors` did not apply. No original or tracked file was modified.
- **Recipe (as-is).**
  1. Add a `PackageReference` to `Gtk4.Forms.Wpf 1.0.0.26323` from the repo `Packages/` feed.
  2. Build once. The converter rewrites the csproj and stops with exit 10.
  3. Build again; this result is recorded.
  - Nothing else is changed.
- **Run.** Each app that built was launched for a smoke run on Windows, rechecked at 25 s because machine load delayed
  window creation.
  - "Runs" means the process stayed alive with a visible top-level window.
  - "Crash" means an unhandled exception or an exit with no window.
  - A run says nothing about visual or behavioural correctness.
- **+CM diagnostic.** Batches 1 and 6 added `System.Configuration.ConfigurationManager 9.0.0` to apps that failed on
  `CS1069` (56 portable apps). Batches 2–5 did not.
- **Windows-only.** Judged from each project's contents (WinForms, `HwndHost`/`HwndSource`, P/Invoke, UIA client,
  COM, WinRT, System.Speech, OLE DB, per-monitor DPI manifest), not from its name. AppCompat-Quirks reflects on real
  WPF internals but is counted as portable.
- **Limits.**
  - Errors mask other errors:
    - The `{x:Type}` syntax errors stop semantic analysis.
    - `CS1069` hid method-body errors in practice.
    - So the API-demand counts are floors.
  - Only Windows was measured; Linux and macOS were not run.
  - `CS2002` (a generated `.g.cs` specified twice) appeared as a warning in about 35 builds. Under the repo's
    `TreatWarningsAsErrors` it would be fatal. It is likely a duplicate `Compile` item from the generator targets, but
    this was not verified.
  - Legacy (non-SDK) csproj files are not converted, so they are reported as failures or as `onLayer: false`.
