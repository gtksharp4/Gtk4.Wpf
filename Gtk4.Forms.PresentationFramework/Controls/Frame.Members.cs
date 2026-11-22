using System.Windows.Navigation;

namespace System.Windows.Controls;

public partial class Frame
{
	/// <summary>Identifies <see cref="NavigationUIVisibility"/>. Default Automatic; stored only.</summary>
	public static readonly DependencyProperty NavigationUIVisibilityProperty = DependencyProperty.Register(
		nameof(NavigationUIVisibility), typeof(NavigationUIVisibility), typeof(Frame),
		new PropertyMetadata(NavigationUIVisibility.Automatic),
		v => v is NavigationUIVisibility n && n >= NavigationUIVisibility.Automatic
			&& n <= NavigationUIVisibility.Hidden);

	/// <summary>
	/// Gets or sets whether the frame shows navigation chrome. GTK frames have none, so it is
	/// stored only.
	/// </summary>
	public NavigationUIVisibility NavigationUIVisibility
	{
		get => (NavigationUIVisibility)GetValue(NavigationUIVisibilityProperty)!;
		set => SetValue(NavigationUIVisibilityProperty, value);
	}

	/// <summary>The URI of the content now shown, or null when it was navigated to as an object.</summary>
	public Uri? CurrentSource => NavigationService.CurrentSource;

	/// <summary>Raised after the navigated content has been placed in the frame and laid out.</summary>
	public event EventHandler? ContentRendered;

	/// <summary>Raises <see cref="ContentRendered"/>.</summary>
	protected virtual void OnContentRendered(EventArgs e) => ContentRendered?.Invoke(this, e);
}
