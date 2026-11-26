using System.Windows.Controls;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

/// <summary>A non-Window x:Class root with no constructor: only the compiler's registration can find it by URI.</summary>
public partial class RegisteredCanvas : Canvas
{
}
