using System.Windows.Media;

namespace System.Windows.Documents;

/// <summary>An element paired with a text effect that can be switched on and off on it.</summary>
public class TextEffectTarget
{
	bool _enabled;

	/// <summary>Creates a target for <paramref name="effect"/> on <paramref name="element"/>.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	/// <exception cref="ArgumentException">The element is not a <see cref="TextElement"/>.</exception>
	internal TextEffectTarget(DependencyObject element, TextEffect effect)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (effect == null) throw new ArgumentNullException(nameof(effect));
		if (!(element is TextElement))
			throw new ArgumentException("Only text elements carry text effects.", nameof(element));
		Element = element;
		TextEffect = effect;
	}

	/// <summary>Gets the element that carries the effect.</summary>
	public DependencyObject Element { get; }

	/// <summary>Gets the effect this target applies.</summary>
	public TextEffect TextEffect { get; }

	/// <summary>Gets whether the effect is in the element's <see cref="TextElement.TextEffects"/>.</summary>
	public bool IsEnabled => _enabled;

	/// <summary>Adds the effect to the element; does nothing when already enabled.</summary>
	public void Enable()
	{
		if (_enabled) return;
		var element = (TextElement)Element;
		var effects = element.TextEffects;
		if (effects == null) element.TextEffects = effects = new TextEffectCollection();
		effects.Add(TextEffect);
		_enabled = true;
	}

	/// <summary>Removes the effect from the element; does nothing when not enabled.</summary>
	public void Disable()
	{
		if (!_enabled) return;
		((TextElement)Element).TextEffects?.Remove(TextEffect);
		_enabled = false;
	}
}
