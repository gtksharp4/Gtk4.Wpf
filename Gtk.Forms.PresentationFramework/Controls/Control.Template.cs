namespace System.Windows.Controls
{
	/// <summary>
	/// Partial extension of <see cref="Control"/> that adds the <see cref="Template"/>
	/// dependency property. When a <see cref="ControlTemplate"/> is set, its visual tree
	/// is built and replaces the control's default widget.
	/// </summary>
	public abstract partial class Control
	{
		/// <summary>The control template for this control.</summary>
		public static readonly DependencyProperty TemplateProperty =
			DependencyProperty.Register(
				nameof(Template),
				typeof(ControlTemplate),
				typeof(Control),
				new PropertyMetadata(null, OnTemplateChanged));

		static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((Control)d).OnTemplateChanged((ControlTemplate?)e.NewValue);
		}

		/// <summary>Gets or sets the control template.</summary>
		public ControlTemplate? Template
		{
			get => (ControlTemplate?)GetValue(TemplateProperty);
			set => SetValue(TemplateProperty, value);
		}

		// The root UIElement built from the current template (null = default widget in use).
		UIElement? _templateRoot;

		/// <summary>
		/// Called when <see cref="Template"/> changes.
		/// Builds the template's visual tree and re-parents it into the control's GTK host.
		/// When <paramref name="template"/> is <c>null</c> the control reverts to its default widget.
		/// </summary>
		protected virtual void OnTemplateChanged(ControlTemplate? template)
		{
			_templateRoot = null;
			if (template == null || !template.HasContent) return;

			_templateRoot = template.CreateVisualTree();
			if (_templateRoot == null) return;

			// If the backing widget has already been created, swap in the template widget.
			// If it has not, Widget property creation will be skipped for the default path
			// — override OnApplyTemplate in the concrete class if needed.
			// For the minimal Phase-2 wire-up we re-host only when already materialised.
			ApplyTemplateRoot();
		}

		/// <summary>
		/// Hosts the current <see cref="_templateRoot"/> inside this control's GTK widget.
		/// Base implementation does nothing — concrete controls override as needed.
		/// </summary>
		protected virtual void ApplyTemplateRoot() { }

		/// <summary>
		/// Calls <see cref="ApplyTemplateRoot"/> after the backing widget is first created.
		/// Subclasses that override <see cref="UIElement.CreateWidget"/> should call this.
		/// </summary>
		protected void OnWidgetCreated()
		{
			if (_templateRoot != null)
				ApplyTemplateRoot();
		}
	}
}
