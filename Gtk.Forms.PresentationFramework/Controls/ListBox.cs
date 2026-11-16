using System.Collections.Generic;

namespace System.Windows.Controls
{
	/// <summary>
	/// An items control with single-item selection, backed by <see cref="Gtk.ListBox"/>.
	/// <para>
	/// Each item is wrapped in a <see cref="ListBoxItem"/>. Selection maps to
	/// <see cref="SelectedItem"/> / <see cref="SelectedIndex"/>;
	/// <see cref="SelectionChanged"/> is raised on every change.
	/// </para>
	/// </summary>
	public class ListBox : ItemsControl
	{
		// ── SelectionChanged event ────────────────────────────────────────────────

		/// <summary>Raised when <see cref="SelectedItem"/> or <see cref="SelectedIndex"/> changes.</summary>
		public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

		// ── SelectedIndex DP ──────────────────────────────────────────────────────

		/// <summary>Zero-based index of the selected item; -1 when nothing selected.</summary>
		public static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.Register(
				nameof(SelectedIndex),
				typeof(int),
				typeof(ListBox),
				new PropertyMetadata(-1, OnSelectedIndexChanged));

		static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ListBox)d).ApplySelectedIndex((int?)e.NewValue ?? -1, fromGtk: false);
		}

		/// <summary>Gets or sets the selected index.</summary>
		public int SelectedIndex
		{
			get => (int?)GetValue(SelectedIndexProperty) ?? -1;
			set => SetValue(SelectedIndexProperty, value);
		}

		// ── SelectedItem DP ───────────────────────────────────────────────────────

		/// <summary>The currently selected item.</summary>
		public static readonly DependencyProperty SelectedItemProperty =
			DependencyProperty.Register(
				nameof(SelectedItem),
				typeof(object),
				typeof(ListBox),
				new PropertyMetadata(null, OnSelectedItemChanged));

		static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ListBox)d).ApplySelectedItem(e.OldValue, e.NewValue);
		}

		/// <summary>Gets or sets the selected item.</summary>
		public object? SelectedItem
		{
			get => GetValue(SelectedItemProperty);
			set => SetValue(SelectedItemProperty, value);
		}

		// ── GTK backing ──────────────────────────────────────────────────────────

		Gtk.ListBox? _gtkListBox;
		readonly List<ListBoxItem> _containers = new List<ListBoxItem>();
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateItemsWidget()
		{
			_gtkListBox = new Gtk.ListBox();
			_gtkListBox.SelectionMode = Gtk.SelectionMode.Single;
			_gtkListBox.RowSelected += OnGtkRowSelected;
			return _gtkListBox;
		}

		/// <inheritdoc/>
		protected override UIElement GetContainerForItem(object? item)
		{
			var lbi = new ListBoxItem { Content = item };
			_containers.Add(lbi);
			return lbi;
		}

		/// <inheritdoc/>
		protected override void AddItemWidget(Gtk.Widget w) => _gtkListBox!.Append(w);

		/// <inheritdoc/>
		protected override void RemoveItemWidget(Gtk.Widget w) => _gtkListBox!.Remove(w);

		/// <inheritdoc/>
		protected override void OnItemsRebuilt()
		{
			_containers.Clear();
			// SelectedIndex/-Item reset on rebuild.
			_updating = true;
			try
			{
				SetValue(SelectedIndexProperty, -1);
				SetValue(SelectedItemProperty, null);
			}
			finally { _updating = false; }
		}

		void OnGtkRowSelected(object? sender, Gtk.RowSelectedArgs e)
		{
			if (_updating) return;
			_updating = true;
			try
			{
				var row = e.Row;
				var oldItem = SelectedItem;

				if (row == null)
				{
					SetValue(SelectedIndexProperty, -1);
					SetValue(SelectedItemProperty, null);
					SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, null));
					return;
				}

				int idx = -1;
				for (int i = 0; i < _containers.Count; i++)
				{
					if (ReferenceEquals(_containers[i].Row, row)) { idx = i; break; }
				}
				if (idx < 0) return;

				var newItem = idx < Items.Count ? Items[idx] : null;
				SetValue(SelectedIndexProperty, idx);
				SetValue(SelectedItemProperty, newItem);
				SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, newItem));
			}
			finally { _updating = false; }
		}

		void ApplySelectedIndex(int idx, bool fromGtk)
		{
			if (_updating) return;
			_updating = true;
			try
			{
				if (idx < 0 || idx >= Items.Count)
				{
					if (_gtkListBox != null) _gtkListBox.UnselectAll();
					SetValue(SelectedItemProperty, null);
					return;
				}
				var newItem = idx < Items.Count ? Items[idx] : null;
				SetValue(SelectedItemProperty, newItem);
				if (_gtkListBox != null && idx < _containers.Count)
				{
					var row = _containers[idx].Row;
					if (row != null) _gtkListBox.SelectRow(row);
				}
			}
			finally { _updating = false; }
		}

		void ApplySelectedItem(object? oldItem, object? newItem)
		{
			if (_updating) return;
			int idx = -1;
			for (int i = 0; i < Items.Count; i++)
			{
				if (Equals(Items[i], newItem)) { idx = i; break; }
			}
			_updating = true;
			try
			{
				if (SelectedIndex != idx)
					SetValue(SelectedIndexProperty, idx);
			}
			finally { _updating = false; }
			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(this, oldItem, newItem));
		}
	}
}
