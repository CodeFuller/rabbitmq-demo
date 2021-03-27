using System.Collections.Generic;

namespace LogConsumer.Common
{
	internal class ApplicationSettings
	{
		public ICollection<string> LogLevels { get; } = new List<string>();

		public RabbitMQSettings RabbitMQ { get; set; }
	}
}
