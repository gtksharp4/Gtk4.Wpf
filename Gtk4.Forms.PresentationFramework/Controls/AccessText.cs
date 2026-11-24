using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Shows text in which the first single '_' marks the access key (WPF's <c>AccessText</c>). It registers itself
/// with <see cref="AccessKeyManager"/> for that key and shows the text through a <see cref="Gtk.Label"/> with
/// <c>UseUnderline</c>; GTK underlines the key only while Alt is held, like WPF's keyboard cues.
/// </summary>
[ContentProperty("Text")]
public partial class AccessText : FrameworkElement, IAddChild
{
	/// <summary>Identifies the <see cref="Text"/> dependency property.</summary>
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		nameof(Text), typeof(string), typeof(AccessText),
		new FrameworkPropertyMetadata(string.Empty,
			FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
			OnTextChanged));

	static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		=> ((AccessText)d).ApplyText();

	Gtk.Label? _label;
	char _accessKey;

	/// <summary>Gets or sets the text; the first single '_' marks the access key, '__' is a literal '_'.</summary>
	[DefaultValue("")]
	public string Text
	{
		get => (string?)GetValue(TextProperty) ?? string.Empty;
		set => SetValue(TextProperty, value);
	}

	/// <summary>Gets the access key character, or the NUL character when the text has none.</summary>
	public char AccessKey => _accessKey;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_label = new Gtk.Label(string.Empty) { Xalign = 0f };
		ApplyText();
		ApplyLayout();
		return _label;
	}

	// Registers the new key and unregisters the old one, as WPF's RegisterAccessKey does on every change.
	void ApplyText()
	{
		var text = Text;
		var parsed = AccessKeyParser.Parse(text);
		_accessKey = parsed.AccessKey == null ? '\0' : parsed.AccessKey[0];
		AccessKeyBinding.Register(this, parsed.AccessKey?.ToUpperInvariant());
		if (_label != null)
			AccessKeyBinding.SetLabel(_label, AccessKeyParser.ToGtkMnemonic(text));
	}

	void IAddChild.AddChild(object value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		throw new ArgumentException("AccessText accepts only text content.", nameof(value));
	}

	void IAddChild.AddText(string text) => Text += text;
}
