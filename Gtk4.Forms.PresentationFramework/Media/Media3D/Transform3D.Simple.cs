namespace System.Windows.Media.Media3D;

/// <summary>Translates 3D content.</summary>
public sealed class TranslateTransform3D : Transform3D
{
	/// <summary>Identifies <see cref="OffsetX"/>.</summary>
	public static readonly DependencyProperty OffsetXProperty =
		Media3DProps.Register(nameof(OffsetX), typeof(double), typeof(TranslateTransform3D), 0.0);

	/// <summary>Identifies <see cref="OffsetY"/>.</summary>
	public static readonly DependencyProperty OffsetYProperty =
		Media3DProps.Register(nameof(OffsetY), typeof(double), typeof(TranslateTransform3D), 0.0);

	/// <summary>Identifies <see cref="OffsetZ"/>.</summary>
	public static readonly DependencyProperty OffsetZProperty =
		Media3DProps.Register(nameof(OffsetZ), typeof(double), typeof(TranslateTransform3D), 0.0);

	/// <summary>Creates an identity translation.</summary>
	public TranslateTransform3D() { }

	/// <summary>Creates a translation by <paramref name="offset"/>.</summary>
	public TranslateTransform3D(Vector3D offset)
	{
		OffsetX = offset.X;
		OffsetY = offset.Y;
		OffsetZ = offset.Z;
	}

	/// <summary>Creates a translation.</summary>
	public TranslateTransform3D(double offsetX, double offsetY, double offsetZ)
	{
		OffsetX = offsetX;
		OffsetY = offsetY;
		OffsetZ = offsetZ;
	}

	/// <summary>The X offset.</summary>
	public double OffsetX
	{
		get => (double)GetValue(OffsetXProperty)!;
		set => SetValue(OffsetXProperty, value);
	}

	/// <summary>The Y offset.</summary>
	public double OffsetY
	{
		get => (double)GetValue(OffsetYProperty)!;
		set => SetValue(OffsetYProperty, value);
	}

	/// <summary>The Z offset.</summary>
	public double OffsetZ
	{
		get => (double)GetValue(OffsetZProperty)!;
		set => SetValue(OffsetZProperty, value);
	}

	/// <inheritdoc/>
	public override Matrix3D Value
	{
		get
		{
			var m = Matrix3D.Identity;
			m.Translate(new Vector3D(OffsetX, OffsetY, OffsetZ));
			return m;
		}
	}

	/// <inheritdoc/>
	public override bool IsAffine => true;

	/// <summary>Creates a modifiable clone.</summary>
	public new TranslateTransform3D Clone() => (TranslateTransform3D)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TranslateTransform3D();
}
