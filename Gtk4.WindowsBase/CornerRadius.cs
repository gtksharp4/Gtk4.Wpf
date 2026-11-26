using System;

namespace System.Windows;

/// <summary>
/// Describes the corner radii of a rectangle's corners (WPF-compatible).
/// Uniform: one value. Four-part: "topLeft,topRight,bottomRight,bottomLeft".
/// </summary>
public struct CornerRadius : IEquatable<CornerRadius>
{
	/// <summary>Top-left corner radius.</summary>
	public double TopLeft { get; set; }
	/// <summary>Top-right corner radius.</summary>
	public double TopRight { get; set; }
	/// <summary>Bottom-right corner radius.</summary>
	public double BottomRight { get; set; }
	/// <summary>Bottom-left corner radius.</summary>
	public double BottomLeft { get; set; }

	/// <summary>Creates a uniform corner radius.</summary>
	public CornerRadius(double uniformRadius)
		: this(uniformRadius, uniformRadius, uniformRadius, uniformRadius) { }

	/// <summary>Creates a corner radius with per-corner values.</summary>
	public CornerRadius(double topLeft, double topRight, double bottomRight, double bottomLeft)
	{
		TopLeft = topLeft;
		TopRight = topRight;
		BottomRight = bottomRight;
		BottomLeft = bottomLeft;
	}

	/// <inheritdoc/>
	public bool Equals(CornerRadius other) =>
		TopLeft == other.TopLeft && TopRight == other.TopRight &&
		BottomRight == other.BottomRight && BottomLeft == other.BottomLeft;
	/// <inheritdoc/>
	public override bool Equals(object? obj) => obj is CornerRadius c && Equals(c);
	/// <inheritdoc/>
	public override int GetHashCode() =>
		PrimitivesHashHelper.CombineHash(
			PrimitivesHashHelper.CombineHash(TopLeft.GetHashCode(), TopRight.GetHashCode()),
			PrimitivesHashHelper.CombineHash(BottomRight.GetHashCode(), BottomLeft.GetHashCode()));
	/// <inheritdoc/>
	public static bool operator ==(CornerRadius a, CornerRadius b) => a.Equals(b);
	/// <inheritdoc/>
	public static bool operator !=(CornerRadius a, CornerRadius b) => !a.Equals(b);
	/// <inheritdoc/>
	public override string ToString() => $"{TopLeft},{TopRight},{BottomRight},{BottomLeft}";
}