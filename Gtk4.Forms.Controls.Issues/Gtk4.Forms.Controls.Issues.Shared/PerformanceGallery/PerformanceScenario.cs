
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.Controls
{
	[Preserve(AllMembers = true)]
	internal class PerformanceScenario
	{
		public View View { get; set; }
		public string Name { get; private set; }

		public PerformanceScenario() { }
		public PerformanceScenario(string name)
		{
			Name = name;
		}
	}
}
