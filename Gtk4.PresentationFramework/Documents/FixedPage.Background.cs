using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Documents;

public sealed partial class FixedPage
{
	/// <summary>Identifies the Background property (shared with <see cref="Panel.BackgroundProperty"/>).</summary>
	public static readonly DependencyProperty BackgroundProperty = Panel.BackgroundProperty.AddOwner(
		typeof(FixedPage),
		new FrameworkPropertyMetadata(Panel.BackgroundProperty.DefaultMetadata.DefaultValue, OnAppearanceChanged));

	/// <summary>Gets or sets the brush that fills the page.</summary>
	public Brush? Background
	{
		get => (Brush?)GetValue(BackgroundProperty);
		set => SetValue(BackgroundProperty, value);
	}

	internal override string BuildAppearanceCss() => ElementCssStyle.Background(this);
}
