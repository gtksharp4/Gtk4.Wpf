using System.ComponentModel;

namespace System.Windows.Media.Animation;

/// <summary>
/// Groups child timelines and applies them to the elements and properties named by the
/// TargetName, TargetProperty and Target attached properties.
/// Mirrors WPF's <c>System.Windows.Media.Animation.Storyboard</c>.
/// </summary>
public partial class Storyboard : ParallelTimeline
{
	/// <summary>Identifies the TargetName attached property.</summary>
	public static readonly DependencyProperty TargetNameProperty = DependencyProperty.RegisterAttached(
		"TargetName", typeof(string), typeof(Storyboard), new PropertyMetadata(null));

	/// <summary>Identifies the TargetProperty attached property.</summary>
	public static readonly DependencyProperty TargetPropertyProperty = DependencyProperty.RegisterAttached(
		"TargetProperty", typeof(PropertyPath), typeof(Storyboard), new PropertyMetadata(null));

	/// <summary>Identifies the Target attached property.</summary>
	public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
		"Target", typeof(DependencyObject), typeof(Storyboard), new PropertyMetadata(null));

	/// <summary>Sets the name of the element a timeline animates.</summary>
	public static void SetTargetName(DependencyObject element, string name) =>
		Check(element).SetValue(TargetNameProperty, name);

	/// <summary>Gets the name of the element a timeline animates.</summary>
	public static string? GetTargetName(DependencyObject element) =>
		(string?)Check(element).GetValue(TargetNameProperty);

	/// <summary>Sets the property path a timeline animates.</summary>
	public static void SetTargetProperty(DependencyObject element, PropertyPath path) =>
		Check(element).SetValue(TargetPropertyProperty, path);

	/// <summary>Gets the property path a timeline animates.</summary>
	public static PropertyPath? GetTargetProperty(DependencyObject element) =>
		(PropertyPath?)Check(element).GetValue(TargetPropertyProperty);

	/// <summary>Sets the object a timeline animates, taking precedence over the target name.</summary>
	public static void SetTarget(DependencyObject element, DependencyObject target) =>
		Check(element).SetValue(TargetProperty, target);

	/// <summary>Gets the object a timeline animates.</summary>
	public static DependencyObject? GetTarget(DependencyObject element) =>
		(DependencyObject?)Check(element).GetValue(TargetProperty);

	static DependencyObject Check(DependencyObject element) =>
		element ?? throw new ArgumentNullException(nameof(element));

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new Storyboard();

	// Instance shims: the markup compiler emits sb.SetTargetName(timeline, "x") and
	// sb.SetTargetPropertyName(timeline, "(A.B)"), which predate the WPF static API.

	/// <summary>Sets the target element name of a child timeline.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SetTargetName(Timeline timeline, string name) => SetTargetName((DependencyObject)timeline, name);

	/// <summary>Gets the target element name of a child timeline.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? GetTargetName(Timeline timeline) =>
		timeline == null ? null : (string?)timeline.GetValue(TargetNameProperty);

	/// <summary>Sets the target property of a child timeline.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SetTargetProperty(Timeline timeline, DependencyProperty dp)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		SetTargetProperty((DependencyObject)timeline, new PropertyPath(dp));
	}

	/// <summary>Gets the target property of a child timeline when it was set as a single property.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public DependencyProperty? GetTargetProperty(Timeline timeline)
	{
		var path = timeline == null ? null : (PropertyPath?)timeline.GetValue(TargetPropertyProperty);
		return path != null && path.Path == "(0)" && path.PathParameters.Count == 1
			? path.PathParameters[0] as DependencyProperty
			: null;
	}

	/// <summary>Sets the target property of a child timeline from a path string.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SetTargetPropertyName(Timeline timeline, string propertyName)
	{
		if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
		SetTargetProperty((DependencyObject)timeline, new PropertyPath(propertyName));
	}

	/// <summary>Gets the path string of a child timeline target property.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? GetTargetPropertyName(Timeline timeline) =>
		timeline == null ? null : ((PropertyPath?)timeline.GetValue(TargetPropertyProperty))?.Path;

	/// <summary>Sets the target element name of <paramref name="timeline"/>.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetTargetName(Timeline timeline, Storyboard storyboard, string name) =>
		storyboard.SetTargetName(timeline, name);

	/// <summary>Sets the target property of <paramref name="timeline"/>.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void SetTargetProperty(Timeline timeline, Storyboard storyboard, DependencyProperty dp) =>
		storyboard.SetTargetProperty(timeline, dp);
}
