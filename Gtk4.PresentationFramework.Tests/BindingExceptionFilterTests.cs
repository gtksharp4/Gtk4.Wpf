using System;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace Gtk4.PresentationFramework.Tests;

public class BindingExceptionFilterTests : WpfGtkTestBase
{
	sealed class ThrowingVm
	{
		public string Name
		{
			get => "n";
			set => throw new InvalidOperationException("setter failed");
		}
	}

	static (WpfTextBox box, Binding binding) Bind(bool validates = true, bool ruleInstead = false)
	{
		var binding = new Binding("Name") { Source = new ThrowingVm(), Mode = BindingMode.TwoWay };
		if (ruleInstead) binding.ValidationRules.Add(new ExceptionValidationRule());
		else binding.ValidatesOnExceptions = validates;
		var box = new WpfTextBox();
		box.SetBinding(WpfTextBox.TextProperty, binding);
		return (box, binding);
	}

	static void Update(WpfTextBox box) => box.GetBindingExpression(WpfTextBox.TextProperty)!.UpdateSource();

	[Fact]
	public void Without_a_filter_the_error_uses_the_exception_message() => Run(() =>
	{
		var (box, _) = Bind();
		box.Text = "x";
		Update(box);
		var error = Assert.Single(Validation.GetErrors(box));
		Assert.Equal("setter failed", error.ErrorContent);
		Assert.IsType<InvalidOperationException>(error.Exception);
		Assert.True(Validation.GetHasError(box));
	});

	[Fact]
	public void A_null_filter_result_means_no_error() => Run(() =>
	{
		var (box, binding) = Bind();
		binding.UpdateSourceExceptionFilter = (_, _) => null!;
		box.Text = "x";
		Update(box);
		Assert.Empty(Validation.GetErrors(box));
		Assert.False(Validation.GetHasError(box));
	});

	[Fact]
	public void A_ValidationError_result_is_used_as_is() => Run(() =>
	{
		var (box, binding) = Bind();
		var custom = new ValidationError(null, "custom");
		binding.UpdateSourceExceptionFilter = (_, _) => custom;
		box.Text = "x";
		Update(box);
		Assert.Same(custom, Assert.Single(Validation.GetErrors(box)));
	});

	[Fact]
	public void Any_other_result_becomes_the_error_content() => Run(() =>
	{
		var (box, binding) = Bind();
		object? seenExpression = null;
		Exception? seenException = null;
		binding.UpdateSourceExceptionFilter = (expr, ex) =>
		{
			seenExpression = expr;
			seenException = ex;
			return "from the filter";
		};
		box.Text = "x";
		Update(box);
		var error = Assert.Single(Validation.GetErrors(box));
		Assert.Equal("from the filter", error.ErrorContent);
		Assert.Same(ExceptionValidationRule.Instance, error.RuleInError);
		Assert.Same(seenException, error.Exception);
		Assert.Same(box.GetBindingExpression(WpfTextBox.TextProperty), seenExpression);
	});

	[Fact]
	public void An_ExceptionValidationRule_alone_enables_exception_validation() => Run(() =>
	{
		var (box, binding) = Bind(ruleInstead: true);
		binding.UpdateSourceExceptionFilter = (_, _) => "via rule";
		box.Text = "x";
		Update(box);
		Assert.Equal("via rule", Assert.Single(Validation.GetErrors(box)).ErrorContent);
	});

	[Fact]
	public void The_filter_is_ignored_when_exception_validation_is_off() => Run(() =>
	{
		var (box, binding) = Bind(validates: false);
		bool called = false;
		binding.UpdateSourceExceptionFilter = (_, _) => { called = true; return "x"; };
		box.Text = "x";
		Assert.ThrowsAny<Exception>(() => Update(box));
		Assert.False(called);
	});

	[Fact]
	public void The_filter_can_be_changed_while_the_binding_is_in_use() => Run(() =>
	{
		var (box, binding) = Bind();
		binding.UpdateSourceExceptionFilter = (_, _) => "first";
		box.Text = "x";
		Update(box);
		binding.UpdateSourceExceptionFilter -= (_, _) => "first";
		binding.UpdateSourceExceptionFilter = null;
		Update(box);
		Assert.Equal("setter failed", Assert.Single(Validation.GetErrors(box)).ErrorContent);
	});
}
