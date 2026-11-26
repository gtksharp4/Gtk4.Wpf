using System;
using System.Collections.Generic;

namespace Gtk4.MarkupCompiler
{
	sealed partial class DpSymbolResolver
	{
		// Common WPF routed/CLR event names, used only when no reference assemblies were supplied
		// (e.g. in unit tests). In a real build the reflection path in ResolveIsEvent is authoritative.
		static readonly HashSet<string> _commonEventNames =
			new HashSet<string>(StringComparer.Ordinal)
			{
				"Click", "Checked", "Unchecked", "Loaded", "Unloaded", "Initialized",
				"SelectionChanged", "TextChanged", "ValueChanged", "Closed", "Closing",
				"GotFocus", "LostFocus", "KeyDown", "KeyUp",
				"MouseDown", "MouseUp", "MouseEnter", "MouseLeave", "MouseMove",
				"MouseLeftButtonDown", "MouseLeftButtonUp", "MouseDoubleClick",
				// Round 36: element lifecycle / size and window lifecycle.
				"SizeChanged", "ContentRendered", "SourceInitialized",
				// Round 36: mouse wheel / right button / preview / capture.
				"MouseWheel", "PreviewMouseWheel", "MouseRightButtonDown", "MouseRightButtonUp",
				"PreviewMouseLeftButtonDown", "PreviewMouseLeftButtonUp",
				"GotMouseCapture", "LostMouseCapture",
				// Round 38: Application lifecycle events (App.xaml root attributes).
				"Startup", "Exit", "Activated", "Deactivated", "SessionEnding", "DispatcherUnhandledException",
			};

		internal static bool FallbackEventHeuristic(string memberName) =>
			memberName != null && _commonEventNames.Contains(memberName);
	}
}
