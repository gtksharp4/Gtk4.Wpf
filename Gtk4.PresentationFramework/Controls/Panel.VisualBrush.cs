using System.Windows.Media;

namespace System.Windows.Controls;

public abstract partial class Panel
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

/// <summary>Repaints a container when its tile-brush background is replaced or mutates.</summary>
sealed class BackdropWatch
{
	readonly UIElement _owner;
	readonly EventHandler _changed;
	TileBrush? _brush;

	internal BackdropWatch(UIElement owner)
	{
		_owner = owner;
		_changed = (s, a) => Redraw();
	}

	internal void Set(Brush? next)
	{
		if (_brush != null) _brush.Changed -= _changed;
		_brush = next as TileBrush;
		if (_brush is { IsFrozen: false }) _brush.Changed += _changed;
		Redraw();
	}

	void Redraw()
	{
		if (_owner.IsWidgetCreated) _owner.Widget.QueueDraw();
	}
}
