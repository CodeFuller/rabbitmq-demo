using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Receiver
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly ILogger<ApplicationLogic> logger;

		private readonly RabbitMQSettings settings;

		public ApplicationLogic(ILogger<ApplicationLogic> logger, IOptions<RabbitMQSettings> options)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public async Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = settings.Hostname,
			};

			logger.LogInformation("Creating connection ...");
			using var connection = factory.CreateConnection();

			logger.LogInformation("Creating channel ...");
			using var channel = connection.CreateModel();

			channel.QueueDeclare(queue: settings.QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (_, ea) =>
			{
				logger.LogInformation("Received!");

				var messageText = Encoding.UTF8.GetString(ea.Body.ToArray());
				logger.LogInformation("Received message: {MessageText}", messageText);
			};

			await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

			logger.LogInformation("Consuming the messages. Press enter to exit.");
			channel.BasicConsume(queue: settings.QueueName, autoAck: true, consumer: consumer);

			Console.ReadLine();

			return 0;
		}
	}
}
