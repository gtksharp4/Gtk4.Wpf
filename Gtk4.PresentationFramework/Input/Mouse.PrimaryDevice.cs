namespace System.Windows.Input;

public static partial class Mouse
{
	/// <summary>The primary mouse device.</summary>
	public static MouseDevice PrimaryDevice { get; } = new GtkMouseDevice();
}
