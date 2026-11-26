using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>The (event, source) constructor assigns the fields directly, without calling OnSetSource.</summary>
public class RoutedEventArgsCtorTests
{
	class Derived : RoutedEventArgs
	{
		public int SetSourceCalls;
		public bool Initialised;

		public Derived(RoutedEvent e, object source) : base(e, source) { Initialised = true; }

		protected override void OnSetSource(object source) => SetSourceCalls++;
	}

	[Fact]
	public void Two_argument_ctor_does_not_call_OnSetSource()
	{
		var evt = OriginalSourceElement.PingEvent;
		var src = new object();
		var args = new Derived(evt, src);
		Assert.Equal(0, args.SetSourceCalls);
		Assert.Same(src, args.Source);
		Assert.Same(src, args.OriginalSource);

		var other = new object();
		args.Source = other;
		Assert.Equal(1, args.SetSourceCalls);
		Assert.Same(other, args.Source);
		Assert.Same(src, args.OriginalSource);
	}
}
