using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="DataTrigger"/> and <see cref="MultiTrigger"/>.
/// </summary>
public class TriggerDataAndMultiTests
{
	sealed class VM : INotifyPropertyChanged
	{
		bool _active;
		public bool Active
		{
			get => _active;
			set { _active = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Active))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class TEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("DtTag", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void DataTrigger_applies_when_bound_value_matches()
	{
		var vm = new VM { Active = false };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("Active"), Value = true };
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "active" });
		style.Triggers.Add(dt);
		el.Style = style;

		vm.Active = true;

		Assert.Equal("active", el.Tag2);
	}

	[Fact]
	public void DataTrigger_reverts_when_bound_value_no_longer_matches()
	{
		var vm = new VM { Active = true };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var dt = new DataTrigger { Binding = new Binding("Active"), Value = true };
		dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "on" });
		style.Triggers.Add(dt);
		el.Style = style;

		Assert.Equal("on", el.Tag2);

		vm.Active = false;

		Assert.Null(el.Tag2);
	}

	// ── MultiTrigger ─────────────────────────────────────────────────────────

	sealed class TEl2 : FrameworkElement
	{
		public static readonly DependencyProperty AProperty =
			DependencyProperty.Register("MTA2", typeof(bool), typeof(TEl2), new PropertyMetadata(false));
		public static readonly DependencyProperty BProperty =
			DependencyProperty.Register("MTB2", typeof(bool), typeof(TEl2), new PropertyMetadata(false));
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("MTOut2", typeof(string), typeof(TEl2), new PropertyMetadata(null));

		public bool A { get => (bool)GetValue(AProperty)!; set => SetValue(AProperty, value); }
		public bool B { get => (bool)GetValue(BProperty)!; set => SetValue(BProperty, value); }
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	[Fact]
	public void MultiTrigger_fires_only_when_all_conditions_met()
	{
		var el = new TEl2();
		var style = new Style(typeof(TEl2));
		var mt = new MultiTrigger();
		mt.Conditions.Add(new Condition { Property = TEl2.AProperty, Value = true });
		mt.Conditions.Add(new Condition { Property = TEl2.BProperty, Value = true });
		mt.Setters.Add(new Setter { Property = TEl2.OutProperty, Value = "both" });
		style.Triggers.Add(mt);
		el.Style = style;

		el.A = true;
		Assert.Null(el.Out);

		el.B = true;
		Assert.Equal("both", el.Out);
	}

	[Fact]
	public void MultiTrigger_reverts_when_one_condition_becomes_false()
	{
		var el = new TEl2();
		var style = new Style(typeof(TEl2));
		var mt = new MultiTrigger();
		mt.Conditions.Add(new Condition { Property = TEl2.AProperty, Value = true });
		mt.Conditions.Add(new Condition { Property = TEl2.BProperty, Value = true });
		mt.Setters.Add(new Setter { Property = TEl2.OutProperty, Value = "both" });
		style.Triggers.Add(mt);
		el.Style = style;

		el.A = true;
		el.B = true;
		Assert.Equal("both", el.Out);

		el.B = false;
		Assert.Null(el.Out);
	}
}