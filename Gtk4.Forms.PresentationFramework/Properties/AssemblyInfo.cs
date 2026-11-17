using System.Windows.Markup;

// Map the WPF presentation xmlns to our System.Windows.* namespaces so the
// markup compiler can resolve unmodified WPF XAML to our types.
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Controls")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Shapes")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Data")]
