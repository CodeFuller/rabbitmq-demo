using System;

namespace WorkQueue.Common
{
	public class TaskPayload
	{
		public int TaskNumber { get; set; }

		public TimeSpan Duration { get; set; }
	}
}
