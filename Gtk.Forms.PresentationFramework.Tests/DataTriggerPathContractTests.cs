using System;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk.Forms.PresentationFramework.Tests
{
	/// <summary>
	/// E5: DataTrigger with a multi-segment binding path throws at attach time with a
	/// clear "not yet supported" message. Single-segment paths continue to work.
	/// </summary>
	public class DataTriggerPathContractTests
	{
		sealed class TEl : FrameworkElement
		{
			public static readonly DependencyProperty TagProperty =
				DependencyProperty.Register("DtPathTag", typeof(string), typeof(TEl),
					new PropertyMetadata(null));
			public string? Tag2 { get => (string?)GetValue(TagProperty); set => SetValue(TagProperty, value); }
			protected override Gtk.Widget CreateWidget() =>
				throw new NotSupportedException("pure test");
		}

		[Fact]
		public void Multi_segment_path_throws_NotSupportedException_at_attach()
		{
			var el = new TEl();
			var style = new Style(typeof(TEl));
			var dt = new DataTrigger
			{
				Binding = new Binding("Parent.Child"),
				Value = "x"
			};
			dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "hit" });
			style.Triggers.Add(dt);

			var ex = Assert.Throws<NotSupportedException>(() => el.Style = style);
			Assert.Contains("nested", ex.Message, StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		public void Single_segment_path_attaches_without_exception()
		{
			var el = new TEl();
			var style = new Style(typeof(TEl));
			var dt = new DataTrigger
			{
				Binding = new Binding("Active"),
				Value = "yes"
			};
			dt.Setters.Add(new Setter { Property = TEl.TagProperty, Value = "ok" });
			style.Triggers.Add(dt);

			// Must not throw.
			var ex = Record.Exception(() => el.Style = style);
			Assert.Null(ex);
		}
	}
}
