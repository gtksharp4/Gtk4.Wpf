using System.Collections.Generic;
using System.Globalization;
using System.Windows.Automation.Peers;

namespace System.Windows.Automation;

/// <summary>How the client reads one property: the value type it must have and how to get it.</summary>
sealed class PropertyEntry
{
	internal PropertyEntry(AutomationProperty property, Type type, Func<AutomationPeer, object?> get)
	{
		Property = property;
		Type = type;
		Get = get;
	}

	internal AutomationProperty Property { get; }

	internal Type Type { get; }

	internal Func<AutomationPeer, object?> Get { get; }
}

/// <summary>The element properties the client reads from a peer.</summary>
static partial class PropertyCatalog
{
	static readonly int _processId = CurrentProcessId();
	static readonly Dictionary<int, PropertyEntry> _element = BuildElementEntries();

	internal static PropertyEntry? LookupElement(AutomationProperty property)
	{
		lock (_element) return _element.TryGetValue(property.Id, out var entry) ? entry : null;
	}

	internal static IEnumerable<PropertyEntry> ElementEntries => _element.Values;

	/// <summary>The value reported for a property the element does not support, when defaults are wanted.</summary>
	internal static object? DefaultOf(Type type) =>
		type == typeof(string) ? string.Empty : type.IsValueType ? Activator.CreateInstance(type) : null;

	static int CurrentProcessId()
	{
		using var process = System.Diagnostics.Process.GetCurrentProcess();
		return process.Id;
	}

	static Dictionary<int, PropertyEntry> BuildElementEntries()
	{
		var map = new Dictionary<int, PropertyEntry>();
		void Add(AutomationProperty p, Type t, Func<AutomationPeer, object?> get) =>
			map[p.Id] = new PropertyEntry(p, t, get);
		Add(AutomationElementIdentifiers.NameProperty, typeof(string), p => p.GetName());
		Add(AutomationElementIdentifiers.AutomationIdProperty, typeof(string), p => p.GetAutomationId());
		Add(AutomationElementIdentifiers.ClassNameProperty, typeof(string), p => p.GetClassName());
		Add(AutomationElementIdentifiers.HelpTextProperty, typeof(string), p => p.GetHelpText());
		Add(AutomationElementIdentifiers.AcceleratorKeyProperty, typeof(string), p => p.GetAcceleratorKey());
		Add(AutomationElementIdentifiers.AccessKeyProperty, typeof(string), p => p.GetAccessKey());
		Add(AutomationElementIdentifiers.ItemStatusProperty, typeof(string), p => p.GetItemStatus());
		Add(AutomationElementIdentifiers.ItemTypeProperty, typeof(string), p => p.GetItemType());
		Add(AutomationElementIdentifiers.LocalizedControlTypeProperty, typeof(string),
			p => p.GetLocalizedControlType());
		Add(AutomationElementIdentifiers.FrameworkIdProperty, typeof(string), p => "WPF");
		Add(AutomationElementIdentifiers.ControlTypeProperty, typeof(ControlType),
			p => ControlType.LookupById(50000 + (int)p.GetAutomationControlType()));
		Add(AutomationElementIdentifiers.IsEnabledProperty, typeof(bool), p => p.IsEnabled());
		Add(AutomationElementIdentifiers.IsOffscreenProperty, typeof(bool), p => p.IsOffscreen());
		Add(AutomationElementIdentifiers.IsKeyboardFocusableProperty, typeof(bool), p => p.IsKeyboardFocusable());
		Add(AutomationElementIdentifiers.HasKeyboardFocusProperty, typeof(bool), p => p.HasKeyboardFocus());
		Add(AutomationElementIdentifiers.IsControlElementProperty, typeof(bool), p => p.IsControlElement());
		Add(AutomationElementIdentifiers.IsContentElementProperty, typeof(bool), p => p.IsContentElement());
		Add(AutomationElementIdentifiers.IsPasswordProperty, typeof(bool), p => p.IsPassword());
		Add(AutomationElementIdentifiers.IsRequiredForFormProperty, typeof(bool), p => p.IsRequiredForForm());
		Add(AutomationElementIdentifiers.OrientationProperty, typeof(OrientationType),
			p => (OrientationType)(int)p.GetOrientation());
		Add(AutomationElementIdentifiers.BoundingRectangleProperty, typeof(Rect), p => p.GetBoundingRectangle());
		Add(AutomationElementIdentifiers.ClickablePointProperty, typeof(Point), ClickablePoint);
		Add(AutomationElementIdentifiers.ProcessIdProperty, typeof(int), p => _processId);
		Add(AutomationElementIdentifiers.NativeWindowHandleProperty, typeof(int), p => 0);
		Add(AutomationElementIdentifiers.RuntimeIdProperty, typeof(int[]), p => AutomationElement.RuntimeIdOf(p));
		Add(AutomationElementIdentifiers.CultureProperty, typeof(CultureInfo), p => CultureInfo.CurrentCulture);
		Add(AutomationElementIdentifiers.LabeledByProperty, typeof(AutomationElement),
			p => p.GetLabeledBy() is { } label ? AutomationElement.Wrap(label, null) : null);
		return map;
	}

	// A hidden or empty element has no clickable point, which reads as "not supported".
	static object? ClickablePoint(AutomationPeer peer)
	{
		if (peer.IsOffscreen()) return AutomationElement.NotSupported;
		var point = peer.GetClickablePoint();
		return double.IsNaN(point.X) || double.IsNaN(point.Y) ? AutomationElement.NotSupported : point;
	}
}
