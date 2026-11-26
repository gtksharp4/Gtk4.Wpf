namespace System.Windows.Data;

/// <summary>Specifies the direction of data flow in a binding.</summary>
public enum BindingMode
{
	/// <summary>Source → target only (Phase-0 implementation).</summary>
	OneWay,
	/// <summary>Source → target and target → source. Phase-1.</summary>
	TwoWay,
	/// <summary>Target initialised from source once; subsequent source changes ignored.</summary>
	OneTime,
	/// <summary>Target → source only. Phase-1.</summary>
	OneWayToSource,
	/// <summary>Use the default mode declared in the target property metadata.</summary>
	Default,
}