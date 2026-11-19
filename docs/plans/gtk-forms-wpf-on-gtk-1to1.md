# Plan — `Gtk4.Forms`: run **unmodified WPF** on the Gtk4.Forms GTK engine (aim 1:1)

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
| Delivery | A one-package drop-in, **`Gtk4.Forms.Wpf`**, reusing the `Gtk4.Forms.Base` pattern just built |

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

| Our assembly (package `Gtk4.Forms`) | Mirrors | Holds (namespaces) |
|---|---|---|
| `Gtk4.Forms.WindowsBase` | `WindowsBase.dll` | `System.Windows` base (`DependencyObject`, `DependencyProperty`, `PropertyMetadata`, `Freezable`, `DispatcherObject`), `System.Windows.Threading` (`Dispatcher`), `System.Windows.Markup` base, primitives (`Point`/`Rect`/`Size`/`Thickness`/`Duration`) |
| `Gtk4.Forms.PresentationCore` | `PresentationCore.dll` | `System.Windows` (`UIElement`, `FrameworkPropertyMetadata`, `RoutedEvent`, input), `System.Windows.Media` (`Brush`, `Pen`, `Geometry`, `Color`, `Visual`), `System.Windows.Input` |
| `Gtk4.Forms.PresentationFramework` | `PresentationFramework.dll` | `System.Windows` (`FrameworkElement`, `Window`, `Application`, `Style`, `ResourceDictionary`), `System.Windows.Controls`, `System.Windows.Controls.Primitives`, `System.Windows.Data` (`Binding`), `System.Windows.Shapes`, `System.Windows.Documents` |
| `Gtk4.Forms.MarkupCompiler` | `PresentationBuildTasks.dll` | The build task + `Gtk4.Forms.props/.targets` (packed, consumer-imported) |

Carrying our own assembly names avoids an identity clash with the real WPF assemblies if a developer
builds on Windows; the `System.Windows.*` namespaces are what the app actually binds to.

### B. The property & event system (`Gtk4.Forms.WindowsBase`)

The backbone — every WPF semantic rests on it, so it is Phase-0 work and must be faithful:
`DependencyObject` + `DependencyProperty` (registration, attached properties, value precedence,
`PropertyMetadata` with coercion + change callbacks, **property-value inheritance** down the tree),
`RoutedEvent` (tunnel/bubble) and `Dispatcher` (marshal to the GTK main thread — bridge to
`Device.BeginInvokeOnMainThread`/the GTK main loop). This is a from-scratch reimplementation; it does
**not** reuse Gtk4.Forms' `BindableProperty` (different precedence/inheritance rules), though that code
is a useful reference for the mechanics.

### C. The build-time markup compiler (`Gtk4.Forms.MarkupCompiler`) — **fork, don't reinvent**

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

### F. Drop-in delivery (`Gtk4.Forms.Wpf`)

Reuse the pattern shipped in `Gtk4.Forms.Base` (see `Gtk4.Forms.Base/` + `Gtk4.Forms.ProjectConverter/`):
one `PackageReference` that (1) rewrites the consumer `.csproj` — strip `<UseWPF>`/`net*-windows`,
set `OutputType=Exe`/`net10.0`, reference the `Gtk4.Forms.*` assemblies + markup compiler — and (2)
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
| B4 | The drop-in works | A stock WPF project + `<PackageReference Include="Gtk4.Forms.Wpf">` builds to a cross-platform exe (two-build conversion) and runs |

Phase 0 = `WindowsBase` property system (subset) + the forked markup compiler (minimal) + `Window`/`Grid`/`TextBlock`/`Button` on Tier 1 + a one-way `Binding` + `Gtk4.Forms.Wpf`. One window, one binding, running on two OSes.

## Progress (living)

