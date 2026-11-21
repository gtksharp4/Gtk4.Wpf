namespace System.Windows.Data;

internal static partial class BindingEngine
{
	internal sealed partial class BindingRecord
	{
		internal void WriteBackPublic(DependencyObject target) => WriteBack(target);

		void WriteBack(DependencyObject target)
		{
			if (_pushing) return;
			_dirty = false;
			_pushing = true;
			try
			{
				var source = ResolveSource(target);
				if (source != null) WriteBackCore(target, source);
			}
			finally
			{
				_pushing = false;
			}
		}
	}
}
