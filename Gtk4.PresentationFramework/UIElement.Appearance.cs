using System.Collections.Generic;
using System.Windows.Documents;

namespace System.Windows;

public abstract partial class UIElement
{
	// Built on first use: referencing Panel/Border from the UIElement type initialiser would be circular.
	static HashSet<DependencyProperty>? _appearanceDps;

	static HashSet<DependencyProperty> AppearanceDps => _appearanceDps ??= new HashSet<DependencyProperty>
	{
		Controls.Panel.BackgroundProperty, TextElement.ForegroundProperty, TextElement.FontFamilyProperty,
		TextElement.FontSizeProperty, TextElement.FontWeightProperty, TextElement.FontStyleProperty,
		TextElement.FontStretchProperty, Controls.Border.BorderBrushProperty,
		Controls.Border.BorderThicknessProperty, Controls.Border.CornerRadiusProperty,
	};

	// The CSS depends on the value source (a local value equal to the inherited one still needs CSS),
	// so refresh on any layer change, not only on an effective-value change.
	static void InstallAppearanceHook()
	{
		DependencyObject.ValueSourceChanged = (d, dp) =>
		{
			if (d is UIElement el && AppearanceDps.Contains(dp)) el.RefreshAppearance();
		};
	}

	ElementCssStyle? _appearance;

	/// <summary>Appearance CSS declarations for this element; overridden by types that own the DPs.</summary>
	internal virtual string BuildAppearanceCss() => string.Empty;

	/// <summary>The appearance CSS last pushed to the widget (empty when none); for tests.</summary>
	internal string AppearanceCss => _appearance?.Declarations ?? string.Empty;

	/// <summary>True when an appearance <c>CssProvider</c> is attached to the widget; for tests.</summary>
	internal bool HasAppearanceProvider => _appearance?.HasProvider == true;

	/// <summary>CSS parse errors GTK reported for the appearance provider; for tests.</summary>
	internal int AppearanceParseErrors => _appearance?.ParseErrors ?? 0;

	partial void ApplyAppearanceOnCreated(Gtk.Widget widget) => RefreshAppearance();

	/// <summary>Re-evaluates the appearance CSS; a no-op until the widget exists.</summary>
	internal void RefreshAppearance()
	{
		if (_widget == null) return;
		var style = _appearance ??= new ElementCssStyle(_widget);
		style.Update(BuildAppearanceCss(), BuildAppearanceRules());
		style.UpdateChild(AppearanceChildTarget, BuildAppearanceChildRules());
		style.UpdateExtra(AppearanceExtraTarget, BuildAppearanceExtraRules());
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
