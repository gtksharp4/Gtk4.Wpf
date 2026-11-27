#pragma warning disable WPF0001 // layer-internal use of the experimental ThemeMode
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows.Themes;

/// <summary>
/// The <c>PresentationFramework.Fluent</c> resource dictionaries. The three XAML files are the .NET 10 (dotnet/wpf release/10.0)
/// <c>dotnet/wpf</c> sources, embedded unchanged (MIT): <c>Fluent.xaml</c> (variables, text and control styles)
/// and the <c>Theme/Light.xaml</c> / <c>Theme/Dark.xaml</c> colour and brush sets. WPF's own
/// <c>Themes/Fluent.xaml</c> is generated as their union, so a <c>pack://application:,,,/PresentationFramework.Fluent;component/Themes/Fluent.xaml</c>
/// source resolves to the base dictionary merged with the colours of the current <see cref="ThemeMode"/>.
/// </summary>
static partial class FluentTheme
{
	const string FluentAssemblyName = "PresentationFramework.Fluent";
	const string Prefix = "Gtk4.PresentationFramework.Themes.Fluent.";

	/// <summary>Whether <paramref name="uri"/> names a Fluent theme dictionary (<c>Themes/Fluent[.Light|.Dark].xaml</c>).</summary>
	internal static bool IsFluentUri(Uri uri, out bool? dark)
	{
		dark = null;
		string text = uri.IsAbsoluteUri ? Uri.UnescapeDataString(uri.AbsolutePath) : uri.OriginalString;
		text = text.Replace('\\', '/');
		int component = text.IndexOf(";component/", StringComparison.OrdinalIgnoreCase);
		if (component < 0) return false;
		string name = text.Substring(0, component).TrimStart('/');
		int semi = name.IndexOf(';');
		if (semi >= 0) name = name.Substring(0, semi);
		if (!string.Equals(name, FluentAssemblyName, StringComparison.OrdinalIgnoreCase)) return false;
		switch (text.Substring(component + ";component/".Length).TrimStart('/').ToLowerInvariant())
		{
			case "themes/fluent.xaml": return true;
			case "themes/fluent.light.xaml": dark = false; return true;
			case "themes/fluent.dark.xaml": dark = true; return true;
			default: return false;
		}
	}

	/// <summary>Builds the dictionary for <paramref name="uri"/>, or <c>null</c> when it is no Fluent URI.</summary>
	internal static ResourceDictionary? TryCreate(Uri uri)
	{
		if (!IsFluentUri(uri, out bool? dark)) return null;
		bool useDark = dark ?? Application.Current?.ThemeMode == ThemeMode.Dark;
		var result = new ResourceDictionary();
		result.MergedDictionaries.Add(Load("Fluent.xaml"));
		result.MergedDictionaries.Add(Load(useDark ? "Dark.xaml" : "Light.xaml"));
		return result;
	}

	/// <summary>
	/// Reloads every dictionary under <paramref name="resources"/> whose Source is the mode-following
	/// <c>Themes/Fluent.xaml</c>, so a changed <see cref="Application.ThemeMode"/> swaps Light and Dark colours.
	/// </summary>
	internal static void Retheme(ResourceDictionary resources)
	{
		foreach (ResourceDictionary merged in resources.MergedDictionaries.ToList())
		{
			if (merged.Source is Uri source && IsFluentUri(source, out bool? dark) && dark == null)
				merged.Source = source;
			else
				Retheme(merged);
		}
	}
}
