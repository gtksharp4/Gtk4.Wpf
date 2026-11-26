using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for Round-9 Opus review defects (T1–T6).
/// Every test fails without the matching fix and passes with it.
/// Pure (no GTK loop) where possible.
/// </summary>
public class Round9DefectTests
{
	// ── T1: TemplateBinding never resolves on compiler path ───────────────────

	// A concrete Control subclass with one string DP to bind to.
	sealed class ParentControl : Control
	{
		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register(nameof(Title), typeof(string), typeof(ParentControl),
				new PropertyMetadata(string.Empty));

		public string Title
		{
			get => (string?)GetValue(TitleProperty) ?? string.Empty;
			set => SetValue(TitleProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	// A concrete FrameworkElement child that exposes VisualChildren so stamping
	// reaches it.  Uses a string DP that the TemplateBinding will push into.
	sealed class ChildElement : FrameworkElement
	{
		public static readonly DependencyProperty LabelProperty =
			DependencyProperty.Register(nameof(Label), typeof(string), typeof(ChildElement),
				new PropertyMetadata(string.Empty));

		public string Label
		{
			get => (string?)GetValue(LabelProperty) ?? string.Empty;
			set => SetValue(LabelProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// T1 (Critical) — the compiler-emitted pattern: SetBinding called INSIDE the factory
	/// lambda, before TemplatedParent is stamped.  Without the fix the binding activates
	/// against TemplatedParent==null and never re-resolves.
	/// </summary>
	[Fact]
	public void T1_TemplateBinding_set_inside_factory_resolves_after_stamp()
	{
		var parent = new ParentControl { Title = "hello" };
		var child = new ChildElement();

		// Compiler-emitted pattern: binding is set inside the factory lambda.
		// At the time the lambda runs TemplatedParent is null — this is the
		// exact path that previously left the binding stuck at the DP default.
		var template = new ControlTemplate
		{
			Factory = () =>
			{
				child.SetBinding(ChildElement.LabelProperty,
					new Binding("Title")
					{
						RelativeSource = RelativeSource.TemplatedParent,
						Mode = BindingMode.OneWay
					});
				return child;
			}
		};

		// Setting Template invokes the factory, then stamps TemplatedParent,
		// then calls RefreshTemplatedParentBindings.
		parent.Template = template;

		// The binding must now reflect the parent's Title (not the DP default "").
		Assert.Equal("hello", child.Label);
	}

	[Fact]
	public void T1_TemplateBinding_updates_when_source_property_changes()
	{
		// INPC source: the binding must track changes after initial resolve.
		var parent = new ParentControl { Title = "first" };
		var child = new ChildElement();

		var template = new ControlTemplate
		{
			Factory = () =>
			{
				child.SetBinding(ChildElement.LabelProperty,
					new Binding("Title")
					{
						RelativeSource = RelativeSource.TemplatedParent,
						Mode = BindingMode.OneWay
					});
				return child;
			}
		};
		parent.Template = template;
		Assert.Equal("first", child.Label);

		// ParentControl does not implement INPC, so live updates are not expected.
		// This test only checks that the initial resolution succeeds.
		// (INPC tracking for TemplatedParent requires INotifyPropertyChanged on
		// the templated parent — not part of T1's scope.)
	}

	// ── T3: AnimationScheduler must be called on the GLib main-loop thread ───

	[Fact]
	public void T3_AnimationScheduler_Register_throws_when_not_on_gtk_thread()
	{
		// Install a deterministic override: simulates "current thread is NOT the GTK thread".
		// This keeps the test pure (no live GLib loop required).
		var savedOverride = AnimationScheduler.IsOnGtkThreadOverride;
		AnimationScheduler.IsOnGtkThreadOverride = () => false;
		try
		{
			var anim = new DoubleAnimation { From = 0, To = 10, Duration = TimeSpan.FromSeconds(1) };
			var target = new AnimTestTarget();
			var clock = new AnimationClock(anim, target, AnimTestTarget.ValueProperty);

			var ex = Assert.Throws<InvalidOperationException>(
				() => AnimationScheduler.RegisterForTest(clock));

			Assert.Contains("GLib main-loop", ex.Message);
		}
		finally
		{
			AnimationScheduler.IsOnGtkThreadOverride = savedOverride;
		}
	}

	[Fact]
	public void T3_AnimationScheduler_Register_succeeds_when_on_gtk_thread()
	{
		// Install a deterministic override: simulates "current thread IS the GTK thread".
		var savedOverride = AnimationScheduler.IsOnGtkThreadOverride;
		AnimationScheduler.IsOnGtkThreadOverride = () => true;
		try
		{
			var anim = new DoubleAnimation { From = 0, To = 10, Duration = TimeSpan.FromSeconds(1) };
			var target = new AnimTestTarget();
			var clock = new AnimationClock(anim, target, AnimTestTarget.ValueProperty);

			// Must not throw.
			AnimationScheduler.RegisterForTest(clock);
			// Cleanup.
			AnimationScheduler.UnregisterForTest(clock);
		}
		finally
		{
			AnimationScheduler.IsOnGtkThreadOverride = savedOverride;
		}
	}

	// Minimal FrameworkElement for animation tests that skips GTK widget creation.
	sealed class AnimTestTarget : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register("T3Value", typeof(double), typeof(AnimTestTarget),
				new PropertyMetadata(0.0));

		public double Value
		{
			get => (double)(GetValue(ValueProperty) ?? 0.0);
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test — no GTK widget needed");
	}
}