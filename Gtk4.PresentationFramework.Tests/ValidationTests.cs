using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for Round-9 validation: ValidationRule, IDataErrorInfo,
/// INotifyDataErrorInfo, ExceptionValidationRule.
/// Each test fails without the feature and passes with it.
/// </summary>
public class ValidationTests
{
	// ── Helpers ──────────────────────────────────────────────────────────────

	sealed class TargetElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(TargetElement),
				new PropertyMetadata(string.Empty));

		public string Value
		{
			get => (string?)GetValue(ValueProperty) ?? string.Empty;
			set => SetValue(ValueProperty, value);
		}

		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class SourceVm : INotifyPropertyChanged
	{
		string _name = string.Empty;
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
	}

	sealed class ThrowingVm
	{
		public string Name
		{
			get => string.Empty;
			set => throw new InvalidOperationException("setter rejected");
		}
	}

	sealed class DataErrorVm : INotifyPropertyChanged, IDataErrorInfo
	{
		string _name = string.Empty;
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Error => string.Empty;
		public string this[string column] =>
			column == nameof(Name) && string.IsNullOrWhiteSpace(_name)
				? "Name is required" : string.Empty;
	}

	sealed class NotifyDataErrorVm : INotifyPropertyChanged, INotifyDataErrorInfo
	{
		string _name = string.Empty;
		public string Name
		{
			get => _name;
			set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Name))); }
		}
		public event PropertyChangedEventHandler? PropertyChanged;
		public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
		public bool HasErrors => string.IsNullOrWhiteSpace(_name);
		public IEnumerable GetErrors(string? prop) =>
			(prop == nameof(Name) || prop == null) && HasErrors
				? new[] { "Name required" } : Array.Empty<string>();
	}

	sealed class RejectEmptyRule : ValidationRule
	{
		public override ValidationResult Validate(object? value, CultureInfo ci)
		{
			var s = value as string ?? string.Empty;
			return string.IsNullOrWhiteSpace(s)
				? new ValidationResult(false, "Cannot be empty")
				: ValidationResult.ValidResult;
		}
	}

	// ── Tests ────────────────────────────────────────────────────────────────

	[Fact]
	public void ValidationRule_rejects_invalid_sets_HasError()
	{
		var vm = new SourceVm { Name = "valid" };
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidationRules = { new RejectEmptyRule() }
			});

		el.Value = string.Empty; // triggers write-back with invalid value

		Assert.True(Validation.GetHasError(el));
		Assert.NotEmpty(Validation.GetErrors(el));
	}

	[Fact]
	public void ValidationRule_clears_error_when_valid()
	{
		var vm = new SourceVm { Name = "valid" };
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidationRules = { new RejectEmptyRule() }
			});

		el.Value = string.Empty; // invalid → HasError
		Assert.True(Validation.GetHasError(el));

		el.Value = "good"; // valid → no error
		Assert.False(Validation.GetHasError(el));
		Assert.Empty(Validation.GetErrors(el));
	}

	[Fact]
	public void ValidatesOnDataErrors_surfaces_IDataErrorInfo()
	{
		var vm = new DataErrorVm { Name = "initial" };
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidatesOnDataErrors = true
			});

		el.Value = "   "; // blank → data error

		Assert.True(Validation.GetHasError(el));
		var err = Validation.GetErrors(el)[0];
		Assert.Equal("Name is required", err.ErrorContent?.ToString());
	}

	[Fact]
	public void ValidatesOnDataErrors_surfaces_INotifyDataErrorInfo()
	{
		var vm = new NotifyDataErrorVm { Name = "init" };
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidatesOnDataErrors = true
			});

		el.Value = string.Empty;

		Assert.True(Validation.GetHasError(el));
	}

	[Fact]
	public void ValidatesOnExceptions_catches_setter_throw()
	{
		var vm = new ThrowingVm();
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidatesOnExceptions = true
			});

		el.Value = "trigger"; // setter throws

		Assert.True(Validation.GetHasError(el));
		var err = Validation.GetErrors(el)[0];
		Assert.IsType<InvalidOperationException>(err.Exception);
	}

	[Fact]
	public void NotifyOnValidationError_raises_Error_event()
	{
		var vm = new SourceVm { Name = "valid" };
		var el = new TargetElement();
		el.SetBinding(TargetElement.ValueProperty,
			new Binding("Name")
			{
				Source = vm,
				Mode = BindingMode.TwoWay,
				ValidationRules = { new RejectEmptyRule() },
				NotifyOnValidationError = true
			});

		bool raised = false;
		Validation.AddErrorHandler(el, (s, e) => raised = true);

		el.Value = string.Empty;

		Assert.True(raised);
	}
}