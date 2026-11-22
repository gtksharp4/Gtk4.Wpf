namespace System.Windows.Documents;

// Target dispatch (a Frame or NavigationWindow whose Name matches) is not wired: the target travels in
// RequestNavigateEventArgs.Target and a handler navigates.
public partial class Hyperlink
{
	/// <summary>Identifies the TargetName property (default empty).</summary>
	public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register(
		nameof(TargetName), typeof(string), typeof(Hyperlink),
		new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.None));

	/// <summary>Gets or sets the name of the frame or window the link navigates in.</summary>
	public string TargetName
	{
		get => (string)GetValue(TargetNameProperty)!;
		set => SetValue(TargetNameProperty, value);
	}
}
