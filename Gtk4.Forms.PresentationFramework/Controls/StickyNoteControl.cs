using System.Windows.Annotations;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace System.Windows.Controls;

/// <summary>
/// The note an <see cref="AnnotationService"/> shows for a sticky-note annotation. Created by the service only.
/// Gap: the default style and template are not built from WPF's StickyNote style; the note is a GTK popover
/// with the author and a text view (see StickyNoteControl.Gtk), and ink notes are not shown.
/// </summary>
public sealed partial class StickyNoteControl : Control
{
	/// <summary>The annotation type of text notes.</summary>
	public static readonly XmlQualifiedName TextSchemaName = StickyNoteData.TextType;

	/// <summary>The annotation type of ink notes.</summary>
	public static readonly XmlQualifiedName InkSchemaName = StickyNoteData.InkType;

	/// <summary>Deletes the note.</summary>
	public static readonly RoutedCommand DeleteNoteCommand = new RoutedCommand("DeleteNote", typeof(StickyNoteControl));

	/// <summary>Switches the ink editing mode; the parameter is the mode.</summary>
	public static readonly RoutedCommand InkCommand = new RoutedCommand("Ink", typeof(StickyNoteControl));

	static readonly DependencyPropertyKey _authorKey = DependencyProperty.RegisterReadOnly(
		nameof(Author), typeof(string), typeof(StickyNoteControl), new FrameworkPropertyMetadata(string.Empty));

	/// <summary>Identifies the Author property (read-only, empty by default).</summary>
	public static readonly DependencyProperty AuthorProperty = _authorKey.DependencyProperty;

	/// <summary>Identifies the IsExpanded property (default true).</summary>
	public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
		nameof(IsExpanded), typeof(bool), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(true, OnIsExpandedChanged));

	/// <summary>Identifies the IsActive property (attached, inherited, default false).</summary>
	public static readonly DependencyProperty IsActiveProperty = DependencyProperty.RegisterAttached(
		"IsActive", typeof(bool), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

	static readonly DependencyPropertyKey _isMouseOverAnchorKey = DependencyProperty.RegisterReadOnly(
		nameof(IsMouseOverAnchor), typeof(bool), typeof(StickyNoteControl), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the IsMouseOverAnchor property (read-only, default false).</summary>
	public static readonly DependencyProperty IsMouseOverAnchorProperty = _isMouseOverAnchorKey.DependencyProperty;

	/// <summary>Identifies the CaptionFontFamily property.</summary>
	public static readonly DependencyProperty CaptionFontFamilyProperty = DependencyProperty.Register(
		nameof(CaptionFontFamily), typeof(FontFamily), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(SystemFonts.MessageFontFamily, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the CaptionFontSize property.</summary>
	public static readonly DependencyProperty CaptionFontSizeProperty = DependencyProperty.Register(
		nameof(CaptionFontSize), typeof(double), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(SystemFonts.MessageFontSize, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the CaptionFontStretch property.</summary>
	public static readonly DependencyProperty CaptionFontStretchProperty = DependencyProperty.Register(
		nameof(CaptionFontStretch), typeof(FontStretch), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(FontStretches.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the CaptionFontStyle property.</summary>
	public static readonly DependencyProperty CaptionFontStyleProperty = DependencyProperty.Register(
		nameof(CaptionFontStyle), typeof(FontStyle), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(SystemFonts.MessageFontStyle, FrameworkPropertyMetadataOptions.AffectsMeasure));

	/// <summary>Identifies the CaptionFontWeight property.</summary>
	public static readonly DependencyProperty CaptionFontWeightProperty = DependencyProperty.Register(
		nameof(CaptionFontWeight), typeof(FontWeight), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(SystemFonts.MessageFontWeight, FrameworkPropertyMetadataOptions.AffectsMeasure));

	// DrawingAttributes.DefaultWidth, 0.53 mm at 96 dpi.
	const double DefaultPenWidth = 2.0031496062992127;

	/// <summary>Identifies the PenWidth property (default 2.0031496062992127, the ink pen width).</summary>
	public static readonly DependencyProperty PenWidthProperty = DependencyProperty.Register(
		nameof(PenWidth), typeof(double), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(DefaultPenWidth, FrameworkPropertyMetadataOptions.AffectsMeasure
			| FrameworkPropertyMetadataOptions.AffectsRender));

	static readonly DependencyPropertyKey _typeKey = DependencyProperty.RegisterReadOnly(
		nameof(StickyNoteType), typeof(StickyNoteType), typeof(StickyNoteControl),
		new FrameworkPropertyMetadata(StickyNoteType.Text));

	/// <summary>Identifies the StickyNoteType property (read-only, default Text).</summary>
	public static readonly DependencyProperty StickyNoteTypeProperty = _typeKey.DependencyProperty;

	/// <summary>The author of the note.</summary>
	public string Author => (string)GetValue(AuthorProperty)!;

	/// <summary>Whether the note shows its content or only its icon.</summary>
	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty)!;
		set => SetValue(IsExpandedProperty, value);
	}

	/// <summary>Whether the note is the active one.</summary>
	public bool IsActive => (bool)GetValue(IsActiveProperty)!;

	/// <summary>Whether the pointer is over the anchored text.</summary>
	public bool IsMouseOverAnchor => (bool)GetValue(IsMouseOverAnchorProperty)!;

	/// <summary>The caption font family.</summary>
	public FontFamily CaptionFontFamily
	{
		get => (FontFamily)GetValue(CaptionFontFamilyProperty)!;
		set => SetValue(CaptionFontFamilyProperty, value);
	}

	/// <summary>The caption font size.</summary>
	public double CaptionFontSize
	{
		get => (double)GetValue(CaptionFontSizeProperty)!;
		set => SetValue(CaptionFontSizeProperty, value);
	}

	/// <summary>The caption font stretch.</summary>
	public FontStretch CaptionFontStretch
	{
		get => (FontStretch)GetValue(CaptionFontStretchProperty)!;
		set => SetValue(CaptionFontStretchProperty, value);
	}

	/// <summary>The caption font style.</summary>
	public FontStyle CaptionFontStyle
	{
		get => (FontStyle)GetValue(CaptionFontStyleProperty)!;
		set => SetValue(CaptionFontStyleProperty, value);
	}

	/// <summary>The caption font weight.</summary>
	public FontWeight CaptionFontWeight
	{
		get => (FontWeight)GetValue(CaptionFontWeightProperty)!;
		set => SetValue(CaptionFontWeightProperty, value);
	}

	/// <summary>The ink pen width. Stored only.</summary>
	public double PenWidth
	{
		get => (double)GetValue(PenWidthProperty)!;
		set => SetValue(PenWidthProperty, value);
	}

	/// <summary>Text or Ink.</summary>
	public StickyNoteType StickyNoteType => (StickyNoteType)GetValue(StickyNoteTypeProperty)!;

	/// <summary>How the note's annotation resolved, or null before it is shown.</summary>
	public IAnchorInfo? AnchorInfo { get; private set; }

	internal StickyNoteControl(Annotation annotation, IAnchorInfo info)
	{
		_annotation = annotation;
		AnchorInfo = info;
		SetValue(_authorKey, annotation.Authors.Count > 0 ? annotation.Authors[0] : string.Empty);
		SetValue(_typeKey, annotation.AnnotationType.Equals(StickyNoteData.InkType)
			? StickyNoteType.Ink : StickyNoteType.Text);
	}

	readonly Annotation _annotation;

	static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((StickyNoteControl)d).SyncExpanded();
}
