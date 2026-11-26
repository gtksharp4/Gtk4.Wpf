using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class BindingGroupOwnerOverloadTests
{
	sealed class OwnerAwareRule : ValidationRule
	{
		public BindingGroup? Owner;

		public override ValidationResult Validate(object? value, CultureInfo cultureInfo) =>
			ValidationResult.ValidResult;

		public override ValidationResult Validate(object? value, CultureInfo cultureInfo, BindingGroup owner)
		{
			Owner = owner;
			return new ValidationResult(false, "owner overload");
		}
	}

	[Fact]
	public void ValidateWithoutUpdate_calls_the_group_owner_overload()
	{
		var group = new BindingGroup();
		var rule = new OwnerAwareRule();
		group.ValidationRules.Add(rule);
		Assert.False(group.ValidateWithoutUpdate());
		Assert.Same(group, rule.Owner);
	}
}
