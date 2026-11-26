namespace System.Windows.Input;

/// <summary>Modifier keys bitmask (matches WPF's <c>System.Windows.Input.ModifierKeys</c>).</summary>
[Flags]
public enum ModifierKeys
{
	None    = 0x0,
	Alt     = 0x1,
	Control = 0x2,
	Shift   = 0x4,
	Windows = 0x8,
}