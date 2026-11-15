using System.Globalization;
using System.Runtime.CompilerServices;

namespace Xamarin.Forms.Core.UnitTests
{
	// Pins the default thread culture to Invariant so DateTime / number formatting is deterministic
	// across CI runners. Newer ICU (ubuntu-24.04, macOS runners) formats the en-US AM/PM designator
	// with a NARROW NO-BREAK SPACE (U+202F); tests that round-trip a date through its string form -
	// DragGestureRecognizerTests / DropGestureRecognizerTests, which hardcode a regular space in
	// their InlineData - then fail "strings differ" while the two strings look identical. Older ICU
	// (the windows-latest runner, ubuntu-22.04) used a regular space, which is why those hosts pass
	// and only Linux-24.04 / macOS regress. InvariantCulture uses a regular space on every platform.
	//
	// This changes only the DEFAULT culture: the handful of tests that need a specific culture set
	// CurrentCulture explicitly (BindingUnitTests, ToStringValueConverterTests, ...) and are
	// unaffected. A ModuleInitializer runs once before any test in the assembly.
	static class CultureModuleInitializer
	{
		[ModuleInitializer]
		internal static void Init()
		{
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
		}
	}
}
