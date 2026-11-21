using System;

namespace System.Windows.Navigation;

/// <summary>Carries the result a <see cref="PageFunction{T}"/> returns to its caller.</summary>
public class ReturnEventArgs<T> : EventArgs
{
	/// <summary>Creates arguments with a default result.</summary>
	public ReturnEventArgs()
	{
		Result = default!;
	}

	/// <summary>Creates arguments carrying <paramref name="result"/>.</summary>
	public ReturnEventArgs(T result)
	{
		Result = result;
	}

	/// <summary>The returned value.</summary>
	public T Result { get; set; }
}
