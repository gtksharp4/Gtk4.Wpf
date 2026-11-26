using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Tests for <see cref="MultiDataTrigger"/>.</summary>
public class MultiDataTriggerTests
{
	// ── Helpers ──────────────────────────────────────────────────────────────

	sealed class VM : INotifyPropertyChanged
	{
		bool _a;
		bool _b;
		public bool A
		{
			get => _a;
			set { _a = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(A))); }
		}
		public bool B
		{
			get => _b;
			set { _b = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(B))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class TEl : FrameworkElement
	{
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("MDTOut", typeof(string), typeof(TEl),
				new PropertyMetadata(null));
		public string? Out { get => (string?)GetValue(OutProperty); set => SetValue(OutProperty, value); }
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class RecordingAction : TriggerAction
	{
		public int Count { get; private set; }
		internal override void Invoke(FrameworkElement target) => Count++;
	}

	// ── Setters ───────────────────────────────────────────────────────────────

	[Fact]
	public void MultiDataTrigger_fires_when_all_conditions_hold()
	{
		var vm = new VM { A = false, B = false };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "both" });
		style.Triggers.Add(mdt);
		el.Style = style;

		vm.A = true;
		Assert.Null(el.Out); // only A matches

		vm.B = true;
		Assert.Equal("both", el.Out); // both match
	}

	[Fact]
	public void MultiDataTrigger_reverts_when_one_condition_stops_holding()
	{
		var vm = new VM { A = true, B = true };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "both" });
		style.Triggers.Add(mdt);
		el.Style = style;

		Assert.Equal("both", el.Out);

		vm.B = false;
		Assert.Null(el.Out); // B no longer matches
	}

	[Fact]
	public void MultiDataTrigger_does_not_fire_when_only_one_condition_holds()
	{
		var vm = new VM { A = true, B = false };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "both" });
		style.Triggers.Add(mdt);
		el.Style = style;

		Assert.Null(el.Out);
	}

	// ── EnterActions / ExitActions ───────────────────────────────────────────

	[Fact]
	public void MultiDataTrigger_EnterActions_run_when_all_conditions_become_active()
	{
		var enter = new RecordingAction();
		var vm = new VM { A = false, B = false };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.EnterActions.Add(enter);
		style.Triggers.Add(mdt);
		el.Style = style;

		vm.A = true;
		Assert.Equal(0, enter.Count);

		vm.B = true;
		Assert.Equal(1, enter.Count);
	}

	[Fact]
	public void MultiDataTrigger_ExitActions_run_when_condition_breaks()
	{
		var exit = new RecordingAction();
		var vm = new VM { A = true, B = true };
		var el = new TEl { DataContext = vm };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.ExitActions.Add(exit);
		style.Triggers.Add(mdt);
		el.Style = style;

		Assert.Equal(0, exit.Count);

		vm.A = false;
		Assert.Equal(1, exit.Count);
	}

	// ── DataContext changes ───────────────────────────────────────────────────

	[Fact]
	public void MultiDataTrigger_re_evaluates_when_DataContext_changes()
	{
		var vm1 = new VM { A = false, B = false };
		var vm2 = new VM { A = true, B = true };
		var el = new TEl { DataContext = vm1 };

		var style = new Style(typeof(TEl));
		var mdt = new MultiDataTrigger();
		mdt.Conditions.Add(new Condition { Binding = new Binding("A"), Value = true });
		mdt.Conditions.Add(new Condition { Binding = new Binding("B"), Value = true });
		mdt.Setters.Add(new Setter { Property = TEl.OutProperty, Value = "both" });
		style.Triggers.Add(mdt);
		el.Style = style;

		Assert.Null(el.Out);

		el.DataContext = vm2;
		Assert.Equal("both", el.Out);
	}
}
