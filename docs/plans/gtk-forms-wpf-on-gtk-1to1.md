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
| Delivery | A one-package drop-in, **`Gtk.Forms.Wpf`**, reusing the `Gtk4.Forms.Base` pattern just built |

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

### F. Drop-in delivery (`Gtk.Forms.Wpf`)

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
| B4 | The drop-in works | A stock WPF project + `<PackageReference Include="Gtk.Forms.Wpf">` builds to a cross-platform exe (two-build conversion) and runs |

Phase 0 = `WindowsBase` property system (subset) + the forked markup compiler (minimal) + `Window`/`Grid`/`TextBlock`/`Button` on Tier 1 + a one-way `Binding` + `Gtk.Forms.Wpf`. One window, one binding, running on two OSes.

## Progress (living)

- **Phase 0 — DONE.** `Gtk.Forms.WindowsBase` (property/event system), `Gtk.Forms.MarkupCompiler`
  (build-time compiler), `Gtk.Forms.PresentationFramework` (`UIElement`→`Window`/`Grid`/`StackPanel`/
  `TextBlock`/`Button` + one-way binding on GtkSharp 4), `Gtk.Forms.ProjectConverter` + `Gtk.Forms.Wpf`
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
- **Round 6 review (Opus) + fixes — DONE.** Review found 1 Critical (masked by Contains-only tests) + fixes:
  keyed `<ControlTemplate>`/`<DataTemplate>` in a `.Resources` block was processed twice, emitting an
  uncompilable `this.Resources = …` and dropping sibling templates — `EmitPropertyElement` now returns for
  the `Resources` property-element (owned solely by `EmitResources`); pinned by tests asserting both keyed
  entries present and **no** `Resources =` assignment. Also: `MenuItem` now raises `Checked` only on
  false→true and `Unchecked` on true→false (added `Unchecked`); `MenuItem` split into `MenuItem.cs` +
  `MenuItem.Commanding.cs` (≤200 lines); `FrameworkTemplate.HasContent` and `UIElement.IsWidgetCreated`
  converted from `new`-shadows to `virtual`/`override` so a base-typed reference sees the correct value.
  **Round 6 totals: 787 tests** (40 WindowsBase + 642 PresentationFramework + 89 MarkupCompiler +
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample` (unmodified WPF) compiles.
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
  + 104 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample`
  (unmodified WPF) still compiles.
- **Round 8 Track B — animation & storyboards runtime (DONE).** `System.Windows.Media.Animation`:
  - `Duration` (finite/Forever/Automatic), `RepeatBehavior` (count/duration/Forever), `FillBehavior` (HoldEnd/Stop).
  - `Timeline` (abstract; `Duration`/`BeginTime`/`AutoReverse`/`RepeatBehavior`/`FillBehavior`) +
    `AnimationTimeline` (abstract; `GetCurrentValue(origin, dest, clock)`).
  - **Animation value layer** added to `DependencyObject` (D10) above local in
    `ResolveEffectiveValue`: `SetAnimationValue(dp,v)` / `ClearAnimationValue(dp)` in
    `Gtk.Forms.WindowsBase/DependencyObject.Animation.cs` (new partial file).
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
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample` (unmodified WPF) compiles.
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
  132 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample`
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
  16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample` (unmodified WPF) compiles.
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
    (`ITaskItem[]`); `Gtk.Forms.MarkupCompiler.targets` passes `@(ReferencePath)` to it; the task
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
  158 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample`
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
  all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample` (unmodified WPF) compiles through the lock-free
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
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed with no hang/crash; `Gtk.Forms.WpfSample`
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
  all pass timeboxed; `Gtk.Forms.WpfSample` (unmodified WPF) still compiles.
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
  169 MarkupCompiler + 16 ProjectConverter), all pass timeboxed; `Gtk.Forms.WpfSample` (unmodified WPF) compiles.
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
  15 ProjectConverter — the tree also compiles concurrent imaging/markup-NuGet work landed in `7a0e25e1`),
  all pass timeboxed; `Gtk.Forms.WpfSample` (unmodified WPF) still compiles.
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
- **Carry-over**: attribute-driven `[XmlnsDefinition]` resolution (replace the built-in xmlns map);
  true `{DynamicResource}` change-tracking;
  `Gtk.PopoverMenu` integration for `MenuItem`; element-level `FrameworkElement.Triggers` auto-attach on
  load (runtime exists, compiler emits `Style.Triggers` only); nested-path `DataTrigger` bindings;
  `Popup`/`ContextMenu` replay of an `IsOpen=true` set before the backing widget exists (currently dropped);
  GTK4-native backings deferred for the Round-6 controls (`Gio.Menu`/`Gtk.PopoverMenuBar` for `Menu`,
  `Gtk.ListView`+`TreeExpander` for `TreeView`, `Gtk.ActionBar` for `ToolBar`);
  `VisualStateManager.VisualStateGroups` XAML compilation (VSM groups and states still set in code-behind only);
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
- **Drop-in e2e**: a stock WPF repo + `Gtk.Forms.Wpf` → cross-platform exe, no source edits.

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
(minimal) + four controls on Tier 1 + one binding + `Gtk.Forms.Wpf`, proving **B1–B4**. That slice
is small, fully testable, and settles every architectural unknown (xmlns binding, the compiler fork,
the hybrid bridge, the drop-in). Only after it runs a real one-window WPF app unmodified on two OSes
is it worth committing to the multi-phase build toward 1:1.
