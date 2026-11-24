namespace System.Windows.Controls;

public partial class ContentControl
{
	/// <summary>
	/// Shows string content in the GTK label. Underscores stay literal; <see cref="Label"/> overrides this to
	/// process access keys, as WPF's default templates that set <c>RecognizesAccessKey</c> do.
	/// </summary>
	internal virtual void ShowContentText(string text) => ContentLabel.Text = text;
}
