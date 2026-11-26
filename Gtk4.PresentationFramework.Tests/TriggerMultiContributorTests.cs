using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// E1: Multiple property triggers on the same DP must not corrupt each other on un-apply.
/// Two triggers in a style both target the same DP; last-declared wins while both
/// match; un-matching the last exposes the first's value; un-matching both reverts to
/// style/default.
/// </summary>
public class TriggerMultiContributorTests
{
	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty ColorProp =
			DependencyProperty.Register("TMultiColor", typeof(string), typeof(TEl),
				new PropertyMetadata("default"));
		public static readonly DependencyProperty AlphaProperty =
			DependencyProperty.Register("TMultiAlpha", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));
		public static readonly DependencyProperty BetaProperty =
			DependencyProperty.Register("TMultiBeta", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));

		public string? Color { get => (string?)GetValue(ColorProp); set => SetValue(ColorProp, value); }
		public bool Alpha { get => (bool)GetValue(AlphaProperty)!; set => SetValue(AlphaProperty, value); }
		public bool Beta { get => (bool)GetValue(BetaProperty)!; set => SetValue(BetaProperty, value); }

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void Two_triggers_both_match_last_declared_wins()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));

		var t1 = new Trigger { Property = TEl.AlphaProperty, Value = true };
		t1.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "first" });

		var t2 = new Trigger { Property = TEl.BetaProperty, Value = true };
		t2.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "second" });

		style.Triggers.Add(t1);
		style.Triggers.Add(t2);
		el.Style = style;

		el.Alpha = true;    // first trigger active
		Assert.Equal("first", el.Color);

		el.Beta = true;     // second trigger also active — last-declared wins
		Assert.Equal("second", el.Color);
	}

	[Fact]
	public void Un_matching_last_trigger_exposes_first_not_default()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));

		var t1 = new Trigger { Property = TEl.AlphaProperty, Value = true };
		t1.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "first" });

		var t2 = new Trigger { Property = TEl.BetaProperty, Value = true };
		t2.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "second" });

		style.Triggers.Add(t1);
		style.Triggers.Add(t2);
		el.Style = style;

		el.Alpha = true;
		el.Beta = true;
		Assert.Equal("second", el.Color);

		// Un-match the last-declared trigger; first must resurface.
		el.Beta = false;
		Assert.Equal("first", el.Color); // NOT "default"
	}

	[Fact]
	public void Un_matching_both_triggers_reverts_to_default()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));

		var t1 = new Trigger { Property = TEl.AlphaProperty, Value = true };
		t1.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "first" });

		var t2 = new Trigger { Property = TEl.BetaProperty, Value = true };
		t2.Setters.Add(new Setter { Property = TEl.ColorProp, Value = "second" });

		style.Triggers.Add(t1);
		style.Triggers.Add(t2);
		el.Style = style;

		el.Alpha = true;
		el.Beta = true;
		el.Beta = false;
		el.Alpha = false;

		Assert.Equal("default", el.Color);
	}
}