using System;
using System.Windows;
using System.Windows.Themes;
using Xunit;
using WpfControl = System.Windows.Controls.Control;
using WpfSetter = System.Windows.Setter;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for Round-12 track B: DefaultStyleKey DP, theme-dictionary
/// default-style resolution and style-precedence rules.
/// </summary>
public class DefaultStyleTests : IDisposable
{
	class TestControl : WpfControl
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("TagDsk", typeof(string), typeof(TestControl),
				new PropertyMetadata(null));

		public string? Tag2
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	sealed class DerivedControl : TestControl { }

	readonly System.Collections.Generic.List<Type> _registeredTypes =
		new System.Collections.Generic.List<Type>();

	void RegisterStyle(Type t, Style s)
	{
		ThemeManager.RegisterDefaultStyle(t, s);
		_registeredTypes.Add(t);
	}

	public void Dispose()
	{
		foreach (var t in _registeredTypes)
			ThemeManager.UnregisterDefaultStyle(t);
	}

	/// <summary>No explicit/implicit style: default style from ThemeManager is applied.</summary>
	[Fact]
	public void NoExplicitStyle_picksUp_defaultStyle_setters()
	{
		var defaultStyle = new Style(typeof(TestControl));
		defaultStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "default" });
		RegisterStyle(typeof(TestControl), defaultStyle);

		var ctrl = new TestControl();
		ctrl.ApplyImplicitStyle();

		Assert.Equal("default", ctrl.Tag2);
	}

	/// <summary>An explicit Style set on the element beats the theme default.</summary>
	[Fact]
	public void ExplicitStyle_wins_over_defaultStyle()
	{
		var defaultStyle = new Style(typeof(TestControl));
		defaultStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "default" });
		RegisterStyle(typeof(TestControl), defaultStyle);

		var ctrl = new TestControl();
		var explicitStyle = new Style(typeof(TestControl));
		explicitStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "explicit" });
		ctrl.Style = explicitStyle;
		ctrl.ApplyImplicitStyle();

		Assert.Equal("explicit", ctrl.Tag2);
	}

	/// <summary>An implicit resource style beats the theme default.</summary>
	[Fact]
	public void ImplicitStyle_wins_over_defaultStyle()
	{
		var defaultStyle = new Style(typeof(TestControl));
		defaultStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "default" });
		RegisterStyle(typeof(TestControl), defaultStyle);

		var parent = new TestControl();
		var implicitStyle = new Style(typeof(TestControl));
		implicitStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestControl)] = implicitStyle;

		var ctrl = new TestControl();
		ctrl.InheritanceParent = parent;
		ctrl.ApplyImplicitStyle();

		Assert.Equal("implicit", ctrl.Tag2);
	}

	/// <summary>Explicit Style beats both implicit and default.</summary>
	[Fact]
	public void ExplicitStyle_wins_over_implicitStyle()
	{
		var defaultStyle = new Style(typeof(TestControl));
		defaultStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "default" });
		RegisterStyle(typeof(TestControl), defaultStyle);

		var parent = new TestControl();
		var implicitStyle = new Style(typeof(TestControl));
		implicitStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestControl)] = implicitStyle;

		var ctrl = new TestControl();
		ctrl.InheritanceParent = parent;
		var explicitStyle = new Style(typeof(TestControl));
		explicitStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "explicit" });
		ctrl.Style = explicitStyle;
		ctrl.ApplyImplicitStyle();

		Assert.Equal("explicit", ctrl.Tag2);
	}

	/// <summary>DefaultStyleKey null falls back to GetType() for the lookup key.</summary>
	[Fact]
	public void DefaultStyleKey_null_fallsBackTo_GetType()
	{
		var style = new Style(typeof(TestControl));
		style.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "found" });
		RegisterStyle(typeof(TestControl), style);

		var ctrl = new TestControl();
		Assert.Null(ctrl.GetValue(FrameworkElement.DefaultStyleKeyProperty));
		ctrl.ApplyDefaultStyle();

		Assert.Equal("found", ctrl.Tag2);
	}

	/// <summary>Subclass inherits base's default style when it has no own registration.</summary>
	[Fact]
	public void SubclassInherits_baseDefaultStyle_when_noOwnKeySet()
	{
		var baseStyle = new Style(typeof(TestControl));
		baseStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "base-default" });
		RegisterStyle(typeof(TestControl), baseStyle);

		var ctrl = new DerivedControl();
		ctrl.ApplyDefaultStyle();

		Assert.Equal("base-default", ctrl.Tag2);
	}

	/// <summary>Explicitly setting DefaultStyleKey controls which registration is used.</summary>
	[Fact]
	public void DefaultStyleKey_explicit_usesNamedTypeStyle()
	{
		var baseStyle = new Style(typeof(TestControl));
		baseStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "base-keyed" });
		RegisterStyle(typeof(TestControl), baseStyle);

		var ctrl = new DerivedControl();
		ctrl.SetValue(FrameworkElement.DefaultStyleKeyProperty, typeof(TestControl));
		ctrl.ApplyDefaultStyle();

		Assert.Equal("base-keyed", ctrl.Tag2);
	}

	/// <summary>
	/// OverridesDefaultStyle = true makes a subsequent explicit ApplyDefaultStyle call a no-op.
	/// Note: constructing a control with OverridesDefaultStyle set in the object initializer
	/// does not suppress the automatic application in the Control() constructor (the initializer
	/// runs after construction).  To suppress auto-apply, set OverridesDefaultStyle before the
	/// control references any style, then call ApplyDefaultStyle explicitly.
	/// </summary>
	[Fact]
	public void OverridesDefaultStyle_true_makes_explicit_ApplyDefaultStyle_noop()
	{
		var defaultStyle = new Style(typeof(TestControl));
		defaultStyle.Setters.Add(new WpfSetter { Property = TestControl.TagProperty, Value = "default" });
		RegisterStyle(typeof(TestControl), defaultStyle);

		// Create control — auto-apply runs in Control() ctor.
		var ctrl = new TestControl();

		// Clear the applied values, set override flag, then re-apply — must be a no-op.
		var snapshot = ctrl.SnapshotStyleValues();
		ctrl.ClearStyleValues(snapshot);
		ctrl.OverridesDefaultStyle = true;
		ctrl.ApplyDefaultStyle();

		Assert.Null(ctrl.Tag2);
	}
}