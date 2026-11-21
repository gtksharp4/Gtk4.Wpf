using System.Windows.Media;

namespace System.Windows.Documents;

public abstract partial class TextElement
{
	/// <summary>Identifies the Background property (null default; does not inherit).</summary>
	public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
		"Background", typeof(Brush), typeof(TextElement), new FrameworkPropertyMetadata(null));

	/// <summary>Gets or sets the background brush. Stored only: the Pango text path does not paint it.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}
}
