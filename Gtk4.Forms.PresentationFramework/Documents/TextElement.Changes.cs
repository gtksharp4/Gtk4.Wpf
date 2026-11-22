namespace System.Windows.Documents;

public abstract partial class TextElement
{
	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (PaginationProperties.Affects(this, e.Property)) ContentParenting.Changed(this);
	}
}
