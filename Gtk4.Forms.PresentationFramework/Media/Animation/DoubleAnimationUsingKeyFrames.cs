using System.Collections;
using System.Windows.Markup;

namespace System.Windows.Media.Animation;

/// <summary>Animates a double through a series of key frames.</summary>
[ContentProperty("KeyFrames")]
public partial class DoubleAnimationUsingKeyFrames : DoubleAnimationBase, IKeyFrameAnimation, IAddChild
{
	/// <summary>Identifies <see cref="IsAdditive"/>.</summary>
	public static readonly DependencyProperty IsAdditiveProperty = DependencyProperty.Register(
		nameof(IsAdditive), typeof(bool), typeof(DoubleAnimationUsingKeyFrames), new PropertyMetadata(false));

	/// <summary>Identifies <see cref="IsCumulative"/>.</summary>
	public static readonly DependencyProperty IsCumulativeProperty = DependencyProperty.Register(
		nameof(IsCumulative), typeof(bool), typeof(DoubleAnimationUsingKeyFrames), new PropertyMetadata(false));

	DoubleKeyFrameCollection? _keyFrames;

	/// <summary>Creates an animation without key frames.</summary>
	public DoubleAnimationUsingKeyFrames() { }

	/// <summary>The key frames.</summary>
	public DoubleKeyFrameCollection KeyFrames
	{
		get
		{
			if (_keyFrames == null)
			{
				if (IsFrozen) return DoubleKeyFrameCollection.Empty;
				KeyFrames = new DoubleKeyFrameCollection();
			}
			return _keyFrames!;
		}
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			ThrowIfFrozen();
			if (_keyFrames != null) _keyFrames.Changed -= OnKeyFramesChanged;
			_keyFrames = value;
			if (!value.IsFrozen) value.Changed += OnKeyFramesChanged;
			OnChanged();
		}
	}

	IList IKeyFrameAnimation.KeyFrames
	{
		get => KeyFrames;
		set => KeyFrames = (DoubleKeyFrameCollection)value;
	}

	/// <summary>Whether the animated value is added to the base value.</summary>
	public bool IsAdditive
	{
		get => (bool)GetValue(IsAdditiveProperty)!;
		set => SetValue(IsAdditiveProperty, value);
	}

	/// <summary>Whether each repeat adds to the result of the previous one.</summary>
	public bool IsCumulative
	{
		get => (bool)GetValue(IsCumulativeProperty)!;
		set => SetValue(IsCumulativeProperty, value);
	}

	void OnKeyFramesChanged(object? sender, EventArgs e)
	{
		if (!IsFrozen) OnChanged();
	}

	void IAddChild.AddChild(object value)
	{
		if (!(value is DoubleKeyFrame frame))
			throw new InvalidOperationException("Only DoubleKeyFrame children are allowed.");
		KeyFrames.Add(frame);
	}

	void IAddChild.AddText(string text)
	{
		if (!string.IsNullOrWhiteSpace(text))
			throw new InvalidOperationException("Text content is not allowed in a key-frame animation.");
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new DoubleAnimationUsingKeyFrames Clone() => (DoubleAnimationUsingKeyFrames)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DoubleAnimationUsingKeyFrames();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		if (_keyFrames == null) return;
		((DoubleAnimationUsingKeyFrames)destination).KeyFrames =
			(DoubleKeyFrameCollection)(currentValueOnly ? _keyFrames.CloneCurrentValue() : _keyFrames.Clone());
	}

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		base.OnFreeze();
		_keyFrames?.Freeze();
	}
}
