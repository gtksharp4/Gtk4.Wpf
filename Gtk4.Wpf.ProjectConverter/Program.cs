namespace Gtk4.Wpf.ProjectConverter;

/// <summary>Entry point for the WPF project-converter tool.</summary>
static class Program
{
	/// <summary>
	/// <para>Usage: <c>dotnet Gtk4.Wpf.ProjectConverter.dll --convert-project &lt;path.csproj&gt;</c></para>
	/// Exit codes: <c>0</c> nothing to do (already converted), <c>10</c> the project was just
	/// converted (the caller's current build was evaluated as WPF and must be re-run),
	/// <c>1</c> bad/missing arguments.
	/// </summary>
	static int Main(string[] args)
	{
		if (args.Length > 0 && string.Equals(args[0], "--convert-project", StringComparison.Ordinal))
		{
			if (args.Length < 2)
			{
				Console.Error.WriteLine("--convert-project requires a project file path.");
				return 1;
			}

			var projectPath = Path.GetFullPath(args[1].Trim('"'));
			if (!File.Exists(projectPath))
			{
				Console.Error.WriteLine($"Project file not found: {projectPath}");
				return 1;
			}

			var converted = CsprojConverter.TryConvertFile(projectPath);
			Console.WriteLine(converted
				? $"Converted: {projectPath}"
				: $"Already converted: {projectPath}");
			// 10 tells Base.targets this build's project was just rewritten and must be re-run;
			// 0 means it was already converted so the current build can proceed.
			return converted ? 10 : 0;
		}

		Console.Error.WriteLine("Usage: Gtk4.Wpf.ProjectConverter --convert-project <path.csproj>");
		return 1;
	}
}
