namespace System.Windows.Controls;

/// <summary>
/// Provides tooltip attached properties as a service on any <see cref="DependencyObject"/>.
/// <para>
/// <b>Actionable vs store-only:</b>
/// <list type="bullet">
///   <item><term><see cref="IsEnabledProperty"/></term>
///     <description>Actionable: when <see langword="false"/> and the element is realized,
///     clears <c>HasTooltip</c>/<c>TooltipText</c> on the backing GTK widget so no tooltip shows.
///     Setting it back to <see langword="true"/> restores the tooltip.</description></item>
///   <item><term><see cref="InitialShowDelayProperty"/>, <see cref="ShowDurationProperty"/>,
///     <see cref="BetweenShowDelayProperty"/></term>
///     <description>Store-only: GTK controls per-widget tooltip timing via toolkit/theme settings;
///     these values round-trip faithfully but have no per-widget native effect.</description></item>
///   <item><term><see cref="PlacementProperty"/>, <see cref="PlacementTargetProperty"/>,
///     <see cref="HorizontalOffsetProperty"/>, <see cref="VerticalOffsetProperty"/></term>
///     <description>Store-only: GTK positions tooltips automatically; values round-trip but
///     are not forwarded to GTK.</description></item>
/// </list>
/// </para>
/// </summary>
public static partial class ToolTipService
{
	// ── InitialShowDelay DP ──────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetInitialShowDelay"/>/<see cref="SetInitialShowDelay"/>
	/// attached property. Default: 1000 ms.
	/// Store-only: GTK controls tooltip timing globally; this value is not forwarded per widget.
	/// </summary>
	public static readonly DependencyProperty InitialShowDelayProperty =
		DependencyProperty.RegisterAttached(
			"InitialShowDelay",
			typeof(int),
			typeof(ToolTipService),
			new PropertyMetadata(1000));

	/// <summary>Gets the initial show delay (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static int GetInitialShowDelay(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (int)(element.GetValue(InitialShowDelayProperty) ?? 1000);
	}

	/// <summary>Sets the initial show delay (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static void SetInitialShowDelay(DependencyObject element, int value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(InitialShowDelayProperty, value);
	}

	// ── ShowDuration DP ──────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetShowDuration"/>/<see cref="SetShowDuration"/>
	/// attached property. Default: 5000 ms.
	/// Store-only: GTK controls tooltip timing globally; this value is not forwarded per widget.
	/// </summary>
	public static readonly DependencyProperty ShowDurationProperty =
		DependencyProperty.RegisterAttached(
			"ShowDuration",
			typeof(int),
			typeof(ToolTipService),
			new PropertyMetadata(5000));

	/// <summary>Gets the show duration (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static int GetShowDuration(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (int)(element.GetValue(ShowDurationProperty) ?? 5000);
	}

	/// <summary>Sets the show duration (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static void SetShowDuration(DependencyObject element, int value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ShowDurationProperty, value);
	}

	// ── BetweenShowDelay DP ──────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetBetweenShowDelay"/>/<see cref="SetBetweenShowDelay"/>
	/// attached property. Default: 100 ms.
	/// Store-only: GTK controls tooltip timing globally; this value is not forwarded per widget.
	/// </summary>
	public static readonly DependencyProperty BetweenShowDelayProperty =
		DependencyProperty.RegisterAttached(
			"BetweenShowDelay",
			typeof(int),
			typeof(ToolTipService),
			new PropertyMetadata(100));

	/// <summary>Gets the between-show delay (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static int GetBetweenShowDelay(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (int)(element.GetValue(BetweenShowDelayProperty) ?? 100);
	}

	/// <summary>Sets the between-show delay (ms) for the tooltip on <paramref name="element"/>.</summary>
	public static void SetBetweenShowDelay(DependencyObject element, int value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(BetweenShowDelayProperty, value);
	}

	// ── IsEnabled DP ─────────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetIsEnabled"/>/<see cref="SetIsEnabled"/> attached property.
	/// Default: <see langword="true"/>.
	/// <b>Actionable:</b> when <see langword="false"/> and the element is realized, clears
	/// <c>HasTooltip</c>/<c>TooltipText</c> on the GTK widget. Restores on <see langword="true"/>.
	/// </summary>
	public static readonly DependencyProperty IsEnabledProperty =
		DependencyProperty.RegisterAttached(
			"IsEnabled",
			typeof(bool),
			typeof(ToolTipService),
			new PropertyMetadata(true, OnIsEnabledChanged));

	static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is FrameworkElement fe && fe.IsWidgetCreated)
			FrameworkElement.ReapplyTooltipEnabled(fe);
	}

	/// <summary>Gets whether the tooltip is enabled on <paramref name="element"/>.</summary>
	public static bool GetIsEnabled(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)(element.GetValue(IsEnabledProperty) ?? true);
	}

	/// <summary>Sets whether the tooltip is enabled on <paramref name="element"/>.</summary>
	public static void SetIsEnabled(DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsEnabledProperty, value);
	}

	// ── ShowOnDisabled DP ────────────────────────────────────────────────────

	/// <summary>
	/// Identifies the <see cref="GetShowOnDisabled"/>/<see cref="SetShowOnDisabled"/>
	/// attached property. Default: <see langword="false"/>.
	/// Store-only: GTK does not distinguish disabled-widget tooltip suppression per widget.
	/// </summary>
	public static readonly DependencyProperty ShowOnDisabledProperty =
		DependencyProperty.RegisterAttached(
			"ShowOnDisabled",
			typeof(bool),
			typeof(ToolTipService),
			new PropertyMetadata(false));

	/// <summary>Gets whether the tooltip shows when the element is disabled.</summary>
	public static bool GetShowOnDisabled(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)(element.GetValue(ShowOnDisabledProperty) ?? false);
	}

	/// <summary>Sets whether the tooltip shows when the element is disabled.</summary>
	public static void SetShowOnDisabled(DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(ShowOnDisabledProperty, value);
	}
}
