using System.Windows.Controls;

namespace System.Windows.Documents;

public partial class Run
{
	/// <summary>Identifies the Text property (default empty; null is coerced to empty).</summary>
	public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
		nameof(Text), typeof(string), typeof(Run),
		new FrameworkPropertyMetadata(
			string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged, CoerceText));

	/// <summary>Creates an empty run.</summary>
	public Run() { }

	/// <summary>Creates a run holding <paramref name="text"/>.</summary>
	public Run(string text) => Text = text;

	/// <summary>Creates a run holding <paramref name="text"/> at <paramref name="insertionPosition"/>.</summary>
	/// <remarks>
	/// Inside a Run the run is split; inside a Paragraph or Span the run is inserted there; between blocks, or in an
	/// empty document, Section or ListItem, a Paragraph is created to hold it.
	/// </remarks>
	/// <exception cref="InvalidOperationException">The position is in a BlockUIContainer or InlineUIContainer.</exception>
	public Run(string text, TextPointer insertionPosition)
	{
		Text = text;
		if (insertionPosition == null) return;
		TextTreeEdit.InsertInline(insertionPosition.Document, insertionPosition.Offset, this);
	}

	/// <summary>Gets or sets the text of the run (backed by <see cref="TextProperty"/>).</summary>
	public string Text
	{
		get => (string)GetValue(TextProperty)!;
		set => SetValue(TextProperty, value);
	}

	static object? CoerceText(DependencyObject d, object? baseValue) => baseValue ?? string.Empty;

	static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Run)d).NotifyHost();

	/// <summary>Re-renders the hosting TextBlock or flow document; the text tree is rebuilt on demand.</summary>
	void NotifyHost()
	{
		DependencyObject? current = Parent;
		for (int steps = 0; current != null && steps < 1000; steps++)
		{
			if (current is TextBlock tb)
			{
				tb.OnInlineContentChanged();
				return;
			}
			if (current is FlowDocument doc)
			{
				doc.NotifyContentEdited();
				return;
			}
			current = (current as FrameworkContentElement)?.Parent;
		}
	}
}
