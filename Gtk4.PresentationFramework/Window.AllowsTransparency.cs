using System;

namespace System.Windows;

public partial class Window
{
	/// <summary>Identifies the <see cref="AllowsTransparency"/> property (default false).</summary>
	public static readonly DependencyProperty AllowsTransparencyProperty = DependencyProperty.Register(
		nameof(AllowsTransparency), typeof(bool), typeof(Window),
		new PropertyMetadata(false, (d, _) => ((Window)d).RefreshAppearance()));

	// Like WPF's HWND, the transparency choice is fixed once the window has been shown.
	bool _transparencyLocked;

	/// <summary>
	/// Gets or sets whether the window client area may be transparent. Requires
	/// <see cref="WindowStyle.None"/>; it cannot change after the window has been shown. It is applied as a
	/// transparent window background (the window is already undecorated). Actual see-through needs a
	/// compositing display; without one GTK paints the area opaque. That is a platform limitation.
	/// </summary>
	public bool AllowsTransparency
	{
		get => (bool)GetValue(AllowsTransparencyProperty)!;
		set => SetValue(AllowsTransparencyProperty, value);
	}

	/// <inheritdoc/>
	protected override void OnBeforeSetValue(DependencyProperty dp)
	{
		base.OnBeforeSetValue(dp);
		if (dp == AllowsTransparencyProperty && _transparencyLocked)
			throw new InvalidOperationException("Cannot set AllowsTransparency after a window has been shown.");
	}

	/// <summary>Validates and locks the transparency choice just before the window is presented.</summary>
	void ApplyTransparencyOnShow()
	{
		if (AllowsTransparency && WindowStyle != WindowStyle.None)
			throw new InvalidOperationException("AllowsTransparency requires WindowStyle to be None.");
		_transparencyLocked = true;
		RefreshAppearance();
	}

	internal override string BuildAppearanceCss() =>
		ElementCssStyle.Join(AllowsTransparency ? "background: transparent;" : string.Empty, base.BuildAppearanceCss());
}
