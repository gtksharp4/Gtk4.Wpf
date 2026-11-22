using System.Collections.ObjectModel;

namespace System.Windows.Media;

/// <summary>
/// A text effect applied to a range of characters. Stored only: the Pango label does not apply it.
/// Gap: Transform and Clip are stored only, and animation is absent.
/// </summary>
public sealed partial class TextEffect : Freezable
{
	/// <summary>Identifies the Foreground property (default null).</summary>
	public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
		nameof(Foreground), typeof(Brush), typeof(TextEffect), new PropertyMetadata(null));

	/// <summary>Identifies the PositionStart property (default 0).</summary>
	public static readonly DependencyProperty PositionStartProperty = DependencyProperty.Register(
		nameof(PositionStart), typeof(int), typeof(TextEffect), new PropertyMetadata(0));

	/// <summary>Identifies the PositionCount property (default 0).</summary>
	public static readonly DependencyProperty PositionCountProperty = DependencyProperty.Register(
		nameof(PositionCount), typeof(int), typeof(TextEffect), new PropertyMetadata(0));

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TextEffect();

	/// <summary>Gets or sets the foreground brush of the affected characters.</summary>
	public Brush? Foreground
	{
		get => (Brush?)GetValue(ForegroundProperty);
		set => SetValue(ForegroundProperty, value);
	}

	/// <summary>Gets or sets the index of the first affected character.</summary>
	public int PositionStart
	{
		get => (int)GetValue(PositionStartProperty)!;
		set => SetValue(PositionStartProperty, value);
	}

	/// <summary>Gets or sets the number of affected characters.</summary>
	public int PositionCount
	{
		get => (int)GetValue(PositionCountProperty)!;
		set => SetValue(PositionCountProperty, value);
	}
}

/// <summary>A collection of <see cref="TextEffect"/>s.</summary>
public sealed class TextEffectCollection : Collection<TextEffect>
{
}
