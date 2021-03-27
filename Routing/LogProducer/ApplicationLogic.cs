using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CodeFuller.Library.Bootstrap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace LogProducer
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

		public Task<int> Run(string[] args, CancellationToken cancellationToken)
		{
			var factory = new ConnectionFactory
			{
				HostName = settings.Hostname,
			};

			logger.LogInformation("Creating connection ...");
			using var connection = factory.CreateConnection();

			logger.LogInformation("Creating channel ...");
			using var channel = connection.CreateModel();

			logger.LogInformation("Creating exchange ...");
			channel.ExchangeDeclare(exchange: settings.ExchangeName, type: ExchangeType.Direct);

			ProduceLogMessage(channel, "info", "This is information message");
			ProduceLogMessage(channel, "warning", "This is warning message");
			ProduceLogMessage(channel, "error", "This is error message");

			logger.LogInformation("Exiting LogProducer ...");

			return Task.FromResult(0);
		}

		private void ProduceLogMessage(IModel channel, string logLevel, string logMessage)
		{
			logger.LogInformation("Publishing message: {LogLevel} {LogMessage}", logLevel, logMessage);

			var body = Encoding.UTF8.GetBytes(logMessage);
			channel.BasicPublish(exchange: settings.ExchangeName, routingKey: logLevel, basicProperties: null, body: body);
		}
	}
}
