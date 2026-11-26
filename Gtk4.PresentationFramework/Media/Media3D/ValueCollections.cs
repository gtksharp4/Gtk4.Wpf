using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>A freezable list of <see cref="Point3D"/>.</summary>
[TypeConverter(typeof(Point3DCollectionConverter))]
public sealed partial class Point3DCollection : Freezable, IFormattable
{
	internal static readonly Point3DCollection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public Point3DCollection() { }

	/// <summary>Creates an empty collection with the given capacity hint.</summary>
	public Point3DCollection(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
	}

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public Point3DCollection(IEnumerable<Point3D> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var p in collection) Add(p);
	}

	static Point3DCollection Frozen()
	{
		var c = new Point3DCollection();
		c.Freeze();
		return c;
	}

	/// <summary>Converts numbers (three per point) separated by commas or whitespace.</summary>
	public static Point3DCollection Parse(string source)
	{
		var reader = new TokenizerHelper.Reader(source);
		var result = new Point3DCollection();
		while (reader.Next() is string x)
			result.Add(new Point3D(Convert(x), reader.NextDouble(), reader.NextDouble()));
		return result;
	}

	static double Convert(string token) => System.Convert.ToDouble(token, CultureInfo.InvariantCulture);

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Point3DCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new Point3DCollection Clone() => (Point3DCollection)base.Clone();

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		List.Format(provider, p => p.ToString(format, provider));
}

/// <summary>A freezable list of <see cref="Vector3D"/>.</summary>
[TypeConverter(typeof(Vector3DCollectionConverter))]
public sealed partial class Vector3DCollection : Freezable, IFormattable
{
	internal static readonly Vector3DCollection Empty = Frozen();

	/// <summary>Creates an empty collection.</summary>
	public Vector3DCollection() { }

	/// <summary>Creates an empty collection with the given capacity hint.</summary>
	public Vector3DCollection(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
	}

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public Vector3DCollection(IEnumerable<Vector3D> collection)
	{
		if (collection == null) throw new ArgumentNullException(nameof(collection));
		foreach (var v in collection) Add(v);
	}

	static Vector3DCollection Frozen()
	{
		var c = new Vector3DCollection();
		c.Freeze();
		return c;
	}

	/// <summary>Converts numbers (three per vector) separated by commas or whitespace.</summary>
	public static Vector3DCollection Parse(string source)
	{
		var reader = new TokenizerHelper.Reader(source);
		var result = new Vector3DCollection();
		while (reader.Next() is string x)
			result.Add(new Vector3D(
				System.Convert.ToDouble(x, CultureInfo.InvariantCulture), reader.NextDouble(), reader.NextDouble()));
		return result;
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Vector3DCollection();

	/// <summary>Creates a modifiable clone.</summary>
	public new Vector3DCollection Clone() => (Vector3DCollection)base.Clone();

	/// <inheritdoc/>
	public override string ToString() => ToString(null, null);

	/// <summary>Formats with the provider list separator.</summary>
	public string ToString(IFormatProvider? provider) => ToString(null, provider);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? provider) =>
		List.Format(provider, v => v.ToString(format, provider));
}
