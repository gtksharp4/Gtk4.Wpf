using System.Text;
using System.Text.RegularExpressions;

namespace Gtk.Forms.ProjectConverter;

/// <summary>
/// Converts a consumer WPF project to build on the Gtk.Forms compatibility layer: collapses
/// Windows-platform TFMs to <c>net10.0</c> and removes <c>&lt;UseWPF&gt;</c> and
/// <c>&lt;UseWindowsForms&gt;</c> (Windows-only SDK opt-ins).
/// <para><c>OutputType</c> is left untouched — a WPF app keeps its <c>WinExe</c>. The entry
/// point comes from the compiled <c>ApplicationDefinition</c>, which works for any executable
/// output type, so there is no reason to downgrade <c>WinExe</c> to <c>Exe</c>.</para>
/// </summary>
/// <remarks>
/// All edits are textual and minimal; unrelated content is left byte-for-byte unchanged.
/// Every transform is idempotent: an already-converted project is not modified.
/// </remarks>
internal static class CsprojConverter
{
	// <TargetFramework(s)> element — captures element name and value.
	static readonly Regex _targetFrameworkElement = new(
		@"<(TargetFrameworks?)(?:\s[^>]*)?>(?<value>[^<]*)</\1>",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Platform suffixes to strip: -windows (with optional version like 10.0.19041.0).
	static readonly Regex _platformSuffix = new(
		@"-windows[0-9.]*",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Legacy/non-net10.0 monikers to replace wholesale with net10.0.
	static readonly Regex _legacyMoniker = new(
		@"^(?:uap10\.0.*|monoandroid.*|xamarin\.ios.*|xamarinios.*|tizen.*|netstandard2\.\d|portable.*)$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// <UseWPF>...</UseWPF> — full line including leading indent and trailing newline.
	static readonly Regex _useWpf = new(
		@"[^\S\r\n]*<UseWPF>[^<]*</UseWPF>[^\S\r\n]*(\r?\n)?",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// <UseWindowsForms>...</UseWindowsForms> — full line including leading indent and trailing newline.
	static readonly Regex _useWindowsForms = new(
		@"[^\S\r\n]*<UseWindowsForms>[^<]*</UseWindowsForms>[^\S\r\n]*(\r?\n)?",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	/// <summary>
	/// Applies all conversion rules to project-file text and reports whether anything changed.
	/// </summary>
	/// <param name="content">Raw <c>.csproj</c> text.</param>
	/// <param name="changed">
	/// <see langword="true"/> when the returned text differs from <paramref name="content"/>.
	/// </param>
	/// <returns>Converted text, or the original when nothing needed changing.</returns>
	internal static string Transform(string content, out bool changed)
	{
		var result = content;

		result = ApplyTfmCollapse(result);
		result = StripWpfOptIns(result);

		changed = !string.Equals(result, content, StringComparison.Ordinal);
		return result;
	}

	/// <summary>
	/// Converts the project file in place, preserving its original UTF-8 BOM state.
	/// </summary>
	/// <param name="csprojPath">Absolute path to the <c>.csproj</c>.</param>
	/// <returns><see langword="true"/> when the file was rewritten.</returns>
	internal static bool TryConvertFile(string csprojPath)
	{
		var bytes = File.ReadAllBytes(csprojPath);
		var hadBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;

		var content = File.ReadAllText(csprojPath);
		var result = Transform(content, out var changed);
		if (changed)
			File.WriteAllText(csprojPath, result, new UTF8Encoding(encoderShouldEmitUTF8Identifier: hadBom));

		return changed;
	}

	// ── private helpers ──────────────────────────────────────────────────────────────────────────

	static string ApplyTfmCollapse(string content)
	{
		return _targetFrameworkElement.Replace(content, match =>
		{
			var raw = match.Groups["value"].Value;
			var tfms = raw
				.Split(';')
				.Select(t => t.Trim())
				.Select(NormaliseTfm)
				.Where(t => t.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			// Any result that is not exactly net10.0 (single or multiple) collapses to net10.0.
			// Idempotency: <TargetFramework>net10.0</TargetFramework> with no suffix → no change.
			if (tfms.Count == 1
				&& string.Equals(tfms[0], "net10.0", StringComparison.OrdinalIgnoreCase)
				&& string.Equals(raw.Trim(), "net10.0", StringComparison.OrdinalIgnoreCase)
				&& match.Groups[1].Value.Equals("TargetFramework", StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return "<TargetFramework>net10.0</TargetFramework>";
		});
	}

	/// <summary>Normalises a single TFM string to its cross-platform base (or <c>net10.0</c>).</summary>
	static string NormaliseTfm(string tfm)
	{
		if (_legacyMoniker.IsMatch(tfm))
			return "net10.0";

		return _platformSuffix.Replace(tfm, string.Empty);
	}

	static string StripWpfOptIns(string content)
	{
		content = _useWpf.Replace(content, string.Empty);
		content = _useWindowsForms.Replace(content, string.Empty);
		return content;
	}
}
