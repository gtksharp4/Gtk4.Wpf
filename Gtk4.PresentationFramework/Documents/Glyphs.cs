using System.Windows.Media;
using System.Windows.Xps.Packaging;

namespace System.Windows.Documents;

/// <summary>
/// A run of positioned glyphs, as XPS pages store text. Mirrors WPF <c>Glyphs</c>: the properties are stored
/// as given. Rendering draws <see cref="UnicodeString"/> with Pango at (<see cref="OriginX"/>, <see cref="OriginY"/>)
/// on the baseline.
/// </summary>
/// <remarks>
/// Gap: the glyph <see cref="Indices"/> (advance and offset adjustments) are stored but not applied, and the
/// package font (obfuscated TrueType) is not loaded into Pango. When the font's family name can be read from the
/// package, that family is requested from the system fonts; otherwise the default sans family is used.
/// </remarks>
public sealed partial class Glyphs : FrameworkElement
{
	/// <summary>Identifies the UnicodeString property.</summary>
	public static readonly DependencyProperty UnicodeStringProperty = Register(nameof(UnicodeString), typeof(string),
		string.Empty);

	/// <summary>Identifies the FontUri property.</summary>
	public static readonly DependencyProperty FontUriProperty = Register(nameof(FontUri), typeof(Uri), null);

	/// <summary>Identifies the FontRenderingEmSize property.</summary>
	public static readonly DependencyProperty FontRenderingEmSizeProperty = Register(nameof(FontRenderingEmSize),
		typeof(double), 1.0);

	/// <summary>Identifies the OriginX property.</summary>
	public static readonly DependencyProperty OriginXProperty = Register(nameof(OriginX), typeof(double), 0.0);

	/// <summary>Identifies the OriginY property.</summary>
	public static readonly DependencyProperty OriginYProperty = Register(nameof(OriginY), typeof(double), 0.0);

	/// <summary>Identifies the Fill property.</summary>
	public static readonly DependencyProperty FillProperty = Register(nameof(Fill), typeof(Brush), null);

	/// <summary>Identifies the Indices property.</summary>
	public static readonly DependencyProperty IndicesProperty = Register(nameof(Indices), typeof(string),
		string.Empty);

	/// <summary>Identifies the StyleSimulations property.</summary>
	public static readonly DependencyProperty StyleSimulationsProperty = Register(nameof(StyleSimulations),
		typeof(StyleSimulations), StyleSimulations.None);

	/// <summary>Identifies the BidiLevel property.</summary>
	public static readonly DependencyProperty BidiLevelProperty = Register(nameof(BidiLevel), typeof(int), 0);

	/// <summary>Identifies the IsSideways property.</summary>
	public static readonly DependencyProperty IsSidewaysProperty = Register(nameof(IsSideways), typeof(bool), false);

	static DependencyProperty Register(string name, Type type, object? defaultValue) =>
		DependencyProperty.Register(name, type, typeof(Glyphs), new PropertyMetadata(defaultValue, OnChanged));

	static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((Glyphs)d).Invalidate();

	/// <summary>The text of the run.</summary>
	public string UnicodeString
	{
		get => (string)GetValue(UnicodeStringProperty)!;
		set => SetValue(UnicodeStringProperty, value);
	}

	/// <summary>The font, as a URI (in an XPS package: an obfuscated font part).</summary>
	public Uri? FontUri
	{
		get => (Uri?)GetValue(FontUriProperty);
		set => SetValue(FontUriProperty, value);
	}

	/// <summary>The font size in device-independent units.</summary>
	public double FontRenderingEmSize
	{
		get => (double)GetValue(FontRenderingEmSizeProperty)!;
		set => SetValue(FontRenderingEmSizeProperty, value);
	}

	/// <summary>Horizontal origin of the first glyph.</summary>
	public double OriginX
	{
		get => (double)GetValue(OriginXProperty)!;
		set => SetValue(OriginXProperty, value);
	}

	/// <summary>Vertical origin (the baseline) of the first glyph.</summary>
	public double OriginY
	{
		get => (double)GetValue(OriginYProperty)!;
		set => SetValue(OriginYProperty, value);
	}

	/// <summary>The brush the glyphs are filled with.</summary>
	public Brush? Fill
	{
		get => (Brush?)GetValue(FillProperty);
		set => SetValue(FillProperty, value);
	}

	/// <summary>The glyph index and placement string (stored only).</summary>
	public string Indices
	{
		get => (string)GetValue(IndicesProperty)!;
		set => SetValue(IndicesProperty, value);
	}

	/// <summary>Bold and italic simulation (stored only).</summary>
	public StyleSimulations StyleSimulations
	{
		get => (StyleSimulations)GetValue(StyleSimulationsProperty)!;
		set => SetValue(StyleSimulationsProperty, value);
	}

	/// <summary>The bidirectional embedding level (stored only).</summary>
	public int BidiLevel
	{
		get => (int)GetValue(BidiLevelProperty)!;
		set => SetValue(BidiLevelProperty, value);
	}

	/// <summary>Whether the glyphs are rotated for vertical text (stored only).</summary>
	public bool IsSideways
	{
		get => (bool)GetValue(IsSidewaysProperty)!;
		set => SetValue(IsSidewaysProperty, value);
	}

	/// <summary>The package the font part lives in, set by the XPS reader.</summary>
	internal XpsPackage? Package { get; set; }
}
