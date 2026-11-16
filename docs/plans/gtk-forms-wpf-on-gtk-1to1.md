# Plan — `Gtk.Forms`: run **unmodified WPF** on the Gtk4.Forms GTK engine (aim 1:1)

**Goal.** An existing WPF application — its `System.Windows.*` code, its WPF XAML (the
`…/winfx/2006/xaml/presentation` namespace), its `App.xaml`/`Window.xaml` — compiles and runs on
**Windows, Linux and macOS** against a GTK-backed compatibility layer, with **no namespace changes
and no XAML edits**. The WinForms analogue (`Gtk.Windows.Forms` in `C:\Temp\GtkWindowsForms`) proves
the pattern for an API that has no XAML; this adds the hard part WPF has and WinForms does not — the
XAML stack, the property system, templates, triggers and the binding engine.

## Decisions (answered — do not re-ask)

| Topic | Decision |
|---|---|
| Fidelity target | **Aim for true 1:1 WPF** (the north star; delivered in phases — see below) |
| XAML → objects | **Build-time markup compiler** (like WPF's `PresentationBuildTasks`), not runtime loading |
| Rendering substrate | **Hybrid** — reuse Gtk4.Forms GTK plumbing + control mappings; implement WPF-faithful layout/templating on GtkSharp 4 where the Forms model cannot express it |
| Location | **This repo** (the Xamarin.Forms→GTK fork), next to the engine it depends on |
| Delivery | A one-package drop-in, **`Gtk.Forms.Base`**, reusing the `Gtk4.Forms.Base` pattern just built |

## Reality check — 1:1 is a program, not a task

WPF is ~3 assemblies and thousands of public types (the full property system, routed events, the
logical+visual trees, `Measure`/`Arrange`, templating, triggers, the binding engine, the control
library, media/geometry). Avalonia is the closest existing "WPF-on-everything" and it **deliberately
is not 1:1** because exact parity is enormous. "Aim for 1:1" here means: **the public surface and
semantics are the target, and the work is sequenced so real apps run early and parity grows by
demand.** Nothing below is a weekend; the plan's job is to make the first vertical slice small,
provable, and architecturally load-bearing so the rest is fill-in, not rework.

## The key enabler — why "no XAML / no namespace changes" is possible

1. **Reimplement WPF types in their real namespaces.** `System.Windows.Window`, `System.Windows.Controls.Grid`,
   `System.Windows.Controls.TextBlock`, `System.Windows.DependencyObject`, … live in a compat assembly
   set. A recompile of the WPF app binds `using System.Windows;` to our assemblies (the project drops
   `<UseWPF>`, so the real WPF framework reference — Windows-only anyway — is gone and cannot clash).
2. **Claim the WPF xmlns.** `[assembly: System.Windows.Markup.XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Controls")]`
   (and for `System.Windows`, `System.Windows.Shapes`, `System.Windows.Data`, …). The markup compiler
   resolves the unchanged presentation-namespace XAML to our types through exactly this attribute —
   the same mechanism `Gtk4.Forms.Xaml` already uses for its own xmlns
   (`Gtk4.Forms.Xaml/Properties/AssemblyInfo.cs:10`).
3. **Compile the XAML, don't translate it.** A build task consumes the `Page`/`ApplicationDefinition`
   build items and emits `InitializeComponent` + field wiring, exactly as WPF's markup compiler does —
   so the `.xaml` and the `x:Class` code-behind are untouched.

## Architecture

### A. Assembly & namespace layering (mirror WPF's three layers)

Own assembly names, **real `System.Windows.*` namespaces**. Mirroring WPF's split keeps the layering
honest and the dependency graph acyclic:

| Our assembly (package `Gtk.Forms`) | Mirrors | Holds (namespaces) |
|---|---|---|
| `Gtk.Forms.WindowsBase` | `WindowsBase.dll` | `System.Windows` base (`DependencyObject`, `DependencyProperty`, `PropertyMetadata`, `Freezable`, `DispatcherObject`), `System.Windows.Threading` (`Dispatcher`), `System.Windows.Markup` base, primitives (`Point`/`Rect`/`Size`/`Thickness`/`Duration`) |
| `Gtk.Forms.PresentationCore` | `PresentationCore.dll` | `System.Windows` (`UIElement`, `FrameworkPropertyMetadata`, `RoutedEvent`, input), `System.Windows.Media` (`Brush`, `Pen`, `Geometry`, `Color`, `Visual`), `System.Windows.Input` |
| `Gtk.Forms.PresentationFramework` | `PresentationFramework.dll` | `System.Windows` (`FrameworkElement`, `Window`, `Application`, `Style`, `ResourceDictionary`), `System.Windows.Controls`, `System.Windows.Controls.Primitives`, `System.Windows.Data` (`Binding`), `System.Windows.Shapes`, `System.Windows.Documents` |
| `Gtk.Forms.MarkupCompiler` | `PresentationBuildTasks.dll` | The build task + `Gtk.Forms.props/.targets` (packed, consumer-imported) |

Carrying our own assembly names avoids an identity clash with the real WPF assemblies if a developer
builds on Windows; the `System.Windows.*` namespaces are what the app actually binds to.

### B. The property & event system (`Gtk.Forms.WindowsBase`)

The backbone — every WPF semantic rests on it, so it is Phase-0 work and must be faithful:
`DependencyObject` + `DependencyProperty` (registration, attached properties, value precedence,
`PropertyMetadata` with coercion + change callbacks, **property-value inheritance** down the tree),
`RoutedEvent` (tunnel/bubble) and `Dispatcher` (marshal to the GTK main thread — bridge to
`Device.BeginInvokeOnMainThread`/the GTK main loop). This is a from-scratch reimplementation; it does
**not** reuse Gtk4.Forms' `BindableProperty` (different precedence/inheritance rules), though that code
is a useful reference for the mechanics.

### C. The build-time markup compiler (`Gtk.Forms.MarkupCompiler`) — **fork, don't reinvent**

Fork `Gtk4.Forms.Build.Tasks` and retarget it to the WPF dialect. The whole shape transfers:

| Reuse from `Gtk4.Forms.Build.Tasks` | Role | Retarget to |
|---|---|---|
| `XamlGTask.cs:10` (+ `XamlGenerator.cs`) | Generate `InitializeComponent` + `x:Name`/`FindName` partial | WPF: `x:Class`, `x:Name` fields, `Application`/`Window` roots, `[STAThread] Main` from `ApplicationDefinition` |
| `XamlCTask.cs:17` + the `*Visitor.cs` tree (`CreateObjectVisitor`, `SetPropertiesVisitor`, `ExpandMarkupsVisitor`, `SetNamescopesAndRegisterNamesVisitor`) | Walk the XAML node tree, emit IL (Mono.Cecil) | Resolve `presentation` xmlns via `XmlnsDefinition`; emit `DependencyProperty.SetValue`/setters; WPF markup extensions (`{Binding}`, `{StaticResource}`, `{x:Type}`, `{TemplateBinding}`) |
| `ErrorMessages.resx` / `BuildException` | MSBuild-surfaced XAML errors | same |

It is **System.Xaml-free** (parses XAML via `XmlReader`, emits via Cecil) — important, because
`System.Xaml.dll` ships only in the Windows Desktop pack and is **absent on Linux/macOS**. The task
hooks `Page`/`ApplicationDefinition`/`Resource` build actions so an unmodified WPF `.csproj` layout
compiles.

### D. The rendering bridge (hybrid) — WPF visual tree → GTK

Two tiers, chosen per element:

- **Tier 1 — map onto Gtk4.Forms.** Where a WPF control has a faithful Forms equivalent
  (`TextBlock`→`Label`, `Button`→`Button`, `TextBox`→`Entry`, `Grid`→`Grid`), render it by delegating
  to the existing `Gtk4.Forms.Platform.Gtk` renderer for the mapped Forms element. Reuses `FormsWindow.cs`
  as the top-level `Window` host and `Platform.cs` for renderer creation/layout.
- **Tier 2 — WPF-faithful on GtkSharp 4.** Where the Forms model can't express WPF semantics
  (two-pass `Measure`/`Arrange` with `DesiredSize`, `ControlTemplate`/`DataTemplate`, `Panel`s like
  `DockPanel`/`Canvas` with attached layout props, triggers, visual brushes), implement the element
  directly on a GtkSharp 4 widget with a WPF layout system, hosted in the same GTK window. `UIElement`
  owns a backing `Gtk.Widget`; a custom `Gtk.Widget` measures/arranges children per WPF rules.

The seam is `UIElement.OnRender`/`MeasureCore`/`ArrangeCore` → a GTK widget. Tier 1 is how apps run
early; Tier 2 is where parity is won.

### E. Binding, resources, styles, templates, triggers (`PresentationFramework`)

Reimplement per WPF semantics: the `Binding` engine (`Path`, `Mode`, `Converter`, `RelativeSource`,
`ElementName`, `UpdateSourceTrigger`, `INotifyPropertyChanged`/`DependencyProperty` sources),
`ResourceDictionary` with `{StaticResource}`/`{DynamicResource}` + merged dictionaries, `Style`
(setters/triggers, `BasedOn`, implicit by `TargetType`), `ControlTemplate`/`DataTemplate`,
`Trigger`/`DataTrigger`/`MultiTrigger`/`EventTrigger`. Gtk4.Forms has analogues to study but different
semantics; this is new code.

### F. Drop-in delivery (`Gtk.Forms.Base`)

Reuse the pattern shipped in `Gtk4.Forms.Base` (see `Gtk4.Forms.Base/` + `Gtk4.Forms.ProjectConverter/`):
one `PackageReference` that (1) rewrites the consumer `.csproj` — strip `<UseWPF>`/`net*-windows`,
set `OutputType=Exe`/`net10.0`, reference the `Gtk.Forms.*` assemblies + markup compiler — and (2)
needs no injected `Main` (WPF's `Main` comes from the compiled `ApplicationDefinition`, which the
markup compiler emits). The converter's `CsprojConverter` gains WPF rules alongside its existing ones.

## Reuse map (concrete anchors in this repo)

| Need | Reuse | Anchor |
|---|---|---|
| Markup compiler (codegen) | `XamlGTask` + `XamlGenerator` | `Gtk4.Forms.Build.Tasks/XamlGTask.cs:10` |
| Markup compiler (IL) | `XamlCTask` + `*Visitor.cs` + Mono.Cecil | `Gtk4.Forms.Build.Tasks/XamlCTask.cs:17` |
| xmlns→CLR resolution | `[assembly: XmlnsDefinition]` | `Gtk4.Forms.Xaml/Properties/AssemblyInfo.cs:10` |
| GTK window host | `FormsWindow` | `Gtk4.Forms.Platform.Gtk/FormsWindow.cs` |
| Renderer creation / layout pass | `Platform` | `Gtk4.Forms.Platform.Gtk/Platform.cs` |
| GTK 4 binding + native runtime | `GtkSharp4` (nuget.org, carries the runtime) | `Directory.Build.props` `$(GtkSharp4Version)` |
| One-package drop-in | `Gtk4.Forms.Base` + `ProjectConverter` | `Gtk4.Forms.Base/`, `Gtk4.Forms.ProjectConverter/CsprojConverter.cs` |

## ⛔ Blocking verifications — Phase 0 vertical slice (build before committing to the program)

Prove the architecture end-to-end on the smallest real thing. Until these pass, do not build out the
control library.

| # | Prove | Pass criterion |
|---|---|---|
| B1 | `[assembly: XmlnsDefinition]` + the forked compiler resolves **unmodified** presentation-namespace XAML to our `System.Windows.Controls.*` types | A `<Window><Grid><TextBlock Text="hi"/></Grid></Window>` (verbatim WPF XAML, WPF `x:Class` code-behind) compiles with our markup task — `InitializeComponent` generated, no XAML edits |
| B2 | `DependencyObject`/`DependencyProperty` carries a bound value with inheritance + change notification | A `{Binding}` on `TextBlock.Text` to an `INotifyPropertyChanged` VM updates the widget |
| B3 | The hybrid bridge paints on GTK via the engine | That window shows a GTK label reading "hi" on Windows **and** headless Linux (Xvfb) |
| B4 | The drop-in works | A stock WPF project + `<PackageReference Include="Gtk.Forms.Base">` builds to a cross-platform exe (two-build conversion) and runs |

Phase 0 = `WindowsBase` property system (subset) + the forked markup compiler (minimal) + `Window`/`Grid`/`TextBlock`/`Button` on Tier 1 + a one-way `Binding` + `Gtk.Forms.Base`. One window, one binding, running on two OSes.

## Progress (living)

- **Phase 0 — DONE.** `Gtk.Forms.WindowsBase` (property/event system), `Gtk.Forms.MarkupCompiler`
  (build-time compiler), `Gtk.Forms.PresentationFramework` (`UIElement`→`Window`/`Grid`/`StackPanel`/
  `TextBlock`/`Button` + one-way binding on GtkSharp 4), `Gtk.Forms.ProjectConverter` + `Gtk.Forms.Base`
  (drop-in), and `Gtk.Forms.WpfSample` (unmodified WPF, compiles through the compiler). ~104 tests;
  blocking checks B1–B4 pass. Reviewed (no foundational defect); inheritance-on-reparent, leaks and
  compile-compat fixed.
- **Phase 1 Round 1 — DONE** (binding + controls + layout; **203 tests total**, sample still compiles):
  - *Two-way binding*: `TwoWay`/`OneTime`/`OneWayToSource`, `ConvertBack`, `ElementName`, reentrancy
    guard (`FrameworkElement.Binding.cs` + `TargetChangeTracker`); compiler `{Binding Path=X, Mode=…,
    ElementName=…}` syntax.
  - *Controls*: `TextBox`/`CheckBox`/`RadioButton`/`ComboBox`/`Image`/`Border`/`Label` on real GtkSharp 4
    widgets (`Gtk.Entry`/`Gtk.CheckButton`/`Gtk.DropDown`/`Gtk.Image`/`Gtk.Frame`), two-way where WPF is.
  - *Layout*: `DockPanel`/`Canvas`/`WrapPanel` + attached props (`DockPanel.Dock`, `Canvas.Left/Top`);
    `Grid` row/col `Definitions` + `Grid.Row/Column` attached props; `GridLength`/`GridUnitType`.
    GTK-backing approximations noted (Star/Auto sizing, dock geometry) — faithful `Measure`/`Arrange`
    is a later refinement.
- **Phase 2 Kickoff — DONE** (templating + items controls; **232 tests total**, all pass):
  - `FrameworkTemplate` (abstract) + `ControlTemplate` (`TargetType`, `Factory`) + `DataTemplate`
    (`ItemFactory`) in `Gtk.Forms.PresentationFramework/Templates/`.
  - `ContentPresenter` — hosts `UIElement` directly, applies `DataTemplate`, or falls back to
    string label; backed by `Gtk.Box`.
  - `ItemsControl` — `ItemsSource` (IEnumerable, INCC-reactive), `ItemTemplate`, `Items` snapshot;
    packed in a vertical `Gtk.Box`; extensible via `CreateItemsWidget`/`AddItemWidget`/`RemoveItemWidget`.
  - `ListBox` — single-selection, `SelectedItem`/`SelectedIndex` (bidirectional, pure + GTK),
    `SelectionChanged`; backed by `Gtk.ListBox` + `Gtk.ListBoxRow` containers per item.
  - `ListView` — thin `ListBox` subclass; column/view support is a later phase.
  - `Control.Template.cs` — `Template` DP added to `Control` as a partial file; `OnTemplateChanged`
    builds the visual tree; `ApplyTemplateRoot` is a virtual hook for concrete controls.
  - `SelectionChangedEventArgs` extended via partial to add an `object`-source constructor for
    non-`ComboBox` selection controls.
  - 48 new tests: 34 pure DP + 14 GTK smoke (all native, none skipped on Windows).
- **Phase 1 Round 2 — DONE** (resources + styles, runtime + compiler):
  - `ResourceDictionary` (own keys → `MergedDictionaries` in reverse/last-wins order), lookup walking
    the **logical-parent** chain then `Application.Current.Resources`; `FindResource`/`TryFindResource`.
  - `Style`/`Setter`/`SetterBase`/`BasedOn` (outermost-first, cycle-guarded) + implicit styles keyed by
    `TargetType`; style setters write a dedicated **style-value precedence layer** (below local, above
    inherited) and are **un-applied** when `Style` is replaced or set null.
  - Compiler: `<X.Resources>`/`<ResourceDictionary>` → `Style` construction + `Resources["k"]=…` emitted
    before the element tree; `{StaticResource}`/`{DynamicResource}` on any attribute and on `Style=`
    emit `FindResource` calls. (`{DynamicResource}` resolves at load for now; `Setter.Value` stays a
    string pending typed type-converter resolution — tracked as carry-over.)
- **Round 2 review (Opus) + fixes — DONE.** Independent review found 7 real defects the passing tests
  missed; all fixed with fail-before/pass-after pins: resource walk now follows `LogicalParent`
  (set by `ItemsControl`/`ContentPresenter` on hosted children) not only `InheritanceParent`;
  stored-`null` resources no longer read as absent; `SelectionChangedEventArgs.Source` retyped `object`
  (was a null `ComboBox` cast); `ListBox.SelectionChanged` fires exactly once per transition;
  programmatic `SelectedItem` syncs the GTK row; style precedence below local with un-apply on change;
  `BasedOn` cycle guard + `ReadLocalValue`-based implicit-style guard. **304 tests total** across the
  four suites (35 WindowsBase + 213 PresentationFramework + 40 MarkupCompiler + 16 ProjectConverter),
  all pass; `Gtk.Forms.WpfSample` (unmodified WPF) still compiles through the compiler.
- **Round 3 — triggers + commands/input (DONE).**
  - *Track A — triggers*: `TriggerBase`/`Trigger` (property), `DataTrigger` (binding+INPC), `MultiTrigger`
    (`Condition` set), `EventTrigger` (+`TriggerAction`/`SetPropertyAction`); `Style.Triggers` and
    element-level `FrameworkElement.Triggers`; a **trigger-value precedence layer** above style, below
    local (full order: local > trigger > style > inherited > default), with per-DP contributor stacking
    so sibling triggers on one property don't stomp each other, and un-apply on `Style` change/null.
    Compiler parses `<Style.Triggers>` (all four trigger kinds) → construction + `Triggers.Add`.
  - *Track B — commands/input*: `System.Windows.Input.ICommand` (BCL; no duplicate),
    `RoutedCommand`/`RoutedUICommand`, `CommandBinding`, `CommandManager` (**weak** `RequerySuggested`,
    `InvalidateRequerySuggested`, tree-walking `Execute`/`CanExecute` over `LogicalParent`),
    `ICommandSource`/`CommandSourceHelper`, `UIElement.CommandBindings`, `Button.Command`/`CommandParameter`
    (sensitivity tracks `CanExecute`; deterministic `Dispose` unsubscribes), `MenuItem` (backed by
    `Gtk.Button`), `InputGesture`/`KeyGesture`/`MouseGesture` + `KeyBinding`/`MouseBinding` +
    `UIElement.InputBindings` with an `InvokeInputBinding` dispatch entry point.
- **Round 3 review (Opus) + fixes — DONE.** Review found 2 Critical + High/Medium defects; all fixed with
  pins: multi-trigger same-DP corruption (per-contributor trigger stack, re-resolve on un-apply);
  **strong-ref command leak** (`RequerySuggested` now a weak handler list, pruned on raise) + dead
  `DisposeCommandSource` now wired into `Button`/`MenuItem` `Dispose`; style/trigger layer writes now
  **coerced**; `DataTrigger` restricts to single-segment paths with an explicit contract (throws on nested);
  `SetPropertyAction` writes the trigger layer not a permanent local; command routing walks `LogicalParent`.
  Plus a crash fix found in re-verification: `CommandManager.CanExecute`/`Execute` **infinitely recursed**
  for a `RoutedCommand` with no binding and a null target (unconditional `command.CanExecute` fallback) —
  now a RoutedCommand with no matching binding is simply non-executable (WPF semantics). **368 tests total**
  (40 WindowsBase + 265 PresentationFramework + 47 MarkupCompiler + 16 ProjectConverter), all pass, PF full
  run completes with no stack overflow; `Gtk.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 4 Track B — shapes & media primitives (DONE).** `System.Windows.Media`: `Color` (ARGB bytes +
  scaled floats, `ToGdkRgba`/`ToCairoColor`), `Colors`/`Brushes` (named statics), `Brush` (abstract,
  `Opacity`) + `SolidColorBrush` (`ToGdkRgba` + Cairo `SetSourceRGBA`), `GradientBrush`/`LinearGradientBrush`/
  `RadialGradientBrush` (`GradientStopCollection`; full Cairo `LinearGradient`/`RadialGradient` paint),
  `Pen` (`Brush` + `Thickness`), `Geometry` base + `RectangleGeometry`/`EllipseGeometry`/`LineGeometry`/
  `PathGeometry` (`PathFigure`/`LineSegment`/`BezierSegment`; `Bounds` on all four), `Transform` base +
  `TranslateTransform`/`ScaleTransform`/`RotateTransform`/`MatrixTransform` + `Matrix` (multiply,
  transform-point, `IsIdentity`, `ToCairo`).  `System.Windows.Shapes`: `Shape : FrameworkElement`
  (`Fill`/`Stroke`/`StrokeThickness`/`Stretch` DPs; `Gtk.DrawingArea` backing; `SetDrawFunc` Cairo
  callback; deterministic `Dispose`), `Rectangle` (`RadiusX`/`RadiusY`; rounded-rect Cairo path),
  `Ellipse`, `Line` (`X1/Y1/X2/Y2`), `Polygon`/`Polyline` (`PointCollection`, `FillRule`), `Path`
  (`Data` = `Geometry`).  71 new tests (18 Color/Brushes, 9 Geometry bounds, 15 Transform matrix math,
  20 Shape DP defaults/round-trips, 10 GTK smoke). **439 tests total** (40 WindowsBase + 336
  PresentationFramework + 47 MarkupCompiler + 16 ProjectConverter), all pass.
  *Gradient rendering limitation*: `LinearGradientBrush`/`RadialGradientBrush` use full Cairo gradient
  paint when `DrawFunc` is active; `ToGdkRgba` returns the first stop colour as a widget-hint fallback
  (noted limitation, full GTK CSS gradient is not wired). `Stretch` is stored as a DP but the
  Cairo path always paints to the full `DrawingArea` size (stretch transform is a Phase-5 addition).
- **Round 4 Track A — navigation, windows & dialogs (DONE).** (65 new PF tests):
  - `System.Windows.Controls.Page` (`FrameworkElement` subclass, `Content`/`Title`/`WindowTitle` DPs,
    internal `NavigationService` setter, backed by `Gtk.Box`).
  - `System.Windows.Navigation.NavigationService` (cursor-based `NavigationJournal` —
    pure, back/forward with forward-trim on `Push`), `Navigate(object)`/`Navigate(Uri)`,
    `GoBack`/`GoForward`, `Navigating`/`Navigated`/`NavigationStopped` events, internal
    `ContentChanged` event, pluggable `IContentResolver`/`DictionaryContentResolver` for URI lookup.
  - `System.Windows.Controls.Frame` (`ContentControl` subclass, `Source` DP, owns `NavigationService`,
    swaps `Gtk.Box` child on `ContentChanged`).
  - `System.Windows.Navigation.NavigationWindow` (`Window` subclass, owns `NavigationService`,
    wires `ContentChanged` to set `Content` and push `Page.WindowTitle` to `Title`,
    exposes `CanGoBack/CanGoForward`/`GoBack`/`GoForward`/`Navigate` pass-throughs).
  - `System.Windows.MessageBox` (static `Show(…)` overloads, injectable `IMessageBoxPresenter`,
    `GtkMessageBoxPresenter` using deprecated-but-available `Gtk.MessageDialog` with `GLib.MainLoop`
    synchrony, `MessageBoxButtonMapping` pure static — `ButtonLabels`/`ResultFromLabel`/`DefaultResult`).
  - `Window.Owner`/`Window.DialogResult`/`Window.ShowDialog()`/`Window.Close(bool?)` — modal semantics
    via nested `GLib.MainLoop` (GTK 4 equivalent of `gtk_dialog_run`).
  - **Root cause of the long investigation**: `Gtk.Window` exists in GtkSharp4; inside namespace
    `Gtk.Forms.PresentationFramework.Tests` Roslyn resolves bare `Window` to `Gtk.Window` (namespace
    chain lookup beats using-directive lookup). Fix: `using WpfWindow = System.Windows.Window;` in
    affected test files, matching the pattern already used in `GtkSmokeTests.cs`.
  - New tests: 10 `NavigationJournalTests` + 13 `NavigationServiceTests` + 7 `NavigationWindowTests`
    + 7 `FrameGtkSmokeTests` + 22 `MessageBoxTests` (button mapping) + 6 `WindowDialogTests`.
  - *Fix applied during integration*: `Window.ShowDialog()` originally ran a **blocking** nested
    `GLib.MainLoop` and `WindowDialogTests` quit it from a `Task.Run` thread — calling GTK off the single
    GTK thread **segfaulted the test host** (and would deadlock the harness regardless). `ShowDialog` now
    **pumps** `Gtk.Application.RunIteration(true)` on the GTK thread until `Close(bool?)` sets a flag, and
    the tests schedule `Close` via a same-thread `GLib.Timeout`. GTK must only be touched on its own thread.
- **Round 4 review (Opus) + fixes — DONE.** Review found 2 Critical latent host-hangs + High/Medium defects;
  all fixed with pins: `Window.Close()` (no-arg) and a window-manager/`CloseRequest` close now set the
  closed flag so `ShowDialog` can't spin forever; the default `GtkMessageBoxPresenter` switched from a
  blocking nested `GLib.MainLoop` to the same-thread `RunIteration` pump; **MessageBox button→result
  mapping made total** (distinct per-button response id → label → `ResultFromLabel`; previously every click
  returned the default); rounded `Rectangle`/`RectangleGeometry` corners use `rx` horizontally and `ry`
  vertically (were both `ry`); `GoBack`/`GoForward` now raise the `Navigating`/`NavigationStopped` pipeline
  with `NavigationMode` and honour `Cancel`; `Color.Sc*` documented as normalized bytes (not linear-light
  scRGB). **Round 4 totals: 556 tests** (40 WindowsBase + 453 PresentationFramework + 47 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample` (unmodified WPF) still
  compiles.
- **Round 5 Track B — common WPF controls (DONE).** `HeaderedContentControl` (base for `Expander`/`GroupBox`/`TabItem`);
  `ScrollViewer` (`Gtk.ScrolledWindow`, `HorizontalScrollBarVisibility`/`VerticalScrollBarVisibility` → `PolicyType`);
  `TabControl`/`TabItem` (`Gtk.Notebook`; `SelectedIndex`/`SelectedItem`/`SelectionChanged`, two-way with reentrancy guard);
  `Expander` (`Gtk.Expander`; `IsExpanded` two-way via `AddNotification("expanded",…)`, `Expanded`/`Collapsed` events);
  `GroupBox` (`Gtk.Frame` with label-widget header); `Slider` (`Gtk.Scale`; `Minimum`/`Maximum`/`Value` clamped via
  `CoerceValueCallback` lambda, `Orientation`, `ValueChanged`, two-way); `ProgressBar` (`Gtk.ProgressBar`;
  `Minimum`/`Maximum`/`Value` as fraction, `IsIndeterminate` → `Pulse`); `Separator` (`Gtk.Separator`, `Orientation`);
  `ToggleButton` (`Gtk.ToggleButton`; `IsChecked` two-way, `Checked`/`Unchecked`; derives from `ContentControl`
  — no `ButtonBase` in this phase). **66 new tests** (40 pure DP + 26 GTK smoke). **Round 5 totals: 519
  PresentationFramework** (622 total across all suites); all pass, no hang/crash on Windows.
  *GTK note*: `Gtk.ScrolledWindow.Child` wraps the content in a `Gtk.Viewport` automatically — tests assert
  `viewport.Child` not `sw.Child` directly.
- **Round 5 Track A — typed attribute values + type-converter system (DONE).**
  - `System.Windows.Markup.XamlValueConverter` (central runtime registry in `PresentationFramework`):
    `Convert(ownerType, dp, rawString)` dispatches on `dp.PropertyType`; `Register<T>` for custom converters.
  - Built-in converters: `Thickness` (1/2/4-part), `CornerRadius` (1/4-part), `GridLength`
    (`Auto`/`*`/`2*`/px), `Color` (named via `Colors` statics + `#RGB`/`#ARGB`/`#RRGGBB`/`#AARRGGBB`),
    `Brush`/`SolidColorBrush` (named + hex), `Point`/`Size`/`Rect` (comma-separated pairs/quads),
    `double`/`float`/`int`/`long`/`bool`, arbitrary `Enum` (case-insensitive), `string` passthrough.
    `PrimitiveConverters` (internal, WindowsBase) + `MediaConverters` (internal, PresentationFramework).
  - `CornerRadius` struct added to `Gtk.Forms.WindowsBase`.
  - `Visibility` enum added to `Gtk.Forms.PresentationFramework`.
  - `Setter.CoerceValue`: string values converted to target DP type at apply time (style, trigger, direct).
  - Compiler (`XamlCodeGenerator`): plain DP attributes emit
    `SetValue(T.PropProperty, XamlValueConverter.Convert(typeof(T), T.PropProperty, "raw"))` instead of
    bare string literals; CLR-only properties (`Title`, `StartupUri`, `Name`, `Text`) keep the existing
    direct-assignment path. `{Binding}`, `{StaticResource}` still take their own paths (unchanged).
  - Compiler emits `XamlValueConverter.Convert(...)` for plain DP attributes; CLR-only props keep direct
    assignment; `{Binding}`/`{StaticResource}` unchanged. (+29 PF, +8 MarkupCompiler tests.)
- **Round 5 defect fixes (Opus review G1–G4, DONE).**
  - **G1** (`Slider.ValueChanged` silent on GTK drag): `OnValueChanged` now raises `ValueChanged`
    unconditionally; `_updating` guards only the GTK↔DP write loop, not the event.
  - **G2** (`TabControl.SelectionChanged` fires twice): all three entry points (`SelectedIndex`,
    `SelectedItem`, GTK page switch) now raise `SelectionChanged` exactly once via a single-choke-point
    `_updating` guard; `OnSelectedIndexChanged` promoted to full coordinator.
  - **G3** (attached-property attributes emit invalid C#): `EmitChildren` now detects `Owner.Prop`
    dotted attr names and emits `OwnerType.SetProp(el, (castType)XamlValueConverter.Convert(...))`;
    runtime form is the static setter (`Grid.SetRow`, `Canvas.SetLeft`, `DockPanel.SetDock`).
    `InternalsVisibleTo` added to `Gtk.Forms.MarkupCompiler.csproj`.
  - **G4** (`EscapeString` omits `\n`/`\r`/`\t`): all three added. `EscapeString` made `internal`
    for direct unit testing (XML parsers normalize whitespace in attribute values before the generator
    sees them, making XAML-round-trip tests impossible).
  - **+18 PF tests** (3 Slider, 10 TabControl, 5 GTK smoke); **+8 MarkupCompiler tests**
    (12 attached-prop / escape). **Round 5 totals: 682 tests** (40 WindowsBase + 558 PresentationFramework
    + 68 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash;
    `Gtk.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 6 Track B — hierarchical & popup controls (DONE).** Runtime only; no compiler changes.
  - `HeaderedItemsControl : ItemsControl` base class (`Header` DP; base for `ToolBar`/`TreeViewItem`).
  - `MenuItem` extended: `Items` (sub-menu list; parent items backed by `Gtk.MenuButton`+`Gtk.Popover`,
    leaf items by `Gtk.Button`), `IsCheckable`/`IsChecked` (toggle, `Checked` event), `InputGestureText`.
  - `Menu : ItemsControl` — horizontal `Gtk.Box` of top-level `MenuItem`s.
  - `ContextMenu : ItemsControl` — `Gtk.Popover`+vertical `Gtk.Box`; `IsOpen` (two-way, widget-creation-
    gated), `Placement`; `FrameworkElement.ContextMenu` attached/instance DP.
  - `ToolTip`/`ToolTipService` — `FrameworkElement.ToolTipProperty` attached DP (deferred to widget
    creation via `UIElement.OnWidgetCreated` hook); `ToolTipService` static accessor. `UIElement` gains
    `IsWidgetCreated` and `OnWidgetCreated(Gtk.Widget)` virtual hook.
  - `ToolBar : HeaderedItemsControl` — horizontal `Gtk.Box`.
  - `TreeViewItem : HeaderedItemsControl` — `Gtk.Expander`+vertical `Gtk.Box` children; `IsExpanded`
    two-way (`AddNotification("expanded",…)`), `IsSelected` (DP, `Selected`/`Unselected` events).
  - `TreeView : ItemsControl` — vertical `Gtk.Box` of `TreeViewItem`s; `SelectedItem` (two-way,
    deselects previous), `SelectedItemChanged`; wires `TreeViewItem.Selected` to sync.
  - `Popup (System.Windows.Controls.Primitives)` — `Gtk.Popover`; `IsOpen` (two-way, widget-creation-
    gated), `Child`, `PlacementTarget`, `StaysOpen` (→ `Autohide`).
  - **Existing `IsWidgetCreated` in `ContentControl`/`ContentPresenter`/`Panel`** shadowed with `new`
    to suppress CS0108 (pre-existing pattern; equivalent to base).
  - **GTK-widget choices + limitations**: `Menu` uses `Gtk.Box` not `Gtk.PopoverMenuBar` (requires
    `Gio.Menu` model — deferred); `ContextMenu` uses plain `Gtk.Popover` not `Gtk.PopoverMenu`
    (keyboard nav/accelerators deferred); `TreeView` uses nested `Gtk.Expander`/`Gtk.Box` not
    `Gtk.TreeView`/`Gtk.ListView`+`Gtk.TreeExpander` (requires `Gio.ListModel` tree model — deferred).
    `ToolBar` uses `Gtk.Box` not `Gtk.ActionBar` (start/center/end sections deferred).
  - **67 new tests** (44 pure DP + 23 GTK smoke).
- **Round 6 Track A — `<ControlTemplate>`/`<DataTemplate>` XAML compiler support (DONE).**
  - `WpfXmlnsMap` gains `ControlTemplate`, `DataTemplate`, `ContentPresenter`, `ItemsControl`, `ListBox`, `ListView`.
  - `XamlCodeGenerator` refactored: `EmitChildren` split into `EmitElement` (reusable per-element construction)
    + `EmitPropertyElement` (dispatches dotted property-element children); property-element `<Button.Template>`,
    `<ListBox.ItemTemplate>`, `<ContentControl.ContentTemplate>` etc. emit templates inline.
  - `EmitResources` handles keyed `<ControlTemplate x:Key="k">` and `<DataTemplate x:Key="d">` entries.
  - `EmitControlTemplateInline`: constructs `ControlTemplate(typeof(T))`, sets `Factory = () => { … return root; }`
    using the same `EmitElement` codegen as the page body.
  - `EmitDataTemplateInline`: constructs `DataTemplate()`, sets `ItemFactory = __item => { … DataContext = __item; return root; }`.
  - `Template="{StaticResource k}"` resolves via the existing `{StaticResource}` → `SetValue`/`FindResource` path; confirmed correct for template objects.
  - `ContentControl` made `partial`; `ContentControl.ContentTemplate.cs` adds `ContentTemplateProperty` DP +
    `ApplyContentTemplate` to swap the hosted widget when the template changes.
  - `DependencyProperty.FindByName` now calls `RuntimeHelpers.RunClassConstructor` for each type in the chain
    before searching (fixes `beforefieldinit` types whose static DP registrations weren't guaranteed to run before
    `FindByName` was called — caused `Setter_StringValue_UniformThickness_Applied` to fail silently).
  - **+19 MarkupCompiler tests** (`XamlCodeGeneratorTemplateTests`): inline `<Button.Template>` / keyed
    `<ControlTemplate x:Key>` / `<DataTemplate>` as `ItemTemplate` / `{Binding}` inside template / `{StaticResource}` on `Template`.
  - **+11 PF pure tests** (`TemplatingXamlDpTests`): `ContentControl.ContentTemplateProperty`, `ControlTemplate` factory, `DataTemplate` factory with `DataContext` wiring, keyed resource resolution.
- **Round 6 totals (both tracks merged): 779 tests** (40 WindowsBase + 636 PresentationFramework +
  87 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Carry-over (Round 5 Opus review — not addressed this round)**: compiler DP-vs-CLR resolution is a
  name heuristic (`IsKnownDp`: "is it in `System.Windows.*` and not on the CLR-only exclusion list?").
  Should be replaced with real assembly symbol resolution — load the referenced assemblies, reflect
  the declared `<Name>Property` fields, and distinguish DP from CLR at the type level. Filed for a
  future MarkupCompiler phase.
- **Carry-over**: attribute-driven `[XmlnsDefinition]` resolution (replace the built-in xmlns map);
  `BindingExpression` live object; `Converter={StaticResource}`; true `{DynamicResource}` change-tracking;
  GTK key-event wiring for `InputBindings` (programmatic `InvokeInputBinding` dispatch exists);
  `Gtk.PopoverMenu` integration for `MenuItem`; element-level `FrameworkElement.Triggers` auto-attach on
  load (runtime exists, compiler emits `Style.Triggers` only); nested-path `DataTrigger` bindings;
  `EventTrigger` storyboards/animations; `TemplateBinding` markup extension inside `ControlTemplate`.

## Phased roadmap (after Phase 0 proves the spine)

1. **Core controls, layout, binding.** `StackPanel`/`DockPanel`/`Canvas`/`WrapPanel` with WPF
   `Measure`/`Arrange` (Tier 2), `TextBox`/`CheckBox`/`RadioButton`/`ComboBox`/`Image`/`Border`,
   two-way binding + converters, `ResourceDictionary` + `StaticResource`, `Style`+`Setter`.
2. **Templating & triggers.** `ControlTemplate`/`DataTemplate`, `ItemsControl`/`ListBox`/`ListView`,
   `Trigger`/`DataTrigger`/`EventTrigger`, implicit styles.
3. **Navigation, windows, dialogs, commands.** `NavigationWindow`/`Page`/`Frame`, `Window` chrome +
   `ShowDialog`, `ICommand`/`RoutedCommand`, `MenuItem`/`ToolBar`.
4. **Media & shapes & text.** `System.Windows.Shapes`, `Brush`/`Geometry`/`Transform`, `FlowDocument`
   subset, `RichTextBox` (hard).
5. **Long tail to 1:1.** The rest of the control set and the WPF features catalogued as app demand
   surfaces; `MultiBinding`, `VisualStateManager`, animation/storyboards, 3D (likely never).

Each phase ships a growing **WPF-sample gallery** (ported real WPF XAML, unmodified) that is the
regression suite.

## Verification

- **Compat gallery**: a project of *unmodified* WPF `.xaml` pages, one per feature, built with the
  markup compiler and run on Windows + Xvfb Linux + macOS, asserted via the GTK renderer-test harness
  (`Gtk4.Forms.Platform.Gtk.UnitTests` is the model).
- **Property-system unit tests** (pure): `DependencyProperty` precedence, inheritance, coercion,
  routed-event tunnel/bubble — fail-before/pass-after, no GTK needed.
- **Markup-compiler unit tests**: given WPF XAML in, assert the generated `InitializeComponent`/IL
  (mirror how `Gtk4.Forms.Xaml.UnitTests` drives `XamlCTask` in-process via `MockCompiler`).
- **Drop-in e2e**: a stock WPF repo + `Gtk.Forms.Base` → cross-platform exe, no source edits.

## Risks

| Risk | Note / mitigation |
|---|---|
| **Scope — this is Avalonia-scale** | Phase 0 de-risks the architecture cheaply; everything after is incremental and demand-driven. Be explicit that "1:1" is a direction, not a date. |
| `DependencyProperty`/visual-tree semantics are subtle | Faithful property system is Phase-0, tested in isolation before anything renders. |
| WPF markup extensions + `System.Xaml` divergence | Compiler is `System.Xaml`-free (Cecil + `XmlReader`), so no Windows-only `System.Xaml` dependency; extension set grows per phase. |
| Tier-1/Tier-2 seam leaks | Keep `UIElement`→`Gtk.Widget` the single boundary; a control can move Tier 1→2 without API change. |
| `ControlTemplate`/triggers/`RichTextBox` | The genuinely hard long tail — scheduled late, flagged as "redesign, not 1:1" if a feature proves intractable on GTK. |
| Fights the active packaging refactor | New `Gtk.Forms.*` projects are additive (own dirs); touch shared `Directory.Build.*` minimally. |

## Recommendation

Approve **Phase 0 only** first: the `WindowsBase` property-system subset + the forked markup compiler
(minimal) + four controls on Tier 1 + one binding + `Gtk.Forms.Base`, proving **B1–B4**. That slice
is small, fully testable, and settles every architectural unknown (xmlns binding, the compiler fork,
the hybrid bridge, the drop-in). Only after it runs a real one-window WPF app unmodified on two OSes
is it worth committing to the multi-phase build toward 1:1.
