using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Per-binding error aggregation, teardown, first-failure semantics and StringFormat.</summary>
public class BindingValidationPipelineTests : BindingWriteBackTestBase
{
	sealed class SiblingVm : INotifyDataErrorInfo, INotifyPropertyChanged, IDataErrorInfo
	{
		public int Age { get; set; } = 1;
		public bool CanEdit { get; set; } = true;
		public bool HasErrors => true;
		public int DataErrorReads;
		public string Error => string.Empty;
		public string this[string column] { get { DataErrorReads++; return string.Empty; } }
		public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged { add { } remove { } }
		public event PropertyChangedEventHandler? PropertyChanged;
		public IEnumerable GetErrors(string? p) => p == nameof(Age) ? new[] { "too young" } : new string[0];
		public void Raise(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
	}

	sealed class FailRule : ValidationRule
	{
		public FailRule(ValidationStep step) : base(step, false) { }
		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
			=> new ValidationResult(false, "bad " + ValidationStep);
	}

	[Fact]
	public void Sibling_INDEI_binding_push_does_not_wipe_other_binding_errors() => Run(() =>
	{
		var vm = new SiblingVm();
		var tb = new WpfTextBox { DataContext = vm };
		tb.SetBinding(WpfTextBox.TextProperty, new Binding("Age"));
		tb.SetBinding(UIElementEnabled, new Binding("CanEdit"));
		Assert.True(Validation.GetHasError(tb));
		var errors = Validation.GetErrors(tb);

		vm.Raise(nameof(SiblingVm.CanEdit));

		Assert.True(Validation.GetHasError(tb));
		Assert.Equal(1, Validation.GetErrors(tb).Count);
		Assert.Same(errors, Validation.GetErrors(tb));
	});

	static readonly System.Windows.DependencyProperty UIElementEnabled = System.Windows.UIElement.IsEnabledProperty;

	[Fact]
	public void ClearBinding_removes_step_rule_errors() => Run(() =>
	{
		var tb = new WpfTextBox { DataContext = new Vm() };
		var b = new Binding("Age");
		b.ValidationRules.Add(new FailRule(ValidationStep.RawProposedValue));
		tb.SetBinding(WpfTextBox.TextProperty, b);
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.True(Validation.GetHasError(tb));

		BindingOperations.ClearBinding(tb, WpfTextBox.TextProperty);

		Assert.False(Validation.GetHasError(tb));
		Assert.Equal(0, Validation.GetErrors(tb).Count);
	});

	[Fact]
	public void IDataErrorInfo_is_not_consulted_when_an_UpdatedValue_rule_fails() => Run(() =>
	{
		var vm = new SiblingVm();
		var tb = new WpfTextBox { DataContext = vm };
		var b = new Binding("Age") { ValidatesOnDataErrors = true, ValidatesOnNotifyDataErrors = false };
		b.ValidationRules.Add(new FailRule(ValidationStep.UpdatedValue));
		tb.SetBinding(WpfTextBox.TextProperty, b);
		vm.DataErrorReads = 0;

		tb.Text = "7";
		MoveFocusAway(tb);

		Assert.Equal(1, Validation.GetErrors(tb).Count);
		Assert.Equal(0, vm.DataErrorReads);
	});

	sealed class Fmt : INotifyPropertyChanged
	{
		public double N { get; set; } = 1234.5;
		public string A { get; set; } = "a";
		public string B { get; set; } = "b";
		public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
	}

	[Fact]
	public void StringFormat_without_braces_formats_numbers()
	{
		var us = new CultureInfo("en-US");
		Assert.Equal("1,234.50", BindingEngine.ApplyStringFormat("N2", 1234.5, us));
		Assert.Equal("$1,234.50", BindingEngine.ApplyStringFormat("C", 1234.5, us));
	}

	[Fact]
	public void MultiBinding_without_converter_formats_the_values_array()
	{
		var tb = new WpfTextBlock { DataContext = new Fmt() };
		var mb = new MultiBinding { StringFormat = "{0} {1}" };
		mb.Bindings.Add(new Binding("A"));
		mb.Bindings.Add(new Binding("B"));
		tb.SetBinding(WpfTextBlock.TextProperty, mb);
		Assert.Equal("a b", tb.Text);
	}
}
