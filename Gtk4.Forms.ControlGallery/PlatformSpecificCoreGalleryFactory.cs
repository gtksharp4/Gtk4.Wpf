using System;
using System.Collections.Generic;
using Gtk4.Forms;
using Gtk4.Forms.ControlGallery.GTK;
using Gtk4.Forms.Controls;

[assembly: Dependency(typeof(PlatformSpecificCoreGalleryFactory))]

namespace Gtk4.Forms.ControlGallery.GTK;

public class PlatformSpecificCoreGalleryFactory : IPlatformSpecificCoreGalleryFactory
{
	public string Title => "GTK# Core Gallery";

	public IEnumerable<(Func<Page> Create, string Title)> GetPages()
	{
#if HAVE_OPENTK
			yield return (() => new BasicOpenGLGallery(), "Basic OpenGL Gallery - Legacy");
			yield return (() => new AdvancedOpenGLGallery(), "Advanced OpenGL Gallery - Legacy");
#else
		yield break;
#endif
	}
}