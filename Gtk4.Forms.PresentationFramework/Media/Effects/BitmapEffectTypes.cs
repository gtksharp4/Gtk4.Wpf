namespace System.Windows.Media.Effects;

/// <summary>Bevel effect (store-only).</summary>
public sealed class BevelBitmapEffect : BitmapEffect
{
	/// <summary>Identifies <see cref="BevelWidth"/>.</summary>
	public static readonly DependencyProperty BevelWidthProperty = DependencyProperty.Register(
		nameof(BevelWidth), typeof(double), typeof(BevelBitmapEffect), new PropertyMetadata(5.0));
	/// <summary>Identifies <see cref="Relief"/>.</summary>
	public static readonly DependencyProperty ReliefProperty = DependencyProperty.Register(
		nameof(Relief), typeof(double), typeof(BevelBitmapEffect), new PropertyMetadata(0.3));
	/// <summary>Identifies <see cref="LightAngle"/>.</summary>
	public static readonly DependencyProperty LightAngleProperty = DependencyProperty.Register(
		nameof(LightAngle), typeof(double), typeof(BevelBitmapEffect), new PropertyMetadata(135.0));
	/// <summary>Identifies <see cref="Smoothness"/>.</summary>
	public static readonly DependencyProperty SmoothnessProperty = DependencyProperty.Register(
		nameof(Smoothness), typeof(double), typeof(BevelBitmapEffect), new PropertyMetadata(0.2));
	/// <summary>Identifies <see cref="EdgeProfile"/>.</summary>
	public static readonly DependencyProperty EdgeProfileProperty = DependencyProperty.Register(
		nameof(EdgeProfile), typeof(EdgeProfile), typeof(BevelBitmapEffect), new PropertyMetadata(EdgeProfile.Linear));

	/// <summary>Bevel width. Default 5.</summary>
	public double BevelWidth
	{
		get => (double)GetValue(BevelWidthProperty)!;
		set => SetValue(BevelWidthProperty, value);
	}

	/// <summary>Relief. Default 0.3.</summary>
	public double Relief { get => (double)GetValue(ReliefProperty)!; set => SetValue(ReliefProperty, value); }

	/// <summary>Light angle. Default 135.</summary>
	public double LightAngle
	{
		get => (double)GetValue(LightAngleProperty)!;
		set => SetValue(LightAngleProperty, value);
	}

	/// <summary>Smoothness. Default 0.2.</summary>
	public double Smoothness
	{
		get => (double)GetValue(SmoothnessProperty)!;
		set => SetValue(SmoothnessProperty, value);
	}

	/// <summary>Edge profile. Default Linear.</summary>
	public EdgeProfile EdgeProfile
	{
		get => (EdgeProfile)GetValue(EdgeProfileProperty)!;
		set => SetValue(EdgeProfileProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BevelBitmapEffect();
}

/// <summary>Blur effect (store-only).</summary>
public sealed class BlurBitmapEffect : BitmapEffect
{
	/// <summary>Identifies <see cref="Radius"/>.</summary>
	public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(
		nameof(Radius), typeof(double), typeof(BlurBitmapEffect), new PropertyMetadata(5.0));
	/// <summary>Identifies <see cref="KernelType"/>.</summary>
	public static readonly DependencyProperty KernelTypeProperty = DependencyProperty.Register(
		nameof(KernelType), typeof(KernelType), typeof(BlurBitmapEffect), new PropertyMetadata(KernelType.Gaussian));

	/// <summary>Blur radius. Default 5.</summary>
	public double Radius { get => (double)GetValue(RadiusProperty)!; set => SetValue(RadiusProperty, value); }

	/// <summary>Kernel. Default Gaussian.</summary>
	public KernelType KernelType
	{
		get => (KernelType)GetValue(KernelTypeProperty)!;
		set => SetValue(KernelTypeProperty, value);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BlurBitmapEffect();
}

/// <summary>Drop shadow effect (store-only).</summary>
public sealed class DropShadowBitmapEffect : BitmapEffect
{
	/// <summary>Identifies <see cref="ShadowDepth"/>.</summary>
	public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register(
		nameof(ShadowDepth), typeof(double), typeof(DropShadowBitmapEffect), new PropertyMetadata(5.0));
	/// <summary>Identifies <see cref="Color"/>.</summary>
	public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
		nameof(Color), typeof(Color), typeof(DropShadowBitmapEffect), new PropertyMetadata(Colors.Black));
	/// <summary>Identifies <see cref="Direction"/>.</summary>
	public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
		nameof(Direction), typeof(double), typeof(DropShadowBitmapEffect), new PropertyMetadata(315.0));
	/// <summary>Identifies <see cref="Noise"/>.</summary>
	public static readonly DependencyProperty NoiseProperty = DependencyProperty.Register(
		nameof(Noise), typeof(double), typeof(DropShadowBitmapEffect), new PropertyMetadata(0.0));
	/// <summary>Identifies <see cref="Opacity"/>.</summary>
	public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
		nameof(Opacity), typeof(double), typeof(DropShadowBitmapEffect), new PropertyMetadata(1.0));
	/// <summary>Identifies <see cref="Softness"/>.</summary>
	public static readonly DependencyProperty SoftnessProperty = DependencyProperty.Register(
		nameof(Softness), typeof(double), typeof(DropShadowBitmapEffect), new PropertyMetadata(0.5));

