using System.Collections.Generic;

namespace System.Windows.Media.Animation;

/// <summary>An ordered, freezable list of <see cref="DoubleKeyFrame"/>.</summary>
public partial class DoubleKeyFrameCollection : Freezable
{
	/// <summary>The shared frozen empty collection.</summary>
	public static DoubleKeyFrameCollection Empty { get; } = MakeEmpty();

	/// <summary>Creates an empty collection.</summary>
	public DoubleKeyFrameCollection() { }

	static DoubleKeyFrameCollection MakeEmpty()
	{
		var c = new DoubleKeyFrameCollection();
		c.Freeze();
		return c;
	}

	/// <summary>Creates a modifiable clone.</summary>
	public new DoubleKeyFrameCollection Clone() => (DoubleKeyFrameCollection)base.Clone();

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new DoubleKeyFrameCollection();
}
