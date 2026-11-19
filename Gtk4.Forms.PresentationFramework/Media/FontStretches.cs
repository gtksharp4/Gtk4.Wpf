using System;

namespace System.Windows;

/// <summary>Provides predefined <see cref="FontStretch"/> values.</summary>
public static class FontStretches
{
	static readonly string[] Names =
	{
		"UltraCondensed", "ExtraCondensed", "Condensed", "SemiCondensed", "Normal",
		"SemiExpanded", "Expanded", "ExtraExpanded", "UltraExpanded",
	};

	/// <summary>Ultra-condensed (1).</summary>
	public static FontStretch UltraCondensed { get; } = new FontStretch(1);
	/// <summary>Extra-condensed (2).</summary>
	public static FontStretch ExtraCondensed { get; } = new FontStretch(2);
	/// <summary>Condensed (3).</summary>
	public static FontStretch Condensed { get; } = new FontStretch(3);
	/// <summary>Semi-condensed (4).</summary>
	public static FontStretch SemiCondensed { get; } = new FontStretch(4);
	/// <summary>Normal (5).</summary>
	public static FontStretch Normal { get; } = new FontStretch(5);
	/// <summary>Medium (5); same as <see cref="Normal"/>.</summary>
	public static FontStretch Medium { get; } = new FontStretch(5);
	/// <summary>Semi-expanded (6).</summary>
	public static FontStretch SemiExpanded { get; } = new FontStretch(6);
	/// <summary>Expanded (7).</summary>
	public static FontStretch Expanded { get; } = new FontStretch(7);
	/// <summary>Extra-expanded (8).</summary>
	public static FontStretch ExtraExpanded { get; } = new FontStretch(8);
	/// <summary>Ultra-expanded (9).</summary>
	public static FontStretch UltraExpanded { get; } = new FontStretch(9);

	internal static string? NameOf(int openTypeStretch) =>
		openTypeStretch >= 1 && openTypeStretch <= 9 ? Names[openTypeStretch - 1] : null;
}
