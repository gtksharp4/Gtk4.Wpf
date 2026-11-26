using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

public static class ArgumentNullExceptionExtension
{
	extension(ArgumentNullException)
	{
		/// <summary>
		/// Throws an <see cref="ArgumentNullException"/> if the specified argument is null.
		/// </summary>
		/// <param name="argument">The argument to validate.</param>
		/// <param name="paramName">The name of the argument, supplied by the compiler.</param>
		public static void ThrowIfNull(
			[NotNull] object? argument,
			[CallerArgumentExpression(nameof(argument))] string? paramName = null)
		{
			if (argument is null)
				throw new ArgumentNullException(paramName);
		}
	}
}
