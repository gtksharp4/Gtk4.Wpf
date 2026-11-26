using System;
using System.IO;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ApplicationResourceStreamTests : IDisposable
{
	readonly string _file = Path.Combine(AppContext.BaseDirectory, "resource-stream-probe.txt");

	public ApplicationResourceStreamTests() => File.WriteAllText(_file, "file body");

	public void Dispose() => File.Delete(_file);

	static string Read(System.Windows.Resources.StreamResourceInfo info)
	{
		using var reader = new StreamReader(info.Stream);
		return reader.ReadToEnd();
	}

	[Fact]
	public void Null_uri_throws() =>
		Assert.Throws<ArgumentNullException>(() => Application.GetResourceStream(null!));

	[Theory]
	[InlineData("http://example.com/a.txt")]
	[InlineData("pack://siteoforigin/a.txt")]
	[InlineData("file:///c:/a.txt")]
	public void Absolute_non_application_uri_throws(string uri) =>
		Assert.Throws<ArgumentException>(() => Application.GetResourceStream(new Uri(uri)));

	[Theory]
	[InlineData("resource-stream-probe.txt")]
	[InlineData("/resource-stream-probe.txt")]
	[InlineData("pack://application/resource-stream-probe.txt")]
	[InlineData("pack://application/Gtk4.PresentationFramework.Tests;component/resource-stream-probe.txt")]
	public void Files_under_the_app_base_resolve(string uri)
	{
		var info = Application.GetResourceStream(new Uri(uri, UriKind.RelativeOrAbsolute))!;
		Assert.True(info.Stream.CanSeek);
		Assert.Equal("text/plain", info.ContentType);
		Assert.Equal("file body", Read(info));
	}

	[Fact]
	public void Manifest_resources_resolve_by_dotted_suffix()
	{
		var info = Application.GetResourceStream(new Uri("ResourceStreamData/embedded-sample.txt", UriKind.Relative));
		Assert.Equal("embedded body", Read(info!));
	}

	[Fact]
	public void Missing_resource_throws_IOException_like_WPF()
	{
		var ex = Assert.Throws<IOException>(
			() => Application.GetResourceStream(new Uri("no/such-resource.txt", UriKind.Relative)));
		Assert.Contains("Cannot locate resource", ex.Message);
		ex = Assert.Throws<IOException>(
			() => Application.GetResourceStream(new Uri("pack://application/no/such.txt")));
		Assert.Contains("Cannot locate resource", ex.Message);
	}

	[Fact]
	public void StreamResourceInfo_exposes_its_constructor_arguments()
	{
		var stream = new MemoryStream();
		var info = new System.Windows.Resources.StreamResourceInfo(stream, "image/png");
		Assert.Same(stream, info.Stream);
		Assert.Equal("image/png", info.ContentType);
	}
}
