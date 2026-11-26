namespace System.Windows.Media.Effects;

/// <summary>
/// Specifies the kernel to use for a <see cref="BlurEffect"/>.
/// Matches WPF's <c>System.Windows.Media.Effects.KernelType</c>.
/// </summary>
public enum KernelType
{
	/// <summary>Gaussian blur kernel (default).</summary>
	Gaussian = 0,

	/// <summary>Box blur kernel.</summary>
	Box = 1,
}
