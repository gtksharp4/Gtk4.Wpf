namespace System.Windows.Documents;

/// <summary>
/// Which property changes force a flow document to repaginate: those whose metadata says AffectsMeasure,
/// AffectsArrange or AffectsRender, and the inherited text properties. Anything else (Tag, DataContext, Name,
/// Background...) leaves the pagination alone.
/// </summary>
static class PaginationProperties
{
	internal static bool Affects(DependencyObject owner, DependencyProperty dp)
	{
		if (dp == TextElement.FontFamilyProperty || dp == TextElement.FontSizeProperty
			|| dp == TextElement.FontStyleProperty || dp == TextElement.FontWeightProperty
			|| dp == TextElement.FontStretchProperty || dp == TextElement.ForegroundProperty
			|| dp == Block.IsHyphenationEnabledProperty)
			return true;
		var metadata = dp.GetMetadata(owner.GetType()) as FrameworkPropertyMetadata;
		return metadata != null && (metadata.AffectsMeasure || metadata.AffectsArrange || metadata.AffectsRender);
	}
}
