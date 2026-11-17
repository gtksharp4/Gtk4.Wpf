using System;

namespace Gtk4.Forms.Platform.GTK;

/// <summary>
/// Marks the application's entry-point <see cref="Gtk4.Forms.Application"/> subclass.
/// Apply this assembly-level attribute when more than one concrete <c>Application</c>
/// subclass exists, or to suppress the automatic "type named App" heuristic.
/// </summary>
/// <example>
/// <code>
/// [assembly: GtkMainApplication(typeof(MyApp), Title = "My App")]
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class GtkMainApplicationAttribute : Attribute
{
	/// <param name="applicationType">
	/// The concrete <see cref="Gtk4.Forms.Application"/> subclass to instantiate.
	/// </param>
	public GtkMainApplicationAttribute(Type applicationType)
	{
		ApplicationType = applicationType ?? throw new ArgumentNullException(nameof(applicationType));
	}

	/// <summary>The application type to launch.</summary>
	public Type ApplicationType { get; }

	/// <summary>
	/// Optional window title. When set, overrides the entry-assembly name that
	/// <see cref="GtkHost"/> uses by default.
	/// </summary>
	public string Title { get; set; }

	/// <summary>
	/// Optional XDG icon name passed to <c>FormsWindow.SetApplicationIcon</c>.
	/// </summary>
	public string Icon { get; set; }
}