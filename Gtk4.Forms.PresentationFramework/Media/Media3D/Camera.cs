using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Base of the cameras.</summary>
public abstract class Camera : Animatable
{
	/// <summary>Identifies <see cref="Transform"/>.</summary>
	public static readonly DependencyProperty TransformProperty = Media3DProps.Register(
		nameof(Transform), typeof(Transform3D), typeof(Camera), Transform3D.Identity);

	ChildLinks? _links;

	internal Camera() { }

	/// <summary>A transform applied to the camera position and orientation.</summary>
	public Transform3D Transform
	{
		get => (Transform3D)GetValue(TransformProperty)!;
		set => SetValue(TransformProperty, value);
	}

	/// <summary>The world-to-camera matrix, including <see cref="Transform"/>.</summary>
	internal Matrix3D GetViewMatrix()
	{
		var view = ViewCore();
		var t = Transform;
		if (t == null) return view;
		var m = t.Value;
		if (!m.HasInverse) return view;
		m.Invert();
		return m * view;
	}

	/// <summary>The camera-to-clip matrix for a viewport of the given aspect ratio (width / height).</summary>
	internal abstract Matrix3D GetProjectionMatrix(double aspectRatio);

	internal abstract Matrix3D ViewCore();

	void RaiseChanged()
	{
		if (!IsFrozen) OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.NewValue is Freezable || e.OldValue is Freezable)
			(_links ??= new ChildLinks(RaiseChanged)).Track(e);
	}

	/// <inheritdoc/>
	protected override void OnFreeze() => Media3DProps.FreezeAll(Transform);
}
