using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>
/// Keeps one element registered under the access key of its current string (or none): the stand-in for WPF's
/// <c>AccessText</c> registering itself, for controls that draw their string without an AccessText.
/// </summary>
static class AccessKeyBinding
{
	static readonly ConditionalWeakTable<UIElement, string?[]> _current = new();

	/// <summary>
	/// Re-registers <paramref name="owner"/> for the key in <paramref name="text"/> (unregistering the previous
	/// one) and returns the GTK <c>UseUnderline</c> text for it. A null text clears the key.
	/// </summary>
	internal static string Apply(UIElement owner, string? text)
	{
		var parsed = AccessKeyParser.Parse(text);
		Register(owner, parsed.AccessKey?.ToUpperInvariant());
		return AccessKeyParser.ToGtkMnemonic(text);
	}

	/// <summary>
	/// Shows GTK mnemonic text on a label. <c>Text</c> resets UseUnderline, so it is set again after.
	/// </summary>
	internal static void SetLabel(Gtk.Label label, string mnemonic)
	{
		label.Text = mnemonic;
		label.UseUnderline = true;
	}

	internal static void Register(UIElement owner, string? key)
	{
		var slot = _current.GetValue(owner, _ => new string?[1]);
		if (slot[0] == key) return;
		if (slot[0] != null) AccessKeyManager.Unregister(slot[0]!, owner);
		slot[0] = key;
		if (key != null) AccessKeyManager.Register(key, owner);
	}
}
