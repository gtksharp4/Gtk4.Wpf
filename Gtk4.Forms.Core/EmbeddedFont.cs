using System;
using System.IO;

namespace Gtk4.Forms
{
	public class EmbeddedFont
	{
		public string FontName { get; set; }
		public Stream ResourceStream { get; set; }
	}
}
