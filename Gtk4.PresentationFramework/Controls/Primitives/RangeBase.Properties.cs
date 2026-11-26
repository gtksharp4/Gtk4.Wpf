namespace System.Windows.Controls.Primitives;

public abstract partial class RangeBase
{
	const FrameworkPropertyMetadataOptions ValueOptions =
		FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal;

	/// <summary>Identifies <see cref="Minimum"/>. Default: 0.</summary>
	public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
		nameof(Minimum), typeof(double), typeof(RangeBase),
		new FrameworkPropertyMetadata(0.0, OnMinimumPropertyChanged), IsValidDouble);

	/// <summary>Identifies <see cref="Maximum"/>. Default: 1; coerced to be at least <see cref="Minimum"/>.</summary>
	public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
		nameof(Maximum), typeof(double), typeof(RangeBase),
		new FrameworkPropertyMetadata(1.0, OnMaximumPropertyChanged, CoerceMaximumToMinimum), IsValidDouble);

	/// <summary>Identifies <see cref="Value"/>. Default: 0; coerced into [Minimum, Maximum].</summary>
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
		nameof(Value), typeof(double), typeof(RangeBase),
		new FrameworkPropertyMetadata(0.0, ValueOptions, OnValuePropertyChanged, CoerceToRange), IsValidDouble);

	/// <summary>Identifies <see cref="SmallChange"/>. Default: 0.1.</summary>
	public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(
		nameof(SmallChange), typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(0.1), IsValidChange);

	/// <summary>Identifies <see cref="LargeChange"/>. Default: 1.</summary>
	public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(
		nameof(LargeChange), typeof(double), typeof(RangeBase), new FrameworkPropertyMetadata(1.0), IsValidChange);

	/// <summary>Gets or sets the lower bound of the range.</summary>
	public double Minimum
	{
		get => (double)GetValue(MinimumProperty)!;
		set => SetValue(MinimumProperty, value);
	}

	/// <summary>Gets or sets the upper bound of the range.</summary>
	public double Maximum
	{
		get => (double)GetValue(MaximumProperty)!;
		set => SetValue(MaximumProperty, value);
	}

	/// <summary>Gets or sets the current value.</summary>
	public double Value
	{
		get => (double)GetValue(ValueProperty)!;
		set => SetValue(ValueProperty, value);
	}

	/// <summary>Gets or sets the value change of a small step (arrow keys).</summary>
	public double SmallChange
	{
		get => (double)GetValue(SmallChangeProperty)!;
		set => SetValue(SmallChangeProperty, value);
	}

	/// <summary>Gets or sets the value change of a large step (page keys).</summary>
	public double LargeChange
	{
		get => (double)GetValue(LargeChangeProperty)!;
		set => SetValue(LargeChangeProperty, value);
	}

	static bool IsValidDouble(object? value) =>
		value is double d && !double.IsNaN(d) && !double.IsInfinity(d);

	static bool IsValidChange(object? value) => IsValidDouble(value) && (double)value! >= 0.0;

	static object? CoerceMaximumToMinimum(DependencyObject d, object? baseValue)
	{
		double max = (double)baseValue!, min = ((RangeBase)d).Minimum;
		return max < min ? min : baseValue;
	}

	internal static object? CoerceToRange(DependencyObject d, object? baseValue)
	{
		var range = (RangeBase)d;
		double value = (double)baseValue!, min = range.Minimum, max = range.Maximum;
		return value < min ? min : value > max ? max : baseValue;
	}

	static void OnMinimumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var range = (RangeBase)d;
		range.CoerceValue(MaximumProperty);
		range.CoerceValue(ValueProperty);
		range.OnMinimumChanged((double)e.OldValue!, (double)e.NewValue!);
	}

	static void OnMaximumPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var range = (RangeBase)d;
		range.CoerceValue(ValueProperty);
		range.OnMaximumChanged((double)e.OldValue!, (double)e.NewValue!);
	}

	static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((RangeBase)d).OnValueChanged((double)e.OldValue!, (double)e.NewValue!);
}
