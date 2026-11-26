namespace System.Windows.Controls.Primitives;

public partial class Popup
{
	/// <summary>Identifies <see cref="AllowsTransparency"/>. Default false.</summary>
	public static readonly DependencyProperty AllowsTransparencyProperty = DependencyProperty.Register(
		nameof(AllowsTransparency), typeof(bool), typeof(Popup), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies <see cref="PopupAnimation"/>. Default None.</summary>
	public static readonly DependencyProperty PopupAnimationProperty = DependencyProperty.Register(
		nameof(PopupAnimation), typeof(PopupAnimation), typeof(Popup),
		new FrameworkPropertyMetadata(PopupAnimation.None),
		v => v is PopupAnimation a && a >= PopupAnimation.None && a <= PopupAnimation.Slide);

	/// <summary>Gets or sets whether the popup can be transparent. Stored only.</summary>
	public bool AllowsTransparency
	{
		get => (bool)GetValue(AllowsTransparencyProperty)!;
		set => SetValue(AllowsTransparencyProperty, value);
	}

	/// <summary>Gets or sets the opening animation. Stored only: GTK popovers choose their own.</summary>
	public PopupAnimation PopupAnimation
	{
		get => (PopupAnimation)GetValue(PopupAnimationProperty)!;
		set => SetValue(PopupAnimationProperty, value);
	}
}
