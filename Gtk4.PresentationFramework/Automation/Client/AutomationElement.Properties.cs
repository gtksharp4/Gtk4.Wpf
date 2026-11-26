using System.Collections.Generic;
using System.Globalization;

namespace System.Windows.Automation;

public partial class AutomationElement
{
	public static readonly AutomationProperty AcceleratorKeyProperty =
		AutomationElementIdentifiers.AcceleratorKeyProperty;
	public static readonly AutomationProperty AccessKeyProperty = AutomationElementIdentifiers.AccessKeyProperty;
	public static readonly AutomationProperty AutomationIdProperty = AutomationElementIdentifiers.AutomationIdProperty;
	public static readonly AutomationProperty BoundingRectangleProperty =
		AutomationElementIdentifiers.BoundingRectangleProperty;
	public static readonly AutomationProperty ClassNameProperty = AutomationElementIdentifiers.ClassNameProperty;
	public static readonly AutomationProperty ClickablePointProperty =
		AutomationElementIdentifiers.ClickablePointProperty;
	public static readonly AutomationProperty ControlTypeProperty = AutomationElementIdentifiers.ControlTypeProperty;
	public static readonly AutomationProperty CultureProperty = AutomationElementIdentifiers.CultureProperty;
	public static readonly AutomationProperty FrameworkIdProperty = AutomationElementIdentifiers.FrameworkIdProperty;
	public static readonly AutomationProperty HasKeyboardFocusProperty =
		AutomationElementIdentifiers.HasKeyboardFocusProperty;
	public static readonly AutomationProperty HelpTextProperty = AutomationElementIdentifiers.HelpTextProperty;
	public static readonly AutomationProperty IsContentElementProperty =
		AutomationElementIdentifiers.IsContentElementProperty;
	public static readonly AutomationProperty IsControlElementProperty =
		AutomationElementIdentifiers.IsControlElementProperty;
	public static readonly AutomationProperty IsEnabledProperty = AutomationElementIdentifiers.IsEnabledProperty;
	public static readonly AutomationProperty IsKeyboardFocusableProperty =
		AutomationElementIdentifiers.IsKeyboardFocusableProperty;
	public static readonly AutomationProperty IsOffscreenProperty = AutomationElementIdentifiers.IsOffscreenProperty;
	public static readonly AutomationProperty IsPasswordProperty = AutomationElementIdentifiers.IsPasswordProperty;
	public static readonly AutomationProperty IsRequiredForFormProperty =
		AutomationElementIdentifiers.IsRequiredForFormProperty;
	public static readonly AutomationProperty ItemStatusProperty = AutomationElementIdentifiers.ItemStatusProperty;
	public static readonly AutomationProperty ItemTypeProperty = AutomationElementIdentifiers.ItemTypeProperty;
	public static readonly AutomationProperty LabeledByProperty = AutomationElementIdentifiers.LabeledByProperty;
	public static readonly AutomationProperty LocalizedControlTypeProperty =
		AutomationElementIdentifiers.LocalizedControlTypeProperty;
	public static readonly AutomationProperty NameProperty = AutomationElementIdentifiers.NameProperty;
	public static readonly AutomationProperty NativeWindowHandleProperty =
		AutomationElementIdentifiers.NativeWindowHandleProperty;
	public static readonly AutomationProperty OrientationProperty = AutomationElementIdentifiers.OrientationProperty;
	public static readonly AutomationProperty ProcessIdProperty = AutomationElementIdentifiers.ProcessIdProperty;
	public static readonly AutomationProperty RuntimeIdProperty = AutomationElementIdentifiers.RuntimeIdProperty;

	public static readonly AutomationEvent AsyncContentLoadedEvent =
		AutomationElementIdentifiers.AsyncContentLoadedEvent;
	public static readonly AutomationEvent AutomationFocusChangedEvent =
		AutomationElementIdentifiers.AutomationFocusChangedEvent;
	public static readonly AutomationEvent AutomationPropertyChangedEvent =
		AutomationElementIdentifiers.AutomationPropertyChangedEvent;
	public static readonly AutomationEvent LayoutInvalidatedEvent = AutomationElementIdentifiers.LayoutInvalidatedEvent;
	public static readonly AutomationEvent MenuClosedEvent = AutomationElementIdentifiers.MenuClosedEvent;
	public static readonly AutomationEvent MenuOpenedEvent = AutomationElementIdentifiers.MenuOpenedEvent;
	public static readonly AutomationEvent StructureChangedEvent = AutomationElementIdentifiers.StructureChangedEvent;
	public static readonly AutomationEvent ToolTipClosedEvent = AutomationElementIdentifiers.ToolTipClosedEvent;
	public static readonly AutomationEvent ToolTipOpenedEvent = AutomationElementIdentifiers.ToolTipOpenedEvent;

	/// <summary>The properties as they are now.</summary>
	public AutomationElementInformation Current => new AutomationElementInformation(this, false);

