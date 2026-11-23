namespace System.Windows;

public partial class Window
{
	static readonly DependencyPropertyKey IsActivePropertyKey =
		DependencyProperty.RegisterReadOnly("IsActive", typeof(bool), typeof(Window),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the read-only <see cref="IsActive"/> dependency property (default false).</summary>
	public static readonly DependencyProperty IsActiveProperty = IsActivePropertyKey.DependencyProperty;

	/// <summary>
	/// True while the window is the active toplevel (GTK <c>is-active</c>). Set before
	/// <see cref="Activated"/> is raised and cleared before <see cref="Deactivated"/>.
	/// </summary>
	public bool IsActive
	{
		get
		{
			VerifyAccess();
			return (bool)GetValue(IsActiveProperty)!;
		}
	}
}
