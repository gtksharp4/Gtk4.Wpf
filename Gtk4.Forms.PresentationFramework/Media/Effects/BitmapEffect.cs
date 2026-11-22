using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>
/// Base for the legacy software bitmap effects. WPF made them no-ops; here they are store-only Animatables so
/// markup and storyboards that target their properties keep working. Nothing is rendered.
/// </summary>
public abstract class BitmapEffect : Animatable
{
	/// <summary>For derived effects.</summary>
	protected BitmapEffect() { }
}

/// <summary>Applies a list of effects in order.</summary>
public sealed class BitmapEffectGroup : BitmapEffect
{
	BitmapEffectCollection _children = new BitmapEffectCollection();

	/// <summary>Creates an empty group.</summary>
	public BitmapEffectGroup() => _children.Changed += OnChildrenChanged;

	/// <summary>The grouped effects.</summary>
	public BitmapEffectCollection Children
	{
		get => _children;
		set
		{
			ThrowIfFrozen();
			_children.Changed -= OnChildrenChanged;
			_children = value ?? new BitmapEffectCollection();
			_children.Changed += OnChildrenChanged;
			OnChanged();
		}
	}

	void OnChildrenChanged(object? sender, EventArgs e) => OnChanged();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapEffectGroup();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var g = (BitmapEffectGroup)destination;
		foreach (var c in _children)
			g.Children.Add((BitmapEffect)(currentValueOnly ? c.CloneCurrentValue() : c.Clone()));
	}
}

/// <summary>Bevel edge profile.</summary>
public enum EdgeProfile { Linear = 0, CurvedIn = 1, CurvedOut = 2, BulgedUp = 3 }

/// <summary>Describes the input area of a bitmap effect.</summary>
public sealed class BitmapEffectInput : Animatable
{
	/// <summary>Identifies <see cref="Input"/>.</summary>
	public static readonly DependencyProperty InputProperty = DependencyProperty.Register(
		nameof(Input), typeof(Imaging.BitmapSource), typeof(BitmapEffectInput), new PropertyMetadata(null));

	/// <summary>Identifies <see cref="AreaToApplyEffectUnits"/>.</summary>
	public static readonly DependencyProperty AreaToApplyEffectUnitsProperty = DependencyProperty.Register(
		nameof(AreaToApplyEffectUnits), typeof(BrushMappingMode), typeof(BitmapEffectInput),
		new PropertyMetadata(BrushMappingMode.RelativeToBoundingBox));

	/// <summary>Identifies <see cref="AreaToApplyEffect"/>.</summary>
	public static readonly DependencyProperty AreaToApplyEffectProperty = DependencyProperty.Register(
		nameof(AreaToApplyEffect), typeof(Rect), typeof(BitmapEffectInput), new PropertyMetadata(Rect.Empty));

	/// <summary>Creates an input with no source.</summary>
	public BitmapEffectInput() { }

	/// <summary>Creates an input over <paramref name="input"/>.</summary>
	public BitmapEffectInput(Imaging.BitmapSource input) => Input = input;

	/// <summary>The source bitmap.</summary>
	public Imaging.BitmapSource? Input
	{
		get => (Imaging.BitmapSource?)GetValue(InputProperty);
		set => SetValue(InputProperty, value);
	}

	/// <summary>Coordinate space of <see cref="AreaToApplyEffect"/>.</summary>
	public BrushMappingMode AreaToApplyEffectUnits
	{
		get => (BrushMappingMode)GetValue(AreaToApplyEffectUnitsProperty)!;
		set => SetValue(AreaToApplyEffectUnitsProperty, value);
	}

	/// <summary>The region the effect applies to.</summary>
	public Rect AreaToApplyEffect
	{
		get => (Rect)GetValue(AreaToApplyEffectProperty)!;
		set => SetValue(AreaToApplyEffectProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapEffectInput();
}
