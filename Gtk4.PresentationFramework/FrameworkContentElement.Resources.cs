namespace System.Windows;

public partial class FrameworkContentElement
{
	ResourceDictionary NewResources() => new ResourceDictionary { Owner = this };

	void SetResources(ResourceDictionary? value)
	{
		_resources = value;
		if (value != null) value.Owner = this;
	}
}
