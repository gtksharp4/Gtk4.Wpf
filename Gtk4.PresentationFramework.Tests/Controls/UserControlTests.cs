using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.XamlBacked;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class UserControlTests
{
	[Fact]
	public void Defaults_match_WPF()
	{
		var u = new UserControl();
		Assert.IsAssignableFrom<ContentControl>(u);
		Assert.False(u.Focusable);
		Assert.False(u.IsTabStop);
		Assert.Equal(HorizontalAlignment.Stretch, u.HorizontalContentAlignment);
		Assert.Equal(VerticalAlignment.Stretch, u.VerticalContentAlignment);
		Assert.True(new ContentControl().Focusable);
	}

	[Fact]
	public void A_markup_root_deriving_from_UserControl_builds_and_shows_Content()
	{
		var u = new SampleUserControl();
		var panel = Assert.IsType<StackPanel>(u.Content);
		Assert.Equal(2, panel.Children.Count);
		Assert.Equal("inside", u.Caption.Text);
		Assert.Same(panel.Children[1], u.Action);
	}
}
