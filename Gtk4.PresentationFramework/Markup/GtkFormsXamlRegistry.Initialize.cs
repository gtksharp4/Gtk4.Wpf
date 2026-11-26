using System.Reflection;

namespace System.Windows.Markup;

public static partial class GtkFormsXamlRegistry
{
	/// <summary>
	/// Runs the generated <c>InitializeComponent</c> of a registered root. WPF's BAML loader populates the
	/// instance itself, so a root class without a constructor calling it must still load. The generated
	/// method is idempotent, so a constructor that already called it is not repeated.
	/// </summary>
	internal static object Initialize(object instance)
	{
		MethodInfo? init = instance.GetType().GetMethod("InitializeComponent", BindingFlags.Public
			| BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
		if (init == null) return instance;
		try
		{
			init.Invoke(instance, null);
		}
		catch (TargetInvocationException e) when (e.InnerException != null)
		{
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
		}
		return instance;
	}
}
