using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogConsumer.Common
{
	internal class ApplicationLogic : IApplicationLogic
	{
		private readonly ILogger<ApplicationLogic> logger;

		private readonly ApplicationSettings settings;

		private RabbitMQSettings RabbitMQSettings => settings.RabbitMQ;

		public ApplicationLogic(ILogger<ApplicationLogic> logger, IOptions<ApplicationSettings> options)
		{
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = RabbitMQSettings.Hostname,
			};

			logger.LogInformation("Creating connection ...");
			using var connection = factory.CreateConnection();

			logger.LogInformation("Creating channel ...");
			using var channel = connection.CreateModel();

			logger.LogInformation("Creating exchange ...");
			channel.ExchangeDeclare(exchange: RabbitMQSettings.ExchangeName, type: ExchangeType.Direct);

			var queueInfo = channel.QueueDeclare();
			logger.LogInformation("Created queue {QueueName}", queueInfo.QueueName);

			foreach (var logLevel in settings.LogLevels)
			{
				channel.QueueBind(queue: queueInfo.QueueName, exchange: RabbitMQSettings.ExchangeName, routingKey: logLevel);
			}

			var consumer = new EventingBasicConsumer(channel);
			consumer.Received += (_, ea) =>
			{
				var messageText = Encoding.UTF8.GetString(ea.Body.ToArray());
				logger.LogInformation("Received message: {MessageText}", messageText);
			};

			logger.LogInformation("Consuming the messages. Press enter to exit.");
			channel.BasicConsume(queue: queueInfo.QueueName, autoAck: true, consumer: consumer);

			Console.ReadLine();

			return Task.FromResult(0);
		}
	}
}
