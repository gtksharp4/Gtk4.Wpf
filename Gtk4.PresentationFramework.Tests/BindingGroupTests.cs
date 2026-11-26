using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="BindingGroup"/>.
/// Each test fails without the feature and passes with it.
/// </summary>
public partial class BindingGroupTests
{
	// ── Helpers ───────────────────────────────────────────────────────────────

	sealed class PersonVm : INotifyPropertyChanged
	{
		string _name = string.Empty;
		int _age;

		public string Name
		{
			get => _name;
			set { _name = value; Raise(nameof(Name)); }
		}

		public int Age
		{
			get => _age;
			set { _age = value; Raise(nameof(Age)); }
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		void Raise(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
	}

	sealed class StringElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(string), typeof(StringElement),
				new PropertyMetadata(string.Empty));
		public string Value
		{
			get => (string?)GetValue(ValueProperty) ?? string.Empty;
			set => SetValue(ValueProperty, value);
		}
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	sealed class IntElement : FrameworkElement
	{
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(nameof(Value), typeof(int), typeof(IntElement),
				new PropertyMetadata(0));
		public int Value
		{
			get => (int)(GetValue(ValueProperty) ?? 0);
			set => SetValue(ValueProperty, value);
		}
		protected override Gtk.Widget CreateWidget() => new Gtk.Label();
	}

	/// <summary>
	/// Validates the group as a whole, as WPF passes the group as the value: rejects a negative proposed Age.
	/// </summary>
	sealed class RejectNegativeAgeRule : ValidationRule
	{
		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		{
			var group = (BindingGroup)value!;
			if (group.TryGetValue(group.Items[0], "Age", out var age) && (int)age! < 0)
				return new ValidationResult(false, "Age must be non-negative");
			return ValidationResult.ValidResult;
		}
	}

	// ── Tests ─────────────────────────────────────────────────────────────────

	[Fact]
	public void BindingGroup_CommitEdit_writes_staged_values_to_sources()
	{
		var vm = new PersonVm { Name = "Alice", Age = 30 };

		var nameEl = new StringElement();
		nameEl.DataContext = vm;
		var nameExpr = (BindingExpression?)nameEl.SetBinding(
			StringElement.ValueProperty,
			new Binding("Name") { Mode = BindingMode.TwoWay });

		var ageEl = new IntElement();
		ageEl.DataContext = vm;
		var ageExpr = (BindingExpression?)ageEl.SetBinding(
			IntElement.ValueProperty,
			new Binding("Age") { Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

		var group = new BindingGroup();
		group.BindingExpressions.Add(nameExpr!);
		group.BindingExpressions.Add(ageExpr!);

		group.BeginEdit();

		// Stage new values on the target elements.
		nameEl.Value = "Bob";
		ageEl.Value = 25;

		bool ok = group.CommitEdit();

		Assert.True(ok);
		Assert.Equal("Bob", vm.Name);
		Assert.Equal(25, vm.Age);
	}

	[Fact]
	public void BindingGroup_CancelEdit_restores_target_from_source()
	{
		var vm = new PersonVm { Name = "Carol", Age = 40 };

		var nameEl = new StringElement();
		nameEl.DataContext = vm;
		var nameExpr = (BindingExpression?)nameEl.SetBinding(
			StringElement.ValueProperty,
			new Binding("Name") { Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

		var group = new BindingGroup();
		group.BindingExpressions.Add(nameExpr!);

		group.BeginEdit();
		nameEl.Value = "X";   // staged but not committed
		group.CancelEdit();

		// Target should revert to source value.
		Assert.Equal("Carol", nameEl.Value);
	}
}
