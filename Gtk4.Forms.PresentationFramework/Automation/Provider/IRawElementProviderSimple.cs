namespace System.Windows.Automation.Provider;

/// <summary>Options that describe how a provider is hosted.</summary>
[Flags]
public enum ProviderOptions
{
	ClientSideProvider = 1,
	ServerSideProvider = 2,
	NonClientAreaProvider = 4,
	OverrideProvider = 8,
	ProviderOwnsSetFocus = 16,
	UseComThreading = 32,
}

/// <summary>The base interface of an automation element provider.</summary>
public interface IRawElementProviderSimple
{
	/// <summary>How the provider is hosted.</summary>
	ProviderOptions ProviderOptions { get; }

	/// <summary>The window host provider, always <c>null</c> in this layer.</summary>
	IRawElementProviderSimple? HostRawElementProvider { get; }

	/// <summary>Returns the provider for a pattern id, or <c>null</c>.</summary>
	object? GetPatternProvider(int patternId);

	/// <summary>Returns the value of a property id, or <c>null</c>.</summary>
	object? GetPropertyValue(int propertyId);
}
