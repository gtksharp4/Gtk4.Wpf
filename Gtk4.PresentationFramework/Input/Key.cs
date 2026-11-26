namespace System.Windows.Input;

/// <summary>
/// WPF key enumeration (subset — extend on demand).
/// Values map to common keyboard keys; name matches WPF's <c>System.Windows.Input.Key</c>.
/// </summary>
[System.ComponentModel.TypeConverter(typeof(KeyConverter))]
public enum Key
{
	None = 0,
	Cancel, Back, Tab, LineFeed, Clear, Return, Enter = Return,
	Pause, Capital, CapsLock = Capital,
	Escape,
	Space, PageUp, PageDown, End, Home,
	Left, Up, Right, Down,
	Select, Print, Execute, Snapshot, PrintScreen = Snapshot,
	Insert, Delete,
	Help,
	D0, D1, D2, D3, D4, D5, D6, D7, D8, D9,
	A, B, C, D, E, F, G, H, I, J, K, L, M,
	N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
	LWin, RWin, Apps, Sleep,
	NumPad0, NumPad1, NumPad2, NumPad3, NumPad4,
	NumPad5, NumPad6, NumPad7, NumPad8, NumPad9,
	Multiply, Add, Separator, Subtract, Decimal, Divide,
	F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12,
	F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24,
	NumLock, Scroll,
	LeftShift, RightShift, LeftCtrl, RightCtrl, LeftAlt, RightAlt,
	OemSemicolon, OemPlus, OemComma, OemMinus, OemPeriod, OemQuestion,
	OemTilde, OemOpenBrackets, OemPipe, OemCloseBrackets, OemQuotes,
	OemBackslash,
}