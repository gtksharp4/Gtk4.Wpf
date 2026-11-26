using System;
using System.IO;
using Xunit;

namespace Gtk4.Wpf.Testing.Tests.Driver;

public class EvidenceTests : WpfUiWindowTest<FormWindow>
{
	static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

	[WpfUiFact]
	public void Screenshot_of_the_window_is_a_PNG()
	{
		byte[] png = App.Screenshot();
		Assert.True(png.Length > PngSignature.Length);
		Assert.Equal(PngSignature, png[..PngSignature.Length]);
	}

	[WpfUiFact]
	public void Screenshot_of_an_element_is_smaller_than_the_window()
	{
		byte[] window = App.Screenshot();
		byte[] button = App.Screenshot(App.Find(By.AutomationId("Login")));
		Assert.Equal(PngSignature, button[..PngSignature.Length]);
		Assert.True(ReadWidth(button) < ReadWidth(window));
	}

	[WpfUiFact]
	public void SaveEvidence_writes_the_tree_and_a_PNG()
	{
		string directory = Path.Combine(Path.GetTempPath(), "wpf-ui-evidence-" + Guid.NewGuid().ToString("N"));
		string previous = WpfAppHost.EvidenceDirectory;
		WpfAppHost.EvidenceDirectory = directory;
		try
		{
			string[] files = App.SaveEvidence("a/b: test");
			Assert.Equal(2, files.Length);
			Assert.All(files, f => Assert.Equal(directory, Path.GetDirectoryName(f)));
			Assert.Contains("Window name=\"Form\"", File.ReadAllText(files[0]));
			Assert.Equal(PngSignature, File.ReadAllBytes(files[1])[..PngSignature.Length]);
		}
		finally
		{
			WpfAppHost.EvidenceDirectory = previous;
			if (Directory.Exists(directory)) Directory.Delete(directory, true);
		}
	}

	// IHDR width: bytes 16..19 of a PNG, big endian.
	static int ReadWidth(byte[] png) => (png[16] << 24) | (png[17] << 16) | (png[18] << 8) | png[19];
}
