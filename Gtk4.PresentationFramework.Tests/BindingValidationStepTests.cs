using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>ValidationStep ordering, rule constructors and the write-back pipeline.</summary>
public class BindingValidationStepTests : BindingWriteBackTestBase
{
	sealed class StepRule : ValidationRule
	{
		readonly List<string> _log;
		public object? Seen;
		public bool Fail;

		public StepRule(ValidationStep step, List<string> log, bool onTarget = false)
			: base(step, onTarget) => _log = log;

		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
		{
			_log.Add(ValidationStep.ToString());
			Seen = value;
			return Fail ? new ValidationResult(false, "bad " + ValidationStep) : ValidationResult.ValidResult;
		}
	}

	sealed class PlainRule : ValidationRule
	{
		public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
			=> ValidationResult.ValidResult;
	}

	static WpfTextBox Bind(Vm vm, params StepRule[] rules)
	{
		var tb = new WpfTextBox { DataContext = vm };
		var b = new Binding("Age");
		foreach (var r in rules) b.ValidationRules.Add(r);
		tb.SetBinding(WpfTextBox.TextProperty, b);
		return tb;
	}

	[Fact]
	public void Enum_values_match_wpf()
	{
		Assert.Equal(0, (int)ValidationStep.RawProposedValue);
		Assert.Equal(1, (int)ValidationStep.ConvertedProposedValue);
		Assert.Equal(2, (int)ValidationStep.UpdatedValue);
		Assert.Equal(3, (int)ValidationStep.CommittedValue);
	}

	[Fact]
	public void Rule_defaults_to_raw_and_not_on_target_updated()
	{
		var r = new PlainRule();
		Assert.Equal(ValidationStep.RawProposedValue, r.ValidationStep);
		Assert.False(r.ValidatesOnTargetUpdated);
	}

	[Fact]
	public void Built_in_rules_have_wpf_steps()
	{
		var d = new DataErrorValidationRule();
		Assert.Equal(ValidationStep.UpdatedValue, d.ValidationStep);
		Assert.True(d.ValidatesOnTargetUpdated);
		var e = new ExceptionValidationRule();
		Assert.Equal(ValidationStep.RawProposedValue, e.ValidationStep);
		Assert.False(e.ValidatesOnTargetUpdated);
		var n = new NotifyDataErrorValidationRule();
		Assert.Equal(ValidationStep.UpdatedValue, n.ValidationStep);
		Assert.True(n.ValidatesOnTargetUpdated);
	}

	[Fact]
	public void Owner_overload_substitutes_owner_only_for_updated_and_committed()
	{
		var log = new List<string>();
		var raw = new StepRule(ValidationStep.RawProposedValue, log);
		raw.Validate("v", CultureInfo.InvariantCulture, (BindingExpressionBase)null!);
		Assert.Equal("v", raw.Seen);
		var committed = new StepRule(ValidationStep.CommittedValue, log);
		committed.Validate("v", CultureInfo.InvariantCulture, (BindingExpressionBase)null!);
		Assert.Null(committed.Seen);
	}

	[Fact]
	public void Steps_run_in_pipeline_order() => Run(() =>
	{
		var log = new List<string>();
		var tb = Bind(new Vm(),
			new StepRule(ValidationStep.CommittedValue, log),
			new StepRule(ValidationStep.UpdatedValue, log),
			new StepRule(ValidationStep.ConvertedProposedValue, log),
			new StepRule(ValidationStep.RawProposedValue, log));
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(new[] { "RawProposedValue", "ConvertedProposedValue", "UpdatedValue", "CommittedValue" }, log);
	});

	[Fact]
	public void Converted_rule_sees_the_converted_int() => Run(() =>
	{
		var rule = new StepRule(ValidationStep.ConvertedProposedValue, new List<string>());
		var tb = Bind(new Vm(), rule);
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(7, Assert.IsType<int>(rule.Seen));
	});

	[Fact]
	public void Updated_rule_gets_the_expression_after_the_write() => Run(() =>
	{
		var vm = new Vm();
		var rule = new StepRule(ValidationStep.UpdatedValue, new List<string>());
		var tb = Bind(vm, rule);
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Same(tb.GetBindingExpression(WpfTextBox.TextProperty), rule.Seen);
		Assert.Equal(7, vm.Age);
	});

	[Fact]
	public void Failing_raw_rule_blocks_conversion_and_write() => Run(() =>
	{
		var vm = new Vm { Age = 3 };
		var log = new List<string>();
		var tb = Bind(vm,
			new StepRule(ValidationStep.RawProposedValue, log) { Fail = true },
			new StepRule(ValidationStep.ConvertedProposedValue, log));
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(3, vm.Age);
		Assert.Equal(new[] { "RawProposedValue" }, log);
		Assert.True(Validation.GetHasError(tb));
	});

	[Fact]
	public void Failing_converted_rule_blocks_the_write_and_later_steps() => Run(() =>
	{
		var vm = new Vm { Age = 3 };
		var log = new List<string>();
		var tb = Bind(vm,
			new StepRule(ValidationStep.ConvertedProposedValue, log) { Fail = true },
			new StepRule(ValidationStep.UpdatedValue, log));
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(3, vm.Age);
		Assert.Equal(new[] { "ConvertedProposedValue" }, log);
	});

	[Fact]
	public void Failing_updated_rule_skips_committed_but_keeps_the_write() => Run(() =>
	{
		var vm = new Vm();
		var log = new List<string>();
		var tb = Bind(vm,
			new StepRule(ValidationStep.UpdatedValue, log) { Fail = true },
			new StepRule(ValidationStep.CommittedValue, log));
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(7, vm.Age);
		Assert.Equal(new[] { "UpdatedValue" }, log);
		Assert.Single(Validation.GetErrors(tb));
	});

	[Fact]
	public void Only_the_first_failing_rule_in_a_step_reports() => Run(() =>
	{
		var log = new List<string>();
		var tb = Bind(new Vm(),
			new StepRule(ValidationStep.RawProposedValue, log) { Fail = true },
			new StepRule(ValidationStep.RawProposedValue, log) { Fail = true });
		tb.Text = "7";
		MoveFocusAway(tb);
		Assert.Equal(1, Validation.GetErrors(tb).Count);
		Assert.Equal(1, log.Count);
	});

	[Fact]
	public void ValidatesOnTargetUpdated_runs_after_a_source_push() => Run(() =>
	{
		var log = new List<string>();
		var rule = new StepRule(ValidationStep.RawProposedValue, log, onTarget: true) { Fail = true };
		var tb = Bind(new Vm(), rule);
		Assert.Contains("RawProposedValue", log);
		Assert.True(Validation.GetHasError(tb));
	});

	[Fact]
	public void Rule_without_ValidatesOnTargetUpdated_does_not_run_on_push() => Run(() =>
	{
		var log = new List<string>();
		Bind(new Vm(), new StepRule(ValidationStep.RawProposedValue, log));
		Assert.Empty(log);
	});
}
