using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace ProbeNpp
{
	internal class BackgroundDeferrer
	{
		private Timer _timer = null;
		private int _timeout = k_defaultTimeout;

		private const int k_defaultTimeout = 1000;

		public event EventHandler Execute;
		public event EventHandler Activity;

		public BackgroundDeferrer(int timeout = k_defaultTimeout)
		{
			_timeout = timeout;

			_timer = new Timer(_timeout);
			_timer.Elapsed += OnElapsed;
			_timer.AutoReset = false;
		}

		public void OnActivity()
		{
			_timer.Stop();
			_timer.Start();

			var ev = Activity;
			if (ev != null) ev(this, new EventArgs());
		}

		public void Cancel()
		{
			_timer.Stop();
		}

		private void OnElapsed(object sender, EventArgs e)
		{
			var ev = Execute;
			if (ev != null) ev(this, new EventArgs());
		}
	}
}
