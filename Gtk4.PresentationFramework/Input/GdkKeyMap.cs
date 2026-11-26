using System.Collections.Generic;

namespace System.Windows.Input;

/// <summary>
/// Maps GDK keyval constants to <see cref="Key"/> and GDK modifier state to
/// <see cref="ModifierKeys"/>.  Used by the GTK input controllers in
/// <c>UIElement.GtkInput.cs</c>.
/// </summary>
internal static class GdkKeyMap
{
	static readonly Dictionary<uint, Key> _map = BuildMap();

	/// <summary>
	/// Converts a GDK keyval (e.g. from <c>EventControllerKey.KeyPressed</c>) to a
	/// <see cref="Key"/>.  Returns <see cref="Key.None"/> for unknown keyvals.
	/// </summary>
	internal static Key FromKeyval(uint keyval) =>
		_map.TryGetValue(keyval, out var k) ? k : Key.None;

	/// <summary>Converts a GDK <see cref="Gdk.ModifierType"/> to <see cref="ModifierKeys"/>.</summary>
	internal static ModifierKeys FromGdkModifier(Gdk.ModifierType mod)
	{
		var result = ModifierKeys.None;
		if ((mod & Gdk.ModifierType.ShiftMask)   != 0) result |= ModifierKeys.Shift;
		if ((mod & Gdk.ModifierType.ControlMask) != 0) result |= ModifierKeys.Control;
		if ((mod & Gdk.ModifierType.AltMask)     != 0) result |= ModifierKeys.Alt;
		if ((mod & Gdk.ModifierType.SuperMask)   != 0) result |= ModifierKeys.Windows;
		return result;
	}

