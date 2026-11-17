using System;
namespace Gtk4.Forms
{
	public interface IEmbeddedFontLoader
	{
		(bool success, string filePath) LoadFont(EmbeddedFont font);
	}
}
