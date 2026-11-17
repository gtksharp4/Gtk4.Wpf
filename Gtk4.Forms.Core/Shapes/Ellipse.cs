using Gtk4.Forms.Platform;

namespace Gtk4.Forms.Shapes
{
	[RenderWith(typeof(_EllipseRenderer))]
	public sealed class Ellipse : Shape
	{
		public Ellipse()
		{
			Aspect = Stretch.Fill;
		}
	}
}