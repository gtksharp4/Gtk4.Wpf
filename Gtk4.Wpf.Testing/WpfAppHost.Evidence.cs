using System;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;

namespace Gtk4.Wpf.Testing;

public sealed partial class WpfAppHost
{
	/// <summary>Where <see cref="SaveEvidence"/> writes. <c>WPF_UI_EVIDENCE_DIR</c> overrides the default.</summary>
	public static string EvidenceDirectory { get; set; } =
		Environment.GetEnvironmentVariable("WPF_UI_EVIDENCE_DIR")
		?? Path.Combine(AppContext.BaseDirectory, "ui-evidence");

	/// <summary>PNG bytes of <paramref name="element"/> (the main window by default), rendered by GTK.</summary>
	public byte[] Screenshot(AutomationElement? element = null) => GtkThread.Invoke(() =>
	{
		UIElement target = element == null ? Window
			: element.Peer is UIElementAutomationPeer peer ? peer.Owner
			: throw new ArgumentException("The element is not backed by a WPF UIElement.", nameof(element));
		return GtkSnapshot.ToPng(target.Widget);
	});

	/// <summary>Saves <see cref="Screenshot"/> as <c>{name}.png</c> in <see cref="EvidenceDirectory"/>.</summary>
	public string SaveScreenshot(string name, AutomationElement? element = null)
	{
		string path = EvidencePath(name, ".png");
		File.WriteAllBytes(path, Screenshot(element));
		return path;
	}

	/// <summary>Writes the automation tree dump and a PNG of the main window; returns the file paths.</summary>
	public string[] SaveEvidence(string name)
	{
		string tree = EvidencePath(name, ".tree.txt");
		File.WriteAllText(tree, DumpTree());
		try { return new[] { tree, SaveScreenshot(name) }; }
		catch (Exception e) when (e is InvalidOperationException || e is DllNotFoundException
			|| e is EntryPointNotFoundException)
		{
			// A window that is gone or a GTK build without the snapshot API still leaves the tree behind.
			File.WriteAllText(EvidencePath(name, ".screenshot-error.txt"), e.Message);
			return new[] { tree };
		}
	}

	static string EvidencePath(string name, string extension)
	{
		Directory.CreateDirectory(EvidenceDirectory);
		foreach (char bad in Path.GetInvalidFileNameChars())
			name = name.Replace(bad, '_');
		return Path.Combine(EvidenceDirectory, name + extension);
	}
}
