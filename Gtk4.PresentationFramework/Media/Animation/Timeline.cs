namespace System.Windows.Media.Animation;

/// <summary>
/// Base class for all timelines. Mirrors WPF's <c>System.Windows.Media.Animation.Timeline</c>:
/// the timing properties are dependency properties and a timeline creates <see cref="Clock"/>s.
/// </summary>
public abstract partial class Timeline : Animatable
{
	/// <summary>Identifies <see cref="BeginTime"/>. Default <see cref="TimeSpan.Zero"/>.</summary>
	public static readonly DependencyProperty BeginTimeProperty = DependencyProperty.Register(
		nameof(BeginTime), typeof(TimeSpan?), typeof(Timeline), new PropertyMetadata(TimeSpan.Zero));

	/// <summary>Identifies <see cref="Duration"/>. Default <see cref="Duration.Automatic"/>.</summary>
	public static readonly DependencyProperty DurationProperty = DependencyProperty.Register(
		nameof(Duration), typeof(Duration), typeof(Timeline), new PropertyMetadata(Duration.Automatic));

	/// <summary>Identifies <see cref="AutoReverse"/>.</summary>
	public static readonly DependencyProperty AutoReverseProperty = DependencyProperty.Register(
		nameof(AutoReverse), typeof(bool), typeof(Timeline), new PropertyMetadata(false));

	/// <summary>Identifies <see cref="RepeatBehavior"/>. Default 1x.</summary>
	public static readonly DependencyProperty RepeatBehaviorProperty = DependencyProperty.Register(
		nameof(RepeatBehavior), typeof(RepeatBehavior), typeof(Timeline),
		new PropertyMetadata(new RepeatBehavior(1)));

	/// <summary>Identifies <see cref="FillBehavior"/>. Default <see cref="FillBehavior.HoldEnd"/>.</summary>
	public static readonly DependencyProperty FillBehaviorProperty = DependencyProperty.Register(
		nameof(FillBehavior), typeof(FillBehavior), typeof(Timeline), new PropertyMetadata(FillBehavior.HoldEnd));

	/// <summary>Identifies <see cref="SpeedRatio"/>. Default 1; must be finite and positive.</summary>
	public static readonly DependencyProperty SpeedRatioProperty = DependencyProperty.Register(
		nameof(SpeedRatio), typeof(double), typeof(Timeline), new PropertyMetadata(1.0),
		v => v is double d && d > 0 && !double.IsInfinity(d));

	/// <summary>Identifies <see cref="AccelerationRatio"/>. Default 0; range 0..1.</summary>
	public static readonly DependencyProperty AccelerationRatioProperty = DependencyProperty.Register(
		nameof(AccelerationRatio), typeof(double), typeof(Timeline), new PropertyMetadata(0.0), IsRatio);

	/// <summary>Identifies <see cref="DecelerationRatio"/>. Default 0; range 0..1.</summary>
	public static readonly DependencyProperty DecelerationRatioProperty = DependencyProperty.Register(
		nameof(DecelerationRatio), typeof(double), typeof(Timeline), new PropertyMetadata(0.0), IsRatio);

	/// <summary>Identifies <see cref="Name"/>.</summary>
	public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
		nameof(Name), typeof(string), typeof(Timeline), new PropertyMetadata(null));

	static bool IsRatio(object? v) => v is double d && d >= 0 && d <= 1;

	/// <summary>Time to wait before the timeline starts; null means it never starts.</summary>
	public TimeSpan? BeginTime
	{
		get => (TimeSpan?)GetValue(BeginTimeProperty);
		set => SetValue(BeginTimeProperty, value);
	}

	/// <summary>Length of one iteration; Automatic takes the natural duration.</summary>
	public Duration Duration
	{
		get => (Duration)GetValue(DurationProperty)!;
		set => SetValue(DurationProperty, value);
	}

	/// <summary>Whether the timeline plays backwards after each forward iteration.</summary>
	public bool AutoReverse
	{
		get => (bool)GetValue(AutoReverseProperty)!;
		set => SetValue(AutoReverseProperty, value);
	}

	/// <summary>How many times, or for how long, the timeline repeats.</summary>
	public RepeatBehavior RepeatBehavior
	{
		get => (RepeatBehavior)GetValue(RepeatBehaviorProperty)!;
		set => SetValue(RepeatBehaviorProperty, value);
	}

	/// <summary>What happens when the active period ends.</summary>
	public FillBehavior FillBehavior
	{
		get => (FillBehavior)GetValue(FillBehaviorProperty)!;
		set => SetValue(FillBehaviorProperty, value);
	}

	/// <summary>Rate relative to the parent timeline.</summary>
	public double SpeedRatio
	{
		get => (double)GetValue(SpeedRatioProperty)!;
		set => SetValue(SpeedRatioProperty, value);
	}

	/// <summary>Fraction of the duration spent speeding up.</summary>
	public double AccelerationRatio
	{
		get => (double)GetValue(AccelerationRatioProperty)!;
		set => SetValue(AccelerationRatioProperty, value);
	}

	/// <summary>Fraction of the duration spent slowing down.</summary>
	public double DecelerationRatio
	{
		get => (double)GetValue(DecelerationRatioProperty)!;
		set => SetValue(DecelerationRatioProperty, value);
	}

	/// <summary>Name of the timeline.</summary>
	public string? Name
	{
		get => (string?)GetValue(NameProperty);
		set => SetValue(NameProperty, value);
	}
}
