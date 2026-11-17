using System.Collections.Generic;
using System.Reflection;

namespace Gtk4.Forms.Controls.Tests
{
	public interface IPlatformTestSettings
	{
		Assembly Assembly { get; }
		Dictionary<string, object> TestRunSettings { get; }
	}
}
