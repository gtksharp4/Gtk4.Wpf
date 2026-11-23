using System;
using System.Windows.Data;

namespace System.Windows;

/// <summary>
/// Extends <see cref="UIElement"/> with WPF framework-level features: Name,
/// DataContext, Resources, Width/Height, Margin, and data binding.
/// </summary>
public abstract partial class FrameworkElement : UIElement
{
	// ── Name ──────────────────────────────────────────────────────────────────

	/// <summary>Name used by <see cref="FindName"/> and x:Name in XAML.</summary>
	public string Name { get; set; } = string.Empty;

	// ── DataContext ─────────────────────────────────────────────────────────

	/// <summary>Inheriting DP for the data-binding source; propagates to children.</summary>
	public static readonly DependencyProperty DataContextProperty =
		DependencyProperty.Register(
			nameof(DataContext),
			typeof(object),
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(null, inherits: true,
				affectsMeasure: false, affectsArrange: false, affectsRender: false,
				propertyChangedCallback: OnDataContextChanged));

	static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is not FrameworkElement fe) return;
		fe.OnDataContextChanged(e.OldValue, e.NewValue);
		fe.RaiseDataContextChanged(e);
	}

	/// <summary>
	/// Called when this element's effective <see cref="DataContext"/> changes (due to a
	/// local set or inherited propagation). Re-evaluates bindings on this element.
	/// </summary>
	protected virtual void OnDataContextChanged(object? oldValue, object? newValue)
	{
		BindingEngine.RefreshBindings(this);
		MultiBindingEngine.RefreshDataContextBindings(this);
	}

	/// <summary>Gets or sets the data context.</summary>
	public object? DataContext
	{
		get => GetValue(DataContextProperty);
		set => SetValue(DataContextProperty, value);
	}

	// ── Resources ────────────────────────────────────────────────────────────

	/// <summary>Local resource dictionary for this element.</summary>
	public ResourceDictionary Resources => _resources ??= NewResources();

	// ── FindName ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Looks for a named element in the logical subtree rooted here.
	/// Phase-0: linear scan of the name-scope registered via XAML codegen.
	/// </summary>
	public virtual object? FindName(string name)
	{
		if (string.IsNullOrEmpty(name)) return null;
		return NameScope.FindName(this, name);
	}

	/// <summary>
	/// Registers a named child into this element's name scope (called from generated
	/// <c>InitializeComponent</c>).
	/// </summary>
	public void RegisterName(string name, object scopedElement)
	{
		NameScope.Register(this, name, scopedElement);
	}

	// ── Layout properties ────────────────────────────────────────────────────

	/// <summary>Explicit width override (<c>double.NaN</c> = auto).</summary>
	public static readonly DependencyProperty WidthProperty =
		DependencyProperty.Register(nameof(Width), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(double.NaN,
				inherits: false, affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Explicit height override (<c>double.NaN</c> = auto).</summary>
	public static readonly DependencyProperty HeightProperty =
		DependencyProperty.Register(nameof(Height), typeof(double), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(double.NaN,
				inherits: false, affectsMeasure: true, affectsArrange: false, affectsRender: false));

	/// <summary>Gets or sets the explicit width. <c>double.NaN</c> = auto.</summary>
	public double Width
	{
		get => (double)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}

	/// <summary>Gets or sets the explicit height. <c>double.NaN</c> = auto.</summary>
	public double Height
	{
		get => (double)GetValue(HeightProperty)!;
		set => SetValue(HeightProperty, value);
	}

	/// <summary>Outer margin applied by the layout system.</summary>
	public static readonly DependencyProperty MarginProperty =
		DependencyProperty.Register(nameof(Margin), typeof(Thickness), typeof(FrameworkElement),
			new FrameworkPropertyMetadata(new Thickness(0),
				inherits: false, affectsMeasure: true, affectsArrange: true, affectsRender: false));

	/// <summary>Gets or sets the outer margin.</summary>
	public Thickness Margin
	{
		get => (Thickness)GetValue(MarginProperty)!;
		set => SetValue(MarginProperty, value);
	}

}