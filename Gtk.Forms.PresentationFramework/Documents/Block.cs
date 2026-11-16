using System.Windows.Media;

namespace System.Windows.Documents
{
	/// <summary>
	/// Base class for block-level flow-document elements.
	/// Carries optional layout properties that influence Pango markup rendering.
	/// </summary>
	public abstract class Block : TextElement
	{
		/// <summary>Outer margin. Default is zero on all sides.</summary>
		public Thickness Margin { get; set; }

		/// <summary>Inner padding. Default is zero on all sides.</summary>
		public Thickness Padding { get; set; }

		/// <summary>Text alignment within the block, or <c>null</c> to inherit.</summary>
		public TextAlignment? TextAlignment { get; set; }

		/// <summary>Line height in device-independent pixels, or <c>double.NaN</c> to inherit.</summary>
		public double LineHeight { get; set; } = double.NaN;

		/// <summary>
		/// Builds the plain-text representation of this block and appends it to
		/// <paramref name="sb"/>.  Separated by blank lines in a <see cref="FlowDocument"/>.
		/// </summary>
		internal abstract void AppendPlainText(System.Text.StringBuilder sb);

		/// <summary>
		/// Builds the Pango markup fragment for this block and appends it to
		/// <paramref name="sb"/>.
		/// </summary>
		internal abstract void AppendMarkup(System.Text.StringBuilder sb);
	}
}
