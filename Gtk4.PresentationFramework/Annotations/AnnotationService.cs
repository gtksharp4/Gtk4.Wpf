using System.Runtime.CompilerServices;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace System.Windows.Annotations;

/// <summary>
/// Shows and edits the annotations of a store on a flow-document viewer. Highlights are stored annotations with
/// their Colors cargo and are not painted (gap: no adorner layer); text sticky notes show as
/// a GTK popover near the viewer. Other gaps: ink notes are store-only, there is no AttachedAnnotationChanged event
/// (no IAttachedAnnotation), and anchors are stored offsets that do not
/// follow later edits of the document.
/// </summary>
public sealed partial class AnnotationService : DispatcherObject
{
	/// <summary>Creates a highlight from the selection; the command parameter is the brush.</summary>
	public static readonly RoutedUICommand CreateHighlightCommand =
		new RoutedUICommand("_Highlight", "CreateHighlight", typeof(AnnotationService));

	/// <summary>Creates a text sticky note on the selection.</summary>
	public static readonly RoutedUICommand CreateTextStickyNoteCommand =
		new RoutedUICommand("Add Text _Note", "CreateTextStickyNote", typeof(AnnotationService));

	/// <summary>Creates an ink sticky note on the selection.</summary>
	public static readonly RoutedUICommand CreateInkStickyNoteCommand =
		new RoutedUICommand("Add In_k Note", "CreateInkStickyNote", typeof(AnnotationService));

	/// <summary>Clears the highlights in the selection.</summary>
	public static readonly RoutedUICommand ClearHighlightsCommand =
		new RoutedUICommand("_Clear Highlight", "ClearHighlights", typeof(AnnotationService));

	/// <summary>Deletes the sticky notes in the selection.</summary>
	public static readonly RoutedUICommand DeleteStickyNotesCommand =
		new RoutedUICommand("D_elete Notes", "DeleteStickyNotes", typeof(AnnotationService));

	/// <summary>Deletes every annotation in the selection.</summary>
	public static readonly RoutedUICommand DeleteAnnotationsCommand =
		new RoutedUICommand("_Delete Notes and Highlights", "DeleteAnnotations", typeof(AnnotationService));

	// Services are keyed by the viewer element; a service is registered from Enable to Disable, as in WPF.
	static readonly ConditionalWeakTable<UIElement, AnnotationService> _services =
		new ConditionalWeakTable<UIElement, AnnotationService>();

	readonly IAnnotationHost _host;
	AnnotationStore? _store;
	bool _enabled;

	/// <summary>Creates a service for a scroll viewer.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="viewer"/> is null.</exception>
	public AnnotationService(FlowDocumentScrollViewer viewer) : this(HostOf(viewer))
	{
	}

	/// <summary>Creates a service for a reader.</summary>
	/// <exception cref="ArgumentNullException"><paramref name="viewer"/> is null.</exception>
	public AnnotationService(FlowDocumentReader viewer) : this(HostOf(viewer))
	{
	}

	internal AnnotationService(IAnnotationHost host) => _host = host;

	static IAnnotationHost HostOf(FlowDocumentScrollViewer viewer)
	{
		if (viewer == null) throw new ArgumentNullException(nameof(viewer));
		return new FlowDocumentScrollViewerHost(viewer);
	}

	static IAnnotationHost HostOf(FlowDocumentReader viewer)
	{
		if (viewer == null) throw new ArgumentNullException(nameof(viewer));
		return new FlowDocumentReaderHost(viewer);
	}

	/// <summary>True between <see cref="Enable"/> and <see cref="Disable"/>.</summary>
	public bool IsEnabled => _enabled;

	/// <summary>The store the service shows, or null before the first <see cref="Enable"/>.</summary>
	public AnnotationStore? Store => _store;

	internal IAnnotationHost Host => _host;

	/// <summary>Starts showing the store's annotations.</summary>
	/// <exception cref="InvalidOperationException">Already enabled, or the viewer (or an ancestor) has a service.</exception>
	public void Enable(AnnotationStore annotationStore)
	{
		if (annotationStore == null) throw new ArgumentNullException(nameof(annotationStore));
		VerifyAccess();
		if (_enabled) throw new InvalidOperationException("The annotation service is already enabled.");
		if (ServiceAbove(_host.Element) != null)
			throw new InvalidOperationException("An annotation service already exists on this viewer.");
		_store = annotationStore;
		_enabled = true;
		_services.Remove(_host.Element);
		_services.Add(_host.Element, this);
		annotationStore.StoreContentChanged += OnStoreContentChanged;
		annotationStore.AnchorChanged += OnAnchorChanged;
		AddCommandBindings();
		AttachAll();
	}

	/// <summary>Stops showing annotations; they stay in the store.</summary>
	/// <exception cref="InvalidOperationException">The service is not enabled.</exception>
	public void Disable()
	{
		VerifyAccess();
		if (!_enabled) throw new InvalidOperationException("The annotation service is not enabled.");
		_store!.StoreContentChanged -= OnStoreContentChanged;
		_store.AnchorChanged -= OnAnchorChanged;
		DetachAll();
		RemoveCommandBindings();
		_enabled = false;
		_services.Remove(_host.Element);
	}

	/// <summary>The enabled service on a reader, or null.</summary>
	public static AnnotationService? GetService(FlowDocumentReader reader)
	{
		if (reader == null) throw new ArgumentNullException(nameof(reader));
		return Lookup(reader);
	}

	/// <summary>The enabled service on a scroll viewer, or null.</summary>
	public static AnnotationService? GetService(FlowDocumentScrollViewer viewer)
	{
		if (viewer == null) throw new ArgumentNullException(nameof(viewer));
		return Lookup(viewer);
	}

	internal static AnnotationService? Lookup(UIElement element) =>
		_services.TryGetValue(element, out var service) ? service : null;

	static AnnotationService? ServiceAbove(UIElement element)
	{
		DependencyObject? current = element;
		while (current != null)
		{
			if (current is UIElement ui && Lookup(ui) is { } found) return found;
			current = LogicalTreeHelper.GetParent(current);
		}
		return null;
	}
}
