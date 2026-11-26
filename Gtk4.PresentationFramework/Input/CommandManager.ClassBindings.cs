using System;
using System.Collections.Generic;

namespace System.Windows.Input;

public static partial class CommandManager
{
	static readonly Dictionary<Type, List<CommandBinding>> _classCommandBindings =
		new Dictionary<Type, List<CommandBinding>>();
	static readonly Dictionary<Type, List<InputBinding>> _classInputBindings =
		new Dictionary<Type, List<InputBinding>>();
	static readonly object _classLock = new object();

	/// <summary>Registers a <see cref="CommandBinding"/> handled by every instance of <paramref name="type"/>.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	public static void RegisterClassCommandBinding(Type type, CommandBinding commandBinding)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (commandBinding == null) throw new ArgumentNullException(nameof(commandBinding));
		Add(_classCommandBindings, type, commandBinding);
	}

	/// <summary>Registers an <see cref="InputBinding"/> active on every instance of <paramref name="type"/>.</summary>
	/// <exception cref="ArgumentNullException">An argument is null.</exception>
	public static void RegisterClassInputBinding(Type type, InputBinding inputBinding)
	{
		if (type == null) throw new ArgumentNullException(nameof(type));
		if (inputBinding == null) throw new ArgumentNullException(nameof(inputBinding));
		Add(_classInputBindings, type, inputBinding);
	}

	static void Add<T>(Dictionary<Type, List<T>> map, Type type, T item)
	{
		lock (_classLock)
		{
			if (!map.TryGetValue(type, out var list)) map[type] = list = new List<T>();
			list.Add(item);
		}
	}

	// Bindings registered for the type and its base types, most derived first.
	static List<T> ForTypeChain<T>(Dictionary<Type, List<T>> map, Type type)
	{
		var result = new List<T>();
		lock (_classLock)
		{
			for (var t = type; t != null; t = t.BaseType)
			{
				if (map.TryGetValue(t, out var list)) result.AddRange(list);
			}
		}
		return result;
	}

	internal static List<InputBinding> GetClassInputBindings(Type type) => ForTypeChain(_classInputBindings, type);
}
