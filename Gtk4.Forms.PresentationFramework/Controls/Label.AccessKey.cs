namespace System.Windows.Controls;

public partial class Label
{
	// WPF's Label template recognizes access keys; the Label registers itself and Target receives the key.
	internal override void ShowContentText(string text)
	{
		AccessKeyBinding.SetLabel(ContentLabel, AccessKeyParser.ToGtkMnemonic(text));
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty)
			AccessKeyBinding.Apply(this, e.NewValue as string);
	}
}
