using System;
using System.ComponentModel;
using Gtk4.Forms.Platform.GTK.Controls;
using Gtk4.Forms.Platform.GTK.Extensions;
using Gtk4.Forms.PlatformConfiguration.GTKSpecific;
using Gtk4.Forms.Shapes;

namespace Gtk4.Forms.Platform.GTK.Renderers;

public class PathRenderer : ShapeRenderer<Path, PathView>
{
	protected override void OnElementChanged(ElementChangedEventArgs<Path> e)
	{
		base.OnElementChanged(e);

		if (e.NewElement != null)
		{
			SetData(Element.Data);
		}
	}

	protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		base.OnElementPropertyChanged(sender, e);

		if (e.PropertyName == Shapes.Path.DataProperty.PropertyName)
			SetData(Element.Data);
	}

	void SetData(Geometry data)
	{
		Control.UpdateGeometry(data as PathGeometry);
	}
}