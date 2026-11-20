using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Controls;

public partial class Button
{
	/// <summary>Identifies the <see cref="IsDefault"/> property.</summary>
	public static readonly DependencyProperty IsDefaultProperty =
		DependencyProperty.Register(
			nameof(IsDefault), typeof(bool), typeof(Button), new PropertyMetadata(false, OnIsDefaultChanged));

	/// <summary>Identifies the <see cref="IsCancel"/> property.</summary>
	public static readonly DependencyProperty IsCancelProperty =
		DependencyProperty.Register(nameof(IsCancel), typeof(bool), typeof(Button), new PropertyMetadata(false));

	static readonly DependencyPropertyKey _isDefaultedKey =
		DependencyProperty.RegisterReadOnly(
			nameof(IsDefaulted), typeof(bool), typeof(Button), new PropertyMetadata(false));

	/// <summary>Identifies the <see cref="IsDefaulted"/> property.</summary>
	public static readonly DependencyProperty IsDefaultedProperty = _isDefaultedKey.DependencyProperty;

	static void OnIsDefaultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Button)d).UpdateIsDefaulted();

	/// <summary>Gets or sets whether Enter in the window clicks this button.</summary>
	public bool IsDefault
	{
		get => (bool)GetValue(IsDefaultProperty)!;
		set => SetValue(IsDefaultProperty, value);
	}

	/// <summary>Gets or sets whether Esc in the window clicks this button.</summary>
	public bool IsCancel
	{
		get => (bool)GetValue(IsCancelProperty)!;
		set => SetValue(IsCancelProperty, value);
	}

	/// <summary>
	/// Gets whether Enter would click this button now: it is the <see cref="IsDefault"/> button and the
	/// keyboard focus is not on another button.
	/// </summary>
	public bool IsDefaulted => (bool)GetValue(IsDefaultedProperty)!;

	internal void UpdateIsDefaulted()
	{
		bool focusOnOtherButton = Keyboard.FocusedElement is ButtonBase other && !ReferenceEquals(other, this);
		SetValue(_isDefaultedKey, IsDefault && !focusOnOtherButton);
	}

	/// <summary>Clicks the button for the default/cancel accelerator.</summary>
	internal void ClickFromAccelerator()
	{
		if (IsEnabled) OnClick();
	}
}
