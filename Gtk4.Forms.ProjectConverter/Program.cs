namespace Gtk4.Forms.ProjectConverter;

/// <summary>Entry point for the project-converter tool.</summary>
static class Program
{
	/// <summary>
	/// <para>Usage: <c>dotnet Gtk4.Forms.ProjectConverter.dll --convert-project &lt;path.csproj&gt;</c></para>
	/// Returns 0 on success, 1 on bad/missing arguments.
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
			return 0;
		}

		Console.Error.WriteLine("Usage: Gtk4.Forms.ProjectConverter --convert-project <path.csproj>");
		return 1;
	}
}
