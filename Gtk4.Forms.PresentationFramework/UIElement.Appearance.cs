namespace System.Windows;

public abstract partial class UIElement
{
	ElementCssStyle? _appearance;

	/// <summary>Appearance CSS declarations for this element; overridden by types that own the DPs.</summary>
	internal virtual string BuildAppearanceCss() => string.Empty;

	/// <summary>The appearance CSS last pushed to the widget (empty when none); for tests.</summary>
	internal string AppearanceCss => _appearance?.Declarations ?? string.Empty;

	/// <summary>True when an appearance <c>CssProvider</c> is attached to the widget; for tests.</summary>
	internal bool HasAppearanceProvider => _appearance?.HasProvider == true;

	partial void ApplyAppearanceOnCreated(Gtk.Widget widget) => RefreshAppearance();

	/// <summary>Re-evaluates the appearance CSS; a no-op until the widget exists.</summary>
	internal void RefreshAppearance()
	{
		if (_widget == null) return;
		(_appearance ??= new ElementCssStyle(_widget)).Update(BuildAppearanceCss());
	}

	/// <summary>Inheriting text-property metadata for an owner; keeps the shared default and refreshes CSS.</summary>
	internal static FrameworkPropertyMetadata InheritingTextMetadata(
		DependencyProperty textProperty, bool affectsMeasure) =>
		new FrameworkPropertyMetadata(
			textProperty.DefaultMetadata.DefaultValue, true, affectsMeasure, false, !affectsMeasure,
			OnAppearanceChanged);

	/// <summary>Shared change callback for every appearance DP.</summary>
	internal static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		(d as UIElement)?.RefreshAppearance();
}
