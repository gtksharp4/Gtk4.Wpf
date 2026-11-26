using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ElementVisibility;

public class VisibilityGtkMapTests
{
	[Fact]
	public void Visible_maps_to_visible_unsuppressed_targetable()
	{
		var f = VisibilityGtkMap.Map(Visibility.Visible);
		Assert.True(f.WidgetVisible);
		Assert.False(f.SuppressOpacity);
		Assert.True(f.AllowTarget);
	}

	[Fact]
	public void Hidden_keeps_the_widget_mapped_but_suppressed()
	{
		var f = VisibilityGtkMap.Map(Visibility.Hidden);
		Assert.True(f.WidgetVisible);
		Assert.True(f.SuppressOpacity);
		Assert.False(f.AllowTarget);
	}

	[Fact]
	public void Collapsed_hides_the_widget()
	{
		var f = VisibilityGtkMap.Map(Visibility.Collapsed);
		Assert.False(f.WidgetVisible);
		Assert.False(f.AllowTarget);
	}

	[Fact]
	public void Opacity_is_restored_unless_Hidden()
	{
		Assert.Equal(0.4, VisibilityGtkMap.EffectiveOpacity(Visibility.Visible, 0.4));
		Assert.Equal(0.4, VisibilityGtkMap.EffectiveOpacity(Visibility.Collapsed, 0.4));
		Assert.Equal(0.0, VisibilityGtkMap.EffectiveOpacity(Visibility.Hidden, 0.4));
	}

	[Fact]
	public void CanTarget_and_CanFocus_need_Visible_plus_the_flag()
	{
		Assert.True(VisibilityGtkMap.CanTarget(Visibility.Visible, true));
		Assert.False(VisibilityGtkMap.CanTarget(Visibility.Visible, false));
		Assert.False(VisibilityGtkMap.CanTarget(Visibility.Hidden, true));
		Assert.True(VisibilityGtkMap.CanFocus(Visibility.Visible, true));
		Assert.False(VisibilityGtkMap.CanFocus(Visibility.Hidden, true));
		Assert.False(VisibilityGtkMap.CanFocus(Visibility.Visible, false));
	}
}
