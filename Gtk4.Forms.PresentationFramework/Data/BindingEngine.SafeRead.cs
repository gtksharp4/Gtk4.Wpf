using System.Reflection;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>
	/// Reads a source property. A getter that throws is a binding error, as in WPF: it is traced and the
	/// step counts as unresolved, so the target falls back instead of the exception reaching the UI thread.
	/// </summary>
	internal static bool TryRead(PropertyInfo prop, object owner, out object? value)
	{
		try
		{
			value = prop.GetValue(owner);
			return true;
		}
		catch (Exception ex) when (ex is not OutOfMemoryException)
		{
			var cause = ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
			System.Diagnostics.Trace.WriteLine(
				$"System.Windows.Data Error: getter of '{prop.Name}' threw {cause.GetType().Name}: {cause.Message}");
			value = null;
			return false;
		}
	}
}
