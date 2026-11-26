using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public partial class BindingGroupTests
{
	[Fact]
	public void BindingGroup_ValidationRule_blocks_invalid_commit()
	{
		// Rule validates the source entity — a vm with Age < 0 is invalid.
		var vm = new PersonVm { Name = "Dave", Age = -5 }; // already negative in source

		var ageEl = new IntElement();
		ageEl.DataContext = vm;
		var ageExpr = (BindingExpression?)ageEl.SetBinding(
			IntElement.ValueProperty,
			new Binding("Age") { Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

		var group = new BindingGroup();
		group.BindingExpressions.Add(ageExpr!);
		group.ValidationRules.Add(new RejectNegativeAgeRule());

		// CommitEdit runs the rule; source Age is -5 → invalid.
		bool ok = group.CommitEdit();

		Assert.False(ok);
	}

	[Fact]
	public void BindingGroup_ValidationRule_allows_valid_commit()
	{
		var vm = new PersonVm { Name = "Eve", Age = 10 };

		var ageEl = new IntElement();
		ageEl.DataContext = vm;
		var ageExpr = (BindingExpression?)ageEl.SetBinding(
			IntElement.ValueProperty,
			new Binding("Age") { Mode = BindingMode.TwoWay,
				UpdateSourceTrigger = UpdateSourceTrigger.Explicit });

		var group = new BindingGroup();
		group.BindingExpressions.Add(ageExpr!);
		group.ValidationRules.Add(new RejectNegativeAgeRule());

		group.BeginEdit();
		ageEl.Value = 20; // valid staged value

		bool ok = group.CommitEdit();

		Assert.True(ok);
		Assert.Equal(20, vm.Age);
	}

	[Fact]
	public void BindingGroup_Items_contains_distinct_resolved_sources()
	{
		var vm = new PersonVm { Name = "Frank", Age = 5 };

		var nameEl = new StringElement();
		nameEl.DataContext = vm;
		var nameExpr = (BindingExpression?)nameEl.SetBinding(
			StringElement.ValueProperty, new Binding("Name"));

		var ageEl = new IntElement();
		ageEl.DataContext = vm;
		var ageExpr = (BindingExpression?)ageEl.SetBinding(
			IntElement.ValueProperty, new Binding("Age"));

		var group = new BindingGroup();
		group.BindingExpressions.Add(nameExpr!);
		group.BindingExpressions.Add(ageExpr!);

		// Same source object — Items should contain it only once.
		Assert.Single(group.Items);
		Assert.Same(vm, group.Items[0]);
	}
}
