using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using IOPath = System.IO.Path;

namespace Gtk4.MarkupCompiler
{
	/// <summary>
	/// MSBuild task that generates <c>InitializeComponent</c> partial classes from WPF XAML files.
	/// Registered via <c>Gtk4.MarkupCompiler.targets</c>; runs before <c>CoreCompile</c>.
	/// </summary>
	public class MarkupCompileTask : Task
	{
		/// <summary>
		/// XAML files to process (Page and ApplicationDefinition build items).
		/// Each item's <c>%(Identity)</c> is the file path.
		/// </summary>
		[Required]
		public ITaskItem[] XamlFiles { get; set; }

		/// <summary>
		/// Reference assembly paths (<c>@(ReferencePath)</c> from MSBuild).
		/// Optional; when supplied the DP symbol resolver uses them instead of the
		/// name heuristic so user-defined controls are handled correctly.
		/// </summary>
		public ITaskItem[] ReferencePaths { get; set; }

		/// <summary>The project's C# sources (<c>@(Compile)</c>); used to detect code-behind.</summary>
		public ITaskItem[] CompileFiles { get; set; }

		/// <summary>Namespace for classes generated from XAML files without <c>x:Class</c>.</summary>
		public string RootNamespace { get; set; }

		/// <summary>Directory under which generated <c>.g.cs</c> files are written.</summary>
		[Required]
		public string OutputDirectory { get; set; }

		/// <summary>Generated <c>.g.cs</c> files; added to <c>@(Compile)</c> by the .targets.</summary>
		[Output]
		public ITaskItem[] GeneratedFiles { get; private set; }

		// Mirrors the item's relative directory so same-named files in different folders never share an output.
		string OutputPathFor(string xamlPath)
		{
			var dirs = new List<string>();
			string[] parts = ImplicitKey(xamlPath).Replace('\\', '/').Split('/');
			for (int i = 0; i < parts.Length - 1; i++)
				if (parts[i].Length > 0 && parts[i] != "." && parts[i] != "..") dirs.Add(parts[i]);
			string dir = IOPath.Combine(OutputDirectory, IOPath.Combine(dirs.ToArray()));
			if (dirs.Count > 0) Directory.CreateDirectory(dir);
			return IOPath.Combine(dir, IOPath.GetFileNameWithoutExtension(xamlPath) + ".g.cs");
		}

		/// <summary>The project directory; a rooted XAML path below it keys by its relative path.</summary>
		public string ProjectDirectory { get; set; }

		string ImplicitKey(string xamlPath)
		{
			if (!IOPath.IsPathRooted(xamlPath)) return xamlPath;
			if (!string.IsNullOrEmpty(ProjectDirectory))
			{
				string root = IOPath.GetFullPath(ProjectDirectory)
					.TrimEnd(IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar)
					+ IOPath.DirectorySeparatorChar;
				string full = IOPath.GetFullPath(xamlPath);
				if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) return full.Substring(root.Length);
			}
			return IOPath.GetFileName(xamlPath);
		}

		HashSet<string> ReadStaticResourceKeys()
		{
			var keys = new HashSet<string>(StringComparer.Ordinal);
			foreach (ITaskItem item in XamlFiles)
			{
				try { XamlCodeGenerator.CollectStaticResourceKeys(File.ReadAllText(item.ItemSpec), keys); }
				catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { }
			}
			return keys;
		}

