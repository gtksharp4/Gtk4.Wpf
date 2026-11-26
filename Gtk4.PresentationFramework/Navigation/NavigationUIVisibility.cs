namespace System.Windows.Navigation;

/// <summary>Whether a navigation host shows its navigation chrome (back and forward buttons).</summary>
public enum NavigationUIVisibility
{
	/// <summary>Shown when there is journal history.</summary>
	Automatic = 0,

	/// <summary>Always shown.</summary>
	Visible = 1,

	/// <summary>Never shown.</summary>
	Hidden = 2,
}
