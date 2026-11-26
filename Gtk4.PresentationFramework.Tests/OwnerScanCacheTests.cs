using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>An owner no loaded type carries is looked up once, not on every evaluation.</summary>
public class OwnerScanCacheTests : WpfGtkTestBase
{
	[Fact]
	public void An_unknown_owner_scans_the_loaded_types_once() => Run(() =>
	{
		var source = new Border();
		var before = BindingEngine.OwnerScanCount;
		for (int i = 0; i < 5; i++)
			Assert.Null(BindingEngine.AttachedProperty(source, "(local:NoSuchOwnerForScanTest.Label)"));
		Assert.Equal(1, BindingEngine.OwnerScanCount - before);
	});
}
