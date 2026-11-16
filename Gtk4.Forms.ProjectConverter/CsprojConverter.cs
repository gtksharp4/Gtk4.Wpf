using System.Text;
using System.Text.RegularExpressions;

namespace Gtk4.Forms.ProjectConverter;

/// <summary>
/// Converts a consumer Xamarin.Forms project to build on the GTK 4 backend: collapses
/// platform TFMs to <c>net10.0</c>, removes official <c>Xamarin.Forms*</c> package refs
/// (which collide with the <c>Gtk4.Forms.*</c> drop-ins), and sets <c>OutputType=Exe</c>
/// with the injected GTK entry point as <c>StartupObject</c>.
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

	// Platform suffixes to strip: -android, -ios, -maccatalyst, -tvos, -windowsX.Y.Z
	static readonly Regex _platformSuffix = new(
		@"-(?:android|ios|maccatalyst|tvos|windows[0-9.]*)",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Legacy monikers to replace wholesale with net10.0.
	static readonly Regex _legacyMoniker = new(
		@"^(?:uap10\.0.*|monoandroid.*|xamarin\.ios.*|xamarinios.*|tizen.*|netstandard2\.\d|portable.*)$",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// A whole PackageReference line for Xamarin.Forms official packages (self-closed).
	// Include may appear after other attributes (e.g. Version="5.0" Include="Xamarin.Forms" />).
	// Matches the entire line including leading indent and trailing newline.
	// The Include value must end with a closing quote boundary — Xamarin.Forms.FooBar must NOT match.
	static readonly Regex _xamarinFormsRef = new(
		@"[^\S\r\n]*<PackageReference\b[^>]*\bInclude=""Xamarin\.Forms(?:\.Maps|\.Visual\.Material|\.Platform\.[^""]*)?""[^>]*/>[^\S\r\n]*(\r?\n)?",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Bodied PackageReference (Include on its own line, closed by </PackageReference>).
	static readonly Regex _xamarinFormsRefBodyed = new(
		@"[^\S\r\n]*<PackageReference\b[^>]*\bInclude=""Xamarin\.Forms(?:\.Maps|\.Visual\.Material|\.Platform\.[^""]*)?""[^>]*>[\s\S]*?</PackageReference>[^\S\r\n]*(\r?\n)?",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Any existing <OutputType>...</OutputType>.
	static readonly Regex _outputTypeAny = new(
		@"<OutputType>[^<]*</OutputType>",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// Any existing <StartupObject>...</StartupObject>.
	static readonly Regex _startupObjectAny = new(
		@"[^\S\r\n]*<StartupObject>[^<]*</StartupObject>[^\S\r\n]*(\r?\n)?",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// First </PropertyGroup> close — insertion point for OutputType/StartupObject when absent.
	static readonly Regex _firstPropertyGroupClose = new(
		@"</PropertyGroup>",
		RegexOptions.Compiled);

	const string _exeOutputType = "<OutputType>Exe</OutputType>";
	const string _startupObject = "<StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>";

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
		result = StripXamarinFormsRefs(result);
		result = ApplyOutputType(result);

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

	static string StripXamarinFormsRefs(string content)
	{
		// bodied form first (longer match wins over self-closed)
		content = _xamarinFormsRefBodyed.Replace(content, string.Empty);
		content = _xamarinFormsRef.Replace(content, string.Empty);
		return content;
	}

	// <OutputType>Exe</OutputType> already present (exact value).
	static readonly Regex _outputTypeExe = new(
		@"<OutputType>\s*Exe\s*</OutputType>",
		RegexOptions.Compiled | RegexOptions.IgnoreCase);

	// The correct StartupObject value already present.
	static readonly Regex _startupObjectCorrect = new(
		@"<StartupObject>\s*Gtk4\.Forms\.Host\.GtkEntryPoint\s*</StartupObject>",
		RegexOptions.Compiled);

	static string ApplyOutputType(string content)
	{
		var hasExe = _outputTypeExe.IsMatch(content);
		var hasStartup = _startupObjectCorrect.IsMatch(content);

		// Already fully correct — touch nothing.
		if (hasExe && hasStartup)
			return content;

		var hasAny = _outputTypeAny.IsMatch(content);

		// No OutputType at all: insert both before first </PropertyGroup>.
		if (!hasAny)
		{
			return _firstPropertyGroupClose.Replace(
				content,
				$"  {_exeOutputType}\n  {_startupObject}\n  </PropertyGroup>",
				count: 1);
		}

		// OutputType exists but is not Exe: replace ONLY the first occurrence
		// (a project may have Debug/Release PropertyGroups each with <OutputType>).
		if (!hasExe)
		{
			var firstMatch = _outputTypeAny.Match(content);
			content = content.Substring(0, firstMatch.Index)
				+ _exeOutputType
				+ content.Substring(firstMatch.Index + firstMatch.Length);
		}

		// StartupObject absent or wrong: remove every stale one, then append after the
		// FIRST <OutputType>Exe</OutputType> (which is now guaranteed to exist — count: 1).
		if (!hasStartup)
		{
			content = _startupObjectAny.Replace(content, string.Empty);
			content = _outputTypeExe.Replace(
				content,
				$"{_exeOutputType}\n    {_startupObject}",
				count: 1);
		}

		return content;
	}
}