		/// <inheritdoc />
		public override bool Execute()
		{
			if (XamlFiles == null || XamlFiles.Length == 0)
			{
				Log.LogMessage(MessageImportance.Low, "MarkupCompileTask: no XAML files to process.");
				GeneratedFiles = Array.Empty<ITaskItem>();
				return true;
			}

			Log.LogMessage(MessageImportance.Normal,
				"MarkupCompileTask: generating code for {0} XAML file(s).", XamlFiles.Length);

			Directory.CreateDirectory(OutputDirectory);

			bool success = true;
			var generated = new List<ITaskItem>();
			// Paths compare case-insensitively only where the output file system does.
			var pathComparer = FileSystemCase.IsCaseSensitive(OutputDirectory)
				? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;
			var seen = new Dictionary<string, string>(pathComparer);
			var generator = new XamlCodeGenerator();
			if (CompileFiles != null)
				generator.CodeBehindSources = CodeBehindScan.ReadSources(
					System.Linq.Enumerable.Select(CompileFiles, i => i.ItemSpec), OutputDirectory);

			generator.ProjectStaticResourceKeys = ReadStaticResourceKeys();

			// Thread reference paths into the DP symbol resolver and xmlns-definition source.
			if (ReferencePaths != null && ReferencePaths.Length > 0)
			{
				var paths = new List<string>(ReferencePaths.Length);
				foreach (ITaskItem item in ReferencePaths)
					paths.Add(item.ItemSpec);
				generator.SetReferencePaths(paths);
				generator.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(paths));
			}

			foreach (ITaskItem item in XamlFiles)
			{
				string xamlPath = item.ItemSpec;
				// Normalise path separators on the current OS.
				if (IOPath.DirectorySeparatorChar == '/' && xamlPath.Contains("\\"))
					xamlPath = xamlPath.Replace('\\', '/');
				else if (IOPath.DirectorySeparatorChar == '\\' && xamlPath.Contains("/"))
					xamlPath = xamlPath.Replace('/', '\\');

				string fullXamlPath = IOPath.GetFullPath(xamlPath);
				string outputPath = OutputPathFor(xamlPath);
				if (seen.TryGetValue(outputPath, out string first))
				{
					// The same item listed twice is harmless; two different files sharing an output is not.
					if (!pathComparer.Equals(first, fullXamlPath))
					{
						Log.LogError(null, null, null, xamlPath, 0, 0, 0, 0,
							"Generated output {0} collides with {1}; give the files distinct names or folders.",
							outputPath, first);
						success = false;
					}
					else
					{
						Log.LogMessage(MessageImportance.Low,
							"  Skipped duplicate XAML item {0} (already generated).", xamlPath);
					}
					continue;
				}
				seen[outputPath] = fullXamlPath;
				string itemType = item.GetMetadata("GFItemType");
				generator.ItemType = string.IsNullOrEmpty(itemType) ? null : itemType;

				try
				{
					string xaml = File.ReadAllText(xamlPath);
					string code = generator.Generate(xaml, out string className, ImplicitKey(xamlPath), RootNamespace);

					if (code == null)
					{
						Log.LogMessage(MessageImportance.Low,
							"  Skipped {0}: no x:Class attribute and not an implicit Page/Window.", xamlPath);
						continue;
					}

					foreach (string note in generator.Messages)
						Log.LogMessage(null, null, null, xamlPath, 0, 0, 0, 0, MessageImportance.High, note);

					File.WriteAllText(outputPath, code);
					Log.LogMessage(MessageImportance.Low,
						"  {0} -> {1} ({2})", xamlPath, outputPath, className);
					generated.Add(new TaskItem(outputPath));
				}
				catch (XamlCodeGeneratorException ex)
				{
					Log.LogError(null, null, null, xamlPath, 0, 0, 0, 0,
						ex.Message, ex.HelpLink, ex.Source);
					// Write an empty file so the .g.cs item still exists in @(Compile).
					File.WriteAllText(outputPath, string.Empty);
					generated.Add(new TaskItem(outputPath));
					success = false;
				}
				catch (Exception ex)
				{
					Log.LogError(null, null, null, xamlPath, 0, 0, 0, 0,
						ex.Message, ex.HelpLink, ex.Source);
					File.WriteAllText(outputPath, string.Empty);
					generated.Add(new TaskItem(outputPath));
					success = false;
				}
			}

			GeneratedFiles = generated.ToArray();
			generator.Dispose();
			return success;
		}
	}
}
