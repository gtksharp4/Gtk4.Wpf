using System.ComponentModel;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>UpdateSourceTrigger behaviour on realised TextBoxes (real Gtk.Entry widgets).</summary>
public class BindingLostFocusTriggerTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		string _name = "n";

		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	static void Type(WpfTextBox tb, string text) => ((Gtk.Entry)tb.Widget).Text = text;

	static void MoveFocusAway(WpfTextBox tb)
	{
		tb.Focus();
		new WpfTextBox().Focus();
	}

	[Fact]
	public void Default_trigger_waits_for_focus_loss() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name"));
		tb.Focus();

		Type(tb, "typed");
		Assert.Equal("typed", tb.Text);
		Assert.Equal("n", vm.Name);

		new WpfTextBox().Focus();
		Assert.Equal("typed", vm.Name);
	});

	[Fact]
	public void LostFocus_without_change_does_not_write() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name"));
		var writes = 0;
		vm.PropertyChanged += (_, _) => writes++;
		MoveFocusAway(tb);
		Assert.Equal(0, writes);
	});

	[Fact]
	public void PropertyChanged_trigger_updates_on_every_change() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty,
			new Binding("Name") { UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
		Type(tb, "a");
		Assert.Equal("a", vm.Name);
		Type(tb, "ab");
		Assert.Equal("ab", vm.Name);
	});

	[Fact]
	public void Explicit_trigger_updates_only_through_UpdateSource() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty,
			new Binding("Name") { UpdateSourceTrigger = UpdateSourceTrigger.Explicit });
		Type(tb, "typed");
		MoveFocusAway(tb);
		Assert.Equal("n", vm.Name);

		((BindingExpression)tb.GetBindingExpression(WpfTextBox.TextProperty)!).UpdateSource();
		Assert.Equal("typed", vm.Name);
	});

	[Fact]
	public void Explicit_LostFocus_trigger_matches_the_default() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty,
			new Binding("Name") { UpdateSourceTrigger = UpdateSourceTrigger.LostFocus });
		tb.Focus();
		Type(tb, "x");
		Assert.Equal("n", vm.Name);
		new WpfTextBox().Focus();
		Assert.Equal("x", vm.Name);
	});

	[Fact]
	public void Source_update_does_not_mark_binding_dirty() => Run(() =>
	{
		var vm = new Vm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Name"));
		vm.Name = "fromSource";
		var writes = 0;
		vm.PropertyChanged += (_, _) => writes++;
		MoveFocusAway(tb);
		Assert.Equal(0, writes);
		Assert.Equal("fromSource", tb.Text);
	});
}
