using System.Windows.Media;

namespace System.Windows.Controls;

public partial class Border
{
	BackdropWatch? _backdrop;

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == BackgroundProperty)
			(_backdrop ??= new BackdropWatch(this)).Set(e.NewValue as Brush);
	}
}
