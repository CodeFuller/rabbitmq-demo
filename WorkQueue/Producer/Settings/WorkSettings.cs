using System;

namespace Producer.Settings
{
	public class WorkSettings
	{
		public int TasksNumber { get; set; }

		public TimeSpan MaxTaskDuration { get; set; }
	}
}
