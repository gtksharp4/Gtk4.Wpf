namespace System.Windows.Media.Effects;

/// <summary>
/// Applies a Gaussian (or box) blur to the target element.
/// Matches WPF's <c>System.Windows.Media.Effects.BlurEffect</c>.
/// <para>
/// <b>GTK application:</b> rendered as CSS <c>filter: blur(Npx)</c> via a per-widget
/// <c>Gtk.CssProvider</c>. The <see cref="KernelType"/> property is stored but CSS
/// <c>blur()</c> is always Gaussian; <see cref="KernelType.Box"/> has no distinct CSS
/// equivalent and renders as Gaussian. GTK 4 honours <c>filter:</c> from GTK 4.14+.
/// </para>
/// </summary>
public sealed class BlurEffect : Effect
{
	double _radius = 5.0;
	KernelType _kernelType = KernelType.Gaussian;

	/// <summary>
	/// Gets or sets the blur radius in pixels. Default: 5.
	/// </summary>
	public double Radius
	{
		get => _radius;
		set => _radius = value;
	}

	/// <summary>
	/// Gets or sets the blur kernel type. Default: <see cref="KernelType.Gaussian"/>.
	/// <para>
	/// CSS has no distinct box-blur function; both values render as Gaussian blur.
	/// </para>
	/// </summary>
	public KernelType KernelType
	{
		get => _kernelType;
		set => _kernelType = value;
	}
}
