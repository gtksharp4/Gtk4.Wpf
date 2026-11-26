using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Xunit;
using DRun = System.Windows.Documents.Run;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class RunBindingTests : WpfGtkTestBase
{
	sealed class Vm : INotifyPropertyChanged
	{
		string _name = "start";
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}
	}

	[Fact]
	public void SetBinding_from_a_TextBlock_data_context_pushes_and_follows_the_source() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		var run = new DRun();
		tb.Inlines.Add(run);
		var expr = run.SetBinding(DRun.TextProperty, new Binding("Name"));
		Assert.NotNull(expr);
		Assert.Equal("start", run.Text);
		Assert.Equal("start", ((Gtk.Label)tb.Widget).Text);
		vm.Name = "next";
		Assert.Equal("next", run.Text);
		Assert.Equal("next", ((Gtk.Label)tb.Widget).Text);
	});

	[Fact]
	public void SetBinding_string_overload_returns_a_BindingExpression() => Run(() =>
	{
		var tb = new TextBlock { DataContext = new Vm() };
		var run = new DRun();
		tb.Inlines.Add(run);
		var expr = run.SetBinding(DRun.TextProperty, "Name");
		Assert.NotNull(expr);
		Assert.Equal("Name", expr!.ParentBinding.Path!.Path);
		Assert.Equal("start", run.Text);
	});

	[Fact]
	public void DataContext_set_after_the_binding_is_picked_up() => Run(() =>
	{
		var tb = new TextBlock();
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, new Binding("Name") { FallbackValue = "fb" });
		Assert.Equal("fb", run.Text);
		tb.DataContext = new Vm { Name = "late" };
		Assert.Equal("late", run.Text);
	});

	[Fact]
	public void GetBindingExpression_and_BindingOperations_accept_a_content_element() => Run(() =>
	{
		var tb = new TextBlock { DataContext = new Vm() };
		var run = new DRun();
		tb.Inlines.Add(run);
		Assert.Null(run.GetBindingExpression(DRun.TextProperty));
		var expr = BindingOperations.SetBinding(run, DRun.TextProperty, new Binding("Name"));
		Assert.Same(expr, run.GetBindingExpression(DRun.TextProperty));
		Assert.Same(expr, BindingOperations.GetBindingExpression(run, DRun.TextProperty));
		Assert.True(BindingOperations.IsDataBound(run, DRun.TextProperty));
		BindingOperations.ClearBinding(run, DRun.TextProperty);
		Assert.Null(run.GetBindingExpression(DRun.TextProperty));
	});

	[Fact]
	public void TwoWay_binding_writes_the_run_text_back_to_the_source() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, new Binding("Name") { Mode = BindingMode.TwoWay });
		run.Text = "edited";
		Assert.Equal("edited", vm.Name);
	});

	[Fact]
	public void Default_mode_binding_is_TwoWay_and_writes_back() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, new Binding("Name"));
		Assert.Equal("start", run.Text);
		run.Text = "edited";
		Assert.Equal("edited", vm.Name);
	});

	[Fact]
	public void OneWay_binding_does_not_write_back() => Run(() =>
	{
		var vm = new Vm();
		var tb = new TextBlock { DataContext = vm };
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, new Binding("Name") { Mode = BindingMode.OneWay });
		run.Text = "edited";
		Assert.Equal("start", vm.Name);
	});

	[Fact]
	public void Binding_through_a_FlowDocument_uses_the_viewer_data_context() => Run(() =>
	{
		var vm = new Vm { Name = "doc" };
		var run = new DRun();
		var doc = new FlowDocument(new Paragraph(run));
		var viewer = new FlowDocumentScrollViewer { Document = doc, DataContext = vm };
		run.SetBinding(DRun.TextProperty, new Binding("Name"));
		Assert.Equal("doc", run.Text);
		vm.Name = "changed";
		Assert.Equal("changed", run.Text);
		var child = ((Gtk.ScrolledWindow)viewer.Widget).Child;
		var label = (Gtk.Label)(child is Gtk.Viewport vp ? vp.Child! : child!);
		Assert.Equal("changed", label.Text);
	});

	[Fact]
	public void ElementName_resolves_through_the_hosting_name_scope() => Run(() =>
	{
		var source = new TextBox { Text = "from box" };
		var panel = new StackPanel();
		var tb = new TextBlock();
		panel.Children.Add(source);
		panel.Children.Add(tb);
		NameScopeFor(panel, "box", source);
		var run = new DRun();
		tb.Inlines.Add(run);
		run.SetBinding(DRun.TextProperty, new Binding("Text") { ElementName = "box" });
		Assert.Equal("from box", run.Text);
	});

	static void NameScopeFor(FrameworkElement root, string name, object element) =>
		root.RegisterName(name, element);

	[Fact]
	public void Source_binding_needs_no_hosting_element() => Run(() =>
	{
		var run = new DRun();
		run.SetBinding(DRun.TextProperty, new Binding("Name") { Source = new Vm { Name = "direct" } });
		Assert.Equal("direct", run.Text);
	});

	[Fact]
	public void MultiBinding_on_a_content_element_is_rejected() => Run(() =>
	{
		var run = new DRun();
		Assert.Throws<NotSupportedException>(() => run.SetBinding(DRun.TextProperty, new MultiBinding()));
	});

	[Fact]
	public void Rebinding_replaces_the_previous_binding() => Run(() =>
	{
		var run = new DRun();
		var a = new Vm { Name = "a" };
		var b = new Vm { Name = "b" };
		run.SetBinding(DRun.TextProperty, new Binding("Name") { Source = a });
		run.SetBinding(DRun.TextProperty, new Binding("Name") { Source = b });
		a.Name = "a2";
		Assert.Equal("b", run.Text);
	});
}
