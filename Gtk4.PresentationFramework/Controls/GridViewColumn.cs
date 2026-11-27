using System.ComponentModel;
using System.Windows.Data;

namespace System.Windows.Controls;

/// <summary>
/// A column of a <see cref="GridView"/>: a header and, per row, a cell made from
/// <see cref="DisplayMemberBinding"/>, else <see cref="CellTemplate"/> (or its selector), else the row item.
/// </summary>
public class GridViewColumn : DependencyObject, INotifyPropertyChanged
{
	/// <summary>Identifies the <see cref="Header"/> property.</summary>
	public static readonly DependencyProperty HeaderProperty = Reg(nameof(Header), typeof(object), null);

	/// <summary>Identifies the <see cref="HeaderTemplate"/> property.</summary>
	public static readonly DependencyProperty HeaderTemplateProperty =
		Reg(nameof(HeaderTemplate), typeof(DataTemplate), null);

	/// <summary>Identifies the <see cref="HeaderTemplateSelector"/> property.</summary>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty =
		Reg(nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), null);

	/// <summary>Identifies the <see cref="HeaderStringFormat"/> property.</summary>
	public static readonly DependencyProperty HeaderStringFormatProperty =
		Reg(nameof(HeaderStringFormat), typeof(string), null);

	/// <summary>Identifies the <see cref="CellTemplate"/> property.</summary>
	public static readonly DependencyProperty CellTemplateProperty =
		Reg(nameof(CellTemplate), typeof(DataTemplate), null);

	/// <summary>Identifies the <see cref="CellTemplateSelector"/> property.</summary>
	public static readonly DependencyProperty CellTemplateSelectorProperty =
		Reg(nameof(CellTemplateSelector), typeof(DataTemplateSelector), null);

	/// <summary>Identifies the <see cref="Width"/> property (NaN: sized to the widest cell).</summary>
	public static readonly DependencyProperty WidthProperty = Reg(nameof(Width), typeof(double), double.NaN);

	BindingBase? _displayMemberBinding;

	static DependencyProperty Reg(string name, Type type, object? defaultValue) =>
		DependencyProperty.Register(name, type, typeof(GridViewColumn), new PropertyMetadata(defaultValue));

	/// <inheritdoc/>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>Raised when anything that shapes the column's header or cells changes.</summary>
	internal event EventHandler? Changed;

	/// <summary>Gets or sets the header content (text, an element, or data shown through the template).</summary>
	public object? Header
	{
		get => GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	/// <summary>Gets or sets the template of the header content.</summary>
	public DataTemplate? HeaderTemplate
	{
		get => (DataTemplate?)GetValue(HeaderTemplateProperty);
		set => SetValue(HeaderTemplateProperty, value);
	}

	/// <summary>Gets or sets the selector of the header template.</summary>
	public DataTemplateSelector? HeaderTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(HeaderTemplateSelectorProperty);
		set => SetValue(HeaderTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the string format of the header content.</summary>
	public string? HeaderStringFormat
	{
		get => (string?)GetValue(HeaderStringFormatProperty);
		set => SetValue(HeaderStringFormatProperty, value);
	}

	/// <summary>Gets or sets the template of a cell; it is ignored while <see cref="DisplayMemberBinding"/> is set.</summary>
	public DataTemplate? CellTemplate
	{
		get => (DataTemplate?)GetValue(CellTemplateProperty);
		set => SetValue(CellTemplateProperty, value);
	}

	/// <summary>Gets or sets the selector of the cell template.</summary>
	public DataTemplateSelector? CellTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(CellTemplateSelectorProperty);
		set => SetValue(CellTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the column width in device-independent units; NaN sizes it to its widest cell.</summary>
	public double Width
	{
		get => (double)GetValue(WidthProperty)!;
		set => SetValue(WidthProperty, value);
	}

	/// <summary>
	/// Gets or sets the binding whose value each cell shows as text, evaluated against the row item.
	/// </summary>
	public BindingBase? DisplayMemberBinding
	{
		get => _displayMemberBinding;
		set
		{
			if (ReferenceEquals(_displayMemberBinding, value)) return;
			_displayMemberBinding = value;
			RaiseChanged(nameof(DisplayMemberBinding));
		}
	}

	/// <summary>The rendered width: <see cref="Width"/> when set, else the width of the header cell once shown.</summary>
	public double ActualWidth
	{
		get
		{
			if (!double.IsNaN(Width)) return Width;
			return HeaderWidget != null ? HeaderWidget.Width : 0;
		}
	}

	/// <summary>The widget of this column's header cell, set while a header shows the column.</summary>
	internal Gtk.Widget? HeaderWidget { get; set; }

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property.OwnerType == typeof(GridViewColumn)) RaiseChanged(e.Property.Name);
	}

	void RaiseChanged(string name)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <inheritdoc/>
	public override string ToString() => nameof(GridViewColumn);
}
