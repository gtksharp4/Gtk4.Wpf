using Gtk4.Forms.ProjectConverter;
using Xunit;

namespace Gtk4.Forms.ProjectConverter.Tests;

/// <summary>Defect #1: PackageReference stripping is attribute-order-independent.</summary>
public class CsprojConverterPackageRefAttrOrderTests
{
	// ── Include-not-first, self-closed ────────────────────────────────────────

	[Fact]
	public void SelfClosed_VersionBeforeInclude_IsStripped()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Version="1.0.0" Include="Gtk4.Forms" />
			    <PackageReference Include="SomeOtherLib" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms\"", result);
		Assert.Contains("SomeOtherLib", result);
	}

	[Fact]
	public void SelfClosed_MapsVersionBeforeInclude_IsStripped()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Version="1.0.0" Include="Gtk4.Forms.Maps" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms.Maps", result);
	}

	// ── Include-not-first, bodied ──────────────────────────────────────────────

	[Fact]
	public void Bodied_VersionBeforeInclude_IsStripped()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Version="1.0.0" Include="Gtk4.Forms">
			      <PrivateAssets>all</PrivateAssets>
			    </PackageReference>
			    <PackageReference Include="SomeOtherLib" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms\"", result);
		Assert.Contains("SomeOtherLib", result);
	}

	// ── Safety boundary: Gtk4.Forms.FooBar must NOT be stripped ────────────

	[Fact]
	public void XamarinFormsFooBar_IsNotStripped()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Include="Gtk4.Forms.FooBar" Version="1.0.0" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		// Only change is from OutputType/StartupObject insertion, not PackageReference removal.
		Assert.Contains("Gtk4.Forms.FooBar", result);
	}

	[Fact]
	public void XamarinFormsPlatformRef_AnyAttrOrder_IsStripped()
	{
		var input =
			"""
			<Project>
			  <ItemGroup>
			    <PackageReference Version="1.0.0" Include="Gtk4.Forms.Platform.Android" />
			  </ItemGroup>
			</Project>
			""";

		var result = CsprojConverter.Transform(input, out var changed);

		Assert.True(changed);
		Assert.DoesNotContain("Gtk4.Forms.Platform.Android", result);
	}
}
