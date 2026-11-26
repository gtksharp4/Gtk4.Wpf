using System;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows;

/// <summary>One system colour: its value, cached brush and the two resource keys that resolve to them.</summary>
sealed class SystemColorEntry
{
	internal SystemColorEntry(string name, byte r, byte g, byte b)
	{
		Color = Color.FromRgb(r, g, b);
		Brush = new SolidColorBrush(Color);
		ColorKey = new SystemColorKey(name + "Color", this, false);
		BrushKey = new SystemColorKey(name + "Brush", this, true);
	}

	internal Color Color { get; }
	internal SolidColorBrush Brush { get; }
	internal ResourceKey ColorKey { get; }
	internal ResourceKey BrushKey { get; }
}

/// <summary>A reference-unique key for a system colour or brush.</summary>
sealed class SystemColorKey : ResourceKey
{
	readonly string _name;
	readonly SystemColorEntry _entry;
	readonly bool _isBrush;

	internal SystemColorKey(string name, SystemColorEntry entry, bool isBrush)
	{
		_name = name;
		_entry = entry;
		_isBrush = isBrush;
	}

	public override Assembly Assembly => typeof(SystemColors).Assembly;

	internal object Value => _isBrush ? _entry.Brush : _entry.Color;

	public override string ToString() => "SystemColors." + _name + "Key";
}

static class SystemResources
{
	/// <summary>Resolves a system colour/brush key; the last step of every resource lookup.</summary>
	internal static bool TryFind(object? key, out object? value)
	{
		if (key is SystemFontKey f)
		{
			value = f.Value;
			return true;
		}
		if (key is SystemColorKey k)
		{
			value = k.Value;
			return true;
		}
		value = null;
		return false;
	}
}
