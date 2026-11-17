using Xunit;

using Gtk4.Forms;

namespace Gtk4.Forms.Xaml.UnitTests;

public partial class Issue3090 : ContentPage
{
	public Issue3090()
	{
		InitializeComponent();
	}

	public Issue3090(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	public class Tests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void NewDoesNotThrow(bool useCompiledXaml)
		{
			var p = new Issue3090(useCompiledXaml);
		}
	}
}