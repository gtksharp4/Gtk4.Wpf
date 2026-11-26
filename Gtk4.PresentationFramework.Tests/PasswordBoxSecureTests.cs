using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="PasswordBox.SecurePassword"/> and <see cref="PasswordBox.PasswordChar"/>.
/// </summary>
public class PasswordBoxSecureTests
{
	[Fact]
	public void SecurePassword_length_matches_Password()
	{
		var pb = new PasswordBox { Password = "hello" };
		using var ss = pb.SecurePassword;
		Assert.Equal(5, ss.Length);
	}

	[Fact]
	public void SecurePassword_on_empty_password_is_length_zero()
	{
		var pb = new PasswordBox();
		using var ss = pb.SecurePassword;
		Assert.Equal(0, ss.Length);
	}

	[Fact]
	public void SecurePassword_is_readonly()
	{
		var pb = new PasswordBox { Password = "abc" };
		using var ss = pb.SecurePassword;
		Assert.True(ss.IsReadOnly());
	}

	[Fact]
	public void SecurePassword_returns_new_instance_each_call()
	{
		var pb = new PasswordBox { Password = "abc" };
		using var ss1 = pb.SecurePassword;
		using var ss2 = pb.SecurePassword;
		Assert.NotSame(ss1, ss2);
	}

	[Fact]
	public void PasswordChar_default_is_bullet()
	{
		var pb = new PasswordBox();
		Assert.Equal('●', pb.PasswordChar);
	}

	[Fact]
	public void PasswordChar_roundtrip()
	{
		var pb = new PasswordBox { PasswordChar = '*' };
		Assert.Equal('*', pb.PasswordChar);
	}
}
