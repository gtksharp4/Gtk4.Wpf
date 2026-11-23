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
	public void A_MultiBinding_on_a_non_element_writes_a_trace_warning_and_returns_null()
	{
		var listener = new Capture();
		Trace.Listeners.Add(listener);
		try
		{
			var result = NonElementBinding.TrySetBinding(
				new DoubleAnimation(), DoubleAnimation.ToProperty, new MultiBinding());

			Assert.Null(result);
			var text = listener.Text.ToString();
			Assert.Contains("DoubleAnimation", text);
			Assert.Contains("To", text);
		}
		finally { Trace.Listeners.Remove(listener); }
	}

	[Fact]
	public void A_Binding_on_a_non_element_is_applied_and_returns_the_expression()
	{
		var animation = new DoubleAnimation();

		var result = NonElementBinding.TrySetBinding(
			animation, DoubleAnimation.ToProperty, new Binding { Source = 7.0 });

		Assert.NotNull(result);
		Assert.Equal(7.0, animation.To);
	}
}
