using System.Windows.Media;

namespace System.Windows.Documents;

public abstract partial class TextElement
{
	/// <summary>Identifies the attached Background property (null default; does not inherit).</summary>
	public static readonly DependencyProperty BackgroundProperty = DependencyProperty.RegisterAttached(
		"Background", typeof(Brush), typeof(TextElement), new FrameworkPropertyMetadata(null));

	/// <summary>Gets the attached Background.</summary>
	public static Brush? GetBackground(DependencyObject element) => (Brush?)Get(element, BackgroundProperty);

	/// <summary>Sets the attached Background.</summary>
	public static void SetBackground(DependencyObject element, Brush? value) =>
		Set(element, BackgroundProperty, value!);
}
