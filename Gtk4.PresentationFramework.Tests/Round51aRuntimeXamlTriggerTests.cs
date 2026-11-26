using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlReader content model for triggers, trigger collections and trigger actions.</summary>
public class Round51aRuntimeXamlTriggerTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	const string Ns = XamlReaderTests.Ns;

	static T Parse<T>(string xaml) => (T)XamlReader.Parse(xaml);

	[Fact]
	public void MultiTrigger_reads_Conditions_and_bare_Setters() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers><MultiTrigger>" +
			"<MultiTrigger.Conditions><Condition Property=\"IsDefault\" Value=\"True\"/>" +
			"<Condition Property=\"IsEnabled\" Value=\"True\"/></MultiTrigger.Conditions>" +
			"<Setter Property=\"Width\" Value=\"9\"/></MultiTrigger></Style.Triggers></Style>");

		var t = Assert.IsType<MultiTrigger>(s.Triggers[0]);
		Assert.Equal(2, t.Conditions.Count);
		Assert.Same(Button.IsDefaultProperty, t.Conditions[0].Property);
		Assert.Equal("True", t.Conditions[0].Value);
		Assert.Single(t.Setters);
	});

	[Fact]
	public void DataTrigger_reads_Binding_and_bare_Setters() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers>" +
			"<DataTrigger Binding=\"{Binding Done}\" Value=\"True\"><Setter Property=\"Width\" Value=\"3\"/>" +
			"</DataTrigger></Style.Triggers></Style>");

		var t = Assert.IsType<DataTrigger>(s.Triggers[0]);
		Assert.Equal("Done", t.Binding!.Path!.Path);
		Assert.Single(t.Setters);
	});

	[Fact]
	public void MultiDataTrigger_reads_Conditions_with_bindings() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers><MultiDataTrigger>" +
			"<MultiDataTrigger.Conditions><Condition Binding=\"{Binding A}\" Value=\"1\"/>" +
			"<Condition Binding=\"{Binding B}\" Value=\"2\"/></MultiDataTrigger.Conditions>" +
			"<Setter Property=\"Width\" Value=\"4\"/></MultiDataTrigger></Style.Triggers></Style>");

		var t = Assert.IsType<MultiDataTrigger>(s.Triggers[0]);
		Assert.Equal(2, t.Conditions.Count);
		Assert.Equal("B", t.Conditions[1].Binding!.Path!.Path);
		Assert.Single(t.Setters);
	});

	[Fact]
	public void Trigger_reads_EnterActions_and_ExitActions() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers>" +
			"<Trigger Property=\"IsDefault\" Value=\"True\">" +
			"<Trigger.EnterActions><BeginStoryboard><Storyboard/></BeginStoryboard></Trigger.EnterActions>" +
			"<Trigger.ExitActions><BeginStoryboard><Storyboard/></BeginStoryboard></Trigger.ExitActions>" +
			"</Trigger></Style.Triggers></Style>");

		var t = Assert.IsType<Trigger>(s.Triggers[0]);
		Assert.IsType<BeginStoryboard>(Assert.Single(t.EnterActions));
		Assert.IsType<BeginStoryboard>(Assert.Single(t.ExitActions));
		Assert.NotNull(((BeginStoryboard)t.EnterActions[0]).Storyboard);
	});

	[Fact]
	public void DataTrigger_reads_EnterActions() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers>" +
			"<DataTrigger Binding=\"{Binding Done}\" Value=\"True\">" +
			"<DataTrigger.EnterActions><BeginStoryboard><Storyboard/></BeginStoryboard></DataTrigger.EnterActions>" +
			"</DataTrigger></Style.Triggers></Style>");

		Assert.Single(Assert.IsType<DataTrigger>(s.Triggers[0]).EnterActions);
	});

	[Fact]
	public void EventTrigger_reads_RoutedEvent_and_bare_Actions() => Run(() =>
	{
		var b = Parse<Button>("<Button " + Ns + "><Button.Triggers>" +
			"<EventTrigger RoutedEvent=\"Button.Click\"><BeginStoryboard><Storyboard/></BeginStoryboard>" +
			"</EventTrigger></Button.Triggers></Button>");

		var t = Assert.IsType<EventTrigger>(Assert.Single(b.Triggers));
		Assert.Same(Button.ClickEvent, t.RoutedEvent);
		Assert.IsType<BeginStoryboard>(Assert.Single(t.Actions));
	});

	[Fact]
	public void EventTrigger_reads_the_Actions_property_element() => Run(() =>
	{
		var b = Parse<Button>("<Button " + Ns + "><Button.Triggers>" +
			"<EventTrigger RoutedEvent=\"Button.Click\"><EventTrigger.Actions><BeginStoryboard><Storyboard/>" +
			"</BeginStoryboard></EventTrigger.Actions></EventTrigger></Button.Triggers></Button>");

		Assert.Single(((EventTrigger)b.Triggers.Single()).Actions);
	});

	[Fact]
	public void ControlTemplate_reads_Triggers_with_property_and_target_setters() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Setter Property=\"Template\"><Setter.Value>" +
			"<ControlTemplate TargetType=\"Button\"><Border x:Name=\"Bd\"/><ControlTemplate.Triggers>" +
			"<Trigger Property=\"IsMouseOver\" Value=\"True\">" +
			"<Setter TargetName=\"Bd\" Property=\"Border.Opacity\" Value=\"0.5\"/></Trigger>" +
			"</ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>");

		var template = Assert.IsType<ControlTemplate>(((Setter)s.Setters[0]).Value);
		var trigger = Assert.IsType<Trigger>(Assert.Single(template.Triggers));
		var setter = (Setter)Assert.Single(trigger.Setters);
		Assert.Equal("Bd", setter.TargetName);
		Assert.Same(Border.OpacityProperty, setter.Property);
	});

	[Fact]
	public void DataTemplate_reads_Triggers() => Run(() =>
	{
		var t = Parse<DataTemplate>("<DataTemplate " + Ns + "><TextBlock/><DataTemplate.Triggers>" +
			"<DataTrigger Binding=\"{Binding Done}\" Value=\"True\"><Setter Property=\"Width\" Value=\"3\"/>" +
			"</DataTrigger></DataTemplate.Triggers></DataTemplate>");

		Assert.IsType<DataTrigger>(Assert.Single(t.Triggers));
	});

	[Fact]
	public void Unknown_trigger_member_throws_with_line_and_position() => Run(() =>
	{
		var ex = Assert.Throws<XamlParseException>(() => XamlReader.Parse(
			"<Style " + Ns + " TargetType=\"Button\"><Style.Triggers>\n  <Trigger Bogus=\"1\"/>" +
			"</Style.Triggers></Style>"));

		Assert.Equal(2, ex.LineNumber);
		Assert.True(ex.LinePosition > 0);
	});

	[Fact]
	public void Unknown_member_in_a_trigger_property_element_throws_with_position() => Run(() =>
	{
		var ex = Assert.Throws<XamlParseException>(() => XamlReader.Parse(
			"<Style " + Ns + " TargetType=\"Button\"><Style.Triggers><Trigger Property=\"IsDefault\" Value=\"True\">\n" +
			"<Trigger.Nope/></Trigger></Style.Triggers></Style>"));

		Assert.Equal(2, ex.LineNumber);
	});

	[Fact]
	public void Unknown_Condition_member_throws_XamlParseException() => Run(() =>
	{
		var ex = Assert.Throws<XamlParseException>(() => XamlReader.Parse(
			"<MultiTrigger " + Ns + ">\n<MultiTrigger.Conditions><Condition Bogus=\"1\"/>" +
			"</MultiTrigger.Conditions></MultiTrigger>"));

		Assert.Equal(2, ex.LineNumber);
	});
}
