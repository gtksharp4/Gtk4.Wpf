using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace System.Windows;

public static partial class RoutedEventHost
{
	/// <summary>
	/// Invokes one handler. <see cref="RoutedEventHandler"/> is called directly; other handler
	/// types go through <see cref="Delegate.DynamicInvoke"/>, whose
	/// <see cref="TargetInvocationException"/> wrapper is stripped so the handler's own exception
	/// propagates unchanged (as WPF's typed <c>RoutedEventArgs.InvokeEventHandler</c>).
	/// </summary>
	internal static void InvokeHandler(Delegate handler, object sender, RoutedEventArgs e)
	{
		if (handler is RoutedEventHandler routed)
		{
			routed(sender, e);
			return;
		}
		try
		{
			handler.DynamicInvoke(sender, e);
		}
		catch (TargetInvocationException tie) when (tie.InnerException != null)
		{
			ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
			throw; // unreachable: Throw() always throws
		}
	}
}
