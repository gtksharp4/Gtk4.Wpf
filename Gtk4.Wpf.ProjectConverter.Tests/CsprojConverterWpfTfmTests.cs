using Gtk4.Wpf.ProjectConverter;
using Xunit;

namespace Gtk4.Wpf.ProjectConverter.Tests;

/// <summary>Transform rules: TFM normalisation for WPF projects.</summary>
public class CsprojConverterWpfTfmTests
{
	const string Prefix = "<Project><PropertyGroup>";
	const string Suffix = "</PropertyGroup></Project>";

	static string Wrap(string inner) => $"{Prefix}{inner}{Suffix}";

	[Fact]
	public void StripWindowsSuffix()
	{
		var input = Wrap("<TargetFramework>net10.0-windows</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void StripWindowsVersionedSuffix()
	{
		var input = Wrap("<TargetFramework>net8.0-windows10.0.19041.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void Netstandard20BecomesNet10()
	{
		var input = Wrap("<TargetFramework>netstandard2.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void AlreadyNet10TfmIsNoOp_WhenFullyConverted()
	{
		var input = Wrap(
			"<TargetFramework>net10.0</TargetFramework>" +
			"<OutputType>Exe</OutputType>");

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}
}
