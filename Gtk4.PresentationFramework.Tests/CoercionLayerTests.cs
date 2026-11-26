using System;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// E4: CoerceValueCallback is applied when the value comes from the style layer
/// or the trigger layer, not only from a local set.
/// </summary>
public class CoercionLayerTests
{
	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty ScoreProp =
			DependencyProperty.Register("CoerceScore", typeof(int), typeof(TEl),
				new PropertyMetadata(0, null, CoerceNotNegative));

		public static readonly DependencyProperty FlagProp =
			DependencyProperty.Register("CoerceFlag", typeof(bool), typeof(TEl),
				new PropertyMetadata(false));

		static object? CoerceNotNegative(DependencyObject d, object? v) =>
			v is int i && i < 0 ? 0 : v;

		public int Score
		{
			get => (int)GetValue(ScoreProp)!;
			set => SetValue(ScoreProp, value);
		}

		public bool Flag
		{
			get => (bool)GetValue(FlagProp)!;
			set => SetValue(FlagProp, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void CoerceValueCallback_applied_to_style_setter_value()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		// Try to set a negative value via style — coercion must clamp it to 0.
		style.Setters.Add(new Setter { Property = TEl.ScoreProp, Value = -10 });
		el.Style = style;

		Assert.Equal(0, el.Score);
	}

	[Fact]
	public void CoerceValueCallback_applied_to_trigger_setter_value()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProp, Value = true };
		// Try to set a negative value via trigger — coercion must clamp it to 0.
		trigger.Setters.Add(new Setter { Property = TEl.ScoreProp, Value = -5 });
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;

		Assert.Equal(0, el.Score);
	}

	[Fact]
	public void Positive_style_value_passes_through_coercion()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		style.Setters.Add(new Setter { Property = TEl.ScoreProp, Value = 42 });
		el.Style = style;

		Assert.Equal(42, el.Score);
	}

	[Fact]
	public void Positive_trigger_value_passes_through_coercion()
	{
		var el = new TEl();
		var style = new Style(typeof(TEl));
		var trigger = new Trigger { Property = TEl.FlagProp, Value = true };
		trigger.Setters.Add(new Setter { Property = TEl.ScoreProp, Value = 7 });
		style.Triggers.Add(trigger);
		el.Style = style;

		el.Flag = true;

		Assert.Equal(7, el.Score);
	}
}