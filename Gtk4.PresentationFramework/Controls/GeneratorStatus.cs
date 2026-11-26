namespace System.Windows.Controls.Primitives;

/// <summary>The state of an <see cref="ItemContainerGenerator"/>.</summary>
public enum GeneratorStatus
{
	/// <summary>The generator has not tried to generate content.</summary>
	NotStarted,

	/// <summary>The generator is generating containers.</summary>
	GeneratingContainers,

	/// <summary>The generator has finished generating containers.</summary>
	ContainersGenerated,

	/// <summary>The generator hit an error.</summary>
	Error,
}
