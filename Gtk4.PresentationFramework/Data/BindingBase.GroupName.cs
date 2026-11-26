namespace System.Windows.Data;

public abstract partial class BindingBase
{
	string _bindingGroupName = string.Empty;

	/// <summary>
	/// Gets or sets the name of the <see cref="BindingGroup"/> this binding joins. Empty (the default) joins the
	/// nearest group above the target.
	/// </summary>
	public string BindingGroupName
	{
		get => _bindingGroupName;
		set => _bindingGroupName = value ?? string.Empty;
	}
}
