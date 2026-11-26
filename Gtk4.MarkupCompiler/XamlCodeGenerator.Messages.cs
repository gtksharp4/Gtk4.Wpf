using System.Collections.Generic;
using System.Xml.Linq;

namespace Gtk4.MarkupCompiler
{
	public partial class XamlCodeGenerator
	{
		readonly List<string> _messages = new List<string>();

		/// <summary>
		/// Informational notes of the last <c>Generate</c>: markup it dropped because the layer lacks the type or
		/// property. Messages, not warnings: the build treats warnings as errors.
		/// </summary>
		public IReadOnlyList<string> Messages => _messages;

		void Note(string message)
		{
			if (!_messages.Contains(message)) _messages.Add(message);
		}

		// A media property or child element the layer lacks is dropped; say so instead of dropping it silently.
		void NoteSkippedMedia(XElement owner, string what)
			=> Note($"Skipped {what} on {owner.Name.LocalName}: not available in the layer.");
	}
}
