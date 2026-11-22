using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Media3D;

namespace System.Windows.Media;

/// <summary>A freezable list of <see cref="int"/>.</summary>
[TypeConverter(typeof(Int32CollectionConverter))]
public sealed partial class Int32Collection : Freezable, IFormattable
{
	internal static readonly Int32Collection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public Int32Collection() { }

	/// <summary>Creates an empty collection with the given capacity hint.</summary>
	public Int32Collection(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
	}

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public Int32Collection(IEnumerable<int> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var i in collection) Add(i);
	}

	static Int32Collection Frozen()
	{
		var c = new Int32Collection();
		c.Freeze();
		return c;
	}

	/// <summary>Converts integers separated by commas or whitespace.</summary>
	public static Int32Collection Parse(string source)
	{
		var reader = new TokenizerHelper.Reader(source);
		var result = new Int32Collection();
		while (reader.Next() is string token)
			result.Add(System.Convert.ToInt32(token, CultureInfo.InvariantCulture));
		return result;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Int32Collection();

	/// <summary>Creates a modifiable clone.</summary>
	public new Int32Collection Clone() => (Int32Collection)base.Clone();

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		List.Format(provider, i => i.ToString(format, provider));
}
