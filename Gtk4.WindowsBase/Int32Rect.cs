namespace System.Windows;

/// <summary>An integer rectangle (WPF <c>Int32Rect</c>).</summary>
public struct Int32Rect : IEquatable<Int32Rect>
{
	/// <summary>Creates a rectangle.</summary>
	public Int32Rect(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	/// <summary>The empty rectangle.</summary>
	public static Int32Rect Empty => default;

	/// <summary>Left edge.</summary>
	public int X { get; set; }

	/// <summary>Top edge.</summary>
	public int Y { get; set; }

	/// <summary>Width.</summary>
	public int Width { get; set; }

	/// <summary>Height.</summary>
	public int Height { get; set; }

	/// <summary>Whether the rectangle has no area (all members zero).</summary>
	public bool IsEmpty => Width == 0 && Height == 0 && X == 0 && Y == 0;

	/// <inheritdoc/>
	public bool Equals(Int32Rect other) =>
		X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is Int32Rect r && Equals(r);

	/// <inheritdoc/>
	public override int GetHashCode() => X ^ (Y << 8) ^ (Width << 16) ^ (Height << 24);

	/// <summary>Equality operator.</summary>
	public static bool operator ==(Int32Rect a, Int32Rect b) => a.Equals(b);

	/// <summary>Inequality operator.</summary>
	public static bool operator !=(Int32Rect a, Int32Rect b) => !a.Equals(b);

	/// <inheritdoc/>
	public override string ToString() => IsEmpty ? "Empty" : $"{X},{Y},{Width},{Height}";
}
