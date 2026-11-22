namespace System.Windows.Media.Imaging;

public abstract partial class BitmapSource
{
	/// <summary>Creates an unfrozen, independent copy of this bitmap.</summary>
	public new BitmapSource Clone() => (BitmapSource)base.Clone();

	/// <summary>Creates an unfrozen, independent copy of this bitmap using its current values.</summary>
	public new BitmapSource CloneCurrentValue() => (BitmapSource)base.CloneCurrentValue();
}
