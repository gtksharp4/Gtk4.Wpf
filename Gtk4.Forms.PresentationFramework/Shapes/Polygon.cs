using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws a closed polygon defined by a collection of points.
/// Mirrors <c>System.Windows.Shapes.Polygon</c>.
/// </summary>
public class Polygon : Shape
{
	/// <summary>Identifies <see cref="Points"/>.</summary>
	public static readonly DependencyProperty PointsProperty = DependencyProperty.Register(
		nameof(Points), typeof(PointCollection), typeof(Polygon),
		new FrameworkPropertyMetadata(null,
			FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
			OnPointsChanged));

	/// <summary>Identifies <see cref="FillRule"/>.</summary>
	public static readonly DependencyProperty FillRuleProperty = DependencyProperty.Register(
		nameof(FillRule), typeof(FillRule), typeof(Polygon),
		new FrameworkPropertyMetadata(FillRule.EvenOdd, FrameworkPropertyMetadataOptions.AffectsRender,
			(d, _) => ((Shape)d).RedrawShape()));

	EventHandler? _pointsChanged;

	/// <summary>The vertices. Never null: an empty collection is created on first read.</summary>
	public PointCollection Points
	{
		get
		{
			var points = (PointCollection?)GetValue(PointsProperty);
			if (points == null)
			{
				points = new PointCollection();
				SetValue(PointsProperty, points);
			}
			return points;
		}
		set => SetValue(PointsProperty, value);
	}

	/// <summary>Fill rule applied when filling the shape.</summary>
	public FillRule FillRule
	{
		get => (FillRule)GetValue(FillRuleProperty)!;
		set => SetValue(FillRuleProperty, value);
	}

	static void OnPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var shape = (Polygon)d;
		shape._pointsChanged ??= (_, _) => shape.OnMeasureAffected();
		if (e.OldValue is PointCollection old) old.Changed -= shape._pointsChanged;
		if (e.NewValue is PointCollection added) added.Changed += shape._pointsChanged;
		shape.OnMeasureAffected();
	}

	internal override Geometry? GetDefiningGeometry() =>
		PointsGeometry.Create((PointCollection?)GetValue(PointsProperty), FillRule, closed: true);
}
