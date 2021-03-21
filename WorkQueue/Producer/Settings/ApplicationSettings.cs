using WorkQueue.Common;

namespace Producer.Settings
{
	public class ApplicationSettings
	{
		public WorkSettings Work { get; set; }

		public RabbitMQSettings RabbitMQ { get; set; }
	}
}
