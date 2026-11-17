namespace System.Windows.Media.Effects;

/// <summary>
/// Renders a drop shadow beneath the target element.
/// Matches WPF's <c>System.Windows.Media.Effects.DropShadowEffect</c>.
/// <para>
/// <b>Direction convention (WPF):</b> 0° points right (+X), angles increase
/// counter-clockwise (90° = up, −Y in screen space). dx = depth·cos(dir),
/// dy = −depth·sin(dir).
/// </para>
/// <para>
/// <b>GTK application:</b> rendered as CSS
/// <c>filter: drop-shadow(dx dy blur rgba(r,g,b,a))</c> via a per-widget
/// <c>Gtk.CssProvider</c>. GTK 4 honours <c>filter:</c> from GTK 4.14+.
/// The object model (DP values) is always available regardless of GTK availability.
/// </para>
/// </summary>
public sealed class DropShadowEffect : Effect
{
	Color _color = Color.FromArgb(255, 0, 0, 0);   // black opaque
	double _blurRadius = 5.0;
	double _shadowDepth = 5.0;
	double _direction = 315.0;
	double _opacity = 1.0;

	/// <summary>
	/// Gets or sets the shadow colour. Default: black.
	/// </summary>
	public Color Color
	{
		get => _color;
		set => _color = value;
	}

	/// <summary>
	/// Gets or sets the blur radius of the shadow in pixels. Default: 5.
	/// </summary>
	public double BlurRadius
	{
		get => _blurRadius;
		set => _blurRadius = value;
	}

	/// <summary>
	/// Gets or sets the distance the shadow is offset from the element in pixels. Default: 5.
	/// </summary>
	public double ShadowDepth
	{
		get => _shadowDepth;
		set => _shadowDepth = value;
	}

	/// <summary>
	/// Gets or sets the direction of the shadow, in degrees, measured counter-clockwise
	/// from the positive X-axis (0° = right). Default: 315°.
	/// </summary>
	public double Direction
	{
		get => _direction;
		set => _direction = value;
	}

	/// <summary>
	/// Gets or sets the opacity of the shadow in [0,1]. Default: 1.
	/// </summary>
	public double Opacity
	{
		get => _opacity;
		set => _opacity = value;
	}
}
