namespace System.Windows.Controls;

/// <summary>The direction in which an <see cref="Expander"/> opens.</summary>
public enum ExpandDirection
{
	/// <summary>Opens downward.</summary>
	Down = 0,

	/// <summary>Opens upward.</summary>
	Up = 1,

	/// <summary>Opens to the left.</summary>
	Left = 2,

	/// <summary>Opens to the right.</summary>
	Right = 3,
}

public partial class Expander
{
	/// <summary>Identifies <see cref="ExpandDirection"/>. Default Down; stored only.</summary>
	public static readonly DependencyProperty ExpandDirectionProperty = DependencyProperty.Register(
		nameof(ExpandDirection), typeof(ExpandDirection), typeof(Expander),
		new FrameworkPropertyMetadata(ExpandDirection.Down, FrameworkPropertyMetadataOptions.AffectsMeasure),
		v => v is ExpandDirection d && d >= ExpandDirection.Down && d <= ExpandDirection.Right);

	/// <summary>Gets or sets the direction of expansion. Gtk.Expander only opens downward, so it is stored only.</summary>
	public ExpandDirection ExpandDirection
	{
		get => (ExpandDirection)GetValue(ExpandDirectionProperty)!;
		set => SetValue(ExpandDirectionProperty, value);
	}
}
