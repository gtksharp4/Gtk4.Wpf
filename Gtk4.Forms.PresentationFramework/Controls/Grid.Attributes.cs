using System;

namespace System.Windows.Controls;

public partial class Grid
{
	/// <summary>Identifies the <c>Grid.IsSharedSizeScope</c> attached property (default false).</summary>
	/// <remarks>Stored only: <see cref="ColumnDefinition.SharedSizeGroup"/> sizes are not shared.</remarks>
	public static readonly DependencyProperty IsSharedSizeScopeProperty =
		DependencyProperty.RegisterAttached("IsSharedSizeScope", typeof(bool), typeof(Grid),
			new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="ShowGridLines"/> dependency property (default false).</summary>
	/// <remarks>Stored only: GTK's grid has no debug grid lines.</remarks>
	public static readonly DependencyProperty ShowGridLinesProperty =
		DependencyProperty.Register(nameof(ShowGridLines), typeof(bool), typeof(Grid),
			new FrameworkPropertyMetadata(false));

	/// <summary>Gets whether <paramref name="element"/> is a shared-size scope.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static bool GetIsSharedSizeScope(UIElement element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)element.GetValue(IsSharedSizeScopeProperty)!;
	}

	/// <summary>Sets whether <paramref name="element"/> is a shared-size scope.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="element"/> is null.</exception>
	public static void SetIsSharedSizeScope(UIElement element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsSharedSizeScopeProperty, value);
	}

	/// <summary>Gets or sets whether grid lines are shown for layout debugging.</summary>
	public bool ShowGridLines
	{
		get => (bool)GetValue(ShowGridLinesProperty)!;
		set => SetValue(ShowGridLinesProperty, value);
	}

	// Mirrors the arranged track sizes onto the definitions (RowDefinition.ActualHeight etc.).
	// Also used for a GTK-owned size, where the managed arrange does not run.
	internal void PublishActualSizes(Size size) =>
		PublishActualSizes(ResolveColumnsArrange(Math.Max(1, ColumnDefinitions.Count), size.Width),
			ResolveRowsArrange(Math.Max(1, RowDefinitions.Count), size.Height));

	void PublishActualSizes(double[] colWidths, double[] rowHeights)
	{
		for (int i = 0; i < ColumnDefinitions.Count; i++)
			ColumnDefinitions[i].ActualWidth = colWidths[i];
		for (int i = 0; i < RowDefinitions.Count; i++)
			RowDefinitions[i].ActualHeight = rowHeights[i];
	}
}
