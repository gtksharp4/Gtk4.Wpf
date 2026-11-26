using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="Trigger"/> (property trigger) and trigger precedence.
/// </summary>
public class TriggerPropertyTests
{
	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("TrigPropTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public static readonly DependencyProperty FlagProperty =
			DependencyProperty.Register("TrigPropFlag", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));
		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		public bool Flag { get => (bool)GetValue(FlagProperty)!; set => SetValue(FlagProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Trigger_applies_setter_when_condition_matches()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;

		Assert.Equal("triggered", el.Tag2);
	}

	[Fact]
	public void Trigger_reverts_setter_when_condition_no_longer_matches()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;
		el.Flag = false;

		Assert.Null(el.Tag2);
	}

	[Fact]
	public void Trigger_local_value_beats_trigger_setter()
	{
		var el = new TEl();
		el.Tag2 = "local";

		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		style.Triggers.Add(trigger);
		el.Style = style;
		el.Flag = true;

		Assert.Equal("local", el.Tag2);
	}

	[Fact]
	public void Trigger_setter_beats_style_setter()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		style.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "styled" });
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "triggered" });
		style.Triggers.Add(trigger);
		el.Style = style;

		Assert.Equal("styled", el.Tag2);

		el.Flag = true;

		Assert.Equal("triggered", el.Tag2);
	}

	[Fact]
	public void Trigger_detaches_when_style_is_replaced()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		style.Triggers.Add(trigger);
		el.Style = style;
		el.Flag = true;
		Assert.Equal("on", el.Tag2);

		el.Style = new Style(typeof(TEl));

		Assert.Null(el.Tag2);
	}

	[Fact]
	public void Trigger_detaches_when_style_is_nulled()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProperty, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		style.Triggers.Add(trigger);
		el.Style = style;
		el.Flag = true;
		el.Style = null;

		Assert.Null(el.Tag2);
	}
}