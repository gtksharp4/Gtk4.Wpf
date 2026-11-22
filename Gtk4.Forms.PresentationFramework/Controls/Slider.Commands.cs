using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>
/// Slider commands and auto-tooltip settings. The commands are handled by class-level
/// <see cref="UIElement.CommandBindings"/>; GTK's <see cref="Gtk.Scale"/> has no auto tooltip, so
/// <see cref="AutoToolTipPlacement"/> and <see cref="AutoToolTipPrecision"/> are stored but not shown.
/// </summary>
public partial class Slider
{
	/// <summary>Increases <see cref="Value"/> by <see cref="LargeChange"/>.</summary>
	public static RoutedCommand IncreaseLarge { get; } = new RoutedCommand(nameof(IncreaseLarge), typeof(Slider));

	/// <summary>Decreases <see cref="Value"/> by <see cref="LargeChange"/>.</summary>
	public static RoutedCommand DecreaseLarge { get; } = new RoutedCommand(nameof(DecreaseLarge), typeof(Slider));

	/// <summary>Increases <see cref="Value"/> by <see cref="SmallChange"/>.</summary>
	public static RoutedCommand IncreaseSmall { get; } = new RoutedCommand(nameof(IncreaseSmall), typeof(Slider));

	/// <summary>Decreases <see cref="Value"/> by <see cref="SmallChange"/>.</summary>
	public static RoutedCommand DecreaseSmall { get; } = new RoutedCommand(nameof(DecreaseSmall), typeof(Slider));

	/// <summary>Sets <see cref="Value"/> to <see cref="Minimum"/>.</summary>
	public static RoutedCommand MinimizeValue { get; } = new RoutedCommand(nameof(MinimizeValue), typeof(Slider));

	/// <summary>Sets <see cref="Value"/> to <see cref="Maximum"/>.</summary>
	public static RoutedCommand MaximizeValue { get; } = new RoutedCommand(nameof(MaximizeValue), typeof(Slider));

	static Slider()
	{
		OverrideRangeMetadata();
		Register(IncreaseLarge, s => s.Value + s.LargeChange, Key.PageUp);
		Register(DecreaseLarge, s => s.Value - s.LargeChange, Key.PageDown);
		Register(IncreaseSmall, s => s.Value + s.SmallChange, Key.Up, Key.Right);
		Register(DecreaseSmall, s => s.Value - s.SmallChange, Key.Down, Key.Left);
		Register(MinimizeValue, s => s.Minimum, Key.Home);
		Register(MaximizeValue, s => s.Maximum, Key.End);
	}

	static void Register(RoutedCommand command, Func<Slider, double> target, params Key[] keys)
	{
		CommandManager.RegisterClassCommandBinding(typeof(Slider), new CommandBinding(command,
			(sender, _) => ((Slider)sender).Value = target((Slider)sender),
			(_, e) => e.CanExecute = true));
		foreach (var key in keys)
			CommandManager.RegisterClassInputBinding(typeof(Slider), new KeyBinding(command, key, ModifierKeys.None));
	}

	/// <summary>Identifies <see cref="AutoToolTipPlacement"/>. Default: <c>None</c>.</summary>
	public static readonly DependencyProperty AutoToolTipPlacementProperty =
		DependencyProperty.Register(nameof(AutoToolTipPlacement), typeof(AutoToolTipPlacement), typeof(Slider),
			new PropertyMetadata(AutoToolTipPlacement.None),
			v => v is AutoToolTipPlacement p
				&& p >= AutoToolTipPlacement.None && p <= AutoToolTipPlacement.BottomRight);

	/// <summary>Gets or sets where the auto tooltip would appear. Stored only on GTK.</summary>
	public AutoToolTipPlacement AutoToolTipPlacement
	{
		get => (AutoToolTipPlacement)GetValue(AutoToolTipPlacementProperty)!;
		set => SetValue(AutoToolTipPlacementProperty, value);
	}

	/// <summary>Identifies <see cref="AutoToolTipPrecision"/>. Default: 0.</summary>
	public static readonly DependencyProperty AutoToolTipPrecisionProperty =
		DependencyProperty.Register(nameof(AutoToolTipPrecision), typeof(int), typeof(Slider),
			new PropertyMetadata(0), v => v is int i && i >= 0);

	/// <summary>Gets or sets the decimal places of the auto tooltip value. Stored only on GTK.</summary>
	public int AutoToolTipPrecision
	{
		get => (int)GetValue(AutoToolTipPrecisionProperty)!;
		set => SetValue(AutoToolTipPrecisionProperty, value);
	}
}
