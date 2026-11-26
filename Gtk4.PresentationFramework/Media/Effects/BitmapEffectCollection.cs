using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace System.Windows.Media.Effects;

/// <summary>A freezable list of <see cref="BitmapEffect"/>.</summary>
public sealed partial class BitmapEffectCollection : Animatable
{
	/// <summary>Creates an empty collection.</summary>
	public BitmapEffectCollection() { }

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public BitmapEffectCollection(IEnumerable<BitmapEffect> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var e in collection) Add(e);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new BitmapEffectCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new BitmapEffectCollection Clone() => (BitmapEffectCollection)base.Clone();
}
