using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace System.Windows.Media;

/// <summary>Enumerates font families and typefaces: the system ones, and those in a folder.</summary>
public static class Fonts
{
	/// <summary>The system font families from the default Pango font map (empty when GTK has no display).</summary>
	public static ICollection<FontFamily> SystemFontFamilies =>
		GtkFontCatalog.Families.Select(f => new FontFamily(f.Name)).ToList();

	/// <summary>Every typeface of every system font family.</summary>
	public static ICollection<Typeface> SystemTypefaces =>
		SystemFontFamilies.SelectMany(f => f.GetTypefaces()).ToList();

	/// <summary>The families in the font files of a folder (or a single font file).</summary>
	public static ICollection<FontFamily> GetFontFamilies(string location)
	{
		if (location == null) throw new ArgumentNullException(nameof(location));
		return FolderFamilies(location);
	}

	/// <summary>The families in the font files at <paramref name="location"/>.</summary>
	public static ICollection<FontFamily> GetFontFamilies(Uri location)
	{
		if (location == null) throw new ArgumentNullException(nameof(location));
		return FolderFamilies(location.IsFile ? location.LocalPath : location.OriginalString);
	}

	/// <summary>The families at <paramref name="location"/> resolved against <paramref name="baseUri"/>.</summary>
	public static ICollection<FontFamily> GetFontFamilies(Uri? baseUri, string location)
	{
		if (location == null) throw new ArgumentNullException(nameof(location));
		var path = baseUri != null && baseUri.IsFile && !Path.IsPathRooted(location)
			? Path.Combine(baseUri.LocalPath, location) : location;
		return FolderFamilies(path);
	}

	/// <summary>The typefaces in the font files of a folder (or a single font file).</summary>
	public static ICollection<Typeface> GetTypefaces(string location) =>
		GetFontFamilies(location).SelectMany(f => f.GetTypefaces()).ToList();

	/// <summary>The typefaces in the font files at <paramref name="location"/>.</summary>
	public static ICollection<Typeface> GetTypefaces(Uri location) =>
		GetFontFamilies(location).SelectMany(f => f.GetTypefaces()).ToList();

	/// <summary>The typefaces at <paramref name="location"/> resolved against <paramref name="baseUri"/>.</summary>
	public static ICollection<Typeface> GetTypefaces(Uri? baseUri, string location) =>
		GetFontFamilies(baseUri, location).SelectMany(f => f.GetTypefaces()).ToList();

	static readonly string[] FontExtensions = { ".ttf", ".otf", ".ttc" };

	// Pango cannot read the name table of a font file, so the family name is the file name without extension and
	// the family is addressed as folder-uri#name. A missing folder yields an empty collection.
	static ICollection<FontFamily> FolderFamilies(string location)
	{
		IEnumerable<string> files;
		string folder;
		if (Directory.Exists(location))
		{
			folder = location;
			files = Directory.EnumerateFiles(location);
		}
		else if (File.Exists(location))
		{
			folder = Path.GetDirectoryName(Path.GetFullPath(location)) ?? string.Empty;
			files = new[] { location };
		}
		else return new List<FontFamily>();
		var full = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		var baseUri = new Uri(full + Path.DirectorySeparatorChar);
		return files.Where(f => FontExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
			.Select(f => Path.GetFileNameWithoutExtension(f))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
			.Select(n => new FontFamily(baseUri, baseUri.AbsoluteUri + "#" + n))
			.ToList();
	}
}
