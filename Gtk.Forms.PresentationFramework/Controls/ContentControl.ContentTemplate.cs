namespace System.Windows.Controls
{
	/// <summary>
	/// Partial extension of <see cref="ContentControl"/> that adds the
	/// <see cref="ContentTemplate"/> dependency property.
	/// When set, the template's visual tree wraps the <see cref="ContentControl.Content"/>
	/// value instead of displaying it directly.
	/// </summary>
	public partial class ContentControl
	{
		/// <summary>The template used to present the content.</summary>
		public static readonly DependencyProperty ContentTemplateProperty =
			DependencyProperty.Register(
				nameof(ContentTemplate),
				typeof(DataTemplate),
				typeof(ContentControl),
				new PropertyMetadata(null, OnContentTemplateChanged));

		static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ContentControl)d).ApplyContentTemplate((DataTemplate?)e.NewValue);
		}

		/// <summary>Gets or sets the content template.</summary>
		public DataTemplate? ContentTemplate
		{
			get => (DataTemplate?)GetValue(ContentTemplateProperty);
			set => SetValue(ContentTemplateProperty, value);
		}

		void ApplyContentTemplate(DataTemplate? template)
		{
			if (!IsWidgetCreated) return;
			// Remove the current content and re-apply through the new template.
			var content = Content;
			if (content is UIElement oldEl)
				ContentContainer.Remove(oldEl.Widget);

			if (template != null && template.HasContent)
			{
				var visual = template.CreateVisualTreeForItem(content);
				if (visual != null)
				{
					visual.LogicalParent = this;
					ContentContainer.Add(visual.Widget);
					return;
				}
			}

			// No template: fall back to default content display.
			if (content is UIElement newEl)
				ContentContainer.Add(newEl.Widget);
			else if (content is string s)
				ContentLabel.Text = s;
			else if (content != null)
				ContentLabel.Text = content.ToString() ?? string.Empty;
		}
	}
}
