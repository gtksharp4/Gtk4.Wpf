using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using IOPath = System.IO.Path;

namespace Gtk4.Forms.MarkupCompiler
{
	/// <summary>
	/// MSBuild task that generates <c>InitializeComponent</c> partial classes from WPF XAML files.
	/// Registered via <c>Gtk4.Forms.MarkupCompiler.targets</c>; runs before <c>CoreCompile</c>.
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

		static string ImplicitKey(string xamlPath) =>
			IOPath.IsPathRooted(xamlPath) ? IOPath.GetFileName(xamlPath) : xamlPath;

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
			var generator = new XamlCodeGenerator();
			if (CompileFiles != null)
				generator.CodeBehindSources = CodeBehindScan.ReadSources(
					System.Linq.Enumerable.Select(CompileFiles, i => i.ItemSpec), OutputDirectory);

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

				string outputPath = IOPath.Combine(
					OutputDirectory,
					IOPath.GetFileNameWithoutExtension(xamlPath) + ".g.cs");

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
						Log.LogMessage(null, null, null, xamlPath, 0, 0, 0, 0, MessageImportance.Normal, note);

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
