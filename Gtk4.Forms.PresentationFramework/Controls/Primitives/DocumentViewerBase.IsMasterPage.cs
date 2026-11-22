namespace System.Windows.Controls.Primitives;

public abstract partial class DocumentViewerBase
{
	/// <summary>Identifies the IsMasterPage attached property (default false, inherited).</summary>
	public static readonly DependencyProperty IsMasterPageProperty = DependencyProperty.RegisterAttached(
		"IsMasterPage", typeof(bool), typeof(DocumentViewerBase),
		new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Gets whether <paramref name="element"/> is shown as the master page of a viewer.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static bool GetIsMasterPage(DependencyObject element)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		return (bool)element.GetValue(IsMasterPageProperty)!;
	}

	/// <summary>Sets whether <paramref name="element"/> is shown as the master page of a viewer.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static void SetIsMasterPage(DependencyObject element, bool value)
	{
		if (element == null)
			throw new ArgumentNullException(nameof(element));
		element.SetValue(IsMasterPageProperty, value);
	}
}
