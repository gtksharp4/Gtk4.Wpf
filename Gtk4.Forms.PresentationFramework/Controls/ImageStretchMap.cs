using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>
/// Maps <see cref="Stretch"/> values to GTK <see cref="Gtk.ContentFit"/>.
/// </summary>
/// <remarks>
/// GTK mapping:
/// <list type="table">
///   <listheader><term>Stretch</term><description>ContentFit</description></listheader>
///   <item><term>None</term><description>ScaleDown — prevents enlargement; closest GTK fit</description></item>
///   <item><term>Fill</term><description>Fill — ignores aspect ratio</description></item>
///   <item><term>Uniform</term><description>Contain — fits inside, preserves aspect ratio</description></item>
///   <item><term>UniformToFill</term><description>Cover — fills, preserves aspect ratio, may clip</description></item>
/// </list>
/// </remarks>
internal static class ImageStretchMap
{
	/// <summary>
	/// Converts a <see cref="Stretch"/> value to the corresponding
	/// <see cref="Gtk.ContentFit"/> for use on a <see cref="Gtk.Picture"/>.
	/// </summary>
	internal static Gtk.ContentFit ToContentFit(Stretch stretch) =>
		stretch switch
		{
			Stretch.None          => Gtk.ContentFit.ScaleDown,
			Stretch.Fill          => Gtk.ContentFit.Fill,
			Stretch.Uniform       => Gtk.ContentFit.Contain,
			Stretch.UniformToFill => Gtk.ContentFit.Cover,
			_                     => Gtk.ContentFit.Contain,
		};
}
