using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System.Windows.Threading;

static class DispatcherInvoke
{
	/// <summary>Runs <paramref name="method"/> and surfaces the callee's own exception, not the reflection wrapper.</summary>
	internal static object? Call(Delegate method, object?[]? args)
	{
		if (args == null || args.Length == 0)
		{
			if (method is Action action)
			{
				action();
				return null;
			}
		}
		else if (args.Length == 1 && method is DispatcherOperationCallback callback)
			return callback(args[0]!);
		try
		{
			return method.DynamicInvoke(args);
		}
		catch (TargetInvocationException ex) when (ex.InnerException != null)
		{
			ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			throw;
		}
	}
}
