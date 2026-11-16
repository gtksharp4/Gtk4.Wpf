using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Transform rules: TargetFramework(s) normalisation.</summary>
public class CsprojConverterTfmTests
{
	const string Prefix = "<Project><PropertyGroup>";
	const string Suffix = "</PropertyGroup></Project>";

	static string Wrap(string inner) => $"{Prefix}{inner}{Suffix}";

	// ── strip platform suffix ─────────────────────────────────────────────────

	[Fact]
	public void StripAndroidSuffix()
	{
		var input = Wrap("<TargetFramework>net8.0-android</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
	}

	[Fact]
	public void StripIosSuffix()
	{
		var input = Wrap("<TargetFramework>net8.0-ios</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
	}

	[Fact]
	public void StripMaccatalystSuffix()
	{
		var input = Wrap("<TargetFramework>net8.0-maccatalyst</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
	}

	[Fact]
	public void StripTvosSuffix()
	{
		var input = Wrap("<TargetFramework>net8.0-tvos</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
	}

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
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
	}

	// ── legacy moniker replacement ─────────────────────────────────────────────

	[Fact]
	public void Netstandard20BecomesNet10()
	{
		var input = Wrap("<TargetFramework>netstandard2.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void Netstandard21BecomesNet10()
	{
		var input = Wrap("<TargetFramework>netstandard2.1</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void Uap10BecomesNet10()
	{
		var input = Wrap("<TargetFramework>uap10.0.18362</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void MonoandroidBecomesNet10()
	{
		var input = Wrap("<TargetFramework>monoandroid12.0</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void XamarinIosBecomesNet10()
	{
		var input = Wrap("<TargetFramework>xamarin.ios10</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	[Fact]
	public void TizenBecomesNet10()
	{
		var input = Wrap("<TargetFramework>tizen60</TargetFramework>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
	}

	// ── multi-TFM deduplication ────────────────────────────────────────────────

	[Fact]
	public void SameBaseCollapsesDedupe()
	{
		// net8.0-android;net8.0-ios → both strip to net8.0 → dedupe → singular <TargetFramework>net8.0
		var input = Wrap("<TargetFrameworks>net8.0-android;net8.0-ios</TargetFrameworks>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
		Assert.DoesNotContain("<TargetFrameworks>", result);
	}

	[Fact]
	public void DifferentBasesCollapseToNet10()
	{
		// net8.0-android;net9.0-ios → net8.0 + net9.0 → >1 entry → collapse to net10.0
		var input = Wrap("<TargetFrameworks>net8.0-android;net9.0-ios</TargetFrameworks>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net10.0</TargetFramework>", result);
		Assert.DoesNotContain("<TargetFrameworks>", result);
	}

	[Fact]
	public void TargetFrameworksPluralBecomeSingular()
	{
		var input = Wrap("<TargetFrameworks>net8.0-android</TargetFrameworks>");
		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.Contains("<TargetFramework>net8.0</TargetFramework>", result);
		Assert.DoesNotContain("<TargetFrameworks>", result);
	}

	// ── idempotent ─────────────────────────────────────────────────────────────

	[Fact]
	public void AlreadyNet10TfmIsNoOp_WhenFullyConverted()
	{
		// A fully-converted project: net10.0, Exe, StartupObject — TFM portion must not change.
		var input = Wrap(
			"<TargetFramework>net10.0</TargetFramework>" +
			"<OutputType>Exe</OutputType>" +
			"<StartupObject>Gtk4.Forms.Host.GtkEntryPoint</StartupObject>");

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.False(changed);
		Assert.Equal(input, result);
	}
}
