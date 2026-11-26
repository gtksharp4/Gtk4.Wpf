using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for Round-14 Opus review defects V1–V4.
/// Each test fails without the fix and passes with it.
/// </summary>
public class Round14DefectTests
{
	// ── Shared helpers ────────────────────────────────────────────────────────

	sealed class R14Element : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("R14Value", typeof(string), typeof(R14Element),
				new PropertyMetadata(null));
		public string? Value
		{
			get => (string?)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class Vm : INotifyPropertyChanged
	{
		string? _name;
		string? _other;

		public string? Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}

		public string? Other
		{
			get => _other;
			set { _other = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Other))); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
	}

	// ── V1: ClearBinding clears PriorityBinding INPC handlers ────────────────

	[Fact]
	public void V1_ClearBinding_stops_PriorityBinding_updates()
	{
		var vm1 = new Vm { Name = null };
		var vm2 = new Vm { Name = "second" };
		var el = new R14Element();

		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });
		BindingOperations.SetBinding(el, R14Element.ValueProperty, pb);

		Assert.Equal("second", el.Value);

		BindingOperations.ClearBinding(el, R14Element.ValueProperty);

		// Expression must be gone.
		Assert.Null(BindingOperations.GetBindingExpressionBase(el, R14Element.ValueProperty));

		// Source change must NOT update target after clear.
		vm2.Name = "changed-after-clear";
		Assert.Equal("second", el.Value);
	}

	// ── V2: ClearAllBindings clears PriorityBinding records ──────────────────

	[Fact]
	public void V2_ClearAllBindings_stops_PriorityBinding_updates()
	{
		var vm = new Vm { Name = "live" };
		var el = new R14Element();

		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm });
		BindingOperations.SetBinding(el, R14Element.ValueProperty, pb);

		Assert.Equal("live", el.Value);

		BindingOperations.ClearAllBindings(el);

		// Source change must NOT update target after clear.
		vm.Name = "changed-after-clear-all";
		Assert.Equal("live", el.Value);
	}

	// ── V3a: PriorityBinding filters by property name (no over-fire) ─────────

	[Fact]
	public void V3a_PriorityBinding_does_not_refire_on_unrelated_property_change()
	{
		var vm = new Vm { Name = "x" };
		var el = new R14Element();

		// Use a binding that watches "Name" but fire "Other".
		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm });
		BindingOperations.SetBinding(el, R14Element.ValueProperty, pb);

		// Capture baseline (the initial push).
		string? baselineValue = el.Value;

		// Hook a property-changed callback on the DP to count updates.
		int updateCount = 0;
		el.AddChangeHook(R14Element.ValueProperty,
			(d, e) => updateCount++);

		// Change a DIFFERENT property — must not trigger a re-push.
		vm.Other = "unrelated";

		Assert.Equal(0, updateCount);
		Assert.Equal(baselineValue, el.Value);
	}

	// ── V3b: PriorityBinding re-resolves when higher-priority source becomes valid ──

	[Fact]
	public void V3b_PriorityBinding_resolves_higher_priority_when_it_becomes_non_null()
	{
		var vm1 = new Vm { Name = null };
		var vm2 = new Vm { Name = "low" };
		var el = new R14Element();

		var pb = new PriorityBinding();
		pb.Bindings.Add(new Binding("Name") { Source = vm1 });
		pb.Bindings.Add(new Binding("Name") { Source = vm2 });
		BindingOperations.SetBinding(el, R14Element.ValueProperty, pb);

		// Initially vm1 is null → falls through to vm2.
		Assert.Equal("low", el.Value);

		// vm1 becomes non-null → higher priority takes over.
		vm1.Name = "high";
		Assert.Equal("high", el.Value);
	}

	// ── V4: ExitActions do NOT fire on style swap; DO fire on condition flip ──

	sealed class V4Element : FrameworkElement
	{
		public static readonly DependencyProperty FlagProperty =
			DependencyProperty.Register("V4Flag", typeof(bool), typeof(V4Element),
				new PropertyMetadata(false));
		public static readonly DependencyProperty OutProperty =
			DependencyProperty.Register("V4Out", typeof(string), typeof(V4Element),
				new PropertyMetadata(null));

		public bool Flag
		{
			get => (bool)GetValue(FlagProperty)!;
			set => SetValue(FlagProperty, value);
		}
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class CountingAction : TriggerAction
	{
		public int Count { get; private set; }
		internal override void Invoke(FrameworkElement target) => Count++;
	}

	[Fact]
	public void V4_ExitActions_do_not_fire_on_style_replacement_when_trigger_is_active()
	{
		var exitAction = new CountingAction();
		var el = new V4Element { Flag = true }; // condition already met

		var trigger = new Trigger { Property = V4Element.FlagProperty, Value = true };
		trigger.ExitActions.Add(exitAction);

		var style1 = new Style(typeof(V4Element));
		style1.Triggers.Add(trigger);

		// Apply the style — trigger is immediately active (Flag == true).
		el.Style = style1;
		Assert.Equal(0, exitAction.Count); // EnterActions territory, ExitActions silent

		// Replace the style — active trigger should be torn down WITHOUT running ExitActions.
		el.Style = new Style(typeof(V4Element));

		Assert.Equal(0, exitAction.Count);
	}

	[Fact]
	public void V4_ExitActions_fire_on_genuine_condition_flip()
	{
		var exitAction = new CountingAction();
		var el = new V4Element();

		var trigger = new Trigger { Property = V4Element.FlagProperty, Value = true };
		trigger.ExitActions.Add(exitAction);

		var style = new Style(typeof(V4Element));
		style.Triggers.Add(trigger);
		el.Style = style;

		// Activate the trigger.
		el.Flag = true;
		Assert.Equal(0, exitAction.Count);

		// Deactivate via genuine condition flip — ExitActions MUST fire.
		el.Flag = false;
		Assert.Equal(1, exitAction.Count);
	}
}
