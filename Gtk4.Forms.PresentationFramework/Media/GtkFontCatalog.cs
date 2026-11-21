using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

/// <summary>One face of a system font family, as Pango reports it.</summary>
sealed class GtkFaceInfo
{
	internal GtkFaceInfo(string name, FontStyle style, FontWeight weight, FontStretch stretch, bool synthesized)
	{
		Name = name;
		Style = style;
		Weight = weight;
		Stretch = stretch;
		Synthesized = synthesized;
	}

	internal string Name { get; }

	internal FontStyle Style { get; }

	internal FontWeight Weight { get; }

	internal FontStretch Stretch { get; }

	internal bool Synthesized { get; }
}

/// <summary>A system font family with its faces.</summary>
sealed class GtkFamilyInfo
{
	internal GtkFamilyInfo(string name, bool monospace, IReadOnlyList<GtkFaceInfo> faces)
	{
		Name = name;
		Monospace = monospace;
		Faces = faces;
	}

	internal string Name { get; }

	internal bool Monospace { get; }

	internal IReadOnlyList<GtkFaceInfo> Faces { get; }
}

/// <summary>
/// The system font list from the Pango font map of the default GDK display. Without GTK or a display the
/// catalog is empty and callers fall back to neutral values. The list is read once and cached.
/// </summary>
static class GtkFontCatalog
{
	static readonly object Gate = new object();
	static Pango.FontMap? _map;
	static List<GtkFamilyInfo>? _families;

	/// <summary>The default Pango font map, or null when GTK has no display.</summary>
	internal static Pango.FontMap? FontMap
	{
		get
		{
			lock (Gate)
			{
				if (_map != null) return _map;
				try
				{
					if (Gdk.Display.Default == null) return null;
					using (var probe = new Gtk.Label(string.Empty))
						_map = probe.PangoContext.FontMap;
				}
				catch (Exception)
				{
					// GTK not initialised or no native runtime: behave as if there are no fonts.
					_map = null;
				}
				return _map;
			}
		}
	}

	internal static IReadOnlyList<GtkFamilyInfo> Families
	{
		get
		{
			lock (Gate)
			{
				if (_families != null) return _families;
				var map = FontMap;
				if (map == null) return new GtkFamilyInfo[0];
				_families = map.Families.Select(Read).OrderBy(f => f.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
				return _families;
			}
		}
	}

	static GtkFamilyInfo Read(Pango.FontFamily family)
	{
		var faces = family.Faces.Select(f =>
		{
			var d = f.Describe();
			return new GtkFaceInfo(f.FaceName, ToStyle(d.Style), ToWeight((int)d.Weight), ToStretch((int)d.Stretch),
				f.IsSynthesized);
		}).ToList();
		return new GtkFamilyInfo(family.Name, family.IsMonospace, faces);
	}

	internal static FontStyle ToStyle(Pango.Style s) =>
		s == Pango.Style.Italic ? FontStyles.Italic : s == Pango.Style.Oblique ? FontStyles.Oblique : FontStyles.Normal;

	internal static FontWeight ToWeight(int pangoWeight) =>
		FontWeight.FromOpenTypeWeight(Math.Max(1, Math.Min(999, pangoWeight)));

	internal static FontStretch ToStretch(int pangoStretch) =>
		FontStretch.FromOpenTypeStretch(Math.Max(1, Math.Min(9, pangoStretch + 1)));

	/// <summary>The family a <see cref="FontFamily.Source"/> names: first list entry, after any '#'.</summary>
	internal static GtkFamilyInfo? Find(string source)
	{
		foreach (var raw in source.Split(','))
		{
			var name = raw.Trim();
			int hash = name.LastIndexOf('#');
			if (hash >= 0) name = name.Substring(hash + 1);
			var hit = Families.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
			if (hit != null) return hit;
		}
		return null;
	}
}
