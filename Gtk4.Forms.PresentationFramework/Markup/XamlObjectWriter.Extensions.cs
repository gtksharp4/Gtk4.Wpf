using System.Collections.Generic;
using System.Windows.Data;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectWriter
{
	// "{DynamicResource Key}" or "{Binding ...}" for a property set by a markup extension; null otherwise.
	static string? ExtensionText(DependencyObject d, DependencyProperty dp)
	{
		if (d is FrameworkElement fe && fe.LocalResourceKeyOf(dp) is { } key && IsPlain(key.ToString()))
			return "{DynamicResource " + key + "}";
		return BindingOperations.GetBinding(d, dp) is { } binding ? BindingText(binding) : null;
	}

	// Only bindings built from Path, ElementName, Mode and UpdateSourceTrigger are written; the rest need
	// object-valued members (Source, Converter, RelativeSource) that have no attribute form.
	static string? BindingText(Binding b)
	{
		if (b.Source != null || b.RelativeSource != null || b.Converter != null || b.ConverterParameter != null
			|| b.ConverterCulture != null)
			return null;
		var parts = new List<string>();
		if (!string.IsNullOrEmpty(b.Path))
		{
			if (!IsPlain(b.Path)) return null;
			parts.Add("Path=" + b.Path);
		}
		if (b.ElementName != null)
		{
			if (!IsPlain(b.ElementName)) return null;
			parts.Add("ElementName=" + b.ElementName);
		}
		if (b.Mode != BindingMode.Default) parts.Add("Mode=" + b.Mode);
		if (b.UpdateSourceTrigger != UpdateSourceTrigger.Default) parts.Add("UpdateSourceTrigger=" + b.UpdateSourceTrigger);
		return "{Binding " + string.Join(", ", parts) + "}";
	}

	static bool IsPlain(string? text) =>
		!string.IsNullOrEmpty(text) && text!.IndexOfAny(new[] { '{', '}', ',', '=', '\'', '"' }) < 0;
}
