using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// Base class for <see cref="DataGrid"/> column definitions.
/// </summary>
public abstract partial class DataGridColumn : DependencyObject
{
	// ── Header DP ────────────────────────────────────────────────────────────

	/// <summary>The column header text.</summary>
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(
			nameof(Header),
			typeof(object),
			typeof(DataGridColumn),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the column header.</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	// ── Width DP ─────────────────────────────────────────────────────────────

	/// <summary>Column width in device-independent units. 0 = auto.</summary>
	public static readonly DependencyProperty WidthProperty =
		DependencyProperty.Register(
			nameof(Width),
			typeof(double),
			typeof(DataGridColumn),
			new PropertyMetadata(0.0));

	/// <summary>Gets or sets the column width (0 = auto).</summary>
	public double Width
	{
		get => (double?)GetValue(WidthProperty) ?? 0.0;
		set => SetValue(WidthProperty, value);
	}

	/// <summary>Returns the string value to display for <paramref name="item"/>.</summary>
	internal abstract string GetCellText(object? item);
}

/// <summary>
/// A <see cref="DataGrid"/> column that displays a bound text value.
/// </summary>
public class DataGridTextColumn : DataGridColumn
{
	// ── Binding DP ───────────────────────────────────────────────────────────

	/// <summary>Binding that resolves the cell text from the row item.</summary>
	public static readonly DependencyProperty BindingProperty =
		DependencyProperty.Register(
			nameof(Binding),
			typeof(Binding),
			typeof(DataGridTextColumn),
			new PropertyMetadata(null));

	/// <summary>Gets or sets the cell binding.</summary>
	public Binding? Binding
	{
		get => (Binding?)GetValue(BindingProperty);
		set => SetValue(BindingProperty, value);
	}

	/// <inheritdoc/>
	internal override string GetCellText(object? item)
	{
		if (item == null) return string.Empty;
		var path = PropertyPath.ResolveOwner(item, Binding?.PathString);
		if (string.IsNullOrEmpty(path)) return item.ToString() ?? string.Empty;
		var prop = item.GetType().GetProperty(path!);
		return prop?.GetValue(item)?.ToString() ?? string.Empty;
	}
}