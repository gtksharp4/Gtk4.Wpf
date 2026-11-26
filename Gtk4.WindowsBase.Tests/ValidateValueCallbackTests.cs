using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class ValidateValueCallbackTests
{
	class Owner : DependencyObject
	{
		public static int Changes;

		public static readonly DependencyProperty Positive = DependencyProperty.Register(
			"VvPositive", typeof(int), typeof(Owner), new PropertyMetadata(1, (_, _) => Changes++),
			v => (int)v! > 0);

		public static readonly DependencyProperty Coerced = DependencyProperty.Register(
			"VvCoerced", typeof(int), typeof(Owner),
			new PropertyMetadata(1, null, (_, v) => -1), v => (int)v! > 0);

		public static readonly DependencyProperty Text = DependencyProperty.Register(
			"VvText", typeof(string), typeof(Owner), new PropertyMetadata("a"));

		public static readonly DependencyProperty NullableInt = DependencyProperty.Register(
			"VvNullable", typeof(int?), typeof(Owner), new PropertyMetadata(null));
	}

	[Fact]
	public void Register_with_invalid_default_throws_with_wpf_message()
	{
		var ex = Assert.Throws<ArgumentException>(() => DependencyProperty.Register(
			"VvBadDefault", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(-1), v => (int)v! > 0));
		Assert.Contains("Default value for the 'VvBadDefault' property is not valid because ValidateValueCallback failed.",
			ex.Message);
	}

	[Fact]
	public void Failed_default_does_not_leave_a_registration()
	{
		Assert.ThrowsAny<ArgumentException>(() => DependencyProperty.Register(
			"VvRetry", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(-1), v => (int)v! > 0));
		DependencyProperty.Register("VvRetry", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(1));
	}

	[Fact]
	public void SetValue_rejected_by_callback_throws_and_stores_nothing()
	{
		var o = new Owner();
		var before = Owner.Changes;
		var ex = Assert.Throws<ArgumentException>(() => o.SetValue(Owner.Positive, -3));
		Assert.Equal("'-3' is not a valid value for property 'VvPositive'.", ex.Message);
		Assert.Equal(1, o.GetValue(Owner.Positive));
		Assert.Equal(DependencyProperty.UnsetValue, o.ReadLocalValue(Owner.Positive));
		Assert.Equal(before, Owner.Changes);
	}

	[Fact]
	public void SetValue_wrong_type_throws_and_raises_no_change()
	{
		var o = new Owner();
		var before = Owner.Changes;
		Assert.Throws<ArgumentException>(() => o.SetValue(Owner.Positive, "x"));
		Assert.Equal(DependencyProperty.UnsetValue, o.ReadLocalValue(Owner.Positive));
		Assert.Equal(before, Owner.Changes);
	}

	[Fact]
	public void SetValue_valid_value_is_stored()
	{
		var o = new Owner();
		o.SetValue(Owner.Positive, 5);
		Assert.Equal(5, o.GetValue(Owner.Positive));
	}

	[Fact]
	public void Coerced_result_is_not_revalidated()
	{
		var o = new Owner();
		o.SetValue(Owner.Coerced, 4);
		Assert.Equal(-1, o.GetValue(Owner.Coerced));
	}

	[Fact]
	public void IsValidType_handles_null_for_reference_value_and_nullable_types()
	{
		Assert.True(Owner.Text.IsValidType(null));
		Assert.False(Owner.Positive.IsValidType(null));
		Assert.True(Owner.NullableInt.IsValidType(null));
		Assert.True(Owner.NullableInt.IsValidType(3));
		Assert.False(Owner.Text.IsValidType(3));
	}

	[Fact]
	public void IsValidValue_combines_type_and_callback()
	{
		Assert.True(Owner.Positive.IsValidValue(2));
		Assert.False(Owner.Positive.IsValidValue(0));
		Assert.False(Owner.Positive.IsValidValue("2"));
		Assert.NotNull(Owner.Positive.ValidateValueCallback);
		Assert.Null(Owner.Text.ValidateValueCallback);
	}

	[Fact]
	public void RegisterAttached_and_ReadOnly_overloads_accept_validators()
	{
		var attached = DependencyProperty.RegisterAttached(
			"VvAtt", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(1), v => (int)v! > 0);
		var key = DependencyProperty.RegisterReadOnly(
			"VvRo", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(1), v => (int)v! > 0);
		var attRo = DependencyProperty.RegisterAttachedReadOnly(
			"VvAttRo", typeof(int), typeof(ValidateValueCallbackTests), new PropertyMetadata(1), v => (int)v! > 0);
		var o = new Owner();
		Assert.Throws<ArgumentException>(() => o.SetValue(attached, 0));
		Assert.Throws<ArgumentException>(() => o.SetValue(key, 0));
		Assert.Throws<ArgumentException>(() => o.SetValue(attRo, 0));
		Assert.True(key.DependencyProperty.IsReadOnly && attRo.DependencyProperty.IsReadOnly);
	}
}
