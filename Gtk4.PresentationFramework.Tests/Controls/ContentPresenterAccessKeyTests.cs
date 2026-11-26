using System.Windows;
using Xunit;
using WpfContentPresenter = System.Windows.Controls.ContentPresenter;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class ContentPresenterAccessKeyTests : WpfGtkTestBase
{
	static Gtk.Label LabelOf(WpfContentPresenter presenter)
	{
		var box = (Gtk.Box)presenter.Widget;
		return (Gtk.Label)box.FirstChild;
	}

	[Fact]
	public void RecognizesAccessKey_is_a_false_default_bool_property()
	{
		var p = WpfContentPresenter.RecognizesAccessKeyProperty;
		Assert.Equal("RecognizesAccessKey", p.Name);
		Assert.Equal(typeof(bool), p.PropertyType);
		Assert.Equal(typeof(WpfContentPresenter), p.OwnerType);
		Assert.Equal(false, p.DefaultMetadata.DefaultValue);
		Assert.IsAssignableFrom<FrameworkPropertyMetadata>(p.DefaultMetadata);
	}

	[Fact]
	public void Underscores_stay_literal_by_default() => Run(() =>
	{
		var presenter = new WpfContentPresenter { Content = "_File" };
		Assert.False(presenter.RecognizesAccessKey);
		var label = LabelOf(presenter);
		Assert.False(label.UseUnderline);
		Assert.Equal("_File", label.Text);
	});

	[Fact]
	public void RecognizesAccessKey_true_turns_the_underscore_into_a_mnemonic() => Run(() =>
	{
		var presenter = new WpfContentPresenter { RecognizesAccessKey = true, Content = "_File" };
		var label = LabelOf(presenter);
		Assert.True(label.UseUnderline);
		Assert.Equal("File", label.Text);
		Assert.Equal('f', char.ToLowerInvariant((char)label.MnemonicKeyval));
	});

	[Fact]
	public void Toggling_after_the_widget_exists_rebuilds_the_label() => Run(() =>
	{
		var presenter = new WpfContentPresenter { Content = "_Edit" };
		Assert.False(LabelOf(presenter).UseUnderline);
		presenter.RecognizesAccessKey = true;
		Assert.True(LabelOf(presenter).UseUnderline);
		presenter.RecognizesAccessKey = false;
		Assert.False(LabelOf(presenter).UseUnderline);
	});
}
