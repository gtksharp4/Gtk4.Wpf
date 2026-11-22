using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>
/// The header of a <see cref="DataGridColumn"/>. The GTK grid draws its header cells itself, so this type is the
/// WPF object model only: a clickable button that knows its <see cref="Column"/>.
/// </summary>
public class DataGridColumnHeader : ButtonBase
{
	Gtk.Button _button = null!;

	/// <summary>The column this header belongs to, or null.</summary>
	public DataGridColumn? Column { get; internal set; }

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_button = new Gtk.Button { Label = (Content as string) ?? string.Empty };
		_button.Clicked += (_, _) => OnClick();
		return _button;
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _button != null && e.NewValue is string s)
			_button.Label = s;
	}
}
