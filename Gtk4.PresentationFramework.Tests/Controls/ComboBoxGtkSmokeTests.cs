using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Row
{
	public string Name { get; set; } = string.Empty;
}

/// <summary>Real-GTK ComboBox tests.</summary>
public class ComboBoxGtkSmokeTests : WpfGtkTestBase
{
	static string[] Labels(Gtk.DropDown dd)
	{
		var m = (Gtk.StringList)dd.Model!;
		var r = new string[m.NItems];
		for (uint i = 0; i < r.Length; i++) r[i] = m.GetString(i)!;
		return r;
	}

	[Fact]
	public void Default_backing_is_DropDown() => Run(() => Assert.IsType<Gtk.DropDown>(new ComboBox().Widget));

	[Fact]
	public void Inline_items_labels() => Run(() =>
	{
		var cb = new ComboBox();
		cb.Items.Add(new ComboBoxItem { Content = "A" });
		cb.Items.Add("B");
		Assert.Equal(new[] { "A", "B" }, Labels((Gtk.DropDown)cb.Widget));
	});

	[Fact]
	public void Inline_items_added_after_realize_update_model() => Run(() =>
	{
		var cb = new ComboBox();
		var dd = (Gtk.DropDown)cb.Widget;
		cb.Items.Add("X");
		Assert.Equal(new[] { "X" }, Labels(dd));
	});

	[Fact]
	public void ItemsSource_labels() => Run(() =>
	{
		var cb = new ComboBox { ItemsSource = new[] { "a", "b" } };
		Assert.Equal(new[] { "a", "b" }, Labels((Gtk.DropDown)cb.Widget));
	});

	[Fact]
	public void DisplayMemberPath_labels() => Run(() =>
	{
		var cb = new ComboBox { DisplayMemberPath = "Name", ItemsSource = new[] { new Row { Name = "r1" } } };
		Assert.Equal(new[] { "r1" }, Labels((Gtk.DropDown)cb.Widget));
	});

	[Fact]
	public void Gtk_selection_updates_SelectedItem_and_Text() => Run(() =>
	{
		var cb = new ComboBox { ItemsSource = new[] { "a", "b" } };
		((Gtk.DropDown)cb.Widget).Selected = 1;
		Assert.Equal("b", cb.SelectedItem);
		Assert.Equal("b", cb.Text);
	});

	[Fact]
	public void Editable_creates_Box_with_Entry() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true, ItemsSource = new[] { "a" } };
		var box = Assert.IsType<Gtk.Box>(cb.Widget);
		Assert.IsType<Gtk.Entry>(box.FirstChild);
	});

	[Fact]
	public void Entry_typing_updates_Text_and_selects() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true, ItemsSource = new[] { "Apple", "Pear" } };
		var entry = (Gtk.Entry)((Gtk.Box)cb.Widget).FirstChild!;
		entry.Text = "pear";
		Assert.Equal("pear", cb.Text);
		Assert.Equal("Pear", cb.SelectedItem);
	});

	[Fact]
	public void Text_set_updates_Entry() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true };
		var entry = (Gtk.Entry)((Gtk.Box)cb.Widget).FirstChild!;
		cb.Text = "hello";
		Assert.Equal("hello", entry.Text);
	});

	[Fact]
	public void IsReadOnly_makes_Entry_non_editable() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true, IsReadOnly = true };
		var entry = (Gtk.Entry)((Gtk.Box)cb.Widget).FirstChild!;
		Assert.False(entry.IsEditable);
		cb.IsReadOnly = false;
		Assert.True(entry.IsEditable);
	});

	[Fact]
	public void EditableTextBoxSite_is_a_reflection_reachable_TextBox_sharing_the_Entry() => Run(() =>
	{
		var cb = new ComboBox { IsEditable = true };
		var widget = cb.Widget;
		var site = Assert.IsType<System.Windows.Controls.TextBox>(typeof(ComboBox).GetProperty("EditableTextBoxSite",
			System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic |
			System.Reflection.BindingFlags.Public)!.GetValue(cb));
		Assert.Same(site.Widget, ((Gtk.Box)widget).FirstChild);
		var changed = 0;
		site.TextChanged += (_, _) => changed++;
		cb.Text = "hello";
		Assert.Equal("hello", site.Text);
		Assert.True(changed > 0);
	});
}
