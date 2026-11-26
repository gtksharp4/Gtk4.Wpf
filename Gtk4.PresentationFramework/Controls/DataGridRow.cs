namespace System.Windows.Controls;

/// <summary>
/// A row of a <see cref="DataGrid"/>. The GTK grid draws its rows itself, so this type is the WPF object model
/// only: it stores <see cref="Item"/>, <see cref="Header"/> and <see cref="IsSelected"/> and is not placed in the grid.
/// </summary>
public partial class DataGridRow : Control
{
	/// <summary>Identifies <see cref="Item"/>.</summary>
	public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(
		nameof(Item), typeof(object), typeof(DataGridRow), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies <see cref="Header"/>.</summary>
	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
		nameof(Header), typeof(object), typeof(DataGridRow), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies <see cref="IsSelected"/>. Default false.</summary>
	public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
		nameof(IsSelected), typeof(bool), typeof(DataGridRow), new FrameworkPropertyMetadata(false));

	/// <summary>Gets or sets the data item the row shows.</summary>
	public object? Item
	{
		get => GetValue(ItemProperty);
		set => SetValue(ItemProperty, value);
	}

	/// <summary>Gets or sets the row header.</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	/// <summary>Gets or sets whether the row is selected.</summary>
	public bool IsSelected
	{
		get => (bool)GetValue(IsSelectedProperty)!;
		set => SetValue(IsSelectedProperty, value);
	}
}
