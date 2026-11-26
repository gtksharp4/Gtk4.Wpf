using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws an arbitrary shape defined by a <see cref="Geometry"/>.
/// Mirrors <c>System.Windows.Shapes.Path</c>.
/// </summary>
public class Path : Shape
{
	/// <summary>The geometry that defines the shape.</summary>
	public static readonly DependencyProperty DataProperty =
		DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(Path),
			new FrameworkPropertyMetadata(null,
				FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
				OnDataChanged));

	EventHandler? _dataChanged;

	/// <summary>Gets or sets the geometry.</summary>
	public Geometry? Data
	{
		get => (Geometry?)GetValue(DataProperty);
		set => SetValue(DataProperty, value);
	}

	static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var path = (Path)d;
		path._dataChanged ??= (_, _) => path.OnMeasureAffected();
		if (e.OldValue is Geometry old) old.Changed -= path._dataChanged;
		if (e.NewValue is Geometry added) added.Changed += path._dataChanged;
		path.OnMeasureAffected();
	}

	internal override Geometry? GetDefiningGeometry() => Data;
}
