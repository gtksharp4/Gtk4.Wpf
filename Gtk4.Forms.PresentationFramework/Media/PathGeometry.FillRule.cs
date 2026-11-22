using System.Windows.Shapes;

namespace System.Windows.Media;

public partial class PathGeometry
{
	/// <summary>Identifies <see cref="FillRule"/>. Default <see cref="Shapes.FillRule.EvenOdd"/>.</summary>
	public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
		nameof(FillRule), typeof(FillRule), typeof(PathGeometry), new PropertyMetadata(FillRule.EvenOdd));

	/// <summary>How overlapping figures are filled. Default <see cref="Shapes.FillRule.EvenOdd"/>.</summary>
	public FillRule FillRule
	{
		get => (FillRule)GetValue(FillRuleProperty)!;
		set => SetValue(FillRuleProperty, value);
	}
}
