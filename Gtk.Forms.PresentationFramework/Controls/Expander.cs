using System.Windows.Markup;

namespace System.Windows.Controls
{
	/// <summary>
	/// A control with a collapsible content area, backed by <see cref="Gtk.Expander"/>.
	/// Two-way: GTK expand/collapse writes back into <see cref="IsExpanded"/>.
	/// </summary>
	[ContentProperty("Content")]
	public class Expander : HeaderedContentControl
	{
		/// <summary>Raised when <see cref="IsExpanded"/> becomes <c>true</c>.</summary>
		public event EventHandler? Expanded;

		/// <summary>Raised when <see cref="IsExpanded"/> becomes <c>false</c>.</summary>
		public event EventHandler? Collapsed;

		// ── IsExpanded DP ────────────────────────────────────────────────────────

		/// <summary><c>true</c> when the expander is open.</summary>
		public static readonly DependencyProperty IsExpandedProperty =
			DependencyProperty.Register(
				nameof(IsExpanded),
				typeof(bool),
				typeof(Expander),
				new PropertyMetadata(false, OnIsExpandedChanged));

		static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var exp = (Expander)d;
			var value = (bool?)e.NewValue == true;
			if (exp._expander != null && exp._expander.Expanded != value)
				exp._expander.Expanded = value;
			if (value)
				exp.Expanded?.Invoke(exp, EventArgs.Empty);
			else
				exp.Collapsed?.Invoke(exp, EventArgs.Empty);
		}

		/// <summary>Gets or sets whether the expander is open.</summary>
		public bool IsExpanded
		{
			get => (bool?)GetValue(IsExpandedProperty) == true;
			set => SetValue(IsExpandedProperty, value);
		}

		// ── GTK backing ─────────────────────────────────────────────────────────

		Gtk.Expander _expander = null!;
		bool _updating;

		/// <inheritdoc/>
		protected override Gtk.Widget CreateWidget()
		{
			_expander = new Gtk.Expander(HeaderText);
			_expander.Expanded = IsExpanded;
			if (Content is UIElement child)
				_expander.Child = child.Widget;
			_expander.AddNotification("expanded", OnGtkNotifyExpanded);
			return _expander;
		}

		/// <inheritdoc/>
		protected override Gtk.Box ContentContainer =>
			throw new InvalidOperationException("Expander manages its own content.");

		/// <inheritdoc/>
		protected override Gtk.Label ContentLabel =>
			throw new InvalidOperationException("Expander manages its own content.");

		/// <inheritdoc/>
		protected override void OnHeaderChanged(object? oldHeader, object? newHeader)
		{
			if (_expander != null)
				_expander.Label = HeaderText;
		}

		/// <inheritdoc/>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == ContentProperty && _expander != null)
			{
				if (e.OldValue is UIElement old && _expander.Child != null)
					_expander.Child = null;
				if (e.NewValue is UIElement newEl)
					_expander.Child = newEl.Widget;
			}
		}

		string HeaderText => Header as string ?? Header?.ToString() ?? string.Empty;

		void OnGtkNotifyExpanded(object? sender, GLib.NotifyArgs e)
		{
			if (e.Property != "expanded") return;
			if (_updating) return;
			_updating = true;
			try { SetValue(IsExpandedProperty, _expander.Expanded); }
			finally { _updating = false; }
		}
	}
}
