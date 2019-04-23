namespace VMS.TPS
{
	using System;
	using System.Windows.Input;

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