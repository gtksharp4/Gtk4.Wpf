using System.Collections;

namespace System.Windows.Media.Animation;

/// <summary>A key frame of a key-frame animation.</summary>
public interface IKeyFrame
{
	/// <summary>When the frame ends.</summary>
	KeyTime KeyTime { get; set; }

	/// <summary>The value of the frame.</summary>
	object Value { get; set; }
}

/// <summary>An animation driven by key frames.</summary>
public interface IKeyFrameAnimation
{
	/// <summary>The key frames.</summary>
	IList KeyFrames { get; set; }
}

/// <summary>A segment of a <see cref="DoubleAnimationUsingKeyFrames"/>.</summary>
public abstract class DoubleKeyFrame : Freezable, IKeyFrame
{
	/// <summary>Identifies <see cref="KeyTime"/>.</summary>
	public static readonly DependencyProperty KeyTimeProperty = DependencyProperty.Register(
		nameof(KeyTime), typeof(KeyTime), typeof(DoubleKeyFrame), new PropertyMetadata(KeyTime.Uniform));

	/// <summary>Identifies <see cref="Value"/>.</summary>
	public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
		nameof(Value), typeof(double), typeof(DoubleKeyFrame), new PropertyMetadata(0.0));

	/// <summary>Creates a frame with a default value and key time.</summary>
	protected DoubleKeyFrame() { }

	/// <summary>Creates a frame with a value.</summary>
	protected DoubleKeyFrame(double value) => Value = value;

	/// <summary>Creates a frame with a value and key time.</summary>
	protected DoubleKeyFrame(double value, KeyTime keyTime)
	{
		Value = value;
		KeyTime = keyTime;
	}

	/// <summary>When the frame ends.</summary>
	public KeyTime KeyTime
	{
		get => (KeyTime)GetValue(KeyTimeProperty)!;
		set => SetValue(KeyTimeProperty, value);
	}

	/// <summary>The value at the end of the frame.</summary>
	public double Value
	{
		get => (double)GetValue(ValueProperty)!;
		set => SetValue(ValueProperty, value);
	}

	object IKeyFrame.Value
	{
		get => Value;
		set => Value = (double)value;
	}

	/// <summary>The interpolated value for a progress through this frame.</summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="keyFrameProgress"/> is outside [0, 1].</exception>
	public double InterpolateValue(double baseValue, double keyFrameProgress)
	{
		if (keyFrameProgress < 0.0 || keyFrameProgress > 1.0)
			throw new ArgumentOutOfRangeException(nameof(keyFrameProgress));
		return InterpolateValueCore(baseValue, keyFrameProgress);
	}

	/// <summary>Computes the interpolated value.</summary>
	protected abstract double InterpolateValueCore(double baseValue, double keyFrameProgress);
}

/// <summary>Jumps to its value when the frame ends.</summary>
public class DiscreteDoubleKeyFrame : DoubleKeyFrame
{
	/// <summary>Creates a frame.</summary>
	public DiscreteDoubleKeyFrame() { }

	/// <summary>Creates a frame.</summary>
	public DiscreteDoubleKeyFrame(double value) : base(value) { }

	/// <summary>Creates a frame.</summary>
	public DiscreteDoubleKeyFrame(double value, KeyTime keyTime) : base(value, keyTime) { }

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DiscreteDoubleKeyFrame();

	/// <inheritdoc/>
	protected override double InterpolateValueCore(double baseValue, double keyFrameProgress) =>
		keyFrameProgress < 1.0 ? baseValue : Value;
}

/// <summary>Interpolates linearly to its value.</summary>
public class LinearDoubleKeyFrame : DoubleKeyFrame
{
	/// <summary>Creates a frame.</summary>
	public LinearDoubleKeyFrame() { }

	/// <summary>Creates a frame.</summary>
	public LinearDoubleKeyFrame(double value) : base(value) { }

	/// <summary>Creates a frame.</summary>
	public LinearDoubleKeyFrame(double value, KeyTime keyTime) : base(value, keyTime) { }

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new LinearDoubleKeyFrame();

	/// <inheritdoc/>
	protected override double InterpolateValueCore(double baseValue, double keyFrameProgress) =>
		baseValue + (Value - baseValue) * keyFrameProgress;
}
