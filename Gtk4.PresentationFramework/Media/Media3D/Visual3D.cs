using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Receives the redraw requests of a 3D visual subtree.</summary>
interface IVisual3DOwner
{
	void Invalidate3D();
}

/// <summary>Base of the 3D visuals. Mirrors <c>System.Windows.Media.Media3D.Visual3D</c>.</summary>
public abstract class Visual3D : DependencyObject, IAnimatable
{
	/// <summary>Identifies <see cref="Transform"/>.</summary>
	public static readonly DependencyProperty TransformProperty = Media3DProps.Register(
		nameof(Transform), typeof(Transform3D), typeof(Visual3D), Transform3D.Identity);

	FreezableChildWatcher? _watcher;

	internal Visual3D() { }

	internal IVisual3DOwner? Owner { get; set; }

	/// <summary>The transform applied to the visual.</summary>
	public Transform3D Transform
	{
		get => (Transform3D)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <inheritdoc/>
	public bool HasAnimatedProperties => AnimationStore.HasAnimatedProperties(this);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation) =>
		BeginAnimation(dp, animation, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void BeginAnimation(DependencyProperty dp, AnimationTimeline? animation, HandoffBehavior handoffBehavior) =>
		AnimationStore.BeginAnimation(this, dp, animation, handoffBehavior);

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock) =>
		ApplyAnimationClock(dp, clock, HandoffBehavior.SnapshotAndReplace);

	/// <inheritdoc/>
	public void ApplyAnimationClock(DependencyProperty dp, AnimationClock? clock, HandoffBehavior handoffBehavior) =>
		AnimationStore.ApplyClock(this, dp, clock, handoffBehavior);

	/// <inheritdoc/>
	public object? GetAnimationBaseValue(DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		return ResolveBaseValue(dp);
	}

	internal void Invalidate() => Owner?.Invalidate3D();

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			(_watcher ??= new FreezableChildWatcher(Invalidate)).Set(e.Property, e.NewValue);
		Invalidate();
	}
}

/// <summary>A 3D visual that shows a <see cref="Model3D"/> and child visuals.</summary>
[ContentProperty("Children")]
public class ModelVisual3D : Visual3D, IVisual3DOwner
{
	/// <summary>Identifies <see cref="Content"/>.</summary>
	public static readonly DependencyProperty ContentProperty =
		Media3DProps.Register(nameof(Content), typeof(Model3D), typeof(ModelVisual3D), null);

	/// <summary>Creates an empty visual.</summary>
	public ModelVisual3D() => Children = new Visual3DCollection(this);

	/// <summary>The model shown by this visual.</summary>
	public Model3D? Content
	{
		get => (Model3D?)GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	/// <summary>The child visuals.</summary>
	public Visual3DCollection Children { get; }

	void IVisual3DOwner.Invalidate3D() => Invalidate();
}
