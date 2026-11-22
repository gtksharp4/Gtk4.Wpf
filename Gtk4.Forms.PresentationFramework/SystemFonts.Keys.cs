using System.Reflection;

namespace System.Windows;

public static partial class SystemFonts
{
	/// <summary>The resource key for the Message font family.</summary>
	public static ResourceKey MessageFontFamilyKey { get; } =
		new SystemFontKey("MessageFontFamilyKey", () => MessageFontFamily);

	/// <summary>The resource key for the Message font size.</summary>
	public static ResourceKey MessageFontSizeKey { get; } =
		new SystemFontKey("MessageFontSizeKey", () => MessageFontSize);

	/// <summary>The resource key for the Message font style.</summary>
	public static ResourceKey MessageFontStyleKey { get; } =
		new SystemFontKey("MessageFontStyleKey", () => MessageFontStyle);

	/// <summary>The resource key for the Message font weight.</summary>
	public static ResourceKey MessageFontWeightKey { get; } =
		new SystemFontKey("MessageFontWeightKey", () => MessageFontWeight);

	/// <summary>The resource key for the Message font text decorations.</summary>
	public static ResourceKey MessageFontTextDecorationsKey { get; } =
		new SystemFontKey("MessageFontTextDecorationsKey", () => MessageFontTextDecorations);

	/// <summary>The resource key for the Caption font family.</summary>
	public static ResourceKey CaptionFontFamilyKey { get; } =
		new SystemFontKey("CaptionFontFamilyKey", () => CaptionFontFamily);

	/// <summary>The resource key for the Caption font size.</summary>
	public static ResourceKey CaptionFontSizeKey { get; } =
		new SystemFontKey("CaptionFontSizeKey", () => CaptionFontSize);

	/// <summary>The resource key for the Caption font style.</summary>
	public static ResourceKey CaptionFontStyleKey { get; } =
		new SystemFontKey("CaptionFontStyleKey", () => CaptionFontStyle);

	/// <summary>The resource key for the Caption font weight.</summary>
	public static ResourceKey CaptionFontWeightKey { get; } =
		new SystemFontKey("CaptionFontWeightKey", () => CaptionFontWeight);

	/// <summary>The resource key for the Caption font text decorations.</summary>
	public static ResourceKey CaptionFontTextDecorationsKey { get; } =
		new SystemFontKey("CaptionFontTextDecorationsKey", () => CaptionFontTextDecorations);

	/// <summary>The resource key for the Icon font family.</summary>
	public static ResourceKey IconFontFamilyKey { get; } =
		new SystemFontKey("IconFontFamilyKey", () => IconFontFamily);

	/// <summary>The resource key for the Icon font size.</summary>
	public static ResourceKey IconFontSizeKey { get; } =
		new SystemFontKey("IconFontSizeKey", () => IconFontSize);

	/// <summary>The resource key for the Icon font style.</summary>
	public static ResourceKey IconFontStyleKey { get; } =
		new SystemFontKey("IconFontStyleKey", () => IconFontStyle);

	/// <summary>The resource key for the Icon font weight.</summary>
	public static ResourceKey IconFontWeightKey { get; } =
		new SystemFontKey("IconFontWeightKey", () => IconFontWeight);

	/// <summary>The resource key for the Icon font text decorations.</summary>
	public static ResourceKey IconFontTextDecorationsKey { get; } =
		new SystemFontKey("IconFontTextDecorationsKey", () => IconFontTextDecorations);

	/// <summary>The resource key for the Menu font family.</summary>
	public static ResourceKey MenuFontFamilyKey { get; } =
		new SystemFontKey("MenuFontFamilyKey", () => MenuFontFamily);

	/// <summary>The resource key for the Menu font size.</summary>
	public static ResourceKey MenuFontSizeKey { get; } =
		new SystemFontKey("MenuFontSizeKey", () => MenuFontSize);

	/// <summary>The resource key for the Menu font style.</summary>
	public static ResourceKey MenuFontStyleKey { get; } =
		new SystemFontKey("MenuFontStyleKey", () => MenuFontStyle);

	/// <summary>The resource key for the Menu font weight.</summary>
	public static ResourceKey MenuFontWeightKey { get; } =
		new SystemFontKey("MenuFontWeightKey", () => MenuFontWeight);

	/// <summary>The resource key for the Menu font text decorations.</summary>
	public static ResourceKey MenuFontTextDecorationsKey { get; } =
		new SystemFontKey("MenuFontTextDecorationsKey", () => MenuFontTextDecorations);

	/// <summary>The resource key for the SmallCaption font family.</summary>
	public static ResourceKey SmallCaptionFontFamilyKey { get; } =
		new SystemFontKey("SmallCaptionFontFamilyKey", () => SmallCaptionFontFamily);

	/// <summary>The resource key for the SmallCaption font size.</summary>
	public static ResourceKey SmallCaptionFontSizeKey { get; } =
		new SystemFontKey("SmallCaptionFontSizeKey", () => SmallCaptionFontSize);

	/// <summary>The resource key for the SmallCaption font style.</summary>
	public static ResourceKey SmallCaptionFontStyleKey { get; } =
		new SystemFontKey("SmallCaptionFontStyleKey", () => SmallCaptionFontStyle);

	/// <summary>The resource key for the SmallCaption font weight.</summary>
	public static ResourceKey SmallCaptionFontWeightKey { get; } =
		new SystemFontKey("SmallCaptionFontWeightKey", () => SmallCaptionFontWeight);

	/// <summary>The resource key for the SmallCaption font text decorations.</summary>
	public static ResourceKey SmallCaptionFontTextDecorationsKey { get; } =
		new SystemFontKey("SmallCaptionFontTextDecorationsKey", () => SmallCaptionFontTextDecorations);

	/// <summary>The resource key for the Status font family.</summary>
	public static ResourceKey StatusFontFamilyKey { get; } =
		new SystemFontKey("StatusFontFamilyKey", () => StatusFontFamily);

	/// <summary>The resource key for the Status font size.</summary>
	public static ResourceKey StatusFontSizeKey { get; } =
		new SystemFontKey("StatusFontSizeKey", () => StatusFontSize);

	/// <summary>The resource key for the Status font style.</summary>
	public static ResourceKey StatusFontStyleKey { get; } =
		new SystemFontKey("StatusFontStyleKey", () => StatusFontStyle);

	/// <summary>The resource key for the Status font weight.</summary>
	public static ResourceKey StatusFontWeightKey { get; } =
		new SystemFontKey("StatusFontWeightKey", () => StatusFontWeight);

	/// <summary>The resource key for the Status font text decorations.</summary>
	public static ResourceKey StatusFontTextDecorationsKey { get; } =
		new SystemFontKey("StatusFontTextDecorationsKey", () => StatusFontTextDecorations);
}

/// <summary>A reference-unique key for a system font setting.</summary>
sealed class SystemFontKey : ResourceKey
{
	readonly string _name;
	readonly Func<object> _value;

	internal SystemFontKey(string name, Func<object> value)
	{
		_name = name;
		_value = value;
	}

	public override Assembly Assembly => typeof(SystemFonts).Assembly;

	internal object Value => _value();

	public override string ToString() => "SystemFonts." + _name;
}
