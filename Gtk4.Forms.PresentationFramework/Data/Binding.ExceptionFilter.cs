namespace System.Windows.Data;

public partial class Binding
{
	/// <summary>
	/// Gets or sets the handler for exceptions thrown while the source is updated. It applies when exception
	/// validation is on (<see cref="ValidatesOnExceptions"/> or an <c>ExceptionValidationRule</c>).
	/// </summary>
	public UpdateSourceExceptionFilterCallback? UpdateSourceExceptionFilter { get; set; }
}