	static Dictionary<uint, Key> BuildMap()
	{
		// GDK keyval constants (from gdk/gdkkeysyms.h).
		// Only the keys declared in Key.cs are listed here.
		var m = new Dictionary<uint, Key>
		{
			{ 0xff08, Key.Back    },   // GDK_KEY_BackSpace
			{ 0xff09, Key.Tab     },   // GDK_KEY_Tab
			{ 0xff0a, Key.LineFeed },  // GDK_KEY_Linefeed
			{ 0xff0b, Key.Clear   },   // GDK_KEY_Clear
			{ 0xff0d, Key.Return  },   // GDK_KEY_Return
			{ 0xff13, Key.Pause   },   // GDK_KEY_Pause
			{ 0xffe5, Key.CapsLock },  // GDK_KEY_Caps_Lock
			{ 0xff1b, Key.Escape  },   // GDK_KEY_Escape
			{ 0x0020, Key.Space   },   // GDK_KEY_space
			{ 0xff55, Key.PageUp  },   // GDK_KEY_Page_Up
			{ 0xff56, Key.PageDown },  // GDK_KEY_Page_Down
			{ 0xff57, Key.End     },   // GDK_KEY_End
			{ 0xff50, Key.Home    },   // GDK_KEY_Home
			{ 0xff51, Key.Left    },   // GDK_KEY_Left
			{ 0xff52, Key.Up      },   // GDK_KEY_Up
			{ 0xff53, Key.Right   },   // GDK_KEY_Right
			{ 0xff54, Key.Down    },   // GDK_KEY_Down
			{ 0xff60, Key.Select  },   // GDK_KEY_Select
			{ 0xff61, Key.Print   },   // GDK_KEY_Print
			{ 0xff62, Key.Execute },   // GDK_KEY_Execute
			{ 0xff63, Key.Insert  },   // GDK_KEY_Insert
			{ 0xffff, Key.Delete  },   // GDK_KEY_Delete
			{ 0xff6a, Key.Help    },   // GDK_KEY_Help
			// Digits
			{ 0x0030, Key.D0 }, { 0x0031, Key.D1 }, { 0x0032, Key.D2 },
			{ 0x0033, Key.D3 }, { 0x0034, Key.D4 }, { 0x0035, Key.D5 },
			{ 0x0036, Key.D6 }, { 0x0037, Key.D7 }, { 0x0038, Key.D8 },
			{ 0x0039, Key.D9 },
			// A-Z (lowercase GDK keyvals → same WPF key)
			{ 0x0061, Key.A }, { 0x0062, Key.B }, { 0x0063, Key.C },
			{ 0x0064, Key.D }, { 0x0065, Key.E }, { 0x0066, Key.F },
			{ 0x0067, Key.G }, { 0x0068, Key.H }, { 0x0069, Key.I },
			{ 0x006a, Key.J }, { 0x006b, Key.K }, { 0x006c, Key.L },
			{ 0x006d, Key.M }, { 0x006e, Key.N }, { 0x006f, Key.O },
			{ 0x0070, Key.P }, { 0x0071, Key.Q }, { 0x0072, Key.R },
			{ 0x0073, Key.S }, { 0x0074, Key.T }, { 0x0075, Key.U },
			{ 0x0076, Key.V }, { 0x0077, Key.W }, { 0x0078, Key.X },
			{ 0x0079, Key.Y }, { 0x007a, Key.Z },
			// A-Z uppercase
			{ 0x0041, Key.A }, { 0x0042, Key.B }, { 0x0043, Key.C },
			{ 0x0044, Key.D }, { 0x0045, Key.E }, { 0x0046, Key.F },
			{ 0x0047, Key.G }, { 0x0048, Key.H }, { 0x0049, Key.I },
			{ 0x004a, Key.J }, { 0x004b, Key.K }, { 0x004c, Key.L },
			{ 0x004d, Key.M }, { 0x004e, Key.N }, { 0x004f, Key.O },
			{ 0x0050, Key.P }, { 0x0051, Key.Q }, { 0x0052, Key.R },
			{ 0x0053, Key.S }, { 0x0054, Key.T }, { 0x0055, Key.U },
			{ 0x0056, Key.V }, { 0x0057, Key.W }, { 0x0058, Key.X },
			{ 0x0059, Key.Y }, { 0x005a, Key.Z },
			// Numpad
			{ 0xffb0, Key.NumPad0 }, { 0xffb1, Key.NumPad1 }, { 0xffb2, Key.NumPad2 },
			{ 0xffb3, Key.NumPad3 }, { 0xffb4, Key.NumPad4 }, { 0xffb5, Key.NumPad5 },
			{ 0xffb6, Key.NumPad6 }, { 0xffb7, Key.NumPad7 }, { 0xffb8, Key.NumPad8 },
			{ 0xffb9, Key.NumPad9 },
			{ 0xffaa, Key.Multiply }, { 0xffab, Key.Add      },
			{ 0xffac, Key.Separator}, { 0xffad, Key.Subtract  },
			{ 0xffae, Key.Decimal  }, { 0xffaf, Key.Divide    },
			// Function keys
			{ 0xffbe, Key.F1  }, { 0xffbf, Key.F2  }, { 0xffc0, Key.F3  },
			{ 0xffc1, Key.F4  }, { 0xffc2, Key.F5  }, { 0xffc3, Key.F6  },
			{ 0xffc4, Key.F7  }, { 0xffc5, Key.F8  }, { 0xffc6, Key.F9  },
			{ 0xffc7, Key.F10 }, { 0xffc8, Key.F11 }, { 0xffc9, Key.F12 },
			// NumLock / Scroll
			{ 0xff7f, Key.NumLock }, { 0xff14, Key.Scroll },
			// Modifier keys
			{ 0xffe1, Key.LeftShift  }, { 0xffe2, Key.RightShift  },
			{ 0xffe3, Key.LeftCtrl   }, { 0xffe4, Key.RightCtrl   },
			{ 0xffe9, Key.LeftAlt    }, { 0xffea, Key.RightAlt    },
			{ 0xffeb, Key.LWin       }, { 0xffec, Key.RWin        },
		};
		return m;
	}
}