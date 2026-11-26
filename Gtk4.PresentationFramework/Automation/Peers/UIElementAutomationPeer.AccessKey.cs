using System.Reflection;
using System.Windows.Input;

namespace System.Windows.Automation.Peers;

public partial class UIElementAutomationPeer
{
	// AutomationProperties.AccessKey belongs to another lane; until that attached property exists the owner has
	// no explicit key. Looked up once so the override honours it as soon as the property is added.
	static readonly MethodInfo? _getExplicitAccessKey =
		typeof(AutomationProperties).GetMethod("GetAccessKey", new[] { typeof(DependencyObject) });

	/// <summary>
	/// As WPF: the normalized access key whose <c>AccessKeyPressed</c> target is the owner (the key of a
	/// <c>Label</c> belongs to its <c>Target</c>), but only when <c>AutomationProperties.AccessKey</c> is empty.
	/// When it is set this returns an empty string. That is WPF's behaviour, kept as is.
	/// </summary>
	protected override string GetAccessKeyCore()
	{
		var explicitKey = _getExplicitAccessKey?.Invoke(null, new object[] { Owner }) as string;
		if (!string.IsNullOrEmpty(explicitKey)) return string.Empty;
		return AccessKeyManager.GetKeyForTarget(Owner) ?? string.Empty;
	}
}
