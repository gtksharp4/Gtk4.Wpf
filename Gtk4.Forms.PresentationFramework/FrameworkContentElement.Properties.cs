using System.Windows.Input;
using System.Windows.Markup;

namespace System.Windows;

public partial class FrameworkContentElement
{
	/// <summary>
	/// Identifies the Name property (default empty). WPF shares one property with <c>FrameworkElement.NameProperty</c>
	/// through <c>AddOwner</c>; <c>FrameworkElement</c> has none yet, so this one is registered here.
	/// </summary>
	public static readonly DependencyProperty NameProperty = DependencyProperty.Register(
		nameof(Name), typeof(string), typeof(FrameworkContentElement), new PropertyMetadata(string.Empty), IsValidName);

	/// <summary>Identifies the DataContext property (inherits from the logical parent).</summary>
	public static readonly DependencyProperty DataContextProperty = FrameworkElement.DataContextProperty.AddOwner(
		typeof(FrameworkContentElement),
		new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the Tag property.</summary>
	public static readonly DependencyProperty TagProperty = FrameworkElement.TagProperty.AddOwner(
		typeof(FrameworkContentElement), new FrameworkPropertyMetadata((object?)null));

	/// <summary>Identifies the Language property (default en-US; inherits).</summary>
	public static readonly DependencyProperty LanguageProperty = FrameworkElement.LanguageProperty.AddOwner(
		typeof(FrameworkContentElement),
		new FrameworkPropertyMetadata(
			XmlLanguage.GetLanguage("en-US"),
			FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Inherits));

	/// <summary>Identifies the ToolTip property.</summary>
	public static readonly DependencyProperty ToolTipProperty = FrameworkElement.ToolTipProperty.AddOwner(
		typeof(FrameworkContentElement), new PropertyMetadata(null));

	/// <summary>Identifies the Cursor property.</summary>
	public static readonly DependencyProperty CursorProperty = FrameworkElement.CursorProperty.AddOwner(
		typeof(FrameworkContentElement), new PropertyMetadata(null));

	/// <summary>Identifies the ForceCursor property (default false).</summary>
	public static readonly DependencyProperty ForceCursorProperty = DependencyProperty.Register(
		nameof(ForceCursor), typeof(bool), typeof(FrameworkContentElement), new PropertyMetadata(false));

	/// <summary>Identifies the Style property.</summary>
	public static readonly DependencyProperty StyleProperty = FrameworkElement.StyleProperty.AddOwner(
		typeof(FrameworkContentElement), new PropertyMetadata(null, OnStyleChanged));

	/// <summary>Gets or sets the data context; inherited from the logical parent when not set.</summary>
	public object? DataContext
	{
		get => GetValue(DataContextProperty);
		set => SetValue(DataContextProperty, value);
	}

	/// <summary>Gets or sets an arbitrary object tied to this element.</summary>
	public object? Tag
	{
		get => GetValue(TagProperty);
		set => SetValue(TagProperty, value);
	}

	/// <summary>Gets or sets the language. The language reaches Pango as <c>lang</c> when set locally.</summary>
	public XmlLanguage Language
	{
		get => (XmlLanguage)GetValue(LanguageProperty)!;
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			SetValue(LanguageProperty, value);
		}
	}

	/// <summary>Gets or sets the tool tip (stored; a flow document is a single label).</summary>
	public object? ToolTip
	{
		get => GetValue(ToolTipProperty);
		set => SetValue(ToolTipProperty, value);
	}

	/// <summary>Gets or sets the cursor (stored).</summary>
	public Cursor? Cursor
	{
		get => GetValue(CursorProperty) as Cursor;
		set => SetValue(CursorProperty, value);
	}

	/// <summary>Gets or sets whether the cursor overrides child cursors (stored).</summary>
	public bool ForceCursor
	{
		get => (bool)GetValue(ForceCursorProperty)!;
		set => SetValue(ForceCursorProperty, value);
	}

	/// <summary>Gets or sets the style; its setters apply through the style layer.</summary>
	public Style? Style
	{
		get => (Style?)GetValue(StyleProperty);
		set => SetValue(StyleProperty, value);
	}

	/// <summary>True when <see cref="Language"/> was set locally.</summary>
	internal bool HasLocalLanguage => !ReferenceEquals(ReadLocalValue(LanguageProperty), DependencyProperty.UnsetValue);

	static void OnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue is Style oldStyle)
		{
			oldStyle.Unapply(d);
			d.ClearStyleValues(d.SnapshotStyleValues());
		}
		if (e.NewValue is Style newStyle) newStyle.Apply(d);
	}
}
