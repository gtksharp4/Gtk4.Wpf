using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>
/// Block DPs with WPF defaults. Page and column breaks, floater clearing and hyphenation are stored only: the
/// document is shown in a single scrolling Pango label, which (as in WPF's scroll viewer) has no pages or columns.
/// </summary>
public abstract partial class Block
{
	/// <summary>Identifies the BreakPageBefore property (default false).</summary>
	public static readonly DependencyProperty BreakPageBeforeProperty = DependencyProperty.Register(
		nameof(BreakPageBefore), typeof(bool), typeof(Block), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the BreakColumnBefore property (default false).</summary>
	public static readonly DependencyProperty BreakColumnBeforeProperty = DependencyProperty.Register(
		nameof(BreakColumnBefore), typeof(bool), typeof(Block), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the BorderBrush property (default null).</summary>
	public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
		nameof(BorderBrush), typeof(Brush), typeof(Block), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the BorderThickness property (default 0; must be finite and not negative).</summary>
	public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register(
		nameof(BorderThickness), typeof(Thickness), typeof(Block), new FrameworkPropertyMetadata(new Thickness(0)),
		v => v is Thickness t && IsValidSide(t.Left) && IsValidSide(t.Top) && IsValidSide(t.Right)
			&& IsValidSide(t.Bottom));

	/// <summary>Identifies the ClearFloaters property (default None).</summary>
	public static readonly DependencyProperty ClearFloatersProperty = DependencyProperty.Register(
		nameof(ClearFloaters), typeof(WrapDirection), typeof(Block), new FrameworkPropertyMetadata(WrapDirection.None),
		v => v is WrapDirection d && d >= WrapDirection.None && d <= WrapDirection.Both);

	/// <summary>Identifies the attached IsHyphenationEnabled property (default false; inherits).</summary>
	public static readonly DependencyProperty IsHyphenationEnabledProperty = DependencyProperty.RegisterAttached(
		"IsHyphenationEnabled", typeof(bool), typeof(Block),
		new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Gets or sets whether a page break precedes the block (stored).</summary>
	public bool BreakPageBefore
	{
		get => (bool)GetValue(BreakPageBeforeProperty)!;
		set => SetValue(BreakPageBeforeProperty, value);
	}

	/// <summary>Gets or sets whether a column break precedes the block (stored).</summary>
	public bool BreakColumnBefore
	{
		get => (bool)GetValue(BreakColumnBeforeProperty)!;
		set => SetValue(BreakColumnBeforeProperty, value);
	}

	/// <summary>Gets or sets the border brush (stored).</summary>
	public Brush? BorderBrush
	{
		get => (Brush?)GetValue(BorderBrushProperty);
		set => SetValue(BorderBrushProperty, value);
	}

	/// <summary>Gets or sets the border thickness (stored).</summary>
	public Thickness BorderThickness
	{
		get => (Thickness)GetValue(BorderThicknessProperty)!;
		set => SetValue(BorderThicknessProperty, value);
	}

	/// <summary>Gets or sets the floater sides this block must clear (stored).</summary>
	public WrapDirection ClearFloaters
	{
		get => (WrapDirection)GetValue(ClearFloatersProperty)!;
		set => SetValue(ClearFloatersProperty, value);
	}

	/// <summary>Gets or sets whether automatic hyphenation is enabled (stored).</summary>
	public bool IsHyphenationEnabled
	{
		get => GetIsHyphenationEnabled(this);
		set => SetIsHyphenationEnabled(this, value);
	}

	/// <summary>Gets the attached IsHyphenationEnabled value.</summary>
	public static bool GetIsHyphenationEnabled(DependencyObject element)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		return (bool)element.GetValue(IsHyphenationEnabledProperty)!;
	}

	/// <summary>Sets the attached IsHyphenationEnabled value.</summary>
	public static void SetIsHyphenationEnabled(DependencyObject element, bool value)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		element.SetValue(IsHyphenationEnabledProperty, value);
	}

	static bool IsValidSide(double v) => !double.IsNaN(v) && !double.IsInfinity(v) && v >= 0;
}
