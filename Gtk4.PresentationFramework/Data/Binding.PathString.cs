namespace System.Windows.Data;

public partial class Binding
{
	// The engine walks CLR property names. A "(0)" path holding one DependencyProperty reads that
	// property's name; any other placeholder path stays unresolved (a binding error, not a crash).
	internal string? PathString => Path?.ResolvedString;
}
