using System.Windows.Input;

namespace System.Windows.Shell;

public partial class WindowChrome
{
	/// <summary>Identifies the attached <c>WindowChrome</c> property of a <see cref="Window"/>.</summary>
	public static readonly DependencyProperty WindowChromeProperty = DependencyProperty.RegisterAttached(
		"WindowChrome", typeof(WindowChrome), typeof(WindowChrome),
		new FrameworkPropertyMetadata(null, OnWindowChromeChanged));

	/// <summary>Identifies the attached <c>IsHitTestVisibleInChrome</c> property. Default false.</summary>
	public static readonly DependencyProperty IsHitTestVisibleInChromeProperty =
		DependencyProperty.RegisterAttached(
			"IsHitTestVisibleInChrome", typeof(bool), typeof(WindowChrome),
			new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the attached <c>ResizeGripDirection</c> property. Default None.</summary>
	public static readonly DependencyProperty ResizeGripDirectionProperty =
		DependencyProperty.RegisterAttached(
			"ResizeGripDirection", typeof(ResizeGripDirection), typeof(WindowChrome),
			new FrameworkPropertyMetadata(ResizeGripDirection.None), IsValidDirection);

	/// <summary>Gets the chrome attached to <paramref name="window"/>.</summary>
	public static WindowChrome? GetWindowChrome(Window window)
	{
		if (window == null) throw new ArgumentNullException(nameof(window));
		return (WindowChrome?)window.GetValue(WindowChromeProperty);
	}

	/// <summary>Attaches <paramref name="chrome"/> to <paramref name="window"/>.</summary>
	public static void SetWindowChrome(Window window, WindowChrome? chrome)
	{
		if (window == null) throw new ArgumentNullException(nameof(window));
		window.SetValue(WindowChromeProperty, chrome);
	}

	/// <summary>Gets whether <paramref name="inputElement"/> receives input inside the chrome caption area.</summary>
	public static bool GetIsHitTestVisibleInChrome(IInputElement inputElement) =>
		(bool)Target(inputElement).GetValue(IsHitTestVisibleInChromeProperty)!;

	/// <summary>Sets whether <paramref name="inputElement"/> receives input inside the chrome caption area.</summary>
	public static void SetIsHitTestVisibleInChrome(IInputElement inputElement, bool hitTestVisible) =>
		Target(inputElement).SetValue(IsHitTestVisibleInChromeProperty, hitTestVisible);

	/// <summary>Gets the direction in which <paramref name="inputElement"/> resizes the window.</summary>
	public static ResizeGripDirection GetResizeGripDirection(IInputElement inputElement) =>
		(ResizeGripDirection)Target(inputElement).GetValue(ResizeGripDirectionProperty)!;

	/// <summary>Sets the direction in which <paramref name="inputElement"/> resizes the window.</summary>
	public static void SetResizeGripDirection(IInputElement inputElement, ResizeGripDirection direction) =>
		Target(inputElement).SetValue(ResizeGripDirectionProperty, direction);

	static DependencyObject Target(IInputElement inputElement)
	{
		if (inputElement == null) throw new ArgumentNullException(nameof(inputElement));
		return inputElement as DependencyObject
			?? throw new ArgumentException("The element must be a DependencyObject.", nameof(inputElement));
	}

	static bool IsValidDirection(object? value) =>
		value is ResizeGripDirection d && d >= ResizeGripDirection.None && d <= ResizeGripDirection.Left;

	static void OnWindowChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var window = (Window)d;
		if (e.OldValue is WindowChrome old) old.Changed -= window.OnShellChromeChanged;
		if (e.NewValue is WindowChrome chrome) chrome.Changed += window.OnShellChromeChanged;
		window.ApplyShellChrome();
	}
}
