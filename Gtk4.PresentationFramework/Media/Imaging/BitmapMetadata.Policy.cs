using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Media.Imaging;

public partial class BitmapMetadata
{
	readonly Dictionary<string, object?> _policy = new Dictionary<string, object?>();

	T? Policy<T>(string name) => _policy.TryGetValue(name, out var v) && v is T t ? t : default;

	void SetPolicy(string name, object? value)
	{
		ThrowIfFrozen();
		_policy[name] = value;
	}

	internal void CopyPolicy(BitmapMetadata destination)
	{
		foreach (var kv in _policy)
			destination._policy[kv.Key] = kv.Value;
	}

	/// <summary>Image title (kept in memory only).</summary>
	public string? Title { get => Policy<string>(nameof(Title)); set => SetPolicy(nameof(Title), value); }

	/// <summary>Image subject (kept in memory only).</summary>
	public string? Subject { get => Policy<string>(nameof(Subject)); set => SetPolicy(nameof(Subject), value); }

	/// <summary>Comment (kept in memory only).</summary>
	public string? Comment { get => Policy<string>(nameof(Comment)); set => SetPolicy(nameof(Comment), value); }

	/// <summary>Authors (kept in memory only).</summary>
	public ReadOnlyCollection<string>? Author
	{
		get => Policy<ReadOnlyCollection<string>>(nameof(Author));
		set => SetPolicy(nameof(Author), value);
	}

	/// <summary>Keywords (kept in memory only).</summary>
	public ReadOnlyCollection<string>? Keywords
	{
		get => Policy<ReadOnlyCollection<string>>(nameof(Keywords));
		set => SetPolicy(nameof(Keywords), value);
	}

	/// <summary>Copyright (kept in memory only).</summary>
	public string? Copyright
	{
		get => Policy<string>(nameof(Copyright));
		set => SetPolicy(nameof(Copyright), value);
	}

	/// <summary>Date taken (kept in memory only).</summary>
	public string? DateTaken
	{
		get => Policy<string>(nameof(DateTaken));
		set => SetPolicy(nameof(DateTaken), value);
	}

	/// <summary>Camera manufacturer (kept in memory only).</summary>
	public string? CameraManufacturer
	{
		get => Policy<string>(nameof(CameraManufacturer));
		set => SetPolicy(nameof(CameraManufacturer), value);
	}

	/// <summary>Camera model (kept in memory only).</summary>
	public string? CameraModel
	{
		get => Policy<string>(nameof(CameraModel));
		set => SetPolicy(nameof(CameraModel), value);
	}

	/// <summary>Application name (kept in memory only).</summary>
	public string? ApplicationName
	{
		get => Policy<string>(nameof(ApplicationName));
		set => SetPolicy(nameof(ApplicationName), value);
	}

	/// <summary>Rating from 0 to 99 (kept in memory only).</summary>
	public int Rating
	{
		get => Policy<int?>(nameof(Rating)) ?? 0;
		set => SetPolicy(nameof(Rating), value);
	}
}
