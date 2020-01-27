namespace VMS.TPS
{
	using System;
	using System.Windows.Input;


  /// <summary>
  /// A simple wait cursor to alert the user the script is in progress.
  /// </summary>
	public class WaitCursor : IDisposable
	{
		private Cursor _previousCursor;

		public WaitCursor()
		{
			_previousCursor = Mouse.OverrideCursor;

			Mouse.OverrideCursor = Cursors.Wait;
		}

		#region IDisposable Members

		public void Dispose()
		{
			Mouse.OverrideCursor = _previousCursor;
		}

		#endregion IDisposable Members
	}
}