using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class NonElementBindingTraceTests
{
	sealed class Capture : TraceListener
	{
		public readonly StringWriter Text = new StringWriter();
		public override void Write(string? message) => Text.Write(message);
		public override void WriteLine(string? message) => Text.WriteLine(message);
	}

	[Fact]
	public void An_unsupported_target_writes_a_trace_warning_and_returns_null()
	{
		var listener = new Capture();
		Trace.Listeners.Add(listener);
		try
		{
			var result = NonElementBinding.TrySetBinding(
				new DoubleAnimation(), DoubleAnimation.ToProperty, new Binding("Width"));

			Assert.Null(result);
			var text = listener.Text.ToString();
			Assert.Contains("DoubleAnimation", text);
			Assert.Contains("To", text);
			Assert.Contains("Width", text);
		}
		finally { Trace.Listeners.Remove(listener); }
	}
}
