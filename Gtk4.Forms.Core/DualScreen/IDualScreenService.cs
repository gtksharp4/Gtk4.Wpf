using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Gtk4.Forms;
using Gtk4.Forms.Internals;

namespace Gtk4.Forms.DualScreen
{
	internal interface IDualScreenService
	{
		event EventHandler OnScreenChanged;
		bool IsSpanned { get; }
		bool IsLandscape { get; }
		Rectangle GetHinge();
		Size ScaledScreenSize { get; }
		Point? GetLocationOnScreen(VisualElement visualElement);
		object WatchForChangesOnLayout(VisualElement visualElement, Action action);
		void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle);
		Task<int> GetHingeAngleAsync();
	}
}
