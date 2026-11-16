using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.Forms.Platform.GTK
{
	/// <summary>
	/// Reflection-based GTK 4 host. Discovers the consumer's
	/// <see cref="Xamarin.Forms.Application"/> subclass, initialises the platform and
	/// runs the GTK main loop.
	/// </summary>
	public static class GtkHost
	{
		/// <summary>
		/// Initialises Xamarin.Forms on the GTK 4 backend and enters the main loop.
		/// Returns 0 on normal exit.
		/// </summary>
		/// <param name="args">Command-line arguments (unused; reserved for future use).</param>
		public static int Run(string[] args)
		{
			GtkThemes.Init();
			Gtk.Application.Init();

			Forms.Init(RendererAssemblies());

			var app = ResolveApplication();

			var window = new FormsWindow();
			window.LoadApplication(app);

			var attr = Assembly.GetEntryAssembly()
				?.GetCustomAttribute<GtkMainApplicationAttribute>();

			var title = attr?.Title
				?? Assembly.GetEntryAssembly()?.GetName().Name
				?? "Xamarin.Forms";

			window.SetApplicationTitle(title);

			if (!string.IsNullOrEmpty(attr?.Icon))
				window.SetApplicationIcon(attr.Icon);

			window.Show();
			Gtk.Application.Run();
			return 0;
		}

		// -----------------------------------------------------------------
		// Private helpers
		// -----------------------------------------------------------------

		/// <summary>
		/// Returns the renderer assemblies to pass to <see cref="Forms.Init"/>.
		/// Always includes <c>Xamarin.Forms.Platform.Gtk4</c>; optionally adds
		/// <c>Xamarin.Forms.Maps.Gtk</c> when that assembly is resolvable.
		/// </summary>
		static IEnumerable<Assembly> RendererAssemblies()
		{
			var list = new List<Assembly>();

			list.Add(Assembly.Load("Xamarin.Forms.Platform.Gtk4"));

			try
			{
				list.Add(Assembly.Load("Xamarin.Forms.Maps.Gtk"));
			}
			catch (Exception)
			{
				// Maps assembly is optional — absent unless the consumer also
				// references Gtk4.Forms.Maps.Gtk.
			}

			return list;
		}

		/// <summary>
		/// Finds the <see cref="Xamarin.Forms.Application"/> subclass to instantiate.
		/// </summary>
		/// <remarks>
		/// Resolution order:
		/// <list type="number">
		///   <item><c>[assembly: GtkMainApplication(typeof(T))]</c> — explicit, wins.</item>
		///   <item>Exactly one concrete subclass with a public parameterless ctor — use it.</item>
		///   <item>Multiple candidates — prefer the one named <c>App</c>.</item>
		///   <item>Zero or still-ambiguous — throw with guidance.</item>
		/// </list>
		/// </remarks>
		static Application ResolveApplication()
		{
			var entry = Assembly.GetEntryAssembly();
			if (entry == null)
				throw new InvalidOperationException(
					"Assembly.GetEntryAssembly() returned null. " +
					"Annotate your application with " +
					"[assembly: GtkMainApplication(typeof(YourApp))].");

			// 1. Explicit attribute.
			var attr = entry.GetCustomAttribute<GtkMainApplicationAttribute>();
			if (attr != null)
				return Instantiate(attr.ApplicationType);

			// 2. Scan for concrete Application subclasses with a public parameterless ctor.
			IEnumerable<Type> allTypes;
			try
			{
				allTypes = entry.GetTypes();
			}
			catch (ReflectionTypeLoadException ex)
			{
				allTypes = ex.Types.Where(t => t != null)!;
			}

			var candidates = allTypes
				.Where(t =>
					!t.IsAbstract &&
					typeof(Application).IsAssignableFrom(t) &&
					t.GetConstructor(Type.EmptyTypes) != null)
				.ToList();

			if (candidates.Count == 0)
				throw new InvalidOperationException(
					"No concrete Xamarin.Forms.Application subclass with a public " +
					"parameterless constructor was found in the entry assembly. " +
					"Annotate your application with " +
					"[assembly: GtkMainApplication(typeof(YourApp))].");

			if (candidates.Count == 1)
				return Instantiate(candidates[0]);

			// 3. Prefer the type named "App", but only when exactly one candidate has that name.
			var byName = candidates.Where(t => t.Name == "App").ToList();
			if (byName.Count == 1)
				return Instantiate(byName[0]);

			var names = string.Join(", ", candidates.Select(t => t.FullName));
			throw new InvalidOperationException(
				$"Multiple Xamarin.Forms.Application subclasses found: {names}. " +
				"Annotate your assembly to disambiguate: " +
				"[assembly: GtkMainApplication(typeof(YourApp))].");
		}

		static Application Instantiate(Type type)
		{
			if (!typeof(Application).IsAssignableFrom(type))
				throw new InvalidOperationException(
					$"{type.FullName} does not inherit from Xamarin.Forms.Application.");

			return (Application)Activator.CreateInstance(type);
		}
	}
}