	/// <summary>Shadow depth. Default 5.</summary>
	public double ShadowDepth
	{
		get => (double)GetValue(ShadowDepthProperty)!;
		set => SetValue(ShadowDepthProperty, value);
	}

	/// <summary>Shadow colour. Default black.</summary>
	public Color Color { get => (Color)GetValue(ColorProperty)!; set => SetValue(ColorProperty, value); }

	/// <summary>Direction in degrees. Default 315.</summary>
	public double Direction
	{
		get => (double)GetValue(DirectionProperty)!;
		set => SetValue(DirectionProperty, value);
	}

	/// <summary>Noise. Default 0.</summary>
	public double Noise { get => (double)GetValue(NoiseProperty)!; set => SetValue(NoiseProperty, value); }

	/// <summary>Opacity. Default 1.</summary>
	public double Opacity { get => (double)GetValue(OpacityProperty)!; set => SetValue(OpacityProperty, value); }

	/// <summary>Softness. Default 0.5.</summary>
	public double Softness { get => (double)GetValue(SoftnessProperty)!; set => SetValue(SoftnessProperty, value); }

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DropShadowBitmapEffect();
}

/// <summary>Emboss effect (store-only).</summary>
public sealed class EmbossBitmapEffect : BitmapEffect
{
	/// <summary>Identifies <see cref="LightAngle"/>.</summary>
	public static readonly DependencyProperty LightAngleProperty = DependencyProperty.Register(
		nameof(LightAngle), typeof(double), typeof(EmbossBitmapEffect), new PropertyMetadata(45.0));
	/// <summary>Identifies <see cref="Relief"/>.</summary>
	public static readonly DependencyProperty ReliefProperty = DependencyProperty.Register(
		nameof(Relief), typeof(double), typeof(EmbossBitmapEffect), new PropertyMetadata(0.44));

	/// <summary>Light angle. Default 45.</summary>
	public double LightAngle
	{
		get => (double)GetValue(LightAngleProperty)!;
		set => SetValue(LightAngleProperty, value);
	}

	/// <summary>Relief. Default 0.44.</summary>
	public double Relief { get => (double)GetValue(ReliefProperty)!; set => SetValue(ReliefProperty, value); }

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new EmbossBitmapEffect();
}

/// <summary>Outer glow effect (store-only).</summary>
public sealed class OuterGlowBitmapEffect : BitmapEffect
{
	/// <summary>Identifies <see cref="GlowColor"/>.</summary>
	public static readonly DependencyProperty GlowColorProperty = DependencyProperty.Register(
		nameof(GlowColor), typeof(Color), typeof(OuterGlowBitmapEffect), new PropertyMetadata(Colors.Gold));
	/// <summary>Identifies <see cref="GlowSize"/>.</summary>
	public static readonly DependencyProperty GlowSizeProperty = DependencyProperty.Register(
		nameof(GlowSize), typeof(double), typeof(OuterGlowBitmapEffect), new PropertyMetadata(5.0));
	/// <summary>Identifies <see cref="Noise"/>.</summary>
	public static readonly DependencyProperty NoiseProperty = DependencyProperty.Register(
		nameof(Noise), typeof(double), typeof(OuterGlowBitmapEffect), new PropertyMetadata(0.0));
	/// <summary>Identifies <see cref="Opacity"/>.</summary>
	public static readonly DependencyProperty OpacityProperty = DependencyProperty.Register(
		nameof(Opacity), typeof(double), typeof(OuterGlowBitmapEffect), new PropertyMetadata(1.0));

	/// <summary>Glow colour. Default gold.</summary>
	public Color GlowColor { get => (Color)GetValue(GlowColorProperty)!; set => SetValue(GlowColorProperty, value); }

	/// <summary>Glow size. Default 5.</summary>
	public double GlowSize { get => (double)GetValue(GlowSizeProperty)!; set => SetValue(GlowSizeProperty, value); }

	/// <summary>Noise. Default 0.</summary>
	public double Noise { get => (double)GetValue(NoiseProperty)!; set => SetValue(NoiseProperty, value); }

	/// <summary>Opacity. Default 1.</summary>
	public double Opacity { get => (double)GetValue(OpacityProperty)!; set => SetValue(OpacityProperty, value); }

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new OuterGlowBitmapEffect();
}