	/// <summary>The properties as they were when cached.</summary>
	/// <exception cref="InvalidOperationException">The element was not retrieved with a cache request.</exception>
	public AutomationElementInformation Cached =>
		_cache != null ? new AutomationElementInformation(this, true)
			: throw new InvalidOperationException("The element has no cached information.");

	public object GetCurrentPropertyValue(AutomationProperty property) => GetCurrentPropertyValue(property, false);

	public object GetCurrentPropertyValue(AutomationProperty property, bool ignoreDefaultValue)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));
		return AutomationThread.Call(() => ReadPropertyCore(property, ignoreDefaultValue))!;
	}

	public object GetCachedPropertyValue(AutomationProperty property) => GetCachedPropertyValue(property, false);

	public object GetCachedPropertyValue(AutomationProperty property, bool ignoreDefaultValue)
	{
		if (property == null) throw new ArgumentNullException(nameof(property));
		if (_cache == null || !_cache.Properties.TryGetValue(property.Id, out var value))
			throw new InvalidOperationException("The property was not cached.");
		return (ReferenceEquals(value, NotSupported) && !ignoreDefaultValue ? DefaultFor(property) : value)!;
	}

	/// <summary>Returns the properties the element supports: every element property and its patterns'.</summary>
	public AutomationProperty[] GetSupportedProperties() => AutomationThread.Call(() =>
	{
		EnsureAvailable();
		var result = new List<AutomationProperty>();
		foreach (var entry in PropertyCatalog.ElementEntries)
			result.Add(entry.Property);
		foreach (var entry in PropertyCatalog.PatternEntries)
			if (Peer.GetPattern(PatternRegistry.InterfaceOf(entry.Pattern)) != null)
				result.Add(entry.Property);
		return result.ToArray();
	});

	/// <summary>Reads a property on the GTK thread. Unsupported is <see cref="NotSupported"/> or the default.</summary>
	internal object? ReadPropertyCore(AutomationProperty property, bool ignoreDefault)
	{
		EnsureAvailable();
		object? value;
		if (PropertyCatalog.LookupElement(property) is { } element)
			value = element.Get(Peer);
		else if (PropertyCatalog.LookupPattern(property) is { } pattern)
		{
			var provider = Peer.GetPattern(PatternRegistry.InterfaceOf(pattern.Pattern));
			value = provider == null ? NotSupported : pattern.Get(provider);
		}
		else
			throw new ArgumentException("The property is not supported.", nameof(property));
		return ReferenceEquals(value, NotSupported) && !ignoreDefault ? DefaultFor(property) : value;
	}

	static object? DefaultFor(AutomationProperty property)
	{
		var pattern = PropertyCatalog.LookupPattern(property);
		if (pattern?.Default != null) return pattern.Default();
		var type = PropertyCatalog.LookupElement(property)?.Type ?? pattern?.Type;
		return type == null ? null : PropertyCatalog.DefaultOf(type);
	}

	/// <summary>The properties of an element, read live (<see cref="Current"/>) or from the cache.</summary>
	public struct AutomationElementInformation
	{
		readonly AutomationElement _element;
		readonly bool _cached;

		internal AutomationElementInformation(AutomationElement element, bool cached)
		{
			_element = element;
			_cached = cached;
		}

		T Get<T>(AutomationProperty property) =>
			(T)(_cached ? _element.GetCachedPropertyValue(property) : _element.GetCurrentPropertyValue(property));

		public int ProcessId => Get<int>(ProcessIdProperty);
		public int NativeWindowHandle => Get<int>(NativeWindowHandleProperty);
		public string AutomationId => Get<string>(AutomationIdProperty);
		public string Name => Get<string>(NameProperty);
		public string ClassName => Get<string>(ClassNameProperty);
		public string HelpText => Get<string>(HelpTextProperty);
		public string AcceleratorKey => Get<string>(AcceleratorKeyProperty);
		public string AccessKey => Get<string>(AccessKeyProperty);
		public string ItemStatus => Get<string>(ItemStatusProperty);
		public string ItemType => Get<string>(ItemTypeProperty);
		public string FrameworkId => Get<string>(FrameworkIdProperty);
		public string LocalizedControlType => Get<string>(LocalizedControlTypeProperty);
		public ControlType ControlType => Get<ControlType>(ControlTypeProperty);
		public OrientationType Orientation => Get<OrientationType>(OrientationProperty);
		public Rect BoundingRectangle => Get<Rect>(BoundingRectangleProperty);
		public bool IsEnabled => Get<bool>(IsEnabledProperty);
		public bool IsOffscreen => Get<bool>(IsOffscreenProperty);
		public bool IsKeyboardFocusable => Get<bool>(IsKeyboardFocusableProperty);
		public bool HasKeyboardFocus => Get<bool>(HasKeyboardFocusProperty);
		public bool IsPassword => Get<bool>(IsPasswordProperty);
		public bool IsControlElement => Get<bool>(IsControlElementProperty);
		public bool IsContentElement => Get<bool>(IsContentElementProperty);
		public bool IsRequiredForForm => Get<bool>(IsRequiredForFormProperty);
		public AutomationElement LabeledBy => Get<AutomationElement>(LabeledByProperty);
	}
}