- **Phase 0 — DONE.** `Gtk4.Forms.WindowsBase` (property/event system), `Gtk4.Forms.MarkupCompiler`
  (build-time compiler), `Gtk4.Forms.PresentationFramework` (`UIElement`→`Window`/`Grid`/`StackPanel`/
  `TextBlock`/`Button` + one-way binding on GtkSharp 4), `Gtk4.Forms.Wpf.ProjectConverter` + `Gtk4.Forms.Wpf`
  (drop-in), and `Gtk4.Forms.WpfSample` (unmodified WPF, compiles through the compiler). ~104 tests;
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
    (`ItemFactory`) in `Gtk4.Forms.PresentationFramework/Templates/`.
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
  all pass; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles through the compiler.
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
  run completes with no stack overflow; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
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
    `Gtk4.Forms.PresentationFramework.Tests` Roslyn resolves bare `Window` to `Gtk.Window` (namespace
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
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) still
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
  - `CornerRadius` struct added to `Gtk4.Forms.WindowsBase`.
  - `Visibility` enum added to `Gtk4.Forms.PresentationFramework`.
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
    `InternalsVisibleTo` added to `Gtk4.Forms.MarkupCompiler.csproj`.
  - **G4** (`EscapeString` omits `\n`/`\r`/`\t`): all three added. `EscapeString` made `internal`
    for direct unit testing (XML parsers normalize whitespace in attribute values before the generator
    sees them, making XAML-round-trip tests impossible).
  - **+18 PF tests** (3 Slider, 10 TabControl, 5 GTK smoke); **+8 MarkupCompiler tests**
    (12 attached-prop / escape). **Round 5 totals: 682 tests** (40 WindowsBase + 558 PresentationFramework
    + 68 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash;
    `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
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
- **Round 6 review (Opus) + fixes — DONE.** Review found 1 Critical (masked by Contains-only tests) + fixes:
  keyed `<ControlTemplate>`/`<DataTemplate>` in a `.Resources` block was processed twice, emitting an
  uncompilable `this.Resources = …` and dropping sibling templates — `EmitPropertyElement` now returns for
  the `Resources` property-element (owned solely by `EmitResources`); pinned by tests asserting both keyed
  entries present and **no** `Resources =` assignment. Also: `MenuItem` now raises `Checked` only on
  false→true and `Unchecked` on true→false (added `Unchecked`); `MenuItem` split into `MenuItem.cs` +
  `MenuItem.Commanding.cs` (≤200 lines); `FrameworkTemplate.HasContent` and `UIElement.IsWidgetCreated`
  converted from `new`-shadows to `virtual`/`override` so a base-typed reference sees the correct value.
  **Round 6 totals: 787 tests** (40 WindowsBase + 642 PresentationFramework + 89 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 7 Track A — advanced data binding (DONE).**
  - `RelativeSource` (`RelativeSourceMode.Self`, `FindAncestor`+`AncestorType`+`AncestorLevel`,
    `TemplatedParent` declared with limitation, `PreviousData` stub); `Binding.RelativeSource`; engine
    resolves via `LogicalParent`/`InheritanceParent` walk.
  - `MultiBinding` (`Bindings`, `IMultiValueConverter` `Convert`/`ConvertBack`, `Mode`, `StringFormat`);
    `MultiBindingEngine` subscribes to per-child INPC, calls converter, re-pushes on any child change;
    `MultiBindingExpression` live object returned by `GetBindingExpression`.
  - `BindingExpression` live object: `UpdateTarget()`/`UpdateSource()`, `ResolvedSource`, `DataItem`;
    `FrameworkElement.SetBinding(dp, Binding)` returns it; `GetBindingExpression(dp)` returns it.
    Engine refactored: each binding is now backed by a `BindingRecord` + `BindingExpression` pair;
    expression map kept in a `ConditionalWeakTable` per element.
  - `StringFormat` on `Binding`/`MultiBinding` (format with `string.Format(InvariantCulture, fmt, v)`;
    WPF `{0}`-less shortcut handled); `FallbackValue` (applied when source unresolvable);
    `TargetNullValue` (applied when resolved+converted value is null).
  - `UpdateSourceTrigger` (`PropertyChanged` auto write-back; `Explicit` defers until
    `BindingExpression.UpdateSource()` is called; `LostFocus`/`Default` fall back to auto).
  - `BindingBase` updated with `StringFormat`, `FallbackValue`, `TargetNullValue` (inherited by
    both `Binding` and `MultiBinding`).
  - Compiler (`XamlCodeGenerator.Binding.cs` partial): `{Binding RelativeSource={RelativeSource Self}}` →
    `RelativeSource.Self` singleton; `{RelativeSource FindAncestor, AncestorType=X}` → constructor call;
    `{RelativeSource TemplatedParent}` → singleton; `Converter={StaticResource k}` → real
    `(IValueConverter)FindResource("k")` (carry-over TODO removed); `StringFormat=`, `FallbackValue=`,
    `TargetNullValue=`, `UpdateSourceTrigger=` emitted in initializer; `<MultiBinding>` property element →
    `MultiBinding` construction + `Bindings.Add` per child + `SetBinding(dp, mb)`. Fixed `TrimEnd('}')`
    bug (was stripping nested-brace values like `StringFormat=Total: {0}`; now removes exactly the outer
    closing brace). `XamlCodeGenerator` split into `partial` with `XamlCodeGenerator.Binding.cs`
    (binding/resource helpers) to stay ≤200 lines per file.
  - **+16 PF tests** (`AdvancedBindingTests`); **+12 MarkupCompiler tests**
    (`XamlCodeGeneratorBindingR7Tests` + updated `Converter_StaticResource` test in Phase1Tests).
  - **+16 PF + 12 MarkupCompiler tests.**
- **Round 7 Track B — remaining common controls (DONE).** Runtime only; no compiler changes.
  `PasswordBox` (`Gtk.PasswordEntry`; `Password` CLR prop + `PasswordChanged`), `RepeatButton`
  (`Gtk.Button` + timer, repeats `Click`), `Thumb` (`Gtk.GestureDrag`; `DragStarted`/`DragDelta`/
  `DragCompleted`), `GridSplitter` (`Gtk.GestureDrag`), `Viewbox` (`Gtk.AspectFrame`; `Child`/`Stretch`),
  `UniformGrid : Panel` (`Gtk.Grid`; `Rows`/`Columns` auto-computed via `ComputeGrid`), `Calendar`
  (`Gtk.Calendar`; `SelectedDate` two-way, `SelectedDatesChanged`), `DatePicker` (`Gtk.Entry`+`Gtk.MenuButton`
  →`Gtk.Calendar` popover; `SelectedDate`/`Text`), `StatusBar : ItemsControl` (`Gtk.Box`), and a
  **functional-minimal `DataGrid`** (`Gtk.ScrolledWindow`+`Gtk.Grid` of labels; `ItemsSource`, explicit
  `Columns`/`DataGridTextColumn`, `AutoGenerateColumns` by reflection, `SelectedItem` — no editing/sorting/
  virtualization yet). **+65 tests** (42 pure DP + 23 GTK smoke).
- **Round 7 review (Opus) + fixes — DONE.** Review found 1 Critical + 3 High + Mediums, all fixed with
  pins (the defects lived in paths the tests missed — binding-kind swap, DataContext-after-binding,
  RepeatButton press, non-string FallbackValue): **R1** `RepeatButton`'s threadpool `Timer` now disposed on
  teardown + before re-press (was a latent native crash surviving widget teardown); **R2** `SetBinding`
  clears ANY prior binding (simple or multi) on the DP before installing (was leaking the old INPC handler,
  fighting the new binding); **R3** `MultiBinding` re-subscribes on a `DataContext` set after the binding;
  **R4** `FallbackValue`/`TargetNullValue` string values now converted to the target DP type via
  `XamlValueConverter` (were breaking non-string DPs); **R5** Fallback applies on unresolvable path (even
  non-null source), converter runs before `TargetNullValue`; **R6** `MultiBinding Mode=TwoWay` throws
  `NotSupportedException` (was silently one-way); **R7** `UniformGrid` only detaches attached children +
  clamps overflow; **R8** `DataGrid` observes `INotifyCollectionChanged` (split into `DataGrid.cs` +
  `DataGrid.Columns.cs`, ≤200 lines); **R9** MultiBinding child `<Binding>` emits `Converter`/`StringFormat`,
  `Binding.DoNothing` sentinel added. **Round 7 totals: 901 tests** (40 WindowsBase + 741 PresentationFramework
  + 104 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 8 Track B — animation & storyboards runtime (DONE).** `System.Windows.Media.Animation`:
  - `Duration` (finite/Forever/Automatic), `RepeatBehavior` (count/duration/Forever), `FillBehavior` (HoldEnd/Stop).
  - `Timeline` (abstract; `Duration`/`BeginTime`/`AutoReverse`/`RepeatBehavior`/`FillBehavior`) +
    `AnimationTimeline` (abstract; `GetCurrentValue(origin, dest, clock)`).
  - **Animation value layer** added to `DependencyObject` (D10) above local in
    `ResolveEffectiveValue`: `SetAnimationValue(dp,v)` / `ClearAnimationValue(dp)` in
    `Gtk4.Forms.WindowsBase/DependencyObject.Animation.cs` (new partial file).
  - `AnimationClock` — per-timeline-per-target clock; `Tick(TimeSpan)` test seam (no GLib loop required);
    handles `BeginTime`, finite/Forever repeat, `AutoReverse`, `FillBehavior.HoldEnd`/`Stop`, `Pause`/`Resume`/`Stop`.
  - `AnimationScheduler` — single `GLib.Timeout` at ~16ms/60fps; registers/unregisters clocks;
    advances active clocks on the GTK thread and removes completed ones.
  - `IEasingFunction` interface + `EasingFunction` property on `DoubleAnimation`.
  - Concrete animations: `DoubleAnimation` (`From`/`To`/`By`/`EasingFunction`), `ColorAnimation`
    (per-channel ARGB lerp), `ThicknessAnimation` (per-side lerp).
  - `Storyboard : Timeline` — `Children` (`TimelineCollection`); per-child `SetTargetName`/`SetTargetProperty`
    (instance dictionary, reference-equality keyed); `Begin(FrameworkElement)` creates+registers clocks,
    `Stop`/`Pause`/`Resume`; `internal ActiveClocks` for test inspection.
  - `BeginStoryboard : TriggerAction` — `Storyboard` property; `Invoke` calls `Storyboard.Begin(target)`;
    `EventTrigger` already invokes any `TriggerAction` — no changes to `EventTrigger.cs`.
  - **13 new pure tests** (`AnimationTests`): at t=0.5s→5, at t=1s→10/Filling, HoldEnd holds, Stop reverts,
    animation beats local, local returns after Stop, AutoReverse@1.5s, AutoReverse HoldEnd at From, Forever Active,
    ColorAnimation channels, BeginStoryboard fires on EventTrigger, null Storyboard noop.
  - **+13 pure animation tests**; limitation: `Storyboard`-in-XAML (`<Storyboard>` property element,
    storyboard keyed resource) is not compiler-wired yet — runtime (code-behind) use works now.
- **Round 8 Track A — TemplatedParent + TemplateBinding + VisualStateManager (DONE).**
  - `FrameworkElement.TemplatedParent` (internal setter): set on the visual-tree root (and subtree,
    via `UIElement.VisualChildren` walk through `Panel.Children`, `ContentControl.Content`,
    `Border.Child`) when `Control.OnTemplateChanged` applies a `ControlTemplate`. Full subtree
    guaranteed for `Panel`/`ContentControl`/`Border`; documented limitation for custom hosts.
  - `RelativeSource.Resolve` for `TemplatedParent` mode now returns `target.TemplatedParent`
    (was null). `{Binding Path, RelativeSource={RelativeSource TemplatedParent}}` resolves correctly.
  - `TemplateBindingExtension` (runtime): `new TemplateBindingExtension("Prop").ToBinding()` →
    `new Binding("Prop") { RelativeSource = RelativeSource.TemplatedParent, Mode = OneWay }`.
  - Compiler: `IsTemplateBindingExpression` parses `{TemplateBinding Prop}` inside any element
    (including `EmitControlTemplateInline` factory body) and emits the equivalent `Binding` with
    `RelativeSource.TemplatedParent` + `BindingMode.OneWay`; checked before `{Binding}` in both
    `EmitElement` and `EmitRootAttributes`.
  - `VisualStateManager` (functional-minimal): `GoToState(control, stateName, useTransitions)` —
    finds the state in attached groups, raises `VisualStateGroup.CurrentStateChanging`/`CurrentStateChanged`,
    applies `VisualState.Setters` via `Setter.Apply`, records `CurrentState`. Storyboard application
    is a seam (stored in `VisualState.Storyboard`, applied by a future animation track).
    `VisualStateGroup`/`VisualState`/`VisualStateChangedEventArgs` in `System.Windows` namespace.
    `GetVisualStateGroups`/`SetVisualStateGroups` attached-property pattern (dictionary, not DP).
  - **+20 PF tests** (`TemplatedParentTests` — 8 tests; `VisualStateManagerTests` — 12 tests);
    **+13 MarkupCompiler tests** (`XamlCodeGeneratorTemplateBindingTests`).
- **Round 8 review (Opus) + fixes — DONE.** Review found 1 Critical + 2 High + Mediums, all fixed with
  pins driving the real paths the tests bypassed: **T1** `{TemplateBinding}` now resolves on the real
  compiler path — the factory activated template bindings against a still-null `TemplatedParent`, so
  `Control.OnTemplateChanged` now calls `BindingEngine.RefreshTemplatedParentBindings` after stamping the
  subtree (the whole feature was silently inert before); **T2** `VisualStateManager`'s state registry is a
  `ConditionalWeakTable` (was a leaking static `Dictionary<Control,…>`); **T3** `AnimationScheduler` guards
  that `GLib.Timeout` registration runs on the GTK thread (off-thread `Storyboard.Begin` now throws instead
  of corrupting GLib); **T4** `VisualState` setters revert on state exit; **T5** `CurrentStateChanged.OldState`
  reports the previous state; **T6** `Duration.Automatic` resolves to 1 s (a `DoubleAnimation` with no explicit
  Duration now animates); **T7** `{TemplateBinding Prop, …}` uses only the property path + stale doc fixed.
  **Round 8 totals: 961 tests** (40 WindowsBase + 786 PresentationFramework + 119 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 9 Track B — data validation + CollectionView runtime (DONE).** No compiler changes.
  - **Validation**:
    - `ValidationRule` (abstract; `Validate(value, culture) → ValidationResult`), `ValidationResult`
      (`IsValid`, `ErrorContent`), `ValidationError` (`RuleInError`, `ErrorContent`, `Exception`).
    - `Validation` static: read-only `Errors`/`HasError` attached DPs, stub `ErrorTemplate` attached DP,
      `Error` attached routed event (Bubble); `SetErrors`/`ClearErrors` internal helpers.
    - `ExceptionValidationRule` and `DataErrorValidationRule` singletons.
    - `Binding` gains `ValidationRules`, `ValidatesOnDataErrors`, `ValidatesOnExceptions`, `NotifyOnValidationError`.
    - Binding engine: pre-write `ValidationRules` gate; post-write `IDataErrorInfo`/`INotifyDataErrorInfo`
      check (`ValidatesOnDataErrors`); `TargetInvocationException` unwrap for `ValidatesOnExceptions`.
      BCL `IDataErrorInfo`/`INotifyDataErrorInfo` used (net10.0 ships them; no redefinition needed).
  - **CollectionView**:
    - `ICollectionView` interface (`SourceCollection`, `Filter`, `SortDescriptions`, `GroupDescriptions`,
      `Groups`, `CurrentItem`/`CurrentPosition`, `MoveCurrentTo*`, `Refresh`, `CurrentChanged`, `CollectionChanged`).
    - `ListCollectionView` over `IEnumerable`/INCC: applies filter → sort → group; `BuildGroups` (single-level);
      `Refresh` raises `CollectionChanged(Reset)`.
    - `CollectionViewSource` (`Source`/`View` instance; `GetDefaultView(source)` stable cache via
      `ConditionalWeakTable`; returns existing `ICollectionView` unchanged).
    - `SortDescription` struct (BCL has `ListSortDirection` in `System.ComponentModel.TypeConverter`; only
      `SortDescription` is defined here — `ListSortDirection` reused from BCL).
    - `GroupDescription` abstract + `PropertyGroupDescription` (single-property grouping).
    - `CollectionViewGroup` (`Name`, `Items`, `ItemCount`).
    - `ItemsControl` wired: `OnItemsSourceChanged` now calls `CollectionViewSource.GetDefaultView` and
      subscribes to the view's `CollectionChanged`; `RebuildItems` iterates `EffectiveItems` (view or raw source).
  - **18 new tests** (6 validation + 12 CollectionView; all pure, all pass):
    - `ValidationTests`: rule rejects/clears, `IDataErrorInfo` surfaced, `INotifyDataErrorInfo` surfaced,
      `ExceptionValidationRule`, `NotifyOnValidationError` routed event.
    - `CollectionViewTests`: filter hides items, sort ascending/descending, `PropertyGroupDescription` groups,
      `MoveCurrentToNext`/`CurrentItem`, `CollectionViewSource.GetDefaultView` stable cache, `ICollectionView`
      passthrough, `ItemsControl` items reflect filtered+sorted view order, plain collection defaults.
  - **18 new tests** (6 validation + 12 CollectionView).
- **Round 9 Track A — Storyboards in XAML + VSM storyboard-states (DONE).**
  - **Converters**: `Duration` (`"0:0:1"` / `"Forever"` / `"Automatic"`) and `RepeatBehavior`
    (`"Forever"` / `"3x"` / `"0:0:5"`) added to `XamlValueConverter` via `AnimationConverters`
    (new file `Markup/XamlValueConverter.Animation.cs`). `XamlValueConverter.ConvertByType(Type, string)`
    overload added for type-only conversion (no DP required — used by compiler-emitted animation attributes).
    `XamlValueConverter.Convert(ownerType, null, targetType, raw)` overload delegates to `ConvertByType`.
  - **`Storyboard.SetTargetPropertyName`**: new method stores a string property name for lazy DP
    resolution at `Begin()` time (compiler-generated path; owner type unknown at code-gen time).
    `Begin()` falls back to `DependencyProperty.FindByName(propertyName, target.GetType())` when no
    pre-set DP is found.
  - **Compiler — keyed `<Storyboard>` resource**: `EmitResources` now handles `<Storyboard x:Key="k">` items,
    calling the new `EmitStoryboardResource` → `EmitStoryboardInline` routines in
    `XamlCodeGenerator.Storyboard.cs`. Emits: `Storyboard` construction, Timeline properties
    (`Duration` via `XamlValueConverter.Convert`, `RepeatBehavior`, `AutoReverse`, `BeginTime`,
    `FillBehavior`), child `DoubleAnimation`/`ColorAnimation`/`ThicknessAnimation` construction with
    `From`/`To`/`By` CLR properties, `Storyboard.TargetName` → `SetTargetName(anim, name)`,
    `Storyboard.TargetProperty` → `SetTargetPropertyName(anim, propName)`, `Children.Add(anim)`,
    `Resources["k"] = sb`.
  - **Compiler — `<EventTrigger><BeginStoryboard><Storyboard>` in `<Style.Triggers>`**: `EmitEventTrigger`
    now delegates to `EmitEventTriggerFull` (new method in `XamlCodeGenerator.Storyboard.cs`), which
    emits `BeginStoryboard` construction, inline `Storyboard` via `EmitStoryboardInline`, wires
    `.Storyboard = sbVar`, and calls `.Actions.Add(bsVar)`. `EmitStyleTriggers` passes `xNs`
    through to `EmitEventTrigger`.
  - **VSM storyboard wiring**: `VisualState.Storyboard` typed from `object?` to real
    `System.Windows.Media.Animation.Storyboard?`. `VisualStateManager.GoToState` now: stops the
    previous state's storyboard (if any) before transitioning, starts the new state's storyboard
    (if any and `useTransitions=true`) after applying setters. No storyboard is started when
    `useTransitions=false` (WPF semantics — instant snap without animation).
  - **`WpfXmlnsMap`** gains `Storyboard`, `DoubleAnimation`, `ColorAnimation`, `ThicknessAnimation`,
    `BeginStoryboard`.
  - **New tests**: 11 `XamlCodeGeneratorStoryboardTests` (compiler; keyed resource + EventTrigger path);
    15 `StoryboardAnimationTests` (PF; Duration/RepeatBehavior converters + VSM storyboard wiring).
  - **Carry-over (VSM compiler)**: `<VisualStateManager.VisualStateGroups>` / `<VisualStateGroup>` /
    `<VisualState><Storyboard>` XAML compilation is not wired — VSM groups must be built in code-behind.
    Runtime (programmatic) storyboard-based states work. Deferred to a future round.
- **Round 9 totals (both tracks merged): 1008 tests** (40 WindowsBase + 820 PresentationFramework +
  132 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 9 review (Opus) + fixes — DONE.** 2 High + Mediums (regressions the CollectionView wiring and
  string-target storyboards introduced), all fixed with pins on the real runtime path: **U1** `ListBox`/
  `TabControl` **preserve selection** across INCC collection changes (Add/Remove/filter/sort no longer silently
  clear the selection; only a genuinely-removed selected item clears); **U2** a real `UIElement.Opacity` DP is
  registered (→ `Gtk.Widget.Opacity`) so string-`TargetProperty` storyboards actually animate (was a silent
  no-op — `FindByName("Opacity")` returned null); `Storyboard.Begin` now **throws** a clear diagnostic on an
  unresolvable target property; **U3** parenthesized/dotted `TargetProperty` (`(UIElement.Opacity)`) normalized;
  **U4** a plain two-way binding does **zero** validation work (was allocating + writing `Validation.Errors`/
  `HasError` every write-back); **U5** `ListCollectionView` keeps null items (WPF parity); **U6**
  `CollectionViewSource` instance `SortDescriptions`/`GroupDescriptions` forwarded into its `View`; **U7** nits.
  **Round 9 totals: 1024 tests** (40 WindowsBase + 836 PresentationFramework + 132 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 10 Track B — Dispatcher/DispatcherTimer + TextBlock inlines (DONE).** Runtime only; no compiler changes.
  - **`Dispatcher`** extended (`System.Windows.Threading`): `CurrentDispatcher` per-thread singleton, `Invoke(Action)`/`Invoke<T>(Func<T>)` (sync, marshals via `InvokeAction` delegate), `BeginInvoke(Action, DispatcherPriority)` (async via `BeginInvokeAction`), `CheckAccess()`/`VerifyAccess()` (via `IsOnDispatcherThread` predicate), `Thread` property. `DispatcherObject` base in `Threading/DispatcherObject.cs`. `DispatcherPriority` enum (Invalid→Send, 12 values). All bridge delegates replaced with real GLib calls in `Application.Run()`.
  - **`DispatcherTimer`**: `Interval`, `Tick`, `IsEnabled`/`Start`/`Stop`, `Priority`, `Tag`. Backed by pluggable `ScheduleAction`/`RemoveSourceAction` delegates (wired to `GLib.Timeout.Add`/`GLib.Source.Remove` in `Application.Run`). **Test seam**: `FireActionOverride` (internal static) + `FireForTest()` (internal instance) allow firing without a GLib loop. Stop calls `RemoveSourceAction` before clearing `_isEnabled` so no post-stop fire occurs.
  - **Inline hierarchy** (`System.Windows.Documents`): `TextElement` (base; `FontFamily`/`FontWeight`/`FontStyle`/`Foreground`), `Inline` (abstract; `AppendMarkup`), `Run` (`Text`), `Span` (child `InlineCollection`), `Bold`, `Italic`, `Underline`, `LineBreak`, `Hyperlink` (`NavigateUri`/`Click`/`RequestNavigate`). `InlineCollection` (typed, `Changed` event). `PangoMarkup.Escape` (internal, `&`/`<`/`>`/`"`). `FontWeight`/`FontWeights` + `FontStyle`/`FontStyles` structs added to PresentationFramework.
  - **`TextBlock`** extended: `Inlines` (`InlineCollection`, `[ContentProperty]`), `Text` ↔ `Inlines` interplay (setting `Text` creates one `Run`, clears prior inlines; adding inlines clears `Text`). `BuildMarkup()` (internal) assembles Pango markup; `ApplyToLabel()` sets `Gtk.Label.UseMarkup = true` and `Markup` when inlines are rich, plain `Text` otherwise.
  - **44 new tests**: 20 `DispatcherTests` (pure, WindowsBase) + 20 `TextBlockInlineTests` (pure, PF) + 4 `DispatcherTimerGtkSmokeTests` (GTK smoke, PF).
  - **44 new tests** (20 Dispatcher + 20 TextBlock inlines + 4 GTK smoke).
- **Round 10 Track A — real assembly-symbol DP resolution + `x:` markup extensions (DONE).**
  *(This resolves the long-standing Round-5 carry-over: compiler DP-vs-CLR is no longer a name heuristic.)*
  - **`DpSymbolResolver`** (`XamlCodeGenerator.SymbolResolver.cs`): loads the referenced assemblies
    via `Assembly.LoadFrom` (MSBuild task path; avoids `System.Runtime.Loader` which is unavailable on
    `netstandard2.0` target), searches for a public static `<Name>Property` field whose type name
    contains "DependencyProperty", walks the full inheritance chain (inherited DPs + attached DPs on
    base types), caches results in a `ConcurrentDictionary`. Falls back to the previous name heuristic
    when no reference paths are supplied (all existing tests keep passing unchanged).
  - **Reference threading**: `MarkupCompileTask` gains an optional `ReferencePaths` task property
    (`ITaskItem[]`); `Gtk4.Forms.MarkupCompiler.targets` passes `@(ReferencePath)` to it; the task
    calls `generator.SetReferencePaths(paths)` before processing. `XamlCodeGenerator` implements
    `IDisposable` (disposes the resolver's loaded assembly list).
  - **`IsKnownDp` replaced**: now delegates to `_resolver.IsDP(fqType, propName)` instead of the
    pure heuristic; `EmitRootAttributes` made non-static to access the resolver.
  - **`x:Null`** (`IsXNull`): `{x:Null}` on a DP prop → `SetValue(…Property, null)`; on CLR →
    `this.Prop = null`.
  - **`x:Type`** (`IsXType`): `{x:Type Button}` → `typeof(global::System.Windows.Controls.Button)`;
    resolved via `WpfXmlnsMap`; unknown types fall back to as-is.
  - **`x:Static`** (`IsXStatic`): `{x:Static local:Foo.Bar}` / `{x:Static SystemColors.ControlBrush}`
    → `global::Ns.Type.Member`; xmlns prefix stripped; type part resolved via `WpfXmlnsMap`.
  - **`x:Array`** (`TryEmitXArray`): `<x:Array Type="sys:String"><sys:String>a</sys:String>…</x:Array>`
    → `new System.String[] { "a", … }`. Supports `String`, `Int32`, `Int64`, `Double`, `Single`,
    `Boolean`, `Object`, and any type mapped by `WpfXmlnsMap`. Limitation: child elements must be
    simple value wrappers (no nested XAML elements).
  - All x: extensions are dispatched **before** the DP/CLR attribute-value paths and checked for
    `IsKnownDp` to route SetValue vs direct assignment; none interfere with
    `{Binding}`/`{StaticResource}`/`{TemplateBinding}` (each starts with a distinct prefix).
  - **+26 tests**: 9 `XamlCodeGeneratorSymbolResolverTests` (resolver: no-ref fallback ×3, known DP,
    CLR prop, inherited DP, unknown type fallback, generator integration ×2) + 17
    `XamlCodeGeneratorXExtensionTests` (x:Null ×3, x:Type ×5, x:Static ×5, x:Array ×2, dispatch ×2).
  - **+26 MarkupCompiler tests.**
- **Round 10 totals (both tracks merged): 1094 tests** (60 WindowsBase + 860 PresentationFramework +
  158 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 10 review (Opus) + fixes — DONE.** 2 High + Mediums, all fixed with pins: **V1** the
  `DpSymbolResolver` **locked consumer DLLs** (`Assembly.LoadFrom` holds a file lock for the MSBuild-node
  lifetime → later targets fail "file in use" — the lock seen this round) → now `Assembly.Load(File.ReadAllBytes)`
  (lock-free) + an `AssemblyResolve` handler serving dependencies from an in-memory map, with heuristic
  fallback on any load failure; pinned by a test asserting the resolved DLL can be deleted afterward; **V2**
  `{x:Static}`/`{x:Type}` now resolve a `local:` prefix against the element's `xmlns:local="clr-namespace:…"`
  scope and emit the **fully-qualified** `global::MyApp.Type.Member` (were dropping the user namespace → invalid
  C#); **V3** an unresolvable `{x:Type}` raises a clear compiler diagnostic (was silent invalid output); **V4**
  `{x:Null}` on a non-nullable value-type CLR property emits `default(T)` (was `= null` → CS0037); **V5** three
  oversized test files split to ≤200 lines; **V6** non-Bold font weights emit a Pango weight span. **Round 10
  totals: 1108 tests** (60 WindowsBase + 863 PresentationFramework + 169 MarkupCompiler + 16 ProjectConverter),
  all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles through the lock-free
  real-symbol-resolution compiler.
- **Round 11 Track B — FlowDocument + RichTextBox + block documents (DONE).** Runtime only; no compiler changes.
  - **Block hierarchy** (`System.Windows.Documents`): `Block : TextElement` (base; `Margin`/`Padding`/`TextAlignment`/`LineHeight`),
    `Paragraph` (`Inlines` — `InlineCollection`; `AppendMarkup`/`AppendPlainText`), `Section` (`Blocks` — `BlockCollection`; nests blocks),
    `List` (`ListItems`; `MarkerStyle`; bullet/number marker rendering; `ToAlpha`/`ToRoman` helpers), `ListItem` (`Blocks`),
    `BlockUIContainer` (`Child : UIElement`; emits nothing in markup/plain-text — no text representation for a hosted widget).
    `BlockCollection` (typed, `Changed` event; mirrors `InlineCollection` pattern). `TextMarkerStyle` enum (9 values).
  - **`TextAlignment` enum** (`System.Windows`) added to `PresentationFramework` (Left/Right/Center/Justify).
  - **`FlowDocument`** (`[ContentProperty("Blocks")]`; `FontFamily`/`FontSize`/`Foreground`/`TextAlignment`/`PagePadding`):
    `BuildMarkup()` — paragraphs joined by `\n\n`, block `AppendMarkup` per block; `BuildPlainText()` — same with
    `AppendPlainText`; `GetAllText()` (internal) — single-newline joined for buffer loading.
  - **`RichTextBox : Control`** (`System.Windows.Controls`): `Document` DP (`FlowDocument`), `IsReadOnly` DP
    (`→ Gtk.TextView.Editable`), `Selection` (`TextSelection`), `AppendText`; backed by `Gtk.TextView` +
    `Gtk.ScrolledWindow`. `SyncBufferFromDocument` populates the buffer from the document, applying `Gtk.TextTag`s
    for bold/italic/underline. `OnBufferChanged` writes plain text back to the first paragraph (minimal two-way;
    full rich round-trip is a later phase). `TextRange`/`TextSelection` cover the whole document (Phase-1 subset).
  - **`FlowDocumentScrollViewer : Control`** (`System.Windows.Controls`): `Document` DP, read-only; backed by
    `Gtk.ScrolledWindow` + `Gtk.Label` with Pango markup from `FlowDocument.BuildMarkup()`.
  - **GTK-widget choices + limitations**: `Gtk.TextView.Buffer.Insert(TextIter, string)` is a GtkSharp4 compat shim
    (`[Obsolete]`, demoted via `WarningsNotAsErrors CS0618`); the `ref TextIter` overload is the idiomatic API —
    switching deferred (shim is functionally correct). `FlowDocumentScrollViewer` uses `Gtk.Label` (Pango markup) for
    read-only display; a `Gtk.TextView` with read-only tags would be more faithful but is a later refinement.
    Two-way editing (buffer → Document) covers only the plain-text content of the first paragraph; re-parsing
    `Gtk.TextTag` state back into `Bold`/`Italic` runs is explicitly deferred.
  - **39 new tests** (24 `FlowDocumentTests` pure + 9 `RichTextBoxTests` pure + 6 `RichTextBoxGtkSmokeTests` GTK):
    - `FlowDocumentTests`: block hierarchy, two-paragraph plain-text/markup with blank-line separation, bold markup,
      markup escaping, disc/decimal list markers, section nesting, `BlockUIContainer` emits nothing, document property round-trips.
    - `RichTextBoxTests`: `Document` get/set, `AppendText` creates paragraph+run, two `AppendText` → two paragraphs,
      `Selection.Text` returns/sets document text, `TextRange.Text` round-trip, `IsReadOnly` toggle, `GetAllText` join.
    - `RichTextBoxGtkSmokeTests`: `CreateWidget` → `Gtk.ScrolledWindow`, scrolled window contains `Gtk.TextView`,
      `IsReadOnly=true` → `Editable=false`, `Document` syncs buffer text, `FlowDocumentScrollViewer` creates/realizes.
  - **+39 tests** (24 FlowDocument + 9 RichTextBox pure + 6 GTK smoke).
- **Round 11 Track A — faithful WPF two-pass Measure/Arrange layout (DONE).**
  - **`UIElement.Layout.cs`** (new partial): proper `Measure(Size)` / `Arrange(Rect)` with dirty flags
    (`_measureDirty`/`_arrangeDirty`); `RenderSize` property; `ArrangeRect` (internal, for tests);
    `InvalidateMeasure`/`InvalidateArrange`/`UpdateLayout`; `MeasureCore` / `ArrangeCore` virtual seam.
    Old Phase-0 stubs removed from `UIElement.cs`.
  - **`FrameworkElement.Layout.cs`** (new partial): `HorizontalAlignment`/`VerticalAlignment` enums +
    DPs (default `Stretch`); `MinWidth`/`MaxWidth`/`MinHeight`/`MaxHeight` DPs (defaults 0/∞/0/∞).
    `MeasureCore`: deflates available by `Margin`, clamps by explicit `Width`/`Height`/`Min*/Max*`,
    delegates to `MeasureOverride`, adds `Margin` back; `DesiredSize` includes margin.
    `ArrangeCore`: removes margin, computes alignment offset (Left/Center/Right/Stretch × Top/Center/Bottom/Stretch),
    delegates to `ArrangeOverride`, sets `RenderSize` to the aligned content size (not the slot).
  - **Panel `MeasureOverride`/`ArrangeOverride`** in new partial files (pure managed math, no GTK):
    - `StackPanel.Layout.cs`: sum along orientation + max across; stack children sequentially.
    - `Grid.Layout.cs`: two-pass `ResolveColumns`/`ResolveRows` (Absolute → fixed; Auto → 0 initially,
      then `UpdateAutoFromChildren` measures span-1 children and expands; Star → `ResolveStarDistribution`
      divides remaining space by total star weight, proportional to each star value); `BuildOffsets` for x/y;
      child clamped to `[0, count-1]` row/col.
    - `DockPanel.Layout.cs`: subtracts each child's size from the remaining slot in order; last child
      fills if `LastChildFill`.
    - `Canvas.Layout.cs`: `MeasureOverride` returns (0,0); `ArrangeOverride` resolves `Left`/`Top` (or
      `Right`/`Bottom` relative to canvas size) per child.
    - `WrapPanel.Layout.cs`: line-breaking algorithm (accumulate main-axis, wrap when limit exceeded);
      `ArrangeOverride` rebuilds lines and places each child at `(mainOffset, crossOffset)`.
    - `UniformGrid.Layout.cs`: `ComputeGrid(rows,cols,n)` for auto-sizing; equal cell dimensions; child
      at `(i%cols * cellW, i/cols * cellH)`.
    - `Border.Layout.cs`: deflates/inflates by `Padding + BorderThickness`; `ArrangeOverride` insets child.
  - **GTK backing**: native GTK containers (`Gtk.Box`/`Gtk.Grid`/`Gtk.Fixed`/`Gtk.FlowBox`/`Gtk.Frame`)
    still used for rendering. Managed layout runs on top: `Measure`/`Arrange` compute correct
    `DesiredSize`/`RenderSize`/`ArrangeRect` (authoritative for unit tests and future GTK fixed-layout
    replacement); existing GTK smoke tests remain green (no GTK structural changes).
  - **33 new pure tests** (`LayoutContractTests.cs`): `FrameworkElement` Width/Margin/MinWidth/MaxWidth/
    MinHeight/MaxHeight; Stretch/Left alignment; InvalidateMeasure/UpdateLayout; `StackPanel` vertical sum +
    max width + stacked y-offsets + horizontal sum; `Grid` 1*+1* equal split, 2*+1* 2:1 split, Auto column
    sizes to child, Absolute column fixed, Row/Column placement; `DockPanel` Top/Left strips + fill remainder;
    `Canvas` desired=(0,0) + Left/Top placement; `WrapPanel` wrap to new line + 2+1 split + y offsets;
    `UniformGrid` auto 2-col + explicit 2×2; `Border` padding/border-thickness inflate + child inset.
  - **+33 pure Measure/Arrange tests** (`LayoutContractTests`). Note: managed layout is authoritative for the
    contract + tests; native GTK containers still render — wiring the computed arrange rects onto a `Gtk.Fixed`
    is a later refinement (carry-over).
- **Round 11 totals (both tracks merged): 1180 tests** (60 WindowsBase + 935 PresentationFramework +
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk4.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 11 review (Opus) + fixes — DONE.** 2 High correctness bugs in the new layout contract (hidden by
  constant-size test children) + 2 medium WPF-fidelity gaps, implemented faithfully (not just documented):
  **W1** `Measure`/`Arrange` now re-run when the constraint/rect changes even if not dirty (WPF
  `previousAvailableSize` semantics) — Grid's resolved-cell second measure pass was a silent no-op;
  **W2** `DesiredSize` is clamped by Min/Max only, not by `availableSize` (content may exceed its slot —
  a 500-tall StackPanel in a 200 slot now reports 500); **W3** Grid star distribution is **iterative** —
  a star hitting its `Min`/`MaxWidth` fixes at the bound and the surplus redistributes among remaining stars
  (1*+1* in 100 with col0 Max=20 → 20/80); **W4** spanning children grow the `Auto` tracks in their span;
  **W5** `RichTextBox.cs` split into `RichTextBox.cs` + `RichTextBox.Buffer.cs` (≤200 lines); **W6** the
  managed-layout-vs-live-GTK gap documented on `UIElement.UpdateLayout` + carry-over. **Round 11 totals:
  1187 tests** (60 WindowsBase + 942 PresentationFramework + 169 MarkupCompiler + 16 ProjectConverter),
  all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **Round 12 Track A — input system + TextBox completeness (DONE).** No compiler changes.
  - **`UIElement` focus + input** (`UIElement.Input.cs` new partial):
    `Focusable`/`IsFocused` (read-only) DPs; `IsKeyboardFocused` alias; `Focus()` (→ `GrabFocus` when
    realised, deferred flag otherwise); `GotFocus`/`LostFocus` bubble events;
    full keyboard routed events (`PreviewKeyDown`/`KeyDown`, `PreviewKeyUp`/`KeyUp`, tunnel+bubble);
    full mouse routed events (`PreviewMouseDown`/`MouseDown`, `PreviewMouseUp`/`MouseUp`,
    `MouseMove`, `MouseEnter`/`MouseLeave`); `PreviewTextInput`/`TextInput`.
    Event arg types: `KeyEventArgs` (`Key`, `KeyStates`, `Modifiers`, `SystemKey`),
    `MouseEventArgs`/`MouseButtonEventArgs` (`GetPosition`, `ChangedButton`, `ClickCount`),
    `TextCompositionEventArgs` (`Text`); delegate types `KeyEventHandler` / `MouseEventHandler` /
    `MouseButtonEventHandler` / `TextCompositionEventHandler`. `IInputElement` interface.
    Internal raise seam `RaiseKeyDownForTest(Key, ModifierKeys)` / `RaiseKeyUpForTest` for pure tests.
    `InvokeInputBinding` wired into `RaiseKeyDown` so `KeyBinding` Ctrl+S dispatches on key events.
  - **GTK wiring** (`UIElement.GtkInput.cs` new partial): `partial void AttachInputControllersOnCreated`
    called from `UIElement.cs OnWidgetCreated`; wires `Gtk.EventControllerFocus` (Enter/Leave →
    `GotFocus`/`LostFocus` + `IsFocused`), `Gtk.EventControllerKey` (`Gtk.KeyPressedArgs` →
    `PreviewKeyDown`/`KeyDown` + `TextInput`; `Gtk.KeyReleasedArgs` → `PreviewKeyUp`/`KeyUp`),
    `Gtk.GestureClick` (`Gtk.PressedArgs`/`ReleasedArgs` → `MouseDown`/`MouseUp`),
    `Gtk.EventControllerMotion` (`Gtk.MotionArgs`/`EnterArgs`/`Leave` → `MouseMove`/`Enter`/`Leave`).
    **GDK→Key map** (`GdkKeyMap.cs`): `FromKeyval(uint)` covers A-Z, 0-9, function keys, numpad,
    special/navigation keys; `FromGdkModifier(Gdk.ModifierType)` → `ModifierKeys` bitmask.
  - **Statics**: `Keyboard` (`Modifiers`, `IsKeyDown`, `Focus`, `FocusedElement`, internal
    `SetFocused`/`ClearFocused`/`SetKeyDown`/`SetKeyUp`); `Mouse` (`LeftButton`/`RightButton`,
    `GetPosition`, `_lastPosition`); `FocusManager` (`FocusedElementProperty` attached DP,
    `GetFocusedElement`/`SetFocusedElement`).
  - **`TextBox` completeness** (`TextBox.cs` pure model, `TextBox.Widget.cs` GTK backing):
    `AcceptsReturn` (single-line = `Gtk.Entry`; multiline = `Gtk.TextView`+`Gtk.ScrolledWindow`),
    `TextWrapping` (→ `Gtk.WrapMode`), `SelectionStart`/`SelectionLength`/`SelectedText`,
    `CaretIndex`, `MaxLength` (→ `Entry.MaxLength`), `IsReadOnly` (→ `Entry.IsEditable`/`TextView.Editable`),
    `SelectionChanged` event, `Select`/`SelectAll`/`Clear`. Original `Text` two-way binding preserved.
  - **36 new tests** (978 PF total; +36):
    - `InputRoutedEventTests` (16 pure): `KeyDown` bubble child→parent; `PreviewKeyDown` tunnel
      parent→child before `KeyDown`; `Handled=true` stops bubble; PreviewKeyDown Handled stops KeyDown;
      `Focus()`/`IsFocused` managed state; `GotFocus`/`LostFocus`; `Keyboard.Modifiers` updated by seam;
      `Keyboard.IsKeyDown` during handler; `Keyboard.FocusedElement`; `FocusManager.GetFocusedElement`;
      `KeyBinding` Ctrl+S invokes command; `KeyBinding` wrong modifier skipped.
    - `TextBoxPureTests` (16 pure): selection round-trips, `SelectedText` get/set, `Select`/`SelectAll`,
      `SelectionChanged` fires, `CaretIndex` round-trip + clears selection, `MaxLength` default/round-trip,
      `TextChanged` fires + carries source, `Clear` empties, `AcceptsReturn`/`IsReadOnly` defaults.
    - `InputGtkSmokeTests` (5 GTK): `KeyDown` fires on realised element (seam); `Focus()` sets `IsFocused`;
      single-line widget is `Gtk.Entry`; multiline widget is `Gtk.ScrolledWindow`; multiline child is `Gtk.TextView`.
  - **GTK note**: `Gtk.EventControllerFocus.Enter/Leave` have `System.EventHandler` signatures (no args);
    `Gtk.EventControllerKey` uses `Gtk.KeyPressedArgs`/`KeyReleasedArgs` (not nested sub-types); `Gtk.GestureClick`
    uses `Gtk.PressedArgs`/`ReleasedArgs`; `Gtk.EventControllerMotion` uses `Gtk.MotionArgs`/`EnterArgs`/no-arg Leave.
  - **Carry-over**: `Mouse.GetPosition(relativeTo)` returns last tracked position (relativeTo ignored; correct
    relative positioning requires a Gdk.Device query on the GTK thread — deferred); `TextBox.MaxLength` in
    multiline (`Gtk.TextView`) mode is not enforced at the widget level (buffer-change guard deferred).
- **Round 12 Track B — default control templates + theming (DONE).** Runtime only; no compiler changes.
  - `FrameworkElement.DefaultStyleKeyProperty` (protected DP a control sets to `typeof(itself)` in its static
    ctor) + `OverridesDefaultStyle`; `ApplyDefaultStyle()` resolves the lowest-precedence style.
  - `Themes/ThemeManager` — a `Type`-keyed default-`Style` registry; `FindDefaultStyle` walks base types so a
    subclass inherits its base's default style unless it registers its own key.
  - `Themes/Generic.cs` — the code-side `generic.xaml`: seeds default `Style`s for `Button` (with a default
    `ControlTemplate` = `Border`→`ContentPresenter`), `CheckBox`, `RadioButton`, `Label`, `Border`; registered
    from `Application`'s ctor. (XAML `generic.xaml` compilation is a later round.)
  - Precedence wired into the existing style path: **explicit `Style` > implicit `TargetType` style > theme
    default** (`ApplyImplicitStyle` falls through to `ApplyDefaultStyle`); `OverridesDefaultStyle` skips the theme.
  - `Media/SystemColors` — `ControlBrush`/`WindowBrush`/`HighlightBrush`/`ControlTextBrush` (fixed values; GTK
    theme query deferred); `Control.Padding`/`HorizontalContentAlignment`/`VerticalContentAlignment` DPs.
  - **+19 tests** (8 precedence + 11 Generic/SystemColors).
- **Round 12 totals (both tracks merged): 1242 tests** (60 WindowsBase + 997 PresentationFramework +
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) compiles.
- **Round 12 review (Opus) + fixes — DONE.** 2 Critical + Highs, all fixed with pins driving the real path:
  **X1** the theme/default-style mechanism was **inert** (nothing called it) — `Control`'s ctor now applies the
  default style and `Button`/`CheckBox`/`RadioButton`/`Label` set `DefaultStyleKey` via `OverrideMetadata`
  (default-style setters land below local values); **X2** the GTK input-translation layer (`GdkKeyMap`) had
  **zero coverage** — added 11 pure keyval/modifier-mapping tests + a seam smoke test; **X3** `Focus()` now
  clears the previously-focused element (single source of truth, raises its `LostFocus`); **X4** removed the
  inert/empty default `Button` `ControlTemplate` (template-over-native-widget is a later round); **X5** multiline
  `TextBox` attaches input/focus controllers to the inner `Gtk.TextView` (via a `FocusWidget` hook), not the
  `ScrolledWindow`; **X6** TextBox selection model fixed (`MaxLength` truncates, `SelectionStart` re-clamps
  `SelectionLength`, `SelectedText` setter leaves a consistent caret); **X7** oversized files split to ≤200 lines.
  Full-suite total now **1289 tests** (60 WindowsBase + 1041 PresentationFramework + 173 MarkupCompiler +
  15 ProjectConverter — the tree also compiles concurrent imaging/markup-NuGet work landed in `feb7c90b`),
  all pass timeboxed; `Gtk4.Forms.WpfSample` (unmodified WPF) still compiles.
- **`WebBrowser` control — DONE (Photino.NET).** `System.Windows.Controls.WebBrowser : Control` with the full
  WPF surface: `Source` DP, read-only `CanGoBack`/`CanGoForward`, `Navigate`/`NavigateToString`/`GoBack`/
  `GoForward`/`Refresh`/`InvokeScript`, `Navigating`(cancelable, with `NavigationMode`)/`Navigated`/
  `LoadCompleted` (fires exactly once), and a back/forward journal. Backed by **Photino.NET 4.0.16**
  (WebView2/WebKitGTK/WKWebView) via an injectable **`IWebBrowserHost`** seam (`PhotinoWebBrowserHost`
  default; tests inject a fake — **no native window is ever created headless**). Native-absent / no-`DISPLAY`
  degradation is handled by the `new PhotinoWindow(...)` try/catch. **Windowing limitation**: Photino owns a
  native top-level window (no public API to embed as a child GTK widget), so the in-GTK area is a
  placeholder/launcher and the page renders in Photino's own window — true in-widget embedding is a later
  refinement. `InvokeScript` JSON-encodes args and sends a web message (Photino has no synchronous JS-eval,
  so it is best-effort/returns null). Opus-reviewed + fixed (single-fire `LoadCompleted`, dead native-probe
  removed, cancelable `Navigating` on journal navigations, files split ≤200 lines). **39 tests**
  (model + history + InvokeScript + GTK smoke); PF suite **1111 pass**, warning-clean; sample + the
  build-and-run integration test still pass with the transitive Photino dependency.
- **Round 13 — infra + command libraries/selectors — DONE.** Two parallel tracks (agents hit a session
  limit mid-run; recovered next session: built, tested, reviewed, fixed, merged).
  - *Track A — Visual/tree/Freezable foundation*: `Freezable` (Freeze/IsFrozen/Clone/Changed, frozen
    rejects mutation) + `DependencyObjectType` (`FromSystemType`, IsInstanceOfType/IsSubclassOf) in
    `WindowsBase`; `Visual`/`VisualTreeHelper` (GetChildrenCount/GetChild/GetParent over the existing
    `UIElement.VisualChildren`), `LogicalTreeHelper` (walks `LogicalParent` + per-container logical children),
    `DrawingVisual`/`DrawingContext`/`VisualCollection`.
  - *Track B — command libraries + selectors*: `ApplicationCommands`/`NavigationCommands`/`ComponentCommands`/
    `MediaCommands`/`EditingCommands` (`RoutedUICommand` statics with default gestures, e.g. Copy=Ctrl+C);
    `DataTemplateSelector`/`StyleSelector`; `HierarchicalDataTemplate` (ItemsSource/ItemTemplate/
    ItemTemplateSelector + `TreeView` hierarchy population); `ItemsControl` `ItemTemplateSelector`/
    `ItemContainerStyle`/`ItemContainerStyleSelector`/`DisplayMemberPath` (selector wins over template).
  - *Recovery fixes*: a `HierarchicalDataTemplate` test poked an internal static (`TreeView.PopulateHierarchicalChildren`)
    that the repo's **package-based PF consumption** doesn't surface cross-assembly — rewritten to exercise the
    public `ResolveChildren`/`ItemTemplate` surface; and an `ItemsControlSelector` test's `ref int` counter
    (unusable in a closure) was replaced with the selector's own `InvokeCount`.
  - **Full suite: 1466 tests** (83 WindowsBase + 1195 PresentationFramework + 173 MarkupCompiler +
    15 ProjectConverter), all pass; warning-clean.
- **Round 14 Track A — data providers + advanced binding (DONE).**
  - **`DataSourceProvider`** (abstract base): `Data`, `DataChanged`, `IsInitialLoadEnabled`, `InitialLoad()`, `Refresh()`, `BeginInit()`/`EndInit()` (ISupportInitialize shape), `OnQueryFinished` protected hook.
  - **`ObjectDataProvider`**: `ObjectType`/`ObjectInstance`, `MethodName`/`MethodParameters`/`ConstructorParameters` — instantiates the type (or uses the instance) and optionally invokes `MethodName` via reflection, exposing result as `Data`.
  - **`XmlDataProvider`**: `Source`(Uri)/`Document`/`XmlSerializer` (inline XML string), `XPath` — loads an `XmlDocument`, evaluates the XPath expression via `XPathNavigator`, exposes matching nodes as `List<XmlNode>` in `Data`.
  - **`PriorityBinding : BindingBase`**: `Bindings` collection; `PriorityBindingEngine` resolves child bindings in order, uses first non-null result, re-evaluates on INPC; `PriorityBindingExpression` live object; wired into `BindingEngine.SetBinding` and the expression map.
  - **`BindingGroup`**: `Name`, `BindingExpressions`, `ValidationRules`, `Items` (deduplicated source objects), `BeginEdit()`/`CommitEdit()`/`CancelEdit()`/`UpdateSources()`/`ValidateWithoutUpdate()`; validates source objects via rules; writes sources on commit via `BindingExpression.UpdateSource`.
  - **`FrameworkElement.BindingGroupProperty`** DP — new partial `FrameworkElement.BindingGroup.cs`.
  - **`BindingOperations`** (static): `SetBinding`, `ClearBinding`, `ClearAllBindings`, `GetBinding`, `GetBindingExpression`, `GetMultiBindingExpression`, `GetBindingExpressionBase`, `IsDataBound`; routes all kinds through `BindingEngine`; `BindingEngine.ClearBinding`/`ClearAllBindings`/`MultiBindingEngine.ClearAll` added as new partials.
  - **`MultiBinding` TwoWay**: `ConvertBack` now writes each child source via `TargetChangeTracker`; `Binding.DoNothing`/`DependencyProperty.UnsetValue` sentinels skip that slot; reentrancy-guarded. The old `NotSupportedException` contract is replaced by functional TwoWay. Existing test updated.
  - **`RelativeSource.PreviousData`**: resolves by walking to the nearest `ItemsControl` ancestor and returning the item at `index - 1` in `Items`. Documented limitation: depends on `DataContext` matching a flat `Items` entry; nested templates with intermediate hosts may not resolve.
  - **`CollectionViewSource` grouping/live-shaping**: already complete from Round 9 (U6) — `SortDescriptions`/`GroupDescriptions`/`Filter` flow into `View` on `Refresh`.
  - **34 new PF tests** (DataSourceProvider/PriorityBinding/BindingGroup/BindingOperations/MultiBinding-TwoWay).
- **Round 14 Track B — MarkupExtension system + trigger actions/MultiDataTrigger (DONE).**
  - **`MarkupExtension`** base (`ProvideValue(IServiceProvider)`) + `IProvideValueTarget`/`IXamlTypeResolver`/
    `IUriContext` services; concrete `StaticResourceExtension` (resolves via the element resource chain +
    `Application.Current.Resources`), `DynamicResourceExtension`, `TypeExtension` (`{x:Type}`), `StaticExtension`
    (`{x:Static}`, reflection), `NullExtension` (`{x:Null}`), `ArrayExtension` (`{x:Array}`). `Binding`/
    `TemplateBindingExtension` gain a `ProvideValue` (plain method — the build-time compiler path is untouched;
    this is the runtime object model for programmatic use / a future runtime XAML loader).
  - **`Trigger`/`DataTrigger`/`MultiTrigger` `EnterActions`/`ExitActions`** run on the active↔inactive transition;
    **`MultiDataTrigger`** (data-bound `Condition`s, AND semantics, INPC-watched, re-evaluates on DataContext change).
  - **+24 PF tests** (MarkupExtension, EnterActions/ExitActions, MultiDataTrigger).
- **Round 14 review (Opus) + fixes — DONE.** No Critical; 3 High + Mediums, all fixed with public-surface pins:
  **V1** `BindingEngine.ClearBinding` now clears multi/priority records (was leaking a PriorityBinding/MultiBinding
  record + its INPC handlers — the Round-7 cross-engine-leak class); **V2** `ClearAllBindings` clears priority
  records via a new `PriorityBindingEngine.ClearAll`; **V3** `PriorityBinding`'s INPC handler filters by the bound
  path segment (no over-fire) and rebuilds subscriptions on re-resolve; **V4** trigger `ExitActions` no longer
  double-fire on `Style` teardown (`UnApply(runActions:false)` from `Dispose`; actions run only on a genuine
  condition flip); **V5** `XmlDataProvider` pins `XmlResolver = null` (no XXE), converter-less TwoWay MultiBinding
  installs no dead target listener, + doc notes (ObjectDataProvider no auto-requery, BindingGroup non-atomic commit).
  **Round 14 totals: 1561 tests** (83 WindowsBase + 1290 PresentationFramework + 173 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 15 Track A — `VisualStateManager.VisualStateGroups` XAML compilation (DONE).**
  The markup compiler now emits `InitializeComponent` code for the `<VisualStateManager.VisualStateGroups>`
  attached-property element form on any `FrameworkElement` (was code-behind only). Multiple groups; states with
  or without a `<Storyboard>` child (both `<Storyboard>` direct-child and `<VisualState.Storyboard>`
  property-element forms); `x:Name` on groups/states declares typed fields. Storyboard construction is delegated
  to the existing `EmitStoryboardInline` (no duplication). Runtime: `VisualStateManager.Get/SetVisualStateGroups`
  widened `Control` → `FrameworkElement` (backward-compatible; `GoToState` still typed `Control`). Files:
  `XamlCodeGenerator.VisualStateManager.cs` (new), `XamlCodeGenerator.cs` hook, `WpfXmlnsMap.cs` (+3 types).
  **+25 tests** (14 compiler + 11 runtime). Note: WPF `VisualState` is storyboard-only in XAML; the runtime
  `VisualState.Setters` extension has no XAML path (programmatic only).
- **Round 15 Track B — `Window` chrome (DONE).** Five WPF `Window` DPs mapped to GTK4 with graceful
  pre-realize deferral: `WindowState` (Normal/Minimized/Maximized → Minimize/Maximize/Unmaximize),
  `WindowStartupLocation` (Manual/CenterScreen/CenterOwner), `ResizeMode`
  (NoResize/CanMinimize/CanResize/CanResizeWithGrip → `Resizable`+title-button affordances),
  `WindowStyle` (None/Single/ThreeD/ToolWindow → `Decorated`), `SizeToContent`
  (Manual/Width/Height/WidthAndHeight, honored in `ApplyExplicitSize`). Enum defaults match WPF exactly.
  Files: `Window.Chrome.cs`, `Window.Chrome.Style.cs`, five enum files, `Window.cs` edits. **+29 tests**
  (20 pure DP + 9 GTK smoke). GTK4 limits documented in the enum XML docs: no `gtk_window_move` (CenterScreen/
  CenterOwner store-only), no per-button title-bar visibility, border variants collapse to `Decorated=true`,
  `WindowState` round-trip is one-way (DP→native; WM-initiated changes not reflected back).
- **Round 15 review (Opus) + fixes — DONE.** Both tracks correct (enum values/defaults, VSM widening,
  deferral guards, no leaked handlers, codegen delegation all verified). Two **Low** test-only findings fixed:
  the 259-line `XamlCodeGeneratorVsmTests.cs` split by behaviour into a shared `XamlCodeGeneratorVsmTestBase.cs`
  + `XamlCodeGeneratorVsmTests.cs` (structure) + `XamlCodeGeneratorVsmStoryboardTests.cs` (storyboard/fields),
  all ≤200 lines; the brittle whole-file `DoesNotContain("Storyboard")` assertion scoped to
  `new …Storyboard()` + `.Storyboard =`.
  **Round 15 totals: 1615 tests** (83 WindowsBase + 1330 PresentationFramework + 187 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 16 Track A — element-level `FrameworkElement.Triggers` XAML compilation + runtime attach (DONE).**
  The markup compiler now emits the `<{element}.Triggers>` / `<FrameworkElement.Triggers>` property-element form
  (triggers added to `element.Triggers`, then `AttachElementTriggers()`). The shared trigger-emit methods were
  renamed (`EmitPropertyTrigger`→`EmitTriggerToCollection`, …; `styleVar`→`containerVar`) and reused for both
  `Style.Triggers` and element triggers (no duplication; Style output unchanged). Runtime
  `FrameworkElement.Triggers.cs` attaches/detaches ALL `TriggerBase` subtypes. Files:
  `XamlCodeGenerator.cs`, `XamlCodeGenerator.ElementTriggers.cs` (new). **+18 tests** (11 compiler + 7 runtime).
  Fidelity note: real WPF permits only `EventTrigger` in `FrameworkElement.Triggers`; this is a documented
  **superset** (property triggers also attach) — harmless for valid WPF XAML, carried over as a future compiler
  diagnostic.
- **Round 16 Track B — Popup/ContextMenu deferred-open replay (DONE).** Fixes the dropped-open defect: an
  `IsOpen` set before the backing GTK widget exists is stored (`_pendingOpen`) and replayed in an
  `OnWidgetCreated` override; a later `IsOpen=false` before realize cancels it (stores the false, no stale open).
  General (replays whatever value was set), not sample-specific. Files: `Controls/Popup.cs`, `Controls/ContextMenu.cs`.
  **+6 tests** (4 pure-DP + 2 GTK cancel-path smoke).
- **Round 16 review (Opus) + fixes — DONE.** No Critical/High. Fixed: **H1** split the 230-line
  `XamlCodeGeneratorElementTriggerTests.cs` into a shared base + EventTrigger/form file +
  property-trigger file (all ≤200 lines); **A4** (pre-existing latent leak, widened by element reuse)
  `Trigger.Attach`/`EventTrigger.Attach` are now idempotent — a re-attach detaches the prior state/handler
  instead of orphaning it (pinned by 2 failing-first reattach tests). Carried over (Low, non-blocking):
  **A1** compiler diagnostic for non-`EventTrigger` element triggers; **B2** positive open-replay path is proven
  only by pure-DP inspection (GTK `Popover.Popup()` crashes without a parent window), cancel path is asserted.
  **Round 16 totals: 1641 tests** (83 WindowsBase + 1345 PresentationFramework + 198 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 17 Track A — `[assembly:XmlnsDefinition]`-driven xmlns resolution (markup compiler) (DONE).**
  The compiler now resolves element/attribute types from referenced assemblies'
  `[assembly: XmlnsDefinition(xmlnsUri, clrNamespace)]` attributes, merged with the built-in `WpfXmlnsMap`
  (probed first, so nothing regresses). New injectable seam `IXmlnsDefinitionSource` + production
  `AssemblyXmlnsDefinitionSource` (reads via `CustomAttributeData`, no new NuGet package); `ResolveType` is now
  an instance method, gating the xmlns-def probe with a new `TypeExists`. `XmlnsDefinitionAttribute` already
  existed in `WindowsBase/MarkupAttributes.cs` with four `[assembly: XmlnsDefinition]` entries on the PF assembly.
  Files: `IXmlnsDefinitionSource.cs`, `AssemblyXmlnsDefinitionSource.cs` (new), `XamlCodeGenerator.cs`,
  `XamlCodeGenerator.SymbolResolver.cs`, `MarkupCompileTask.cs`. **+12 tests** (6 compiler + 6 WindowsBase attr).
- **Round 17 Track B — true `{DynamicResource}` live change-tracking (DONE).** New
  `FrameworkElement.SetResourceReference(dp, key)`/`ClearResourceReference` establish a live link;
  `ResourceDictionary` raises `ResourceChanged` on add/replace/remove/clear; a **new D7 dynamic-resource
  precedence layer** sits between trigger (D8) and style (D6):
  `animation > local > trigger > dynamic-resource > style > inherited > default`. `DynamicResourceExtension`
  routes a `FrameworkElement`+DP target through `SetResourceReference`. Files: `ResourceDictionary.cs`,
  `DependencyObject.DynamicResourceLayer.cs` (D7 layer, split out), `DependencyObject.cs`,
  `FrameworkElement.ResourceReferences.cs` (new). **+6 tests**.
- **Round 17 review (Opus) + fixes — DONE.** No Critical/High; precedence order verified unchanged for
  non-dynamic values (no regression). Fixed: **M2** merged-dictionary live-updates — `CollectResourceChain` now
  recurses into `MergedDictionaries`, so a key resolved from a merged dict also live-updates (pinned by a
  failing-first mutate test); **L1** the modified `DependencyObject.Layers.cs` (238 lines) D7 block extracted to
  `DependencyObject.DynamicResourceLayer.cs` (Layers back to 197), class-doc priority line corrected.
  Carried over (Medium): **M1** app-lifetime subscription leak — a dictionary holds a strong delegate to the
  element, so `SetResourceReference` pins the element until `ClearResourceReference` (no unload hook yet);
  documented in the `SetResourceReference` XML doc, weak-event fix deferred to a dedicated round.
  **Round 17 totals: 1661 tests** (89 WindowsBase + 1353 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 18 Track A — weak `ResourceChanged` subscription (fixes Round-17 M1 leak) (DONE).**
  `ResourceDictionary.ResourceChanged` is backed by a weak-handler list (`WeakResourceChangedHandlers.cs`,
  `List<WeakReference<Action>>`, pruned on raise/subscribe/remove). `FrameworkElement` caches its callback in a
  field so the delegate lives exactly as long as the element; the dictionary holds it weakly — a collected
  element is no longer pinned by a long-lived (e.g. `Application`-level) dictionary. `ClearResourceReference`
  still detaches deterministically. **+2 tests** (a deterministic GC leak test + a live-update-after-GC
  regression). The Round-17 `SetResourceReference` "must clear to avoid pinning" caveat is removed.
- **Round 18 Track B — nested binding paths in `DataTrigger` (DONE).** Removed the `NotSupportedException`
  nested-path guard (the E5 gap); added a reusable internal `PathWatcher` (`BindingEngine.PathWatcher.cs`) that
  subscribes per-segment INPC and re-subscribes from a changed segment forward. `DataTriggerState` (split into
  `DataTrigger.State.cs`) owns one `PathWatcher`, recreated on DataContext change. Leaf change, intermediate
  reassignment, DataContext re-root, null-along-path, single-segment regression, and Enter/ExitActions all
  covered. **+12 tests** (the inverted contract test now asserts "no throw").
- **Round 18 review (Opus) + fixes — DONE.** Both tracks correct (weak scheme verified: no reverse strong path,
  raise-reentrancy index-safe, GC test deterministic; PathWatcher per-segment resubscription/null-safety/
  DataContext re-root all clean; D8 precedence and the Round-14 no-double-ExitActions contract preserved). No
  Critical/High/Medium. Two **Low** house-rule nits fixed: the 217-line `DataTriggerNestedPathTests.cs` split
  into a shared base + core + actions files (all ≤200); a `private set` in a test changed to `internal set`.
  **Round 18 totals: 1676 tests** (89 WindowsBase + 1368 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 19 Track A — `Clipboard` + data-object model (DONE).** New `System.Windows.IDataObject`/`DataObject`
  (format-keyed store, Text↔UnicodeText auto-convert), `DataFormats`, and static `Clipboard` behind an internal
  `IClipboardBackend` seam: `InMemoryClipboardBackend` (headless/tests) + `GtkClipboardBackend` (in-memory mirror
  for synchronous reads + best-effort push to `Gdk.Display.Default.Clipboard`, degrades when no display).
  **+33 tests** (incl. a headless round-trip through the real `GtkClipboardBackend`). Documented limits:
  cross-process reads not supported (GTK4 clipboard read is async-only), `StoreAsync` not called on
  `SetDataObject(copy:true)`, no arbitrary-object serialization, `DataFormats.StringFormat` not auto-converted
  to UnicodeText (only Text↔UnicodeText).
  - **CI hotfix (macOS):** `GtkClipboardBackend` disables the native GTK clipboard side-channel on macOS —
    GTK4's Quartz `GdkClipboard` SIGSEGVs (139) on `SetContent`/`Text=` writes in this GtkSharp build (the
    `gdk_content_provider`/`gdk_content_formats` path), crashing the whole macOS test process. A native crash is
    not catchable, so the only safe fix is not to call it; macOS runs entirely off the in-memory mirror
    (within-process clipboard works; cross-app push is a documented no-op there). Linux/Windows unaffected.
- **Round 30 Track A — Popup placement (DONE).** `Popup.Placement` DP (`PlacementMode`, default Bottom; 6 missing
  enum members added to the existing `PlacementMode`), `HorizontalOffset`/`VerticalOffset` DPs, applied to the GTK
  popover via pure `PopupPlacementMap.ToGtkPosition` (Bottom/Top/Left/Right exact; Center/Absolute/Relative/Mouse →
  Bottom fallback, documented) + `Gtk.Popover.SetOffset` (double→int). **+37 tests**.
- **Round 30 Track B — DataGrid column sorting (DONE).** `DataGrid.CanUserSortColumns` + `DataGridColumn.CanUserSort`/
  `SortMemberPath`/`SortDirection`; an `internal ToggleSort(column)` seam cycles null→Asc→Desc, clears sibling
  columns, and re-sorts rows via pure `DataGridSort.SortItems` (reflection member read, single + recursive dotted
  path, nulls-first, stable via explicit index tiebreaker on `List.Sort`). Header-click wired when realized. **+23 tests**.
- **Round 30 review (Opus) + fixes — DONE.** Both tracks correct and **netstandard2.0-safe** (no MaxBy/Order/span
  APIs; built on all 5 TFMs). Fixed two **Low**: split the 286-line `DataGridSortTests.cs` into a shared base +
  SortItems/DP/ToggleSort files (all ≤150); corrected a stale `DataGridSort` doc (dotted paths resolve recursively,
  not top-level fallback). Carry-over: replacing `ItemsSource` resets rows to unsorted while a column's
  `SortDirection` stays non-null (stale sort state across source swaps — WPF re-sorts via its collection view).
  **Round 30 totals: 2385 tests** (89 WindowsBase + 2077 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across `netstandard2.0;net8.0;net10.0` (+`-windows`).
  (Constraint adopted this round: PF new code must compile on netstandard2.0 — the owner multi-targeted the
  assembly to ship a netstandard2.0-compatible NuGet package.)
- **Round 31 Track A — ListBox multi-select (DONE).** `SelectionMode` enum (Single/Multiple/Extended) + DP
  (default Single), `SelectedItems` (IList), mapped to `Gtk.ListBox.SelectionMode` (Single→Single;
  Multiple/Extended→Multiple — GTK doesn't distinguish, documented). `SelectionChangedEventArgs` gained
  `AddedItems`/`RemovedItems` (non-null by default — existing ComboBox/TreeView/TabControl consumers unaffected).
  GTK `SelectedRowsChanged`→delta→single `SelectionChanged`, guarded against DP↔GTK re-entry. **+28 tests**.
- **Round 31 Track B — tab navigation (DONE).** `UIElement.IsTabStop` (default true) + `TabIndex`
  (default `int.MaxValue`) DPs → `widget.CanFocus = IsTabStop && Focusable`; `System.Windows.Input.KeyboardNavigation`
  static class (`TabNavigation`/`DirectionalNavigation`/`IsTabStop`/`TabIndex` attached DPs + `KeyboardNavigationMode`
  enum); pure `TabOrderHelper.Order` (stable by TabIndex then declaration, skips non-tab-stops). **+40 tests**.
- **Round 31 review (Opus) + fixes — DONE.** Top risks clean (shared `SelectionChangedEventArgs` non-null → no
  consumer NRE; selection-sync single `_updating` guard → no re-entry; netstandard2.0-safe). Fixed two **Minor**:
  **F1** create-time `CanFocus` applied to the outer widget instead of `FocusWidget` — a multiline TextBox with
  pre-realize `IsTabStop=false` left its inner TextView focusable; now uses `FocusWidget` (pinned by a new
  multiline-TextBox GTK smoke test). **F2** corrected the `KeyboardNavigation.IsTabStop`/`TabIndex` docs (they are
  independent attached DPs, not mirrors that cross-propagate).
  **Round 31 totals: 2452 tests** (89 WindowsBase + 2144 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 31)**: GTK can't distinguish `SelectionMode.Multiple` vs `Extended` (keyboard-modifier
  range/toggle semantics not reproduced); `TabIndex` has no per-widget GTK order API (GTK uses tree order — the
  managed `TabOrderHelper` computes WPF order); `KeyboardNavigation.IsTabStop` attached DP does not propagate to
  `UIElement.IsTabStop`.
- **Round 32 Track A — TextBlock layout (DONE).** `TextAlignment` (reused enum) → `Gtk.Label.Justify`+`Xalign`,
  `TextTrimming` (new enum; Char & Word both → `Pango.EllipsizeMode.End`), `Padding` → label margins, `LineHeight`
  (store-only — GtkSharp4 doesn't surface the Pango line-spacing factory). Pure `MapTextAlignment`/`MapTextTrimming`
  helpers. **+39 tests**.
- **Round 32 Track B — SelectedValue/SelectedValuePath (DONE).** Added to BOTH `ListBox` and `ComboBox` via a
  shared pure `SelectorValueSupport` (`GetValue` reflection single+dotted path, `FindByValue`); two-way
  `SelectedValue`↔`SelectedItem` sync (`SelectedValuePath`-resolved; set selects the matching item or clears on
  no-match), guarded against loops by a per-instance `_updatingValue` flag. **+30 tests**.
- **Round 32 review (Opus) + fixes — DONE.** No correctness defects (two-way sync-loop guard verified symmetric
  on both controls; netstandard2.0-safe; mappings correct, no default-left regression). Fixed house-rule/doc:
  split the 202-line `TextBlock.Layout.cs` (mapping helpers → `TextBlock.LayoutMapping.cs`), split the 330-line
  `SelectedValueTests.cs` (→ SelectorValueSupport/ListBox/ComboBox files) and the 204-line
  `TextBlockLayoutDpTests.cs` (→ + mapping-tests file) — all ≤160; corrected a false `LineHeight` doc claim about
  re-application.
  **Round 32 totals: 2521 tests** (89 WindowsBase + 2213 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 32)**: `TextBlock.LineHeight` is store-only (no GTK line-spacing applied); `TextTrimming`
  Char vs Word both ellipsize at End; `SelectedValue` retains a non-matching value after a no-match clear
  (WPF behaviour, untested).
- **Round 33 Track A — Slider ticks (DONE).** `SmallChange`/`LargeChange` (→ adjustment step/page),
  `TickFrequency`/`TickPlacement` (→ `Gtk.Scale.AddMark` per tick; `Both` adds both sides), `IsSnapToTickEnabled`
  (snap in Value coercion via pure `SliderSnap.SnapToTick`, round-half-away-from-zero), `IsDirectionReversed`
  (→ `Range.Inverted`), `Orientation`. **+44 tests**.
- **Round 33 Track B — PasswordBox completeness (DONE).** `PasswordChanged` (`RoutedEventHandler`, fire-once,
  `_updating` guard), `MaxLength` (managed clamp — no native GTK API), `Clear()`, `SecurePassword` (fresh
  read-only `SecureString`), `PasswordChar` (store-only — `Gtk.PasswordEntry` has no echo-char API). **+38 tests**.
- **Round 33 review (Opus) + fixes — DONE.** No functional defects (snap↔GTK loop guarded, clamp-after-snap,
  PasswordChanged fire-once + MaxLength clamp + SecurePassword all verified; netstandard2.0-safe). Fixed **F1**
  (house rule): split the 222-line `Slider.Ticks.cs` (GTK apply → `Slider.TicksGtk.cs`) and the two 260/216-line
  test files (snap-math and SecurePassword into their own files) — all ≤165. **F3** aligned `SnapToTick` to the
  documented round-half-away-from-zero (`Math.Round(..., AwayFromZero)`); **F2** corrected the `Clear()` doc
  (fires only when non-empty).
  **Round 33 totals: 2595 tests** (89 WindowsBase + 2287 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 33)**: `PasswordChar` and `MaxLength` have no native `Gtk.PasswordEntry` API (managed-only);
  `Slider.AddMark` is one call per tick (no uniform-mark GTK API).
- **Round 34 Track A — AutomationProperties (DONE).** `System.Windows.Automation.AutomationProperties` attached
  DPs (`Name`/`HelpText`/`AutomationId`/`LabeledBy`/`IsRequiredForForm`/`LiveSetting`); `Name`→GTK
  `AccessibleProperty.Label` and `HelpText`→`AccessibleProperty.HelpText` pushed via
  `Gtk.AccessibleExtensions.UpdateProperty` on realize (API reachable in GtkSharp4); the rest object-model-only.
  **+28 tests**.
- **Round 34 Track B — ToolTipService timing/placement (DONE).** Attached DPs `InitialShowDelay`/`ShowDuration`/
  `BetweenShowDelay`/`ShowOnDisabled`/`Placement`/`PlacementTarget`/`HorizontalOffset`/`VerticalOffset` (store-only
  — GTK tooltip timing/placement is toolkit-controlled) + `IsEnabled` (actionable: suppresses `HasTooltip` on the
  realized widget). **+26 tests**.
- **Round 34 review (Opus) + fixes — DONE.** No functional/netstandard2.0 defect; `IsEnabled` gate doesn't
  regress the default tooltip apply. Fixed three **house-rule** >200 files (split `AutomationProperties.cs`→
  `+ .Semantics.cs`; split the 351-line `ToolTipDpTests.cs`→control + service-defaults + service-roundtrip;
  extracted `AutomationTestBase` stubs to trim `AutomationPropertiesTests.cs`). Fixed a latent bug Opus flagged:
  `OnToolTipChanged` now consults `ToolTipService.IsEnabled`, so setting a new ToolTip on an already-realized
  disabled element no longer re-shows it (pinned by a new GTK smoke test).
  **Round 34 totals: 2648 tests** (89 WindowsBase + 2340 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 34)**: ToolTip timing/placement DPs are store-only on GTK; `AutomationProperties`
  LabeledBy/IsRequiredForForm/LiveSetting/AutomationId are object-model-only (no GTK accessible push).
- **Round 35 Track A — RenderTargetBitmap (DONE).** `Media.Imaging.RenderTargetBitmap : BitmapSource` +
  `Media.PixelFormat`/`PixelFormats.Pbgra32`; `Render(Visual)` rasterises a `DrawingVisual`'s recorded
  `DrawingContext` into an off-screen `Cairo.ImageSurface(Argb32)` (in-memory — works headless) via the Round-28
  `DrawingContextRenderer`; `CopyPixels`/`GetPixel` read back premultiplied BGRA (wrong-size buffer throws per WPF).
  DrawingVisual-only (documented). **+29 tests** (real pixel read-back, not degrade path).
- **Round 35 Track B — Typography (DONE).** `System.Windows.Documents.Typography` attached DPs
  (`Capitals`/`NumeralStyle`/`Fraction`/`StandardLigatures`/`Kerning`/`Variants` + 4 enums) mapped to Pango
  `font_features` tokens (smcp/c2sc/onum/lnum/frac/afrc/sups/subs/ordn/… ; `liga=0`/`kern=0` only when explicitly
  false; all-default → no span) and woven into `TextBlock.BuildMarkup` (fast-path still skips markup when no
  features). **+33 tests**.
- **Round 35 review (Opus) + fix — DONE.** No functional/netstandard2.0 defect (CopyPixels contract + layout
  correct; TextBlock static-ctor metadata override + markup change don't regress the 74 existing TextBlock tests;
  `font_features` mapping correct). Fixed one **Low** house-rule breach: split the 318-line `TypographyTests.cs`
  into defaults/round-trips + `TypographyFontFeaturesTests` (both ≤188).
  **Round 35 totals: 2704 tests** (89 WindowsBase + 2396 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 35)**: `RenderTargetBitmap` rasterises only `DrawingVisual` content (general live visuals
  render nothing); Pango `font_features` requires Pango ≥ 1.38 at runtime.
- **Round 36 Track A — Element lifecycle + size events (DONE).** `FrameworkElement.Loaded`/`Unloaded` (Direct,
  pre-order raise-once broadcast over logical + attached children, `IsLoaded`; deferred attach/detach at
  `DispatcherPriority.Loaded` re-checks tree state), `Initialized`/`IsInitialized` + `ISupportInitialize`
  (`BeginInit`/`EndInit` with WPF's exception messages, lazy fire on attach/before Loaded), read-only
  `ActualWidth`/`ActualHeight` DPs + `SizeChanged` (`SizeChangedInfo`/`SizeChangedEventArgs`, pure
  `SizeChangeMath` = `DoubleUtil.AreClose`), `Window.SourceInitialized` (GTK realize) / `ContentRendered`
  (frame-clock `after-paint`); live GTK allocation fed into `RenderSize` from the toplevel frame-clock `layout`
  signal (`GtkAllocationSync`). **+66 tests**.
- **Round 36 Track B — Mouse input completeness (DONE).** `EventManager.RegisterClassHandler` (both overloads,
  most-derived first, before instance handlers, `handledEventsToo`) + `GetRoutedEvents`/`GetRoutedEventsForOwner`;
  tunnel no longer stops on Handled; button cracking (`(Preview)MouseLeft/RightButtonDown/Up`, Direct, via
  UIElement class handlers), `(Preview)MouseWheel` (`Gtk.EventControllerScroll`, `RetVal` = Handled so
  `ScrolledWindow` still scrolls), `IsMouseOver`/`IsMouseDirectlyOver` (`contains-pointer`/`is-pointer`),
  `CaptureMouse`/`Mouse.Capture`/`CaptureMode`/`IsMouseCaptured`/`Got`/`LostMouseCapture` (managed re-targeting),
  `Control.(Preview)MouseDoubleClick`; `GestureClick.Button = 0`. **+64 tests**.
- **Round 36 review (Opus) + fixes — DONE.** Track A: **R36A-1 (High)** live `ActualWidth` was GTK's *content*
  box (`gtk_widget_get_width`, CSS border+padding removed — a themed Button read ~22px narrow) → now the border
  box via `gtk_widget_compute_bounds` (`GtkAllocationSync.BorderBoxSize`); the tautological real-GTK assertion
  replaced by one against `ComputeBounds` plus `ActualWidth > Widget.Width` on a padded Button. **R36A-2**
  `ContentRendered` re-armed by every `Content` change (WPF `OnContentChanged`/`PostContentRendered`: next
  `after-paint` with a queued redraw, or dispatcher work at `Input` headless), never raised for a window whose
  Content was never set; the "GTK limitation" wording dropped. **R36A-3** `Arrange` now only queues
  `SizeChangedInfo` on a thread-static chain fired after the outermost arrange pass in prepend order (parent
  first; repeat arranges merge axes — WPF `SizeChangedInfo.Update`), so handlers see final sizes and never
  re-enter mid-pass (new `UIElement.SizeChanged.cs`). **R36A-4** one writer at a time: a managed arrange no
  longer writes `RenderSize` once the element is loaded with a mapped widget (`IsRenderSizeOwnedByGtk`) —
  `UpdateLayout()` on a shown window raises zero SizeChanged; a subtree attached to a live window syncs its
  allocation right after its deferred Loaded. **R36A-5** the MarkupCompiler brackets every non-Application
  root with `ISupportInitialize.BeginInit`/`EndInit` (warning-free `(object)this is` check), so a XAML root is
  `IsInitialized` (children first) before its constructor returns. **R36A-6** `RenderSize` setter is
  store-only (internal `SetRenderSizeAndNotify` for the live path); test flipped. **R36A-7** `GetChildren`
  dedupes with a reference `HashSet`; the per-frame sync walks attached children linearly (`EnumerateAttached`).
  **R36A-8** FE-only/single-segment DP-source tracking documented as a gap in XML docs. **R36A-9** house rule:
  path resolution split to `BindingEngine.Path.cs` (546→491), `CloseCore`/`OnCloseRequest`/`Close(bool?)` moved
  to `Window.Closing.cs` (`Window.cs` 236→191). Track B: **F1 (High)** same-GDK-event dedupe now unconditional
  (pure `MouseDispatchDedupe` + `MouseEventKey`, `DispatchGtkButton`/`DispatchGtkMotion` seams): capturing in
  `MouseLeftButtonDown` (or releasing in `MouseUp`) no longer re-delivers the event via an ancestor's
  controller, and uncaptured nesting no longer double-raises press/motion. **F2** pure `MouseButtonMap.TryFromGdk`:
  4/8→XButton1, 5/9→XButton2 (Win32 `WM_XBUTTON*`/macOS `buttonNumber+1` report 4/5), anything else raises
  nothing (was Left). **F3** scroll is unit-aware: `GDK_SCROLL_UNIT_SURFACE` pixels → notches at 48 px (WPF
  ScrollViewer 3 lines × 16 px) before ×120. **F4** the dispatch decision is now covered by pure + nested
  real-widget tests. **F5** `PreviewMouseMoveEvent` (Tunnel, shared Handled flow with MouseMove) +
  `IsMouseCaptureWithin` read-only DP / `IsMouseCaptureWithinChanged` (captor + logical ancestors).
  **F6** no-auto-release documented in `Mouse` capture remarks. **F8** release passes `NPress`. **F9** handler
  invocation unwraps `TargetInvocationException` (`RoutedEventHandler` invoked typed; `ExceptionDispatchInfo`
  rethrow; `RoutedEventHost.Invoke.cs` split keeps the host ≤200). **F7** deferred (see carry-over).
  **+55 tests** (PF +49: SizeChanged chain, ContentRendered re-arm/none, border box, UpdateLayout-no-SizeChanged,
  generated-root Initialized, GDK dispatch/dedupe/button/wheel maps, PreviewMouseMove/IsMouseCaptureWithin,
  nested real-widget dispatch; MarkupCompiler +3; WindowsBase +3).
  **Round 36 totals: 2889 tests** (105 WindowsBase + 2543 PresentationFramework + 226 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 36)**: mouse/capture/wheel routed events are still owned by `UIElement` (WPF: registered on
  `Mouse`, `AddOwner`ed to UIElement, with `Mouse.Add*Handler` helpers — needs `RoutedEvent.AddOwner`; **F7**);
  capture is never auto-released (disable/hide/remove/OS grab loss) and has no explicit grab without a held
  button; DP-source binding tracking is FrameworkElement + single-segment only; `Dispatcher.BeginInvoke`
  ignores `DispatcherPriority`; no `UIElement.Visibility`/`IsVisible`/`IsEnabled` DPs yet (capture gate and
  Loaded semantics read GTK `visible`/`sensitive`); `BindingEngine.cs` still 491 lines.
- **Round 37 Track A — UIElement Visibility/IsEnabled/IsHitTestVisible (DONE).** `Visibility` (Visible/Hidden/
  Collapsed) + read-only `IsVisible`/`IsVisibleChanged`, `IsEnabled`/`IsEnabledCore`/`IsEnabledChanged` (coerced
  through the logical tree and command sources), `IsHitTestVisible`/`IsHitTestVisibleChanged`, `CoerceValue`/
  `InvalidateProperty`, `BooleanToVisibilityConverter`. GTK mapping in pure `VisibilityGtkMap`: Collapsed hides the
  widget (containers skip it), Hidden keeps it mapped at opacity 0, untargetable and unfocusable; focus/capture
  released on disable/collapse/hide. A `Window`'s Visibility is its show state (default Collapsed, `Show`/`Hide`).
- **Round 37 Track B — Brush/Font DPs (DONE).** `Background`/`Foreground`/`BorderBrush`/`BorderThickness`/
  `CornerRadius` + `Font*` on Control/TextBlock/Panel/Border (`TextElement`-inherited via `DependencyProperty.
  AddOwner`), rendered as a lazy per-widget `Gtk.CssProvider` (priority 600) through pure `BrushCss`/`FontCss`/
  `ElementCssStyle`; only non-inherited values emit CSS, GTK CSS inheritance carries the rest. Image/Visual/Drawing
  brushes store-only; gradient foreground falls back to the first stop.
- **Round 37 review (Opus) + fixes — DONE.** Track A: **F1 (High)** a Window's own Visibility (show state)
  drove the GTK mapping: a widget touched before `Show()` (as `ShowDialog` does) got `CanFocus=false`, and
  Hide→Opacity→Show left opacity 0; new `internal virtual WidgetVisibility` (Window: always Visible) used by
  CanFocus/CanTarget/Opacity/created-state; `Opacity` now also applies to a Window (it was gated on a content
  box a Window never has). **F2 (High)** `ClearValue` never re-ran coercion (cleared child under a disabled
  parent read enabled). **F3** style/trigger/dynamic-resource/animation layers stored already-coerced values, so
  coercion could not relax: all layers now store raw values and every mutation goes through one
  `CommitLayerChange` (old → layer → `RecomputeCoerced` → fire → `NotifyValueSource`), which removed
  `_baseValues`; `ClearStyleValues`/`ClearTriggerValue` re-coerce and compute old from the resolved value (the
  snapshot value is wrong when a local shadows it). **F4** `Show`/`ShowDialog`/`Visibility=Visible` after `Close`
  throw `InvalidOperationException` (WPF text); `CloseCore` sets Hidden so the re-set is a change. **F5**
  `ReadLocalValue` returns the raw local (automatic with F3). Track B: **B37-1 (High)** CSS emission depends on the
  value source but refreshed only on effective-value change (local==inherited rendered the ancestor's later
  value): static `DependencyObject.ValueSourceChanged` hook fired after every layer set/clear (and for children
  shadowed during inheritance propagation) refreshes appearance DPs; `ElementCssStyle.Update` skips unchanged CSS.
  **Also found by the real-GTK tests**: GTK 4 `Widget.Name` returns the type name (`GtkLabel`) for an unnamed
  widget, so the `#name` selector matched nothing and no appearance/effect/render-transform CSS ever applied;
  new `ElementCssStyle.IsUnnamed` (used by Effect and RenderTransform too). **B37-2** CSS builders pattern-match
  instead of hard-casting; `BindingEngine.CoerceToTargetType` converts int→double, string→struct (XamlValueConverter),
  IConvertible and TypeConverter sources, falling back instead of throwing. **B37-3** `FromOpenTypeWeight`/
  `FromOpenTypeStretch` moved onto the `FontWeight`/`FontStretch` structs (+ FontStretch `<`/`>`/`Compare`).
  **B37-4** `TextElement.*` attached attributes: `TextElement` in `WpfXmlnsMap`, setter value type resolved from
  the referenced assembly (`DpSymbolResolver.GetAttachedSetterValueType`) with a hardcoded TextElement fallback.
  **B37-5** `ElementCssStyle.ParseErrors` (`CssProvider.ParsingError`) → `AppearanceParseErrors`; theory over
  every builder output ×4 element types plus measure-based effect assertions (font-size height, border width).
  **B37-6** Generic styles set `Foreground` (ControlTextBrush for Button/CheckBox/RadioButton/Label,
  `SystemColors.WindowTextBrush` (new) for TextBox) as WPF themes do; a theme brush the style layer merely repeats
  is not emitted (`HasOnlyStyleValue`/`GetInheritedValue`), so untouched controls stay provider-free. **B37-7**
  a border with thickness but no brush emits `border-style: solid; border-color: transparent`. **B37-8** linear
  stop offsets remapped onto the CSS gradient line (partial spans keep their extent); GradientOrigin ignored and
  only `SpreadMethod.Pad` honoured, documented. **B37-9** TextBlock padding-vs-background gap documented.
  **B37-10** `FontFamily` locations reduced to the name after `#` (font files are not loaded). **B37-11**
  `FontCss` skips non-positive/NaN/infinite sizes; DP validation (`ValidateValueCallback`) documented as a gap,
  flag alignment skipped (no code reads `AffectsMeasure`/`AffectsRender`). **B37-12** stale Border docs and the
  five long `TextElement.Properties.cs` lines + `FontCss` weight line wrapped. Rejected: none; B37-8's offset
  remap done (optional), B37-11's flag alignment and validation overload deferred.
  **Round 37 totals: 3140 tests** (126 WindowsBase + 2764 PresentationFramework + 235 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Carry-over (Round 37)**: no `ValidateValueCallback` (invalid-enum/FontSize/FontFamily validation); Focus and
  capture gates skip the presentation-source half of `IsVisible`; Window ignores Opacity/CanTarget suppression for
  Hidden; radial `GradientOrigin` and non-Pad `SpreadMethod` unsupported; linear remap assumes
  `RelativeToBoundingBox`; `TextBlock.Background` does not cover Padding (margins); font files by URI not loaded
  and a `pack://` URI containing commas is split by the family list; `AffectsMeasure`/`AffectsRender` flags not
  aligned with WPF; Image/Visual/DrawingBrush backgrounds store-only; the on-screen CSS result was only verified
  through GTK measurement, not visually.
- **Round 38 Track A — FrameworkPropertyMetadataOptions + ValidateValueCallback + two-way defaults (DONE).**
  `FrameworkPropertyMetadataOptions` (WPF numeric values), `UIPropertyMetadata`, metadata sealing and base-merge
  (only explicitly set flags win), `ValidateValueCallback` (`IsValidType`/`IsValidValue`, direct `SetValue` throws
  `ArgumentException`), and `BindsTwoWayByDefault`/`DefaultUpdateSourceTrigger` resolved by `BindingDefaults`
  (`TextBox.Text` → LostFocus, `IsChecked`/`Value`/`SelectedItem`/… two-way); `UpdateSourceTrigger` moved to
  WindowsBase with a `TypeForwardedTo`. Store-only on GTK: `Affects*` layout/render flags, `Journal`,
  `OverridesInheritanceBehavior`, `SubPropertiesDoNotAffectRender`, `IsAnimationProhibited`, `NotDataBindable`.
  **+107 tests**.
- **Round 38 Track B — Application lifecycle (DONE).** `Startup`/`Exit`/`ShutdownMode`/`Windows`/`MainWindow`,
  `Activated`/`Deactivated`, `FindResource`/`Properties`, `StartupUri` resolution and
  `DispatcherUnhandledException`. `SessionEnding` is store-only (GTK reports it only through a `GtkApplication`).
  **+74 tests**.
- **Round 38 review (Opus) + fixes — DONE.** Track A (BLOCK): **A1 (High)** bindings pushed null into value-type
  DPs and threw now that `SetValue` validates → `BindingEngine.SetTargetSafe`/`SetFallbackOrDefault` (valid value,
  else converted `FallbackValue`, else metadata default; never throws) wired into `PushToTarget`, `ApplyFallback`,
  `ApplyTargetNullValue` and `MultiBindingEngine.PushCore`. **A2 (High)** `WriteBack` handed the raw target value
  to `PropertyInfo.SetValue`, so `TextBox.Text` on an `int` threw at LostFocus → `TryResolveWriteTarget` +
  `TryConvertToSourceType` (empty→null for `Nullable`, TypeConverter/`Convert.ChangeType`); a failed conversion
  skips the write and sets an `ExceptionValidationRule` error, cleared by the next good write. **A3** the
  `FrameworkPropertyMetadata(callback, coerce)` ctor no longer marks the default as explicit null. **A4** Window and
  the `AddOwner` appearance metadata (Control/Border/TextBlock Background, Control BorderBrush/BorderThickness)
  passed a callback the base already chains, so it ran twice → dropped. **A5** `ConvertBack` exceptions (single and
  multi) are caught (error only with `ValidatesOnExceptions`), `DoNothing`/`UnsetValue` skip the write, and
  `MultiBinding.UpdateSourceTrigger` now resolves like `Binding` (LostFocus dirty flag, Explicit never writes);
  stale `MultiBinding` docs fixed. **A6** reused/duplicate metadata and a second `OverrideMetadata` for one type
  throw `ArgumentException`; the default is validated before sealing. **A7** `BindingEngine.cs` 518→185 lines
  (`.Push/.Refresh/.Target/.SourceConvert/.TargetChanges/.WriteBack/.Validation` partials), `MultiBindingEngine`
  split, `TabItem`/`ComboBox.Items` extracted. Track B (SHIP-WITH-FIXES): **F1 (High)** `CloseForShutdown` now
  raises `Closing` once and ignores `Cancel` (no re-entry from a handler). **F2 (High)** `StartupUri` loads
  after `Startup` (a Startup-requested `Shutdown` is async, so the window is still created; see follow-up).
  **F3** `DispatcherUnhandledException` forwards to `Dispatcher.UnhandledException` (dispatcher
  sender, all handlers run, active from subscription); the invented `ApplicationUnhandledException` hook removed.
  **F4** `DoStartup` and the initial `Show` route exceptions through `RaiseUnhandledException`. **F5**
  `Shutdown` is thread-checked and posted via `Dispatcher.BeginInvoke` (first request wins). **F6** `Deactivated`
  is deferred and cancelled when another window activates. **F7** `Run(Window)` keeps an existing `MainWindow`;
  second `Run` throws. **F8** a finished/shut-down app stops tracking windows; `ResetCurrentForTests` in
  lifecycle test teardown. **F9** `Window.Activation.cs`, `Application.StartupUri.cs`. Carry-in: **CI-1** a
  throwing `CoerceValueCallback` stores nothing (`SetValueCore` rolls back) and `Window` rejects Visible-after-close
  from a coerce callback; **CI-2** `ToggleButton`/`RepeatButton`/`PasswordBox` get a `DefaultStyleKey` and theme
  Foreground (`Generic.Buttons.cs`); **CI-3** every layer above Inherited now shadows inheritance propagation
  (coerced values compare effective values); **CI-4** a failed conversion reverts to the default instead of a
  stale value; **CI-5** `BrushCss`/`ClosedMessage` lines wrapped. Rejected: none.
  **Round 38 totals: 3364 tests** (177 WindowsBase + 2930 PresentationFramework + 242 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs.
- **Round 38 follow-up (Opus-verified fixes) — DONE.** **R38C-1** `WriteBack` now runs validation rules on the
  RAW target value before `ConvertBack`/default conversion (WPF `RawProposedValue`; every rule is treated as raw, no
  `ValidationStep` yet), so a string rule on an `int` source sees the text and a failing rule blocks conversion.
  **R38C-2** `Binding.DoNothing` from a converter (single and multi) leaves the target unchanged and `UnsetValue`
  uses `FallbackValue`/the default, both before `TargetNullValue`/`StringFormat`; `SetTargetSafe` never passes either
  sentinel to `SetValue`. **R38C-3** `PriorityBinding` pushes through `SetTargetConverted` (value converted to the
  DP type, invalid → fallback/default) instead of a throwing raw `SetValue`. **R38C-4** the throwing-`ConvertBack`
  `MultiBinding` test was vacuous (never focused); now `…_does_not_escape_LostFocus` plus a PropertyChanged-trigger
  test, both verified to fail without the catch. **R38C-5** `Application.OnWindowActivationChanged` ignores windows
  not in `Windows`, so a window created after the app finished cannot raise `Activated`. **R38C-6** `StartupUri`
  docs and tests now pin WPF-style asynchronous `Shutdown` from a Startup handler (window still created, then
  closed). Open: `DependencyObject.SetValue(dp, UnsetValue)` stores the sentinel instead of clearing (WPF clears).
  **Follow-up totals: 3382 tests** (177 WindowsBase + 2948 PresentationFramework + 242 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 38)**: `SessionEnding` is never raised on GTK; a second `new Application()` does not throw;
  `Application` Activated/Deactivated depend on window-manager `notify::is-active` ordering (verified only through
  the `NotifyActivation` seam); `LostFocus` tests drive managed `Focus()`, not the native focus controller;
  registered defaults are only run through `ValidateValueCallback` (no type check); `Binding` has no
  `ConverterCulture` (conversions use the current culture); metadata `IsAnimationProhibited` is not merged.
- **Round 39 Track B — Style/Trigger Setter values (DONE).** `Setter.Value` objects (`<Setter.Value>` elements, inline
  `ControlTemplate`/`DataTemplate`/`MultiBinding`), markup-extension values (`DynamicResource` live, `StaticResource`
  resolved once, `Binding`/`TemplateBinding`, `x:Static`/`x:Null`), `Setter(dp, value[, targetName])` ctors, and
  `EventSetter` (handler added at apply, removed on unapply). Gaps on GTK: `Setter.TargetName` is store-only;
  EventSetter handlers run in registration order (WPF runs them after instance handlers); no control hosts a template
  root as its widget, so the `Template` setter reaches `Button.Template` and the tree is built, but the widget is
  unchanged. **+41 tests**.
- **Round 39 Track D — Binding markup + validation pipeline (DONE).** `ConverterParameter`/`ConverterCulture`/`Source`/
  `ValidatesOn*`/`NotifyOnValidationError` in XAML and element-syntax `<Binding>` with `ValidationRules`;
  `ValidationStep` (Raw/Converted/Updated/Committed) + `ValidatesOnTargetUpdated`; `INotifyDataErrorInfo`
  (`NotifyDataErrorValidationRule`, `ValidatesOnNotifyDataErrors`); `ConverterCulture` on `Binding`/`MultiBinding`;
  `SetValue(dp, UnsetValue)` clears the local value. No GTK work. Gaps: CommittedValue runs right after UpdatedValue
  (no BindingGroup/IEditableObject cycle); culture falls back to en-US (no `FrameworkElement.Language`);
  `MultiBinding` has no validation pipeline; the INDEI subscription is re-resolved on push/write-back, not on every
  intermediate path-object change. **+52 tests**.
- **Round 39 review (Opus) + fixes — DONE.** Track B (SHIP-WITH-FIXES): **B-F1 (High)** the compiler emitted
  `new RoutedEventHandler(this.H)` for every `<EventSetter>`, so `MouseDown`/`KeyDown`/`MouseDoubleClick` handlers
  failed with CS0123 → `Delegate.CreateDelegate(Owner.XEvent.HandlerType, this, "H")` (binds non-public handlers as
  WPF); runtime test with a private `MouseButtonEventHandler`-shaped handler. **B-F2 (High)** a style `DynamicResource`
  wrote the dynamic-resource layer, which outranks the style layer: a base-style link beat a derived plain value, it
  replaced a user's own `SetResourceReference`, and `Unapply` deleted it → style-layer contributor
  (`StyleLayerContributor`, `SetStyleResourceReference`/`DropStyleResourceReference`, `ClearStyleValue`); a derived
  style's plain value drops the base link; only `Layer.Local` still uses `SetResourceReference`. **B-F3** trigger
  setter `StaticResource` resolved on `this` instead of the style's owner element → receiver threaded through the
  trigger emitters. **B-F4** style/trigger `Binding` values are local values (engine limitation, not GTK): a trigger
  setter never shows over a style binding, `ValueSource` reports Local, a trigger binding is ignored when the DP
  already has one → documented, pinned by `Trigger_setter_does_not_show_over_style_Binding_known_gap`; layer-targeted
  `BindingExpression` is a backlog item. **B-F5** `Setter.Value = UnsetValue` threw only at apply, after earlier setters
  applied → throws at assignment; `Setter.Property`/ctors validate (`ArgumentNullException("property")`, read-only DP
  `ArgumentException`); the NameProperty check is skipped (no `NameProperty` DP here). **B-F6** `EventSetter` ctor
  uses WPF parameter names, validates the handler type against `RoutedEvent.HandlerType`, and a trigger containing an
  `EventSetter` throws `InvalidOperationException` on attach. **B-F7** nested `Setter.Value` emission restarted from a
  stale counter (duplicate `__svN`/`__eN`) → counter passed by `ref`. **B-F9** `FrameworkElement.ResourceChain.cs`,
  `MultiDataTrigger.State.cs`, `Style.Unapply.cs` extracted. Track D (SHIP-WITH-FIXES): **D-F1 (High)** `Publish`
  replaced the element's whole error collection, so a sibling INDEI binding wiped other bindings' errors and swapped
  the `Validation.Errors` instance on every push → per-element, per-binding error state merged into one stable
  collection (`Validation.SetErrorsFor`); unchanged error sets raise nothing. **D-F2** `ClearBinding` left INDEI/step
  errors on the element → `Deactivate` clears them. **D-F3** rule attributes went through the type-blind
  `ClrValueExpression` (`ValidationStep="UpdatedValue"` → string, CS0029) → `RuleAttrExpression` (enum/string/
  decimal/float/double/bool; `ValidationStep` special-cased); the built-in rule types are in `WpfXmlnsMap`. **D-F4**
  rules stop at the first failure and `IDataErrorInfo` runs only when nothing failed (WPF `Validate`).
  **D-F5** binding arguments: `{x:Null}`, `{x:Type}`, leading `{}` escape; `{DynamicResource}` and unknown
  extensions are build errors. **D-F6** `<Binding.Converter>` inline object; unresolved rule types and unsupported
  `Binding.*` property elements are build errors (`XamlCodeGeneratorException`). **D-F7** the push reuses the raw value
  already resolved (no second path walk). **D-F8** `StringFormat` without braces is `{0:fmt}` (`N2` → `1,234.50`);
  `MultiBinding` without a converter formats the values array (`"{0} {1}"`). The old
  `StringFormat_without_placeholder_prepends_value` test pinned the non-WPF `"fmt{0}"` shortcut and was corrected.
  Rejected in triage: **B-F8** (default/theme style never unapplied when an explicit Style is set): wrong for WPF, where
  the theme style stays a separate lower-precedence layer whose setters/handlers/triggers keep applying; unapplying it
  would drop theme Templates and defaults. The real harm (a default-style `DynamicResource` beating the explicit
  style) was the B-F2 symptom and is gone. Separating theme-style from Style precedence is a larger pre-existing
  design item. **D-F9** (house-rule size of `XamlCodeGenerator.cs` 1180 / `.Binding.cs` 446 lines): not a defect of the
  round; new code was in new partials. Scheduled with the backlog split below.
  **Round 39 totals: 3509 tests** (182 WindowsBase + 3014 PresentationFramework + 298 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean across all TFMs. `gf39/xaml-content-model` and
  `gf39/combobox-itemscontrol` were NOT merged in this round (not part of the merge set that was reviewed); their
  branches and worktrees are kept untouched for a later round.
- **Carry-over (Round 39)**: backlog: layer-targeted `BindingExpression` (style/trigger bindings as style/trigger
  layer values, B-F4); split `XamlCodeGenerator.cs` and `XamlCodeGenerator.Binding.cs` (house line limit); separate
  theme-style and Style precedence layers (B-F8); `Setter.TargetName` store-only; EventSetter handlers run in
  registration order, not after instance handlers; `<ControlTemplate.Resources>` is not emitted by the compiler;
  element-level trigger `StaticResource` now resolves on the owning element variable (verify behaviour while that
  element is not yet parented at emit time); CommittedValue has no BindingGroup cycle; `MultiBinding` has no validation; INDEI
  subscription is not refreshed on intermediate path-object changes.
- **Round 19 Track B — Adorners (DONE).** New `System.Windows.Documents.Adorner` (abstract, `AdornedElement`),
  `AdornerLayer` (`Add`/`Remove`/`GetAdorners` returning null when none, static `GetAdornerLayer` walking
  `VisualParent` to the nearest `AdornerDecorator`), and `AdornerDecorator` (`Decorator`+`Child`, exposes a
  non-null `AdornerLayer`, backed by `Gtk.Overlay`). **+23 tests** (object-model + decorator walk + GTK overlay
  smoke). Object model is fully headless; overlay placement happens when both the overlay and the adorner widget
  are realized.
- **Round 19 review (Opus) + fixes — DONE.** No Critical/High/Medium. Both tracks verified correct (backend
  restore via IDisposable + no-parallelism, lazy GTK init, bidirectional format auto-convert, `GetAdornerLayer`
  null-safe VisualParent walk, `GetAdorners` null-not-empty, Remove detaches). Two **Low** fixes: **A1** added the
  missing `GtkClipboardBackend` headless coverage (degrade path); **B1** split the 294-line `AdornerTests.cs` into
  a shared helper + object-model + decorator + GTK-smoke files (all ≤200). Carried over (Low, documented):
  **A2** `DataFormats.StringFormat`→UnicodeText auto-convert; **B2** deferred adorner-overlay placement (adorners
  added before realize are tracked logically but not placed until a rendering adorner + realized overlay exist).
  **Round 19 totals: 1732 tests** (89 WindowsBase + 1424 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 20 Track A — DragDrop (DONE).** New `System.Windows.DragDropEffects`/`DragDropKeyStates` ([Flags]),
  `DragEventArgs : RoutedEventArgs` (+`QueryContinueDragEventArgs`/`GiveFeedbackEventArgs`), static `DragDrop`
  behind an internal `IDragDropBackend` seam (`InProcessDragDropBackend` default-for-tests, `GtkDragDropBackend`
  degrades to in-process when no `Gdk.Display.Default`). `UIElement.AllowDrop` DP + the four tunnel/bubble drag
  routed-event pairs (`UIElement.DragDrop.cs`); GTK `Gtk.DropTarget` wiring (`UIElement.GtkDragDrop.cs`) attached
  only when a display exists. **+46 tests** (incl. 6 pinning the production tunnel-then-bubble raisers directly).
- **Round 20 Track B — ToolTip control (DONE).** New `System.Windows.Controls.ToolTip : ContentControl`
  (`IsOpen`/`Placement`/`PlacementTarget`/`StaysOpen`). `FrameworkElement.Popups.cs` wires the tooltip to
  `Gtk.Widget.TooltipText`+`HasTooltip` (string value, or a `ToolTip` whose `Content` stringifies); headless
  stores the value and applies it on realize. **+24 tests** (DP + GTK smoke). Rich-content (UIElement) tooltip
  rendering falls back to text; `IsOpen`/`Placement` are object-model-only for now.
- **Round 20 review (Opus) + fixes — DONE.** Investigated a reported intermittent native AV
  (`0xC0000005`) in GTK smoke tests: Opus confirmed the diff is **not implicated** (GTK calls are thread-confined
  via the fixture and guarded by `HasGtkDisplay()`/`AllowDrop`; the crashing ToolTip run does no drag wiring) —
  pre-existing flakiness; the authoritative combined run was clean. Fixed **Medium** test gap: the production
  `Raise*Pair` methods were untested (tests re-implemented the sequence) — made `internal` and pinned with 6
  tests asserting preview→bubble ordering + Handled-suppression. Carried over (Low): `Gtk.DropTarget` is not
  detached on widget teardown (leak when a realized `AllowDrop` element is destroyed); `GtkDragDropBackend`'s
  synchronous `DoDragDrop` returns `None` (GTK4 drag is async — full sync needs a nested main loop).
  **Round 20 totals: 1804 tests** (89 WindowsBase + 1496 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 21 Track A — RenderTransform (DONE).** Added `SkewTransform`/`TransformGroup`/`TransformCollection`
  (`Media/Transform.Group.cs`), `UIElement.RenderTransform` (default `Transform.Identity`) + `RenderTransformOrigin`
  DPs, and a GTK application via a per-widget `Gtk.CssProvider` (`transform: matrix(...)`/`transform-origin`, a
  unique `#rt-N` selector). The matrix math + CSS mapping are extracted into testable pure functions
  (`internal RenderTransformCss`). CSS `transform` renders on GTK 4.14+; the object model (`Transform.Value`
  matrix + DP) is always correct. **+28 tests** (matrix composition order, center rotation/scale/skew, CSS
  mapping, headless).
- **Round 21 Track B — `Primitives.ScrollBar` (DONE).** New `System.Windows.Controls.Primitives.ScrollBar`
  (Orientation [default Vertical], Minimum/Maximum/Value[coerced into range, re-coerced on Min/Max change]/
  ViewportSize/SmallChange/LargeChange, `Scroll`/`ValueChanged` events, `ScrollEventArgs`/`ScrollEventType`),
  backed by `Gtk.Scrollbar`+`Gtk.Adjustment` (`Upper = Maximum + ViewportSize`) with a `_updating` reentrancy
  guard both directions. No `RangeBase` exists — owns its DPs directly like `Slider`. **+43 tests**.
- **Round 21 review (Opus) + fixes — DONE.** Both tracks correct (matrix composition order = WPF first-child-
  first, CSS element order right, coercion + no adjustment feedback loop verified). No Critical/High/Medium.
  Two **Low** fixes: **A1** the `#rt-<hashcode>` widget name could cascade a parent's rule onto a same-hash
  descendant — switched to a process-wide `Interlocked` monotonic counter; **B** split the 222-line
  `ScrollBarDpTests.cs` into defaults/roundtrips + behavior files (both ≤120).
  **Round 21 totals: 1869 tests** (89 WindowsBase + 1561 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 22 Track A — LayoutTransform (DONE).** `FrameworkElement.LayoutTransform` DP; `MeasureCore`
  expands `DesiredSize` to the transform's axis-aligned bounding box via an `internal static`
  `LayoutTransformBounds(Size, Matrix)` (identity short-circuits — no layout regression). GTK visual is
  best-effort CSS, composed with `RenderTransform` (see fix). **+17 tests** (bounds math, DP, measure).
- **Round 22 Track B — bitmap Effects (DONE).** `Media.Effects.Effect`/`BlurEffect`/`DropShadowEffect`/
  `KernelType` + `UIElement.Effect` DP applied via its OWN per-widget `Gtk.CssProvider` (`#ef-N`, priority 601)
  building `filter: blur()/drop-shadow(dx dy blur rgba())` from a pure `internal EffectCss.BuildFilter`
  (culture-invariant; `Direction` 0°=right CCW → dx=depth·cos, dy=−depth·sin). **+35 tests**.
- **Round 22 review (Opus) + fixes — DONE.** Fixed a **HIGH** defect: LayoutTransform and RenderTransform
  shared one CSS provider/rule, so the second apply (or a reset to identity) wiped the other — now both are
  **composed into a single CSS matrix** via `internal UIElement.ComposeLayoutAndRender` (layout applied first,
  then render; pinned by 5 new coexistence/offset tests). Fixed a **Medium** doc defect (the `LayoutTransformBounds`
  summary falsely claimed arrange used it). Split the 217-line `LayoutTransformTests.cs` into bounds + DP/measure
  files (**Low**). Verified clean: Effect uses a separate provider (no collision), bounds math + measure
  integration + Direction/rgba/culture formatting all correct.
  **Round 22 totals: 1916 tests** (89 WindowsBase + 1608 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 23 Track A — `ImageBrush` + `Media.Stretch` consolidation (DONE).** New `Media.Stretch` enum (the
  canonical `System.Windows.Media.Stretch`, replacing two divergent duplicates previously in
  `System.Windows.Shapes`/`System.Windows.Controls` — all consumers reference by name, no value break),
  abstract `TileBrush : Brush` (+`TileMode`/`BrushMappingMode`), `ImageBrush : TileBrush`, and
  `internal StretchMath.ComputeDestRect` (None/Fill/Uniform/UniformToFill). Painting via
  `Cairo.ImageSurface`+`SurfacePattern` (local PNG files; degrades to object-model-only otherwise). **+28 tests**.
- **Round 23 Track B — Path markup mini-language parser (DONE).** `Geometry.Parse(string)` →
  `PathGeometry`, supporting `M L H V C Q Z` absolute+relative with implicit repeats (`A`/`S`/`T` parsed but
  skipped, params consumed so the stream stays synced); added `QuadraticBezierSegment`. `Polyline` already
  existed. **+17 tests**.
- **Round 23 review (Opus) + fixes — DONE.** Both tracks correct (Stretch consolidation name-only — no value
  break; StretchMath centering; parser relative accumulation/implicit-repeat/FormatException all verified).
  Fixed **B1** (ship-blocker): split the 244-line `PathMarkupParser.cs` into parser + `PathMarkupParser.Lexer.cs`
  (partial) + `PathParseContext.cs` (all ≤160). Fixed **B2**: after `Z` the current point now returns to the
  closed figure's start (SVG contract) so a following relative command accumulates correctly. Fixed **B3**: added
  tests proving the `A`/`S`/`T` skips consume the right param counts and keep the stream synced. Carried over
  (Low): **A1** `ImageBrush.Opacity` is ignored on the image-paint path (rendering-only, unreachable headless).
  **Round 23 totals: 1965 tests** (89 WindowsBase + 1657 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Round 24 Track A — `ArcSegment` + `A`/`a` path command (DONE).** New `Media.SweepDirection`,
  `Media/ArcToBezier.cs` (SVG §B.2.4 endpoint→center conversion + arc→cubic split, ≤90°/segment,
  α=4/3·tan(θ/4), ≈0.027% error), `Media/ArcSegment.cs` (degenerate radius → straight line). `PathMarkupParser`
  now parses `A`/`a` into `ArcSegment`s (replacing the Round-23 skip); `PathGeometry.Bounds` expands over the
  arc end point. **+14 tests**.
- **Round 24 Track B — TextBox selection API (DONE).** New `Controls/TextBox.Selection.cs`:
  `SelectionStart`/`SelectionLength`/`SelectedText`/`CaretIndex` (CLR properties, clamped to text length),
  `Select(start,length)`/`SelectAll()`, `SelectionChanged`, and `AcceptsTab` DP. GTK sync via
  `Gtk.Entry.SelectRegion` (single-line) / `Gtk.TextView.Buffer.SelectRange` (multiline), character offsets,
  deferred until realized; managed fields authoritative headless. **+15 tests**.
- **Round 24 review (Opus) + fixes — DONE.** Both tracks verified mathematically/semantically correct
  (arc endpoint→center sign/sweep mapping, α + segment count, degenerate guards; selection clamping, text-change
  reclamp on binding, GTK char-offset sync). No Critical/High/Medium. Two **Low** fixes: corrected a broken XML
  cref in `ArcToBezier.cs` (→ `PathGeometry.Bounds`, accurately noting endpoint-only under-estimate); added the
  missing `SelectedText`-set (replace-selection) + `CaretIndex`-clamp tests.
  **Round 24 totals: 1996 tests** (89 WindowsBase + 1688 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean. (Note: the `WPF-Samples` submodule + its `Directory.Nuget.Props`/
  `Gtk4.Forms.Wpf.csproj` edits on `main` are the repo owner's concurrent commits, left untouched.)
- **Carry-over (Round 24)**: `PathGeometry.Bounds` under-estimates arcs bulging past the chord (endpoint-only).
- **Round 25 Track A — Shape stroke style (DONE).** `Media.PenLineCap`/`PenLineJoin`/`PenDashCap` enums,
  `Media.DoubleCollection` (+`Parse`), and 7 `Shape` stroke DPs (`StrokeDashArray`/`StrokeDashOffset`/
  `StrokeDashCap`/`StrokeStartLineCap`/`StrokeEndLineCap`/`StrokeLineJoin`/`StrokeMiterLimit`) applied to Cairo
  via pure `StrokeStyle` mappers (dash values × thickness, enum→`Cairo.LineCap`/`LineJoin`). **+38 tests**.
  Documented: Cairo has one line cap (WPF start/end collapse; `Triangle`→`Butt`) and no per-dash cap.
- **Round 25 Track B — Window state round-trip (DONE).** `Topmost`/`ShowInTaskbar` DPs (stored; GTK4 has no
  portable always-on-top/taskbar API — documented no-op), `Activate()`→`Present`, `Activated`/`Deactivated`/
  `StateChanged` events, and the `WindowState` **WM→DP round-trip** via GTK `notify::maximized`/`suspended`/
  `is-active`, guarded against the DP↔native echo by an instance `_updatingWindowState` flag. **+26 tests**.
- **Round 25 review (Opus) + fixes — DONE.** Track A clean (dash×thickness + null/zero guards verified). Track B
  logic correct (guard leak-free, single `StateChanged`). Fixed a **High** test-adequacy gap — the WM→DP seam
  was never actually invoked (tests bypassed it); made `SyncWindowStateFromGtk` `internal` and rewrote the tests
  to drive it (assert DP update + exactly one event + guard released + no-op when unchanged). Fixed a **Medium**:
  extracted `internal static ResolveGtkState(suspended, maximized)` applying minimized-over-maximized precedence
  (both handlers now read both flags), pinned by a 4-case theory. Added doc notes (odd-dash pattern;
  window-lifetime subscriptions).
  **Round 25 totals: 2055 tests** (89 WindowsBase + 1747 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 25)**: `Topmost`/`ShowInTaskbar` have no GTK4 native effect; `WindowState.Minimized`
  WM→DP relies on `notify::suspended` (may not fire on some Wayland compositors); Cairo single line-cap.
- **Round 26 Track A — Drawing model + DrawingBrush (DONE).** abstract `Media.Drawing` (`Bounds`+`Draw`),
  `GeometryDrawing` (Geometry/Brush/Pen → Cairo fill+stroke), `DrawingGroup` (children, `Bounds` union,
  Transform/Clip/Opacity via `PushGroup`), `DrawingBrush : TileBrush` (renders the drawing to an off-screen
  `Cairo.ImageSurface`, scaled via `StretchMath`, Stretch/TileMode honoured). Added `Rect.Empty`/`IsEmpty`/`Union`
  to WindowsBase `Primitives.cs` (NegativeInfinity sentinel — distinct from a zero rect, WPF parity). **+49 tests**.
- **Round 26 Track B — TextDecorations (DONE).** `System.Windows.TextDecoration`/`TextDecorationCollection`/
  `TextDecorationLocation` + static `TextDecorations` (Underline/Strikethrough/OverLine/Baseline); `TextDecorations`
  on both `TextBlock` (DP) and `TextElement`/`Inline` (property); rendered via Pango markup
  (`underline="single"`/`strikethrough="true"`; OverLine→`overline` Pango≥1.46; Baseline→underline fallback).
  **+26 tests**.
- **Round 26 review (Opus) — DONE, no defects.** Verified the shared `Rect` change is purely additive (no
  existing-member or zero-rect semantic shift; `Rect.Empty`≠`Rect(0,0,0,0)`), `DrawingGroup` save/restore +
  PushGroup balance correct, `DrawingBrush` zero-size-surface guarded + surfaces disposed, decoration Pango
  markup nests/escapes validly, parity on both TextBlock+Inline. Both tracks clean — no fixes required.
  **Round 26 totals: 2130 tests** (89 WindowsBase + 1822 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 26)**: `DrawingBrush`/`GeometryDrawing` render only on GTK with a Cairo surface
  (object-model always correct); `VisualBrush` still unimplemented; Pango `overline`/baseline runtime-dependent.
- **Round 27 Track A — Cursor (DONE).** `System.Windows.Input.Cursor`/`CursorType` (29 members)/`Cursors` +
  `FrameworkElement.Cursor` DP (→ `Gtk.Widget.CursorFromName` on realize) + `Mouse.OverrideCursor` (store-only —
  GTK4 has no global override API). WPF→CSS cursor-name mapping is a pure `internal CursorMap.ToGdkName`
  (Arrow→default, Hand→pointer, IBeam→text, …, None→none). **+60 tests**. `.cur`/`.ani` ctors fall back to
  `default` (unsupported, documented).
- **Round 27 Track B — cancelable `Window.Closing` (DONE).** `event CancelEventHandler? Closing`; both `Close()`
  and the GTK `CloseRequest` signal funnel through an `internal bool RequestClose()` that raises `Closing`,
  aborts (no `Closed`) on `e.Cancel`, else runs `CloseCore`; `_closingInProgress`+`_closed` guard re-entrancy and
  double-close; the WM path maps the outcome to `e.RetVal` (veto ⇒ suppress GTK destroy). **+13 tests**.
- **Round 27 review (Opus) + fixes — DONE.** Both tracks correct (cursor mapping total, realize-deferral,
  close veto/retry/re-entrancy, RetVal mapping all verified) — no high/medium. Fixed two **Low** house-rule
  breaches: split the 232-line `WindowClosingTests.cs` (→ basics + `WindowClosingGuardTests`) and the 228-line
  `CursorObjectModelTests.cs` (→ Cursor/Cursors + `CursorElementTests`), all ≤150. Pinned the GTK close-request
  return-value inversion behind an `internal static MapCloseRetVal(bool)` seam with a 2-case theory (closes the
  coverage gap where `OnCloseRequest`'s `!proceeded` was untestable headlessly).
  **Round 27 totals: 2205 tests** (89 WindowsBase + 1897 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 27)**: `.cur`/`.ani` cursor files unsupported (fall back to default); `Mouse.OverrideCursor`
  has no GTK global-override effect; the live GTK `CloseRequest` signal path is exercised only via the
  `RequestClose` seam (GTK can't raise it deterministically headless).
- **Round 28 Track A — Image.Stretch (DONE).** `Image.Stretch` (default Uniform) + `StretchDirection` (default
  Both, enum reused from `Viewbox`) DPs; backing switched `Gtk.Image`→`Gtk.Picture` (which supports sizing);
  pure `internal ImageStretchMap.ToContentFit` maps `Stretch`→`Gtk.ContentFit` (None→ScaleDown, Fill→Fill,
  Uniform→Contain, UniformToFill→Cover). **+21 tests**. `StretchDirection` has no GTK equivalent (stored only).
- **Round 28 Track B — VisualBrush (DONE).** `Media.VisualBrush : TileBrush` renders a `DrawingVisual`'s recorded
  `DrawingContext` into an off-screen `Cairo.ImageSurface` (via new `DrawingContextRenderer`: rect/ellipse/line/
  geometry; `DrawText` no-op) then tiles/scales via `StretchMath` — mirroring the Round-26 `DrawingBrush` path.
  A general (non-`DrawingVisual`) visual paints nothing (GTK can't portably snapshot a foreign/unrealized widget
  — documented). **+27 tests**.
- **Round 28 review (Opus) — DONE, no defects.** Verified the `Gtk.Image`→`Gtk.Picture` switch preserves
  `Source` semantics with no other widget-type consumer broken (only the shared smoke assertion updated),
  `Gtk.ContentFit` enum values real, Stretch/StretchDirection defaults match WPF, `DrawingContextRenderer`
  command replay + `ComputeBounds` union correct, VisualBrush mirrors the clean DrawingBrush (zero-size guard +
  disposed surface). Both tracks clean — no fixes required.
  **Round 28 totals: 2253 tests** (89 WindowsBase + 1945 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean.
- **Carry-over (Round 28)**: `Image.Stretch.None`→`ScaleDown` is an approximation (WPF None = natural size);
  `VisualBrush` paints only `DrawingVisual` content (general live visuals are no-paint); `DrawText` in
  `DrawingContextRenderer` contributes no bounds/render.
- **Round 29 Track A — ItemsControl alternation (DONE).** `AlternationCount` DP (int, default 0) + read-only
  attached `AlternationIndex` (`GetAlternationIndex`); each generated container gets `index % count` (count 0/1 →
  all 0, WPF parity), reassigned on `AlternationCount` change and on every items rebuild. **+17 tests**.
- **Round 29 Track B — ScrollViewer scroll API (DONE).** Read-only `HorizontalOffset`/`VerticalOffset`/`Extent*`/
  `Viewport*`/`Scrollable*` (mapped from the `Gtk.ScrolledWindow` H/V `Adjustment`), 14 scroll commands
  (`ScrollTo*`/`Line*`/`Page*`, clamped), and a `ScrollChanged` event (`ScrollChangedEventArgs` with offsets +
  extents + viewports + deltas), with a `_scrollUpdating` reentrancy guard so a programmatic scroll raises
  `ScrollChanged` exactly once (WPF parity) while external adjustment changes also raise it. **+63 tests**.
- **Round 29 review (Opus) — SHIP, no functional defects.** Verified programmatic-scroll raises ScrollChanged
  once (matches WPF), delta snapshots correct, clamp/headless/reentrancy all sound, alternation reassignment has
  no stale-index path. Added one failing-first test pinning `ScrollToVerticalOffset` raises ScrollChanged exactly
  once (closing the last-wins test gap). Carry-over (Low): `AlternationIndex` uses `RegisterReadOnly` (no
  `RegisterAttachedReadOnly` factory exists yet — functionally equivalent in this property system);
  `ScrollToHome`/`ScrollToEnd` raise one ScrollChanged per axis (WPF coalesces to one).
  **Round 29 totals: 2323 tests** (89 WindowsBase + 2015 PresentationFramework + 204 MarkupCompiler +
  15 ProjectConverter), all pass; warning-clean (verified against the committed `net10.0` TFMs).
  Note: the repo owner is concurrently editing `PresentationFramework.csproj`/`Gtk4.Forms.Wpf.csproj` TFMs
  (→ `netstandard2.0`/multi-target) in the working tree — left untouched; that WIP currently breaks Photino.NET
  restore (NU1202) independently of this round.
- **Carry-over**: `ImageBrush.Opacity` on the painted-image path (**A1**); `ImageBrush` only paints local PNGs
  (no HTTP/non-PNG); arc (`A`) and smooth (`S`/`T`) path commands are skipped, not rendered;
  `LayoutTransform` is honoured in measure (DesiredSize) but **not in arrange** — `RenderSize`
  stays the untransformed inner size (WPF applies it in arrange too); `RenderTransform`/`LayoutTransform`/`Effect`
  CSS render only on GTK 4.14+ (object-model always correct); detach `Gtk.DropTarget` on widget teardown
  (drag lifecycle leak); synchronous `DoDragDrop`
  result on the GTK path (needs a nested GLib main loop for async drag);
  rich-content ToolTip custom-widget rendering + `ToolTip.IsOpen`/`Placement` honoured natively;
  `DataFormats.StringFormat`↔UnicodeText auto-convert (**A2**); deferred adorner-overlay
  placement on realize (**B2**);
  compiler diagnostic/warning for a non-`EventTrigger` placed in `FrameworkElement.Triggers`
  (match WPF's compile-time rejection; runtime superset stays); positive Popup/ContextMenu open-replay GTK test
  under a realized parent window;
  `Gtk.PopoverMenu` integration for `MenuItem`; nested-path `DataTrigger` bindings;
  GTK4-native backings deferred for the Round-6 controls (`Gio.Menu`/`Gtk.PopoverMenuBar` for `Menu`,
  `Gtk.ListView`+`TreeExpander` for `TreeView`, `Gtk.ActionBar` for `ToolBar`);
  `WindowState` round-trip (reflect WM-initiated state changes back into the DP); `gtk_window_move`-free
  `WindowStartupLocation` (CenterScreen/CenterOwner currently store-only);
  wire computed arrange rects onto a `Gtk.Fixed` so managed layout drives live GTK positioning
  (replace Box/Grid native backing with `Gtk.Fixed` + SizeAllocate driven by the computed
  ArrangeRects — deferred until the managed math is verified stable).

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
- **Drop-in e2e**: a stock WPF repo + `Gtk4.Forms.Wpf` → cross-platform exe, no source edits.

## Risks

| Risk | Note / mitigation |
|---|---|
| **Scope — this is Avalonia-scale** | Phase 0 de-risks the architecture cheaply; everything after is incremental and demand-driven. Be explicit that "1:1" is a direction, not a date. |
| `DependencyProperty`/visual-tree semantics are subtle | Faithful property system is Phase-0, tested in isolation before anything renders. |
| WPF markup extensions + `System.Xaml` divergence | Compiler is `System.Xaml`-free (Cecil + `XmlReader`), so no Windows-only `System.Xaml` dependency; extension set grows per phase. |
| Tier-1/Tier-2 seam leaks | Keep `UIElement`→`Gtk.Widget` the single boundary; a control can move Tier 1→2 without API change. |
| `ControlTemplate`/triggers/`RichTextBox` | The genuinely hard long tail — scheduled late, flagged as "redesign, not 1:1" if a feature proves intractable on GTK. |
| Fights the active packaging refactor | New `Gtk4.Forms.*` projects are additive (own dirs); touch shared `Directory.Build.*` minimally. |

## Recommendation

Approve **Phase 0 only** first: the `WindowsBase` property-system subset + the forked markup compiler
(minimal) + four controls on Tier 1 + one binding + `Gtk4.Forms.Wpf`, proving **B1–B4**. That slice
is small, fully testable, and settles every architectural unknown (xmlns binding, the compiler fork,
the hybrid bridge, the drop-in). Only after it runs a real one-window WPF app unmodified on two OSes
is it worth committing to the multi-phase build toward 1:1.
