using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Validation rules run against the raw target value before conversion.</summary>
public class BindingWriteBackRuleTests : BindingWriteBackTestBase
{
	[Fact]
	public void Required_string_rule_on_int_source_sees_the_raw_text_and_allows_the_write() => Run(() =>
	{
		var vm = new Vm();
		var rule = new RecordingRule { Required = true };
		var tb = BindWithRule(vm, rule);

		tb.Text = "42";
		MoveFocusAway(tb);

		Assert.Equal(42, vm.Age);
		Assert.Equal("42", Assert.IsType<string>(rule.Seen));
		Assert.False(Validation.GetHasError(tb));
	});

	[Fact]
	public void Rule_runs_before_conversion_and_its_error_wins() => Run(() =>
	{
		var vm = new Vm { Age = 9 };
		var rule = new RecordingRule { Required = true };
		var tb = BindWithRule(vm, rule);

		tb.Text = "";
		MoveFocusAway(tb);

		Assert.Equal(9, vm.Age);
		Assert.Equal(1, rule.Calls);
		var error = Validation.GetErrors(tb)[0];
		Assert.Same(rule, error.RuleInError);
		Assert.Equal("required", error.ErrorContent);
	});

	[Fact]
	public void Passing_rule_runs_even_when_conversion_then_fails() => Run(() =>
	{
		var vm = new Vm { Age = 9 };
		var rule = new RecordingRule();
		var tb = BindWithRule(vm, rule);

		tb.Text = "abc";
		MoveFocusAway(tb);

		Assert.Equal(1, rule.Calls);
		Assert.Equal("abc", rule.Seen);
		Assert.Equal(9, vm.Age);
		Assert.True(Validation.GetHasError(tb));
	});

	[Fact]
	public void Rule_sees_raw_target_value_not_ConvertBack_output() => Run(() =>
	{
		var vm = new Vm();
		var rule = new RecordingRule();
		var tb = BindWithRule(vm, rule, new ParseConverter());

		tb.Text = "5";
		MoveFocusAway(tb);

		Assert.Equal("5", Assert.IsType<string>(rule.Seen));
		Assert.Equal(5, vm.Age);
	});
}
