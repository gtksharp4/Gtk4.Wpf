using System.Windows.Media.Animation;

namespace System.Windows.Media;

/// <summary>Makes every brush an <see cref="Animatable"/>, as in WPF.</summary>
public abstract partial class Brush : Animatable
{
	/// <summary>
	/// Creates a new instance of the same type through its parameterless constructor.
	/// Brushes with a richer constructor override this.
	/// </summary>
	protected override Freezable CreateInstanceCore() =>
		(Freezable)(Activator.CreateInstance(GetType()) ?? throw new NotSupportedException(GetType().Name));

	/// <summary>Copies the non-dependency-property state of the brush.</summary>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		((Brush)destination).Opacity = Opacity;
	}
}
