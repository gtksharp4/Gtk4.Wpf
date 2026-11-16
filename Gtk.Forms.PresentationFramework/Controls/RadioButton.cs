using System;
using System.Collections.Generic;

namespace System.Windows.Controls
{
	/// <summary>
	/// An exclusive-choice button backed by <see cref="Gtk.CheckButton"/>.
	/// Radio behaviour is achieved via <see cref="Gtk.CheckButton.SetGroup"/>, grouped
	/// by <see cref="GroupName"/> (empty name = standalone).
	/// Two-way: GTK toggle writes back into <see cref="IsChecked"/>.
	/// </summary>
	public class RadioButton : ContentControl
	{
		// ── IsChecked DP ─────────────────────────────────────────────────────────

		/// <summary>Gets or sets whether this button is selected.</summary>
		public static readonly DependencyProperty IsCheckedProperty =
			DependencyProperty.Register(
				nameof(IsChecked),
				typeof(bool),
				typeof(RadioButton),
				new PropertyMetadata(false, OnIsCheckedChanged));

		static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var rb = (RadioButton)d;
			if (rb._button != null)
			{
				bool active = (bool?)e.NewValue == true;
				if (rb._button.Active != active)
					rb._button.Active = active;
			}
		}

		/// <summary>Gets or sets whether this button is selected.</summary>
		public bool IsChecked
		{
			get => (bool?)GetValue(IsCheckedProperty) == true;
			set => SetValue(IsCheckedProperty, value);
		}

		// ── GroupName DP ─────────────────────────────────────────────────────────

		/// <summary>
		/// Name that groups radio buttons together. Buttons with the same non-empty
		/// GroupName are mutually exclusive via <see cref="Gtk.CheckButton.SetGroup"/>.
		/// </summary>
		public static readonly DependencyProperty GroupNameProperty =
			DependencyProperty.Register(
				nameof(GroupName),
				typeof(string),
				typeof(RadioButton),
				new PropertyMetadata(string.Empty, OnGroupNameChanged));

		static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((RadioButton)d).UpdateGroup((string?)e.OldValue, (string?)e.NewValue);
		}

		/// <summary>Gets or sets the group name.</summary>
		public string GroupName
		{
			get => (string?)GetValue(GroupNameProperty) ?? string.Empty;
			set => SetValue(GroupNameProperty, value);
		}

		// ── Group registry ───────────────────────────────────────────────────────

		// Groups are per-process (like WPF — there's no visual-tree scope here yet).
		static readonly Dictionary<string, List<WeakReference<RadioButton>>> _groups =
			new Dictionary<string, List<WeakReference<RadioButton>>>();

		void UpdateGroup(string? oldName, string? newName)
		{
			if (!string.IsNullOrEmpty(oldName))
				RemoveFromGroup(oldName!);
			if (!string.IsNullOrEmpty(newName))
				AddToGroup(newName!);
			if (_button != null)
				ApplyGtkGroup();
		}

		void RemoveFromGroup(string name)
		{
			if (!_groups.TryGetValue(name, out var list)) return;
			list.RemoveAll(r => !r.TryGetTarget(out var rb) || ReferenceEquals(rb, this));
		}

		void AddToGroup(string name)
		{
			if (!_groups.TryGetValue(name, out var list))
			{
				list = new List<WeakReference<RadioButton>>();
				_groups[name] = list;
			}
			list.Add(new WeakReference<RadioButton>(this));
		}

		void ApplyGtkGroup()
		{
			var name = GroupName;
			if (string.IsNullOrEmpty(name) || _button == null) return;

			if (!_groups.TryGetValue(name, out var list)) return;

			// Find the first live other member that has a widget; set this as its group peer.
			// GtkSharp4 exposes the GTK "group" via a property setter (not SetGroup()).
			foreach (var wr in list)
			{
				if (!wr.TryGetTarget(out var other)) continue;
				if (ReferenceEquals(other, this)) continue;
				if (other._button == null) continue;
				_button.Group = other._button;
				return;
			}
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.CheckButton _button = null!;
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_button = new Gtk.CheckButton { Active = IsChecked };
			_button.Toggled += OnGtkToggled;
			ApplyGtkGroup();
			return _button;
		}

		/// <inheritdoc/>
		protected override Gtk.Box ContentContainer =>
			throw new InvalidOperationException("RadioButton manages its own widget.");

		/// <inheritdoc/>
		protected override Gtk.Label ContentLabel =>
			throw new InvalidOperationException("RadioButton manages its own widget.");

		void OnGtkToggled(object? sender, EventArgs e)
		{
			if (_updating) return;
			_updating = true;
			try { SetValue(IsCheckedProperty, _button.Active); }
			finally { _updating = false; }
		}
	}
}
