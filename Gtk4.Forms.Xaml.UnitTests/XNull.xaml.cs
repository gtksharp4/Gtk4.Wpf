using Xunit;
using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class XNull : ContentPage
{
	public XNull()
	{
		InitializeComponent();
	}

	public XNull(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SupportsXNull(bool useCompiledXaml)
		{
			var layout = new XNull(useCompiledXaml);
			Assert.True(layout.Resources.ContainsKey("null"));
			Assert.Null(layout.Resources["null"]);
		}
	}
}