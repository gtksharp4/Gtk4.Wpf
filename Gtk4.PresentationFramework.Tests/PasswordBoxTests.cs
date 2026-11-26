using System;
using System.Security;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for <see cref="PasswordBox"/>.
/// </summary>
public class PasswordBoxTests
{
	// ── PasswordChanged ───────────────────────────────────────────────────────

	[Fact]
	public void PasswordChanged_fires_on_Password_set()
	{
		var pb = new PasswordBox();
		bool fired = false;
		pb.PasswordChanged += (s, e) => fired = true;
		pb.Password = "secret";
		Assert.True(fired);
	}

	[Fact]
	public void PasswordChanged_fires_exactly_once_per_change()
	{
		var pb = new PasswordBox();
		int count = 0;
		pb.PasswordChanged += (s, e) => count++;
		pb.Password = "abc";
		Assert.Equal(1, count);
	}

	[Fact]
	public void PasswordChanged_not_fired_on_no_op_set()
	{
		var pb = new PasswordBox { Password = "same" };
		int count = 0;
		pb.PasswordChanged += (s, e) => count++;
		pb.Password = "same"; // no-op
		Assert.Equal(0, count);
	}

	[Fact]
	public void PasswordChanged_sender_is_passwordbox()
	{
		var pb = new PasswordBox();
		object? sender = null;
		pb.PasswordChanged += (s, e) => sender = s;
		pb.Password = "x";
		Assert.Same(pb, sender);
	}

	[Fact]
	public void PasswordChanged_args_is_RoutedEventArgs()
	{
		var pb = new PasswordBox();
		System.Windows.RoutedEventArgs? args = null;
		pb.PasswordChanged += (s, e) => args = e;
		pb.Password = "x";
		Assert.IsType<System.Windows.RoutedEventArgs>(args);
	}

	// ── MaxLength ─────────────────────────────────────────────────────────────

	[Fact]
	public void MaxLength_default_is_zero()
	{
		var pb = new PasswordBox();
		Assert.Equal(0, pb.MaxLength);
	}

	[Fact]
	public void MaxLength_roundtrip()
	{
		var pb = new PasswordBox { MaxLength = 8 };
		Assert.Equal(8, pb.MaxLength);
	}

	[Fact]
	public void MaxLength_clamps_Password_on_set()
	{
		var pb = new PasswordBox { MaxLength = 5 };
		pb.Password = "abcdefgh";
		Assert.Equal("abcde", pb.Password);
	}

	[Fact]
	public void MaxLength_clamps_Password_on_MaxLength_shrink()
	{
		var pb = new PasswordBox { Password = "abcdefgh" };
		pb.MaxLength = 5;
		Assert.Equal("abcde", pb.Password);
	}

	[Fact]
	public void MaxLength_zero_means_unlimited()
	{
		var pb = new PasswordBox { MaxLength = 0 };
		pb.Password = "averylongpasswordstring";
		Assert.Equal("averylongpasswordstring", pb.Password);
	}

	[Fact]
	public void MaxLength_clamp_raises_PasswordChanged()
	{
		var pb = new PasswordBox { MaxLength = 3 };
		bool fired = false;
		pb.PasswordChanged += (s, e) => fired = true;
		pb.Password = "abcdef";
		Assert.True(fired);
		Assert.Equal("abc", pb.Password);
	}

	// ── Clear ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Clear_empties_password()
	{
		var pb = new PasswordBox { Password = "secret" };
		pb.Clear();
		Assert.Equal(string.Empty, pb.Password);
	}

	[Fact]
	public void Clear_raises_PasswordChanged()
	{
		var pb = new PasswordBox { Password = "secret" };
		bool fired = false;
		pb.PasswordChanged += (s, e) => fired = true;
		pb.Clear();
		Assert.True(fired);
	}

	[Fact]
	public void Clear_on_already_empty_does_not_raise_PasswordChanged()
	{
		var pb = new PasswordBox();
		int count = 0;
		pb.PasswordChanged += (s, e) => count++;
		pb.Clear();
		Assert.Equal(0, count);
	}

	// ── Before-realize deferral ───────────────────────────────────────────────

	[Fact]
	public void Password_set_before_realize_does_not_throw()
	{
		var ex = Record.Exception(() =>
		{
			var pb = new PasswordBox { Password = "test", MaxLength = 10 };
			Assert.Equal("test", pb.Password);
		});
		Assert.Null(ex);
	}

	[Fact]
	public void MaxLength_set_before_realize_persists()
	{
		var pb = new PasswordBox { MaxLength = 6 };
		Assert.Equal(6, pb.MaxLength);
	}
}
