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
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Controls.Primitives")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Documents")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Input")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Media")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Media.Animation")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Media.Effects")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Media.Imaging")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Automation")]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Navigation")]

// UpdateSourceTrigger moved to WindowsBase so FrameworkPropertyMetadata can carry a default trigger.
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.Windows.Data.UpdateSourceTrigger))]
[assembly: XmlnsDefinition(
	"http://schemas.microsoft.com/winfx/2006/xaml/presentation",
	"System.Windows.Media.Media3D")]
