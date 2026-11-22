using System.Windows.Media;

namespace System.Windows.Documents;

public abstract partial class Adorner
{
	/// <summary>
	/// Returns the transform the adorner layer applies to this adorner. The base returns
	/// <paramref name="transform"/> unchanged (WPF composes the adorned element's transform with the layout offset).
	/// </summary>
	public virtual GeneralTransform GetDesiredTransform(GeneralTransform transform) => transform;
}
